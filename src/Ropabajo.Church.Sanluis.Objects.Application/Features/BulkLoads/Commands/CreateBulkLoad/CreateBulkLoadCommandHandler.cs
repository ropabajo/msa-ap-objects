using MediatR;
using Microsoft.Extensions.Logging;
using Ropabajo.Churc.Sanluis.Framework.Authz.Authorization;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Application.Events.BulkLoads.BulkLoadCreated;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Commands.CreateBulkLoad;

public class CreateBulkLoadCommandHandler : CommandHandler, IRequestHandler<CreateBulkLoadCommand>
{
    private readonly IMediatorBus _bus;
    private readonly IBulkLoadRepository _bulkLoadRepository;
    private readonly IBulkLoadStateRepository _bulkLoadStateRepository;
    private readonly IFormatRepository _formatRepository;
    private readonly IObjectRepository _objectRepository;
    private readonly ILogger<CreateBulkLoadCommandHandler> _logger;
    private readonly ICurrentContextAccessor _currentContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBulkLoadCommandHandler"/> class.
    /// </summary>
    /// <param name="bus">The mediator bus.</param>
    /// <param name="bulkLoadRepository">The bulk load repository.</param>
    /// <param name="bulkLoadStateRepository">The bulk load state repository.</param>
    /// <param name="formatRepository">The format repository.</param>
    /// <param name="objectRepository">The object repository.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="currentContextAccessor">The current context accessor.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CreateBulkLoadCommandHandler(
        IMediatorBus bus,
        IBulkLoadRepository bulkLoadRepository,
        IBulkLoadStateRepository bulkLoadStateRepository,
        IFormatRepository formatRepository,
        IObjectRepository objectRepository,
        ILogger<CreateBulkLoadCommandHandler> logger,
        ICurrentContextAccessor currentContextAccessor
        ) : base(bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
        _bulkLoadStateRepository = bulkLoadStateRepository ?? throw new ArgumentNullException(nameof(bulkLoadStateRepository));
        _currentContextAccessor = currentContextAccessor ?? throw new ArgumentNullException(nameof(currentContextAccessor));
        _formatRepository = formatRepository ?? throw new ArgumentNullException(nameof(formatRepository));
        _objectRepository = objectRepository ?? throw new ArgumentNullException(nameof(objectRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(CreateBulkLoadCommand command, CancellationToken cancellationToken)
    {
        // Get format
        var format = await _formatRepository.GetOneAsync(x => x.Code == Guid.Parse(command.FormatCode) && !x.Delete);
        if (format is null)
        {
            await _bus.RaiseAsync(new Notification("404", "No se encontró el formato.", NotificationType.NotFound), cancellationToken);
            return;
        }

        // Get object
        var @object = await _objectRepository.GetOneAsync(x => x.Code == Guid.Parse(command.ObjectCode) && !x.Delete);
        if (@object is null)
        {
            await _bus.RaiseAsync(new Notification("404", "No se encontró el objeto.", NotificationType.NotFound), cancellationToken);
            return;
        }

        // Validate object
        if (@object.StateCode != Shared.Enums.ObjectState.Uploaded)
        {
            await _bus.RaiseAsync(new Notification("422.0001", "El objecto aún no ha sido cargado."), cancellationToken);
            return;
        }

        // Validate configuration
        if (format.Path != @object.Path
            || format.AllowedExtensions != @object.AllowedExtensions
            || format.MaxLength != @object.MaxLength
            || format.Expiration != @object.Expiration)
        {
            await _bus.RaiseAsync(new Notification("422.0002", "La configuración del formato de carga masiva no coincide con la del objeto."), cancellationToken);
            return;
        }

        // Get bulk Load
        var bulkLoad = await _bulkLoadRepository.GetOneAsync(x => x.ObjectCode == Guid.Parse(command.ObjectCode) && !x.Delete);
        if (bulkLoad != null)
        {
            await _bus.RaiseAsync(new Notification("422.0003", "El objeto previamente ha sido asociado a un formato de carga masiva."), cancellationToken);
            return;
        }

        var state = Shared.Enums.BulkLoadState.Pending;
        var date = DateTime.UtcNow;
        var username = _currentContextAccessor.Username;

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
            User = username
        };
        await _bulkLoadRepository.AddAsync(bulkLoadToCreate, cancellationToken);

        var bulkLoadStateToCreate = new BulkLoadState
        {
            BulkLoadId = bulkLoadToCreate.Id,
            Date = date,
            StateCode = state,
            User = username
        };

        await _bulkLoadStateRepository.AddAsync(bulkLoadStateToCreate, cancellationToken);

        // Raise event
        await _bus.RaiseAsync(new BulkLoadCreatedEvent(bulkLoadToCreate), cancellationToken);
        _logger.LogInformation($"Bulk load created event {bulkLoadToCreate.Code} is successfully raised.");

    }
}
