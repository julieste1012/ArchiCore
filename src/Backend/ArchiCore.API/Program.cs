using System.Text.Json.Serialization;
using ArchiCore.Application;
using ArchiCore.Infrastructure;
using ArchiCore.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
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
