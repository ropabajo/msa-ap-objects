using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject
{
    public class CreateObjectCommand : Command
    {
        /// <summary>
        /// Ruta en MinIO donde se cargan los objetos asociados al formato
        /// </summary>
        /// <example>ropabajo/church/sanluis/bulk-load/persons/loads</example>
        public string? Path { get; set; }

        /// <summary>
        /// Extensiones permitidas para el fichero de carga masiva
        /// </summary>
        /// <example>.xls,.xlsx</example>
        public string? AllowedExtensions { get; set; }

        /// <summary>
        /// Tamaño (en MB) máximo permitido para el fichero de carga masiva
        /// </summary>
        /// <example>1</example>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Tiempo (en segundos) de vigencia que tiene la url firmada para la carga de objeto asociado al formato
        /// </summary>
        /// <example>3600</example>
        public int? Expiration { get; set; }

        /// <summary>
        /// Nombre del fichero
        /// </summary>
        /// <example>file_example.xlsx</example>
        public string? FileName { get; set; }
    }
}
