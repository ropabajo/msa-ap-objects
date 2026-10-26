using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.RabbitMq;
using Ropabajo.Church.Sanluis.Objects.Api.Extensions;
using Ropabajo.Church.Sanluis.Objects.Application;
using Ropabajo.Church.Sanluis.Objects.Infraestructure;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Hosting;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Producers.FileJobs;
using Xunit;

public sealed class StartupCompositionTests
{
    [Fact]
    public async Task Layer_registration_resolves_one_confirmed_producer_and_one_disabled_dispatcher_without_network()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "server=mysql-local.church-dev.svc.cluster.local;port=3306;database=db_object;user=fixture;password=local-test-only",
            ["RabbitMq:HostName"] = "legacy-broker", ["RabbitMq:Port"] = "5672", ["RabbitMq:VirtualHost"] = "legacy",
            ["FileJobs:Enabled"] = "false", ["TemporaryExports:Enabled"] = "false"
        }).Build();
        var services = new ServiceCollection(); services.AddLogging(); services.AddSingleton<IConfiguration>(configuration);
        services.AddApplication(); services.AddInfrastructure(configuration, "Local"); services.AddObjectsApi(configuration);
        await using var provider = services.BuildServiceProvider();
        Assert.IsType<FileJobProducer>(Assert.Single(provider.GetServices<IProducer<GenerateFileCommand>>()));
        var dispatcher = Assert.Single(provider.GetServices<IHostedService>().OfType<FileJobsDispatcher>());
        await dispatcher.StartAsync(default);
        await dispatcher.StopAsync(default);
    }
}
