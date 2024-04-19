using Ropabajo.Church.Sanluis.Objects.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Entities
{
    [Table("objeto_estado")]
    public class ObjectState : BaseEntity
    {
        [Key]
        [Column("ID_OBJETO_ESTADO")]
        public int Id { get; set; }

        [Column("ID_OBJETO")]
        public int ObjectId { get; set; }

        [Column("FECHA")]
        public DateTime Date { get; set; }

        [Column("CODIGO_ESTADO")]
        public string StateCode { get; set; } = null!;

        [Column("USUARIO")]
        public string User { get; set; } = null!;

    }
}