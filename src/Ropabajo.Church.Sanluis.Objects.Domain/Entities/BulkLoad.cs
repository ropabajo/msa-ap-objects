using Ropabajo.Church.Sanluis.Objects.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    [Table("carga_masiva")]
    public class BulkLoad : BaseEntity
    {
        [Key]
        [Column("ID_CARGA_MASIVA")]
        public int Id { get; set; }

        [Column("CODIGO_CARGA_MASIVA")]
        public Guid Code { get; set; }

        [Column("ID_FORMATO")]
        public int FormatId { get; set; }

        [Column("CODIGO_FORMATO")]
        public Guid FormatCode { get; set; }

        [Column("ID_OBJETO")]
        public int ObjectId { get; set; }

        [Column("CODIGO_OBJETO")]
        public Guid ObjectCode { get; set; }

        [Column("NOMBRE_OBJETO")]
        public string? ObjectName { get; set; }

        [Column("NOMBRE_ARCHIVO")]
        public string? FileName { get; set; }

        [Column("DESCRIPCION")]
        public string? Description { get; set; }

        [Column("NUMERO_REGISTROS")]
        public int? Records { get; set; }

        [Column("NUMERO_REGISTROS_CARGADOS")]
        public int? UploadedRecords { get; set; }

        [Column("NUMERO_REGISTROS_OBSEVADOS")]
        public int? ObservedRecords { get; set; }

        [Column("FECHA")]
        public DateTime Date { get; set; }

        [Column("CODIGO_ESTADO")]
        public string StateCode { get; set; } = null!;

        [Column("USUARIO")]
        public string User { get; set; } = null!;

        public virtual ICollection<BulkLoadState> BulkLoadStates { get; } = new List<BulkLoadState>();

        public virtual ICollection<BulkLoadResult> BulkLoadResults { get; } = new List<BulkLoadResult>();

    }
}