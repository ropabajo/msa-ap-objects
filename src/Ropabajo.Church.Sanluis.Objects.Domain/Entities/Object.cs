using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    public class Object : BaseEntity
    {
        public int Id { get; set; }

        public Guid Code { get; set; }

        public string ObjectName { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string Path { get; set; } = null!;

        public string AllowedExtensions { get; set; } = null!;

        public int MaxLength { get; set; }

        public int Expiration { get; set; }

        public DateTime Date { get; set; }

        public string StateCode { get; set; } = null!;

        public string User { get; set; } = null!;

        public virtual ICollection<BulkLoad> BulkLoads { get; } = new List<BulkLoad>();

        public virtual ICollection<ObjectState> ObjectStates { get; } = new List<ObjectState>();
    }
}