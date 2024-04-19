using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    public partial class BulkLoadState : BaseEntity
    {
        public int Id { get; set; }

        public int BulkLoadId { get; set; }

        public DateTime Date { get; set; }

        public string StateCode { get; set; } = null!;

        public string User { get; set; } = null!;
    }
}