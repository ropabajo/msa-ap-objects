using Ropabajo.Church.Sanluis.Objects.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    [Table("objeto")]
    public class Object : BaseEntity
    {
        [Key]
        [Column("ID_OBJETO")]
        public int Id { get; set; }

        [Column("CODIGO_OBJETO")]
        public Guid Code { get; set; }

        [Column("NOMBRE_OBJETO")]
        public string ObjectName { get; set; } = null!;

        [Column("NOMBRE_ARCHIVO")]
        public string FileName { get; set; } = null!;

        [Column("RUTA")]
        public string Path { get; set; } = null!;

        [Column("EXTENSIONES_PERMITIDAS")]
        public string AllowedExtensions { get; set; } = null!;

        [Column("LONGITUD_MAXIMA")]
        public int MaxLength { get; set; }

        [Column("EXPIRACION")]
        public int Expiration { get; set; }

        [Column("FECHA")]
        public DateTime Date { get; set; }

        [Column("CODIGO_ESTADO")]
        public string StateCode { get; set; } = null!;

        [Column("USUARIO")]
        public string User { get; set; } = null!;

        public virtual ICollection<BulkLoad> BulkLoads { get; } = new List<BulkLoad>();

        public virtual ICollection<ObjectState> ObjectStates { get; } = new List<ObjectState>();
    }
}