using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    public partial class BulkLoad : BaseEntity
    {
        public int Id { get; set; }

        public Guid Code { get; set; }

        public int FormatId { get; set; }

        public Guid FormatCode { get; set; }

        public int ObjectId { get; set; }

        public Guid ObjectCode { get; set; }

        public string? ObjectName { get; set; }

        public string? FileName { get; set; }

        public string? Description { get; set; }

        public int? Records { get; set; }

        public int? UploadedRecords { get; set; }

        public int? ObservedRecords { get; set; }

        public DateTime Date { get; set; }

        public string StateCode { get; set; } = null!;

        public string User { get; set; } = null!;

        public virtual ICollection<BulkLoadState> BulkLoadStates { get; } = new List<BulkLoadState>();

        public virtual ICollection<BulkLoadResult> BulkLoadResults { get; } = new List<BulkLoadResult>();

    }
}