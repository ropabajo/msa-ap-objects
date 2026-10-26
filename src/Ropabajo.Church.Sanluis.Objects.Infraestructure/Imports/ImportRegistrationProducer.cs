using Naza.Church.BulkLoads.Contracts;
using RabbitMQ.Client;
using Ropabajo.Churc.Sanluis.Framework.Integration;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Imports;

/// <summary>Publica solicitudes usando la conexión RabbitMq y los nombres del servicio.</summary>
public sealed class ImportRegistrationProducer(ConnectionFactory factory) : JsonMessageProducer<BulkImportRegistered>(factory,
    "nc.objects.exchange", "nc.objects.imports.registered", "bulk.load.registered.v1", x => x.MessageId, x => x.CorrelationId);
