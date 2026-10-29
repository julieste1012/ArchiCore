using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using ArchiCore.Application;
using ArchiCore.Application.Authentication;
using ArchiCore.Infrastructure;
using ArchiCore.Infrastructure.Persistence;
using ArchiCore.Infrastructure.Security;
using ArchiCore.Infrastructure.Persistence.Outbox;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var signingKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey) && builder.Environment.IsEnvironment("Testing"))
{
    signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Jwt:SigningKey"] = signingKey
    });
}

if (string.IsNullOrWhiteSpace(signingKey))
{
    throw new InvalidOperationException(
        "Missing Jwt:SigningKey. Set Jwt__SigningKey via environment variables or .NET user-secrets. Use a base64-encoded random key of at least 32 bytes.");
}

byte[] signingKeyBytes;
try
{
    signingKeyBytes = Convert.FromBase64String(signingKey);
}
catch (FormatException exception)
{
    throw new InvalidOperationException("Jwt:SigningKey must be base64-encoded key material.", exception);
}

if (signingKeyBytes.Length < 32)
{
    throw new InvalidOperationException("Jwt:SigningKey must decode to at least 32 bytes.");
}

var jwtSettings = new JwtSettings
{
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "ArchiCore",
    Audience = builder.Configuration["Jwt:Audience"] ?? "ArchiCore.Client",
    SigningKey = signingKey,
    AccessTokenMinutes = int.TryParse(builder.Configuration["Jwt:AccessTokenMinutes"], out var accessMinutes)
        ? Math.Clamp(accessMinutes, 5, 30)
        : 15,
    RefreshTokenDays = int.TryParse(builder.Configuration["Jwt:RefreshTokenDays"], out var refreshDays)
        ? Math.Clamp(refreshDays, 1, 30)
        : 14
};

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "ValidationFailed",
            Instance = context.HttpContext.Request.Path
        };
        problem.Extensions["code"] = "Api.ValidationFailed";
        return new UnprocessableEntityObjectResult(problem);
    };
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var subject = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                var versionClaim = context.Principal?.FindFirst("ver")?.Value;
                if (!Guid.TryParse(subject, out var userId)
                    || !int.TryParse(versionClaim, out var tokenVersion))
                {
                    context.Fail("The token does not contain a valid user identity.");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<ArchiCoreDbContext>();
                var account = await dbContext.UserAccounts.AsNoTracking()
                    .Where(user => user.Id == userId)
                    .Select(user => new { user.IsActive, user.TokenVersion })
                    .SingleOrDefaultAsync(context.HttpContext.RequestAborted);

                if (account is null || !account.IsActive || account.TokenVersion != tokenVersion)
                {
                    context.Fail("The user account is inactive or the access token has been revoked.");
                }
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

if (!builder.Environment.IsEnvironment("Testing") &&
    builder.Configuration.GetValue("Outbox:DispatchEnabled", true))
{
    builder.Services.AddHostedService<DomainEventOutboxHostedService>();
}

var app = builder.Build();

// Automatic schema updates are opt-in and intentionally restricted to the local
// container profile. Development machines and production deployments should use
// an explicit migration/deployment step instead.
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    if (!app.Environment.IsEnvironment("Container"))
    {
        throw new InvalidOperationException(
            "Database:ApplyMigrationsOnStartup is only supported by the local Container environment.");
    }

    await using var migrationScope = app.Services.CreateAsyncScope();
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<ArchiCoreDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Container"))
{
    app.MapOpenApi();
}

var bootstrapEmail = app.Configuration["BootstrapAdmin:Email"];
if (!string.IsNullOrWhiteSpace(bootstrapEmail))
{
    var bootstrapPassword = app.Configuration["BootstrapAdmin:Password"];
    var bootstrapDisplayName = app.Configuration["BootstrapAdmin:DisplayName"] ?? "ArchiCore Administrator";
    if (string.IsNullOrWhiteSpace(bootstrapPassword))
    {
        throw new InvalidOperationException("BootstrapAdmin:Password must be configured when BootstrapAdmin:Email is set.");
    }

    await using var scope = app.Services.CreateAsyncScope();
    var seeder = scope.ServiceProvider.GetRequiredService<BootstrapAdminSeeder>();
    await seeder.EnsureAsync(bootstrapEmail, bootstrapPassword, bootstrapDisplayName);
}

// The local container profile is behind a loopback-only HTTP reverse proxy;
// host-based development continues to use HTTPS redirection.
if (!app.Environment.IsEnvironment("Container"))
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }))
    .WithName("Liveness");

app.MapGet("/health/ready", async (ArchiCoreDbContext dbContext, CancellationToken cancellationToken) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        IResult result = canConnect
            ? Results.Ok(new { status = "Ready", database = "Connected" })
            : Results.Problem(title: "DatabaseUnavailable", detail: "PostgreSQL is not reachable.", statusCode: StatusCodes.Status503ServiceUnavailable);
        return result;
    }
    catch (Exception)
    {
        return Results.Problem(
            title: "DatabaseUnavailable",
            detail: "PostgreSQL is not reachable.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).WithName("Readiness");

app.Run();

public partial class Program { }
