using MediatR;
using Ropabajo.Churc.Sanluis.Framework.RabbitMq;

namespace Ropabajo.Church.Sanluis.Objects.Application.Events.BulkLoads.BulkLoadCreated
{
    public class BulkLoadCreatedEventHandler : INotificationHandler<BulkLoadCreatedEvent>
    {
        private readonly IProducer<BulkLoadCreatedEvent> _producer;

        public BulkLoadCreatedEventHandler(
            IProducer<BulkLoadCreatedEvent> producer
            )
        {
            _producer = producer;
        }

        public async Task Handle(BulkLoadCreatedEvent @event,
                                    CancellationToken cancellationToken
            )
        {
            await _producer.Publish(@event);
        }
    }
}
