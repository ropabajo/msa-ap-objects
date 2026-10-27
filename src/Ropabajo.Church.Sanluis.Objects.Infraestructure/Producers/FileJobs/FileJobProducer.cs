using RabbitMQ.Client;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Files.Messaging;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Producers.FileJobs;

/// <summary>
/// Publica las referencias de la outbox mediante el productor confirmado del framework.
/// </summary>
public sealed class FileJobProducer : ConfirmedFilePublisher
{
    /// <inheritdoc />
    protected override string Exchange => "nc.objects.exchange";

    /// <inheritdoc />
    protected override string GetRoutingKey(GenerateFileCommand message)
        => $"nc.objects.file-jobs.{message.Producer}.requested";

    /// <summary>
    /// Recibe la misma fábrica RabbitMq que los productores existentes de Objects.
    /// </summary>
    /// <param name="factory">Conexión del microservicio registrada por AddRabbitMq.</param>
    public FileJobProducer(ConnectionFactory factory) : base(factory) { }
}
