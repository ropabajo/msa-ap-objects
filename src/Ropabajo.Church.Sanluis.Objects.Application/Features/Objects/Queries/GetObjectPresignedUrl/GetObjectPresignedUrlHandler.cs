using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.MinIo;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Queries.GetObjectPresignedUrl
{

    public class GetObjectPresignedUrlHandler : QueryHandler, IRequestHandler<GetObjectPresignedUrlQuery, GetObjectPresignedUrlVm>
    {
        private readonly IMediatorBus _bus;
        private readonly MinIoOptions _minIoOptions;
        private readonly ILogger<GetObjectPresignedUrlHandler> _logger;
        private readonly IValidator<GetObjectPresignedUrlQuery> _validator;

        public GetObjectPresignedUrlHandler(
            IMediatorBus bus,
            IOptionsSnapshot<MinIoOptions> minIoOptions,
            ILogger<GetObjectPresignedUrlHandler> logger,
            IValidator<GetObjectPresignedUrlQuery> validator
            ) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _minIoOptions = minIoOptions.Value ?? throw new ArgumentNullException(nameof(minIoOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<GetObjectPresignedUrlVm> Handle(GetObjectPresignedUrlQuery query, CancellationToken cancellationToken)
        {

            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return null;
            }

            var _minioClient = new MinioClient()
                                .WithEndpoint(_minIoOptions.Endpoint)
                                .WithCredentials(_minIoOptions.AccessKey, _minIoOptions.SecretKey)
                                .WithSSL(_minIoOptions.UseSsl)
                                .Build();

            var templateUri = new Uri($"https://{query.Template}");

            string objectName = templateUri.AbsolutePath.TrimStart('/');
            if (objectName.StartsWith(_minIoOptions.BucketName + "/"))
            {
                objectName = objectName.Substring(_minIoOptions.BucketName.Length + 1); // Eliminar la parte del bucket y el "/"
            }


            if (string.IsNullOrWhiteSpace(objectName))
            {
                await _bus.RaiseAsync(new Notification("422.0002", "El template proporcionado no es válido."));
                return null;
            }

            // Establecer el tiempo de expiración basado en el valor proporcionado o usar 1 hora por defecto
            var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(query.Expiration.GetValueOrDefault(3600));
            var expirationInSeconds = (int)(expiresAt - DateTime.UtcNow).TotalSeconds;

            // Generar la URL prefirmada para descargar el archivo desde MinIO utilizando PresignedGetObjectArgs
            var presignedUrl = await _minioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_minIoOptions.BucketName)
                    .WithObject(objectName)
                    .WithExpiry(expirationInSeconds) // Expiración en segundos
            );

            // Crear y devolver la respuesta con los detalles de la URL prefirmada
            return new GetObjectPresignedUrlVm
            {
                Url = presignedUrl,
                FileName = Path.GetFileName(objectName),
                ObjectPath = objectName,
                Expiration = expirationInSeconds // Tiempo exacto de expiración en segundos
            };

        }
    }


}
