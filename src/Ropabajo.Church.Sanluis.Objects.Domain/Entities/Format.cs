using Ropabajo.Church.Sanluis.Objects.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    [Table("formato")]
    public class Format : BaseEntity
    {
        [Key]
        [Column("ID_FORMATO")]
        public int Id { get; set; }

        [Column("CODIGO_FORMATO")]
        public Guid Code { get; set; }

        [Column("NOMBRE")]
        public string Name { get; set; } = null!;

        [Column("RUTA")]
        public string Path { get; set; } = null!;

        [Column("EXTENSIONES_PERMITIDAS")]
        public string AllowedExtensions { get; set; } = null!;

        [Column("LONGITUD_MAXIMA")]
        public int MaxLength { get; set; }

        [Column("EXPIRACION")]
        public int Expiration { get; set; }

        [Column("PLANTILLA")]
        public string Template { get; set; } = null!;

        public virtual ICollection<BulkLoad> BulkLoads { get; } = new List<BulkLoad>();
    }
}