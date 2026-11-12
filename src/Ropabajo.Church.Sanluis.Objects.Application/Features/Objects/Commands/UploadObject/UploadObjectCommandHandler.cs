using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Authz.Authorization;
using Ropabajo.Churc.Sanluis.Framework.Integration;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Application.Photos;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;

/// <summary>

/// Confirma el estado y su historial sólo después de comprobar las fotos en almacenamiento.

/// </summary>
public sealed class UploadObjectCommandHandler : IRequestHandler<UploadObjectCommand, Result<UploadObjectResponseDto>>
{
    private readonly IObjectRepository _objects;
    private readonly IObjectStateRepository _states;
    private readonly ICurrentContextAccessor _context;
    private readonly IObjectUploadStore _store;
    private readonly IIntegrationWork _work;
    /// <summary>
    /// Recibe persistencia, actor autenticado, verificador y transacción local.
    /// </summary>
    public UploadObjectCommandHandler(IObjectRepository objects, IObjectStateRepository states, ICurrentContextAccessor context,
        IObjectUploadStore store, IIntegrationWork work)
    {
        _objects = objects ?? throw new ArgumentNullException(nameof(objects));
        _states = states ?? throw new ArgumentNullException(nameof(states));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _work = work ?? throw new ArgumentNullException(nameof(work));
    }
    /// <inheritdoc />
    public async Task<Result<UploadObjectResponseDto>> Handle(UploadObjectCommand command, CancellationToken cancellationToken)
    {
        var code = Guid.Parse(command.ObjectCode!);
        var item = await _objects.GetOneAsync(x => x.Code == code && !x.Delete, cancellationToken);
        if (item is null) return Result.Failure<UploadObjectResponseDto>(new Error("OBJECT_NOT_FOUND", "No se encontró el archivo. Vuelve a seleccionarlo.", ErrorType.NotFound));
        if (PhotoUploadPolicy.IsPhotoPath(item.Path) || item.ObjectName.StartsWith("photos/", StringComparison.OrdinalIgnoreCase))
        {
            var verified = await _store.VerifyPhotoAsync(item.ObjectName, cancellationToken);
            if (verified.IsFailure) return Result.Failure<UploadObjectResponseDto>(verified.Errors.ToArray());
        }
        // No hay una transacción abierta durante la lectura/decodificación de MinIO.
        return await _work.ExecuteAsync(async token =>
        {
            var date = DateTime.UtcNow; var user = _context.Username;
            item.Date = date; item.StateCode = Shared.Enums.ObjectState.Uploaded; item.User = user;
            await _objects.UpdateAsync(item, token);
            await _states.AddAsync(new ObjectState { ObjectId = item.Id, Date = date, StateCode = item.StateCode, User = user }, token);
            return Result<UploadObjectResponseDto>.Success(new UploadObjectResponseDto { Uploaded = true });
        }, cancellationToken);
    }
}
