using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Authz.Authorization;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;

public class UploadObjectCommandHandler : CommandHandler, IRequestHandler<UploadObjectCommand>
{
    private readonly IMediatorBus _bus;
    private readonly IObjectRepository _objectRepository;
    private readonly IObjectStateRepository _objectStateRepository;
    private readonly ICurrentContextAccessor _currentContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadObjectCommandHandler"/> class.
    /// </summary>
    /// <param name="bus">Instance of <see cref="IMediatorBus"/> used for publishing notifications.</param>
    /// <param name="objectRepository">Instance of <see cref="IObjectRepository"/> used for accessing object data.</param>
    /// <param name="objectStateRepository">Instance of <see cref="IObjectStateRepository"/> used for accessing object state data.</param>
    /// <param name="currentContextAccessor">Instance of <see cref="ICurrentContextAccessor"/> used for accessing the current context.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public UploadObjectCommandHandler(
        IMediatorBus bus,
        IObjectRepository objectRepository,
        IObjectStateRepository objectStateRepository,
        ICurrentContextAccessor currentContextAccessor
        ) : base(bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _objectRepository = objectRepository ?? throw new ArgumentNullException(nameof(objectRepository));
        _objectStateRepository = objectStateRepository ?? throw new ArgumentNullException(nameof(objectStateRepository));
        _currentContextAccessor = currentContextAccessor ?? throw new ArgumentNullException(nameof(currentContextAccessor));
    }

    public async Task Handle(UploadObjectCommand command, CancellationToken cancellationToken)
    {
        // Get object
        var objectToUpdate = await _objectRepository.GetOneAsync(x => x.Code == Guid.Parse(command.ObjectCode) && !x.Delete, cancellationToken);
        if (objectToUpdate is null)
        {
            await _bus.RaiseAsync(new Notification("404", "No se encontró el objeto.", NotificationType.BadRequest), cancellationToken);
            return;
        }

        var state = Shared.Enums.ObjectState.Uploaded;
        var date = DateTime.UtcNow;
        var user = _currentContextAccessor.Username;

        objectToUpdate.Date = date;
        objectToUpdate.StateCode = state;
        objectToUpdate.User = user;

        await _objectRepository.UpdateAsync(objectToUpdate, cancellationToken);

        var objectStateToCreate = new ObjectState
        {
            ObjectId = objectToUpdate.Id,
            Date = date,
            StateCode = state,
            User = user
        };

        await _objectStateRepository.AddAsync(objectStateToCreate, cancellationToken);
    }
}
