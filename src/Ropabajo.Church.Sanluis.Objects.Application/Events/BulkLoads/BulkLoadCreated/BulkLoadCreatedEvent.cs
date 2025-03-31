using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;

namespace Ropabajo.Church.Sanluis.Objects.Application.Events.BulkLoads.BulkLoadCreated
{
    public class BulkLoadCreatedEvent : Event, INotification
    {
        public BulkLoad BulkLoad { get; set; }

        public BulkLoadCreatedEvent(BulkLoad bulkLoad)
        {
            BulkLoad = bulkLoad;
            EventType = EventType.Created;
        }
    }
}
