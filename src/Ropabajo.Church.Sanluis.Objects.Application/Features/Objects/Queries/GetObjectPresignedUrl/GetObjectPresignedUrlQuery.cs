using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Queries.GetObjectPresignedUrl
{
    public class GetObjectPresignedUrlQuery : Query<GetObjectPresignedUrlVm>
    {
        /// <summary>
        /// La plantilla que contiene la ruta completa del archivo en MinIO
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Tiempo (en segundos) de vigencia que tiene la URL firmada para la carga del objeto
        /// </summary>
        public int? Expiration { get; set; }
    }

}
