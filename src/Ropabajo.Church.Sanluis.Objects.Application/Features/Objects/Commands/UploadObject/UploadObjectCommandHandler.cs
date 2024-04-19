using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject
{
    public class UploadObjectCommandHandler : CommandHandler, IRequestHandler<UploadObjectCommand>
    {
        private readonly IMediatorBus _bus;
        private readonly ILogger<UploadObjectCommandHandler> _logger;
        private readonly IValidator<UploadObjectCommand> _validator;
        private readonly IObjectRepository _objectRepository;
        private readonly IObjectStateRepository _objectStateRepository;

        public UploadObjectCommandHandler(
            IMediatorBus bus,
            ILogger<UploadObjectCommandHandler> logger,
            IValidator<UploadObjectCommand> validator,
            IObjectRepository objectRepository,
            IObjectStateRepository objectStateRepository
            ) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _objectRepository = objectRepository ?? throw new ArgumentNullException(nameof(objectRepository));
            _objectStateRepository = objectStateRepository ?? throw new ArgumentNullException(nameof(objectStateRepository));
        }

        public async Task<Unit> Handle(UploadObjectCommand command, CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return Unit.Value;
            }

            // Get object
            var objectToUpdate = await _objectRepository.GetOneAsync(x => x.Code == Guid.Parse(command.ObjectCode) && !x.Delete);
            if (objectToUpdate is null)
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotFound));
                return Unit.Value;
            }

            var state = Shared.Enums.ObjectState.Uploaded;
            var date = DateTime.Now;
            var user = "USER";

            objectToUpdate.Date = date;
            objectToUpdate.StateCode = state;
            objectToUpdate.User = user;
            await _objectRepository.UpdateAsync(objectToUpdate);
            _logger.LogInformation($"Object {objectToUpdate.Id} is successfully updated.");

            var objectStateToCreate = new ObjectState
            {
                ObjectId = objectToUpdate.Id,
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
