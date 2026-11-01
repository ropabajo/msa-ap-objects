using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Files.Messaging;
using Ropabajo.Churc.Sanluis.Framework.RabbitMq;
using Xunit;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Producers.FileJobs;

public sealed class FileJobBrokerTests
{
    [Fact]
    public async Task Discovered_producer_uses_main_RabbitMq_and_routes_to_the_canonical_Church_queue()
    {
        var factory = Factory();
        await using var connection = await factory.CreateConnectionAsync(); await using var channel = await connection.CreateChannelAsync();
        await DeclareTopology(channel);
        try
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
            {
                ["RabbitMq:HostName"]="127.0.0.1", ["RabbitMq:Port"]="13372", ["RabbitMq:VirtualHost"]="local-vhost",
                ["RabbitMq:UserName"]="files-test", ["RabbitMq:Password"]="local-fixture-only"
            }).Build();
            var services = new ServiceCollection(); services.AddSingleton<IConfiguration>(configuration);
            services.AddRabbitMq(typeof(FileJobProducer).Assembly);
            await using var provider = services.BuildServiceProvider();
            var producer = Assert.IsType<FileJobProducer>(Assert.Single(provider.GetServices<IProducer<GenerateFileCommand>>()));
            var command = Command(); await producer.Publish(command);
            var message = await channel.BasicGetAsync("nc.churches.file-jobs.queue", false);
            Assert.NotNull(message); Assert.True(message.BasicProperties.Persistent);
            Assert.Equal(command.MessageId.ToString("D"), message.BasicProperties.MessageId);
            Assert.Equal(command.JobId, GenerateFileCommandCodec.Decode(message.Body.Span).JobId);
            await channel.BasicAckAsync(message.DeliveryTag, false);
        }
        finally { await Cleanup(channel); }
    }

    private static ConnectionFactory Factory()
    {
        Assert.Equal("isolated", Environment.GetEnvironmentVariable("FILE_JOBS_TEST_RABBITMQ"));
        return new ConnectionFactory { HostName = "127.0.0.1", Port = 13372, VirtualHost = "local-vhost",
            UserName = "files-test", Password = "local-fixture-only", AutomaticRecoveryEnabled = false };
    }

    private static async Task DeclareTopology(IChannel channel)
    {
        using var topology = JsonDocument.Parse(await File.ReadAllTextAsync(Environment.GetEnvironmentVariable("FILE_JOBS_TEST_TOPOLOGY_PATH")
            ?? throw new InvalidOperationException("Use the documented isolated broker and canonical definitions fixture.")));
        var exchange = topology.RootElement.GetProperty("exchanges").EnumerateArray().Single(x => x.GetProperty("name").GetString() == "nc.objects.exchange");
        Assert.Equal("direct", exchange.GetProperty("type").GetString());
        await channel.ExchangeDeclareAsync("nc.objects.exchange", "direct", true);
        foreach (var queue in topology.RootElement.GetProperty("queues").EnumerateArray().Where(x => x.GetProperty("name").GetString()!.StartsWith("nc.churches.file-jobs.")))
        {
            var arguments = queue.GetProperty("arguments").EnumerateObject().ToDictionary(x => x.Name, x => Value(x.Value));
            await channel.QueueDeclareAsync(queue.GetProperty("name").GetString()!, true, false, false, arguments);
        }
        foreach (var binding in topology.RootElement.GetProperty("bindings").EnumerateArray().Where(x => x.GetProperty("destination").GetString()!.StartsWith("nc.churches.file-jobs.")))
            await channel.QueueBindAsync(binding.GetProperty("destination").GetString()!, binding.GetProperty("source").GetString()!, binding.GetProperty("routing_key").GetString()!);
    }

    private static object? Value(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.TryGetInt32(out var number) ? (object)number : value.GetInt64(),
        JsonValueKind.True => true, JsonValueKind.False => false, _ => throw new InvalidDataException("Unexpected routing argument")
    };

    private static GenerateFileCommand Command() => new() { JobId = Guid.NewGuid(), MessageId = Guid.NewGuid(), ChurchCode = Guid.NewGuid(),
        CorrelationId = Guid.NewGuid(), CausationId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow,
        Environment = "development", Producer = "church", ReportType = "members.excel", ContractVersion = 1 };

    private static async Task Cleanup(IChannel channel)
    {
        foreach (var queue in new[] { "nc.churches.file-jobs.queue", "nc.churches.file-jobs.retry.queue", "nc.churches.file-jobs.dead.queue" })
            await channel.QueueDeleteAsync(queue);
        await channel.ExchangeDeleteAsync("nc.objects.exchange");
    }
}
