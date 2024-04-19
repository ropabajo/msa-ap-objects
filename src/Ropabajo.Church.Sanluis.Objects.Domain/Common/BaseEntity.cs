using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Domain.Common
{
    public abstract class BaseEntity
    {

        [Column("USUARIO_CREACION")]
        public string CreatedBy { get; set; }

        [Column("FECHA_CREACION")]
        public DateTime CreatedDate { get; set; }

        [Column("IP_CREACION")]
        public string CreatedIp { get; set; }

        [Column("USUARIO_MODIFICACION")]
        public string? LastModifiedBy { get; set; }

        [Column("FECHA_MODIFICACION")]
        public DateTime? LastModifiedDate { get; set; }

        [Column("IP_MODIFICACION")]
        public string? LastModifiedIp { get; set; }

        [Column("ELIMINADO")]
        public bool Delete { get; set; } = true;
    }
}