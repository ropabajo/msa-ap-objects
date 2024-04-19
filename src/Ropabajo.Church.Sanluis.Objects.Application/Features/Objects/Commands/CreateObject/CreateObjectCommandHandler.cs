using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Object = Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject
{
    public class CreateObjectCommandHandler : CommandHandler, IRequestHandler<CreateObjectCommand>
    {
        private readonly IMediatorBus _bus;
        private readonly ILogger<CreateObjectCommandHandler> _logger;
        private readonly IValidator<CreateObjectCommand> _validator;
        private readonly MinIoOptions _minIoOptions;
        private readonly IObjectRepository _objectRepository;
        private readonly IObjectStateRepository _objectStateRepository;

        public CreateObjectCommandHandler(
            IMediatorBus bus,
            ILogger<CreateObjectCommandHandler> logger,
            IValidator<CreateObjectCommand> validator,
            IOptionsSnapshot<MinIoOptions> minIoOptions,
            IObjectRepository objectRepository,
            IObjectStateRepository objectStateRepository
            ) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _minIoOptions = minIoOptions.Value ?? throw new ArgumentNullException(nameof(minIoOptions));
            _objectRepository = objectRepository ?? throw new ArgumentNullException(nameof(objectRepository));
            _objectStateRepository = objectStateRepository ?? throw new ArgumentNullException(nameof(objectStateRepository));
        }

        public async Task<Unit> Handle(CreateObjectCommand command, CancellationToken cancellationToken)
        {
            const string cacheControl = "max-age=86400,s-maxage=86400,proxy-revalidate"; /*"none"*/

            // Validate command
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return Unit.Value;
            }

            // Get extension
            string extension = Path.GetExtension(command.FileName);

            // Validate extension
            var isValidExtension = command.AllowedExtensions.Split(",").Any(x => x.Trim() == extension);
            if (!isValidExtension)
            {
                await _bus.RaiseAsync(new Notification("422.0001", "La extensión del fichero no es soportado por el formato de carga masiva."));
                return Unit.Value;
            }

            Guid code = Guid.NewGuid();
            string objectName = $"{command.Path}/{code}{extension}";
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(Path.GetExtension(command.FileName), out string contentType))
            {
                await _bus.RaiseAsync(new Notification("422.0002", "El tipo de MIME no ha podido ser determinado."));
                return Unit.Value;
            }

            var maxContentRange = command.MaxLength.Value * 1024 * 1024;
            var expires = DateTime.UtcNow + new TimeSpan(0, 0, command.Expiration.Value);

            var _minioClient = new MinioClient()
                                .WithEndpoint(_minIoOptions.Endpoint)
                                .WithCredentials(_minIoOptions.AccessKey, _minIoOptions.SecretKey)
                                .WithSSL(_minIoOptions.UseSsl)
                                .Build();

            PostPolicy policy = new();
            policy.SetContentType(contentType);
            policy.SetContentRange(1, maxContentRange);
            policy.SetCacheControl(cacheControl);
            policy.SetExpires(expires);
            policy.SetKey(objectName);
            policy.SetBucket(_minIoOptions.BucketName);

            PresignedPostPolicyArgs args = new PresignedPostPolicyArgs()
                                       .WithBucket(_minIoOptions.BucketName)
                                       .WithObject(objectName)
                                       .WithPolicy(policy);

            var presignedPost = await _minioClient.PresignedPostPolicyAsync(args);
            await _bus.RaiseAsync(new Header("url", presignedPost.Item1.AbsoluteUri));
            await _bus.RaiseAsync(new Header("payroll-object-code", code.ToString()));
            foreach (var item in presignedPost.Item2)
            {
                await _bus.RaiseAsync(new Header($"x-form-data-{item.Key}", item.Value));
            }
            await _bus.RaiseAsync(new Header("x-form-data-content-type", contentType));
            await _bus.RaiseAsync(new Header("x-form-data-cache-control", cacheControl));

            var state = Shared.Enums.ObjectState.PresignedUrl;
            var date = DateTime.Now;
            var user = "USER";

            var objectToCreate = new Object
            {
                Code = code,
                ObjectName = objectName,
                FileName = command.FileName,
                Path = command.Path,
                AllowedExtensions = command.AllowedExtensions,
                MaxLength = command.MaxLength.Value,
                Expiration = command.Expiration.Value,
                Date = date,
                StateCode = state,
                User = user
            };
            await _objectRepository.AddAsync(objectToCreate);
            _logger.LogInformation($"Object {objectToCreate.Code} is successfully created.");

            var objectStateToCreate = new ObjectState
            {
                ObjectId = objectToCreate.Id,
                Date = date,
                StateCode = state,
                User = user
            };
            await _objectStateRepository.AddAsync(objectStateToCreate);
            _logger.LogInformation($"Object state {objectStateToCreate.Id} is successfully created.");

            return Unit.Value;
        }
    }
}
