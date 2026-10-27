using Naza.Church.BulkLoads.Contracts;
using RabbitMQ.Client;
using Ropabajo.Churc.Sanluis.Framework.Integration;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Imports;

/// <summary>Publica solicitudes usando la conexión RabbitMq y los nombres del servicio.</summary>
public sealed class ImportRetryProducer(ConnectionFactory factory) : JsonMessageProducer<ImportRowRetryRequested>(factory,
    "nc.objects.exchange", "nc.objects.imports.retry-requested", "bulk.row.retry-requested.v1", x => x.MessageId, x => x.CorrelationId);
