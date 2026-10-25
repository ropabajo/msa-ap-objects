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
    [InlineData("db_object")]
    [InlineData("db_election")]
    [InlineData("db_file_jobs_prod")]
    public void Development_rejects_shared_or_productive_databases(string database)
    {
        var values = Settings(); values["ConnectionStrings:FileJobs"] = $"Server=127.0.0.1;Database={database};User ID=fixture;Password=local-test-only";
        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddFileJobs(new ConfigurationBuilder().AddInMemoryCollection(values).Build()));
        Assert.Contains("isolated metadata database", error.Message);
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
        ["FileJobs:Enabled"] = "true", ["FileJobs:Environment"] = "development",
        ["FileJobs:WorkerAuthority"] = "https://identity.example.test/realms/church", ["FileJobs:WorkerAudience"] = "objects-files", ["FileJobs:WorkerClients:test-worker"] = "church",
        ["FileJobs:Broker:HostName"] = "127.0.0.1", ["FileJobs:Broker:VirtualHost"] = "/", ["FileJobs:Broker:UserName"] = "fixture", ["FileJobs:Broker:Password"] = "local-test-only", ["FileJobs:Broker:Exchange"] = "development.file-jobs.v1",
        ["ConnectionStrings:FileJobs"] = "Server=127.0.0.1;Database=db_file_jobs_dev;User ID=fixture;Password=local-test-only"
    };
}
