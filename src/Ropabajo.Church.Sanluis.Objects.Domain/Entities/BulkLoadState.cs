using Ropabajo.Church.Sanluis.Objects.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    [Table("carga_masiva_estado")]
    public class BulkLoadState : BaseEntity
    {
        [Key]
        [Column("ID_CARGA_MASIVA_ESTADO")]
        public int Id { get; set; }

        [Column("ID_CARGA_MASIVA")]
        public int BulkLoadId { get; set; }

        [Column("FECHA")]
        public DateTime Date { get; set; }

        [Column("CODIGO_ESTADO")]
        public string StateCode { get; set; } = null!;

        [Column("USUARIO")]
        public string User { get; set; } = null!;
    }
}