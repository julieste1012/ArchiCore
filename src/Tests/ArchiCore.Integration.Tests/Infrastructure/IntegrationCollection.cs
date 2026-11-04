using Xunit;

namespace ArchiCore.Integration.Tests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "ArchiCore API with PostgreSQL";
}
