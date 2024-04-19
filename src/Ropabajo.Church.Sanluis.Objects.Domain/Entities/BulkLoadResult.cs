using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    public partial class BulkLoadResult : BaseEntity
    {
        public int Id { get; set; }

        public int BulkLoadId { get; set; }

        public int RowNumber { get; set; }

        public string Row { get; set; } = null!;

        public string StateCode { get; set; } = null!;

        public string? Observation { get; set; }

    }
}