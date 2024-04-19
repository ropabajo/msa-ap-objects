using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    public class ObjectState : BaseEntity
    {
        public int Id { get; set; }

        public int ObjectId { get; set; }

        public DateTime Date { get; set; }

        public string StateCode { get; set; } = null!;

        public string User { get; set; } = null!;

    }
}