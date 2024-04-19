using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Commands.CreateBulkLoad
{
    public class CreateBulkLoadCommand : Command
    {
        /// <summary>
        /// Código único de formato
        /// </summary>
        /// <example>bulk_load_person_load</example>
        public string? FormatCode { get; set; }

        /// <summary>
        /// Código único de objeto
        /// </summary>
        /// <example>4352279a-d37b-4f80-8bd8-42e018d7a98a</example>
        public string? ObjectCode { get; set; }

        /// <summary>
        /// Descripción del formato de carga masiva a importar
        /// </summary>
        /// <example>Descuento de acreditatores para el periodo 2023-11</example>
        public string? Description { get; set; }
    }
}
