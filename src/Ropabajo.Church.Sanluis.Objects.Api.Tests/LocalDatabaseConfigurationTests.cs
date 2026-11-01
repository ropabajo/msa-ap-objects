using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ropabajo.Church.Sanluis.Objects.Infraestructure;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;
using Xunit;

public sealed class LocalDatabaseConfigurationTests
{
    [Theory]
    [InlineData("161.132.54.155")]
    [InlineData("mysql-local.church-dev.svc.cluster.local")]
    public void Local_accepts_the_development_database_without_opening_connections(string server)
    {
        LocalDatabaseConfiguration.Validate(Settings(server, 3306, "db_object"), "Local");
    }

    [Theory]
    [InlineData("161.132.54.155", 3307, "db_object")]
    [InlineData("mysql-objects-shared.church-dev.svc.cluster.local", 3306, "db_object")]
    [InlineData("another-server", 3306, "db_object")]
    [InlineData("161.132.54.155", 3306, "another_database")]
    public void Local_rejects_shared_and_unconfirmed_destinations_even_with_file_jobs_disabled(string server, int port, string database)
    {
        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddInfrastructure(Settings(server, port, database), "Local"));
        Assert.DoesNotContain("local-test-only", error.Message);
    }

    [Theory]
    [InlineData("", "local-test-only")]
    [InlineData("usr_object", "")]
    public void Local_requires_runtime_credentials(string user, string password)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = $"Server=161.132.54.155;Port=3306;Database=db_object;User ID={user};Password={password}"
        }).Build();
        Assert.Throws<InvalidOperationException>(() => LocalDatabaseConfiguration.Validate(configuration, "Local"));
    }

    [Fact]
    public void Production_keeps_its_existing_connection_contract()
    {
        LocalDatabaseConfiguration.Validate(Settings("161.132.54.155", 3307, "db_object"), "Production");
    }

    private static IConfiguration Settings(string server, int port, string database)
        => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = $"Server={server};Port={port};Database={database};User ID=usr_object;Password=local-test-only",
            ["FileJobs:Enabled"] = "false"
        }).Build();
}
