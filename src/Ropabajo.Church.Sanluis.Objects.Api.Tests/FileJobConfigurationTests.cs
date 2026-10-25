using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Persistence;
using Xunit;

public sealed class FileJobConfigurationTests
{
    [Fact]
    public void Disabled_feature_requires_no_database_or_network_credentials()
    {
        var services = new ServiceCollection(); services.AddFileJobs(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        Assert.IsType<DisabledFileJobStore>(provider.GetRequiredService<IFileJobStore>());
        Assert.Empty(new FileJobsRuntimeSettings().WorkerClients);
        Assert.Empty(new FileJobsRuntimeSettings().WorkerAuthority);
    }

    [Theory]
    [InlineData("db_file_jobs_dev")]
    [InlineData("db_election")]
    [InlineData("db_file_jobs_prod")]
    public void Development_rejects_other_databases(string database)
    {
        var values = Settings(); values["ConnectionStrings:FileJobs"] = $"Server=mysql-local.church-dev.svc.cluster.local;Database={database};User ID=fixture;Password=local-test-only";
        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddFileJobs(new ConfigurationBuilder().AddInMemoryCollection(values).Build()));
        Assert.Contains("configured metadata database", error.Message);
    }

    [Theory]
    [InlineData("161.132.54.155", 3307)]
    [InlineData("mysql-objects-shared.church-dev.svc.cluster.local", 3306)]
    [InlineData("127.0.0.1", 3306)]
    [InlineData("another-server", 3306)]
    public void Development_rejects_the_previous_shared_endpoint_and_unconfirmed_hosts(string host, int port)
    {
        var values = Settings();
        values["ConnectionStrings:FileJobs"] = $"Server={host};Port={port};Database=db_object;User ID=fixture;Password=local-test-only";
        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddFileJobs(new ConfigurationBuilder().AddInMemoryCollection(values).Build()));
        Assert.Contains("same 3306 endpoint", error.Message);
    }

    [Theory]
    [InlineData("161.132.54.155")]
    [InlineData("mysql-local.church-dev.svc.cluster.local")]
    public void Development_registers_the_confirmed_database_without_opening_a_connection(string host)
    {
        var values = Settings();
        var connection = $"Server={host};Port=3306;Database=db_object;User ID=fixture;Password=local-test-only";
        values["ConnectionStrings:DefaultConnection"] = connection;
        values["ConnectionStrings:FileJobs"] = connection;
        var services = new ServiceCollection().AddFileJobs(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IFileJobStore) && descriptor.ImplementationType == typeof(SqlFileJobStore));
    }

    [Theory]
    [InlineData("mysql-objects-shared.church-dev.svc.cluster.local", 3306, "db_object")]
    [InlineData("mysql-local.church-dev.svc.cluster.local", 3307, "db_object")]
    [InlineData("mysql-local.church-dev.svc.cluster.local", 3306, "another_database")]
    public void Metadata_and_application_must_use_the_same_development_database(string host, int port, string database)
    {
        var values = Settings();
        values["ConnectionStrings:DefaultConnection"] = $"Server={host};Port={port};Database={database};User ID=fixture;Password=local-test-only";
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddFileJobs(new ConfigurationBuilder().AddInMemoryCollection(values).Build()));
    }

    [Theory]
    [InlineData("", "local-test-only")]
    [InlineData("fixture", "")]
    public void Activation_requires_private_database_credentials(string user, string password)
    {
        var values = Settings();
        values["ConnectionStrings:FileJobs"] = $"Server=mysql-local.church-dev.svc.cluster.local;Database=db_object;User ID={user};Password={password}";
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddFileJobs(new ConfigurationBuilder().AddInMemoryCollection(values).Build()));
    }

    [Fact]
    public void Production_keeps_its_existing_metadata_database_contract()
    {
        var values = Settings();
        values["FileJobs:Environment"] = "production";
        values["FileJobs:Broker:Exchange"] = "production.file-jobs.v1";
        values["ConnectionStrings:FileJobs"] = "Server=production.example.test;Database=db_file_jobs_prod;User ID=fixture;Password=local-test-only";
        var services = new ServiceCollection().AddFileJobs(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        Assert.Contains(services, descriptor => descriptor.ImplementationType == typeof(SqlFileJobStore));
    }

    [Fact]
    public void Activation_rejects_unspecified_identity_and_another_environment_exchange()
    {
        var values = Settings(); values["FileJobs:WorkerAuthority"] = "";
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddFileJobs(new ConfigurationBuilder().AddInMemoryCollection(values).Build()));
        values = Settings(); values["FileJobs:Broker:Exchange"] = "production.file-jobs.v1";
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddFileJobs(new ConfigurationBuilder().AddInMemoryCollection(values).Build()));
    }

    private static Dictionary<string, string?> Settings() => new()
    {
        ["FileJobs:Enabled"] = "true",
        ["FileJobs:Environment"] = "development",
        ["FileJobs:WorkerAuthority"] = "https://identity.example.test/realms/church",
        ["FileJobs:WorkerAudience"] = "objects-files",
        ["FileJobs:WorkerClients:test-worker"] = "church",
        ["FileJobs:Broker:HostName"] = "127.0.0.1",
        ["FileJobs:Broker:VirtualHost"] = "/",
        ["FileJobs:Broker:UserName"] = "fixture",
        ["FileJobs:Broker:Password"] = "local-test-only",
        ["FileJobs:Broker:Exchange"] = "development.file-jobs.v1",
        ["ConnectionStrings:DefaultConnection"] = "Server=mysql-local.church-dev.svc.cluster.local;Database=db_object;User ID=fixture;Password=local-test-only",
        ["ConnectionStrings:FileJobs"] = "Server=mysql-local.church-dev.svc.cluster.local;Database=db_object;User ID=fixture;Password=local-test-only"
    };
}
