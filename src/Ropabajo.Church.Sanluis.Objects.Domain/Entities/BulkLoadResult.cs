using Ropabajo.Church.Sanluis.Objects.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    [Table("carga_masiva_resultado")]
    public  class BulkLoadResult : BaseEntity
    {
        [Key]
        [Column("ID_CARGA_MASIVA_RESULTADO")]
        public int Id { get; set; }

        [Column("ID_CARGA_MASIVA")]
        public int BulkLoadId { get; set; }

        [Column("NUMERO_FILA")]
        public int RowNumber { get; set; }

        [Column("FILA")]
        public string Row { get; set; } = null!;

        [Column("CODIGO_ESTADO")]
        public string StateCode { get; set; } = null!;

        [Column("OBSERVACION")]
        public string? Observation { get; set; }
    }
}