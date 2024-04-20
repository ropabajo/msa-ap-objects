namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Formats.Queries.GetFormats
{
    public class FormatsVm
    {
        /// <summary>
        /// Código único del formato de carga masiva
        /// </summary>
        /// <example>creditor_discounts</example>
        public string FormatCode { get; set; }

        /// <summary>
        /// Nombre del formato de carga masiva
        /// </summary>
        /// <example>Descuento de terceros</example>
        public string Name { get; set; }

        /// <summary>
        /// Ruta en MinIO donde se cargan los objetos asociados al formato
        /// </summary>
        /// <example>ropabajo/church/sanluis/bulk-load/persons/loads</example>
        public string Path { get; set; }

        /// <summary>
        /// Extensiones permitidas para el fichero de carga masiva
        /// </summary>
        /// <example>.xls,.xlsx</example>
        public string AllowedExtensions { get; set; }

        /// <summary>
        /// Tamaño (en MB) máximo permitido para el fichero de carga masiva
        /// </summary>
        /// <example>1</example>
        public int MaxLength { get; set; }

        /// <summary>
        /// Tiempo (en segundos) de vigencia que tiene la url firmada para la carga de objeto asociado al formato
        /// </summary>
        /// <example>3600</example>
        public int Expiration { get; set; }

        /// <summary>
        /// Nombre de la plantilla en MinIO que es utilizada por el formato de carga masiva
        /// </summary>
        /// <example>minio.dev.meta.in.otic.pe:15406/ropabajo/public/templates/Church_Plantilla_Carga_Persona.xlsx</example>
        public string Template { get; set; }
    }
}
