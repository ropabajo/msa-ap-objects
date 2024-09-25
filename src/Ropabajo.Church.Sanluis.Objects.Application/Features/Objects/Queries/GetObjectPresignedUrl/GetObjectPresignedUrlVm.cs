namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Queries.GetObjectPresignedUrl
{
    /// <summary>
    /// Representa la respuesta de una URL prefirmada generada para un objeto en MinIO.
    /// </summary>
    public class GetObjectPresignedUrlVm
    {
        /// <summary>
        /// URL prefirmada generada para acceder al archivo en MinIO.
        /// </summary>
        /// <example>https://minio-api.ropabajo.pe/ropabajo/public/templates/Church_Plantilla_Carga_Persona.xlsx?X-Amz-Algorithm=AWS4-HMAC-SHA256&...</example>
        public string Url { get; set; }

        /// <summary>
        /// Nombre del archivo para el cual se generó la URL prefirmada.
        /// </summary>
        /// <example>Church_Plantilla_Carga_Persona.xlsx</example>
        public string FileName { get; set; }

        /// <summary>
        /// Ruta completa del objeto dentro del bucket de MinIO.
        /// </summary>
        /// <example>ropabajo/public/templates/Church_Plantilla_Carga_Persona.xlsx</example>
        public string ObjectPath { get; set; }

        /// <summary>
        /// Tiempo de expiración de la URL prefirmada en segundos.
        /// </summary>
        /// <example>3600</example>
        public int Expiration { get; set; }
    }
}
