using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Ropabajo.Churc.Sanluis.Framework.RabbitMq;
using Ropabajo.Church.Sanluis.Objects.Application.Events.BulkLoads.BulkLoadCreated;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Producers.BulkLoads
{
    public class BulkLoadCreatedProducer : Producer<BulkLoadCreatedEvent>
    {
        protected override string Exchange => "nc.objects.exchange";

        protected override string RoutingKey => "nc.objects.bulk-loads.created";

        public BulkLoadCreatedProducer(
            ConnectionFactory connectionFactory,
            ILogger<RabbitMqClient> clientLogger,
            ILogger<Producer<BulkLoadCreatedEvent>> producerLogger
            ) : base(
                connectionFactory,
                clientLogger,
                producerLogger
                )
        { }
    }
}
