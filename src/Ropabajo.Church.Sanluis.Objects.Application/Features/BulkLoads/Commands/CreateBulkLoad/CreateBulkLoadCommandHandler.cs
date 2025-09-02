using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Application.Events.BulkLoads.BulkLoadCreated;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Commands.CreateBulkLoad
{
    public class CreateBulkLoadCommandHandler : CommandHandler, IRequestHandler<CreateBulkLoadCommand>
    {
        private readonly IMediatorBus _bus;
        private readonly ILogger<CreateBulkLoadCommandHandler> _logger;
        private readonly IValidator<CreateBulkLoadCommand> _validator;
        private readonly IBulkLoadRepository _bulkLoadRepository;
        private readonly IBulkLoadStateRepository _bulkLoadStateRepository;
        private readonly IFormatRepository _formatRepository;
        private readonly IObjectRepository _objectRepository;

        public CreateBulkLoadCommandHandler(
            IMediatorBus bus,
            ILogger<CreateBulkLoadCommandHandler> logger,
            IValidator<CreateBulkLoadCommand> validator,
            IBulkLoadRepository bulkLoadRepository,
            IBulkLoadStateRepository bulkLoadStateRepository,
            IFormatRepository formatRepository,
            IObjectRepository objectRepository
            ) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
            _bulkLoadStateRepository = bulkLoadStateRepository ?? throw new ArgumentNullException(nameof(bulkLoadStateRepository));
            _formatRepository = formatRepository ?? throw new ArgumentNullException(nameof(formatRepository));
            _objectRepository = objectRepository ?? throw new ArgumentNullException(nameof(objectRepository));
        }

        public async Task<Unit> Handle(CreateBulkLoadCommand command, CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return Unit.Value;
            }

            // Get format
            var format = await _formatRepository.GetOneAsync(x => x.Code == Guid.Parse(command.FormatCode) && !x.Delete);
            if (format is null)
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotFound));
                return Unit.Value;
            }

            // Get object
            var @object = await _objectRepository.GetOneAsync(x => x.Code == Guid.Parse(command.ObjectCode) && !x.Delete);
            if (@object is null)
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotFound));
                return Unit.Value;
            }

            // Validate object
            if (@object.StateCode != Shared.Enums.ObjectState.Uploaded)
            {
                await _bus.RaiseAsync(new Notification("422.0001", "El objecto aún no ha sido cargado."));
                return Unit.Value;
            }

            // Validate configuration
            if (format.Path != @object.Path
                || format.AllowedExtensions != @object.AllowedExtensions
                || format.MaxLength != @object.MaxLength
                || format.Expiration != @object.Expiration)
            {
                await _bus.RaiseAsync(new Notification("422.0002", "La configuración del formato de carga masiva no coincide con la del objeto."));
                return Unit.Value;
            }

            // Get bulk Load
            var bulkLoad = await _bulkLoadRepository.GetOneAsync(x => x.ObjectCode == Guid.Parse(command.ObjectCode) && !x.Delete);
            if (bulkLoad != null)
            {
                await _bus.RaiseAsync(new Notification("422.0003", "El objeto previamente ha sido asociado a un formato de carga masiva."));
                return Unit.Value;
            }

            var state = Shared.Enums.BulkLoadState.Processing;
            var date = DateTime.Now;
            var user = "USER";

            var bulkLoadToCreate = new BulkLoad
            {
                Code = Guid.NewGuid(),
                FormatId = format.Id,
                FormatCode = format.Code,
                ObjectId = @object.Id,
                ObjectCode = @object.Code,
                Description = command.Description,
                Date = date,
                StateCode = state,
                User = user
            };
            await _bulkLoadRepository.AddAsync(bulkLoadToCreate);
            _logger.LogInformation($"Bulk load {bulkLoadToCreate.Code} is successfully created.");

            var bulkLoadStateToCreate = new BulkLoadState
            {
                BulkLoadId = bulkLoadToCreate.Id,
                Date = date,
                StateCode = state,
                User = user
            };
            await _bulkLoadStateRepository.AddAsync(bulkLoadStateToCreate);
            _logger.LogInformation($"Bulk load state {bulkLoadStateToCreate.Id} is successfully created.");

            // Raise event
            await _bus.RaiseAsync(new BulkLoadCreatedEvent(bulkLoadToCreate));
            _logger.LogInformation($"Bulk load created event {bulkLoadToCreate.Code} is successfully raised.");

            return Unit.Value;
        }
    }
}
