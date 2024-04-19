using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    public partial class Format : BaseEntity
    {
        public int Id { get; set; }

        public Guid Code { get; set; }

        public string Name { get; set; } = null!;

        public string Path { get; set; } = null!;

        public string AllowedExtensions { get; set; } = null!;

        public int MaxLength { get; set; }

        public int Expiration { get; set; }

        public string Template { get; set; } = null!;

        public virtual ICollection<BulkLoad> BulkLoads { get; } = new List<BulkLoad>();
    }
}