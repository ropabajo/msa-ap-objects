using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Ropabajo.Churc.Sanluis.Framework.Files.Messaging;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Producers.FileJobs;

/// <summary>
/// Publica las referencias de la outbox mediante el productor confirmado del framework.
/// </summary>
public sealed class FileJobProducer : ConfirmedFilePublisher
{
    /// <summary>Selecciona el exchange de Objects y su conexión técnica propia.</summary>
    /// <param name="factory">Fábrica inyectada por AddRabbitMqProducer, independiente del bus legacy.</param>
    /// <param name="settings">Opciones del ambiente.</param>
    public FileJobProducer(ConnectionFactory factory, IOptions<FileJobsRuntimeSettings> settings)
        : base(factory, (settings ?? throw new ArgumentNullException(nameof(settings))).Value.Broker.Exchange) { }
}
