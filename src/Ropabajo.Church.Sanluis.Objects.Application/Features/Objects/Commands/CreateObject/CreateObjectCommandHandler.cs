using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.StaticFiles;
using Ropabajo.Churc.Sanluis.Framework.Authz.Authorization;
using Ropabajo.Churc.Sanluis.Framework.Integration;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Application.Photos;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Object = Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;

/// <summary>

/// Reserva archivos con límites de servidor, conservando rutas, cabeceras y cuerpo HTTP existentes.

/// </summary>
public sealed class CreateObjectCommandHandler : IRequestHandler<CreateObjectCommand, Result<CreateObjectResponseDto>>
{
    private readonly IObjectUploadStore _store;
    private readonly IObjectRepository _objects;
    private readonly IObjectStateRepository _states;
    private readonly ICurrentContextAccessor _context;
    private readonly IIntegrationWork _work;
    private readonly IMapper _mapper;
    /// <summary>
    /// Recibe almacenamiento, persistencia, actor autenticado, mapas y transacción local.
    /// </summary>
    public CreateObjectCommandHandler(IObjectUploadStore store, IObjectRepository objects, IObjectStateRepository states,
        ICurrentContextAccessor context, IIntegrationWork work, IMapper mapper)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _objects = objects ?? throw new ArgumentNullException(nameof(objects));
        _states = states ?? throw new ArgumentNullException(nameof(states));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _work = work ?? throw new ArgumentNullException(nameof(work));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    /// <inheritdoc />
    public async Task<Result<CreateObjectResponseDto>> Handle(CreateObjectCommand command, CancellationToken cancellationToken)
    {
        var path = Uri.UnescapeDataString(command.Path ?? "").TrimStart('/');
        if (path.Equals("temporary-exports", StringComparison.OrdinalIgnoreCase) || path.StartsWith("temporary-exports/", StringComparison.OrdinalIgnoreCase)
            || path.Equals("file-jobs", StringComparison.OrdinalIgnoreCase) || path.StartsWith("file-jobs/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("..") || path.Contains('\\'))
            return Result.Failure<CreateObjectResponseDto>(new Error("OBJECT_PATH_RESERVED", "No se permite cargar archivos en esa carpeta.", ErrorType.Forbidden));
        var photo = PhotoUploadPolicy.IsPhotoPath(path);
        var extension = Path.GetExtension(command.FileName ?? "").ToLowerInvariant();
        var extensions = photo ? PhotoUploadPolicy.Extensions : command.AllowedExtensions ?? "";
        var contentType = photo ? PhotoUploadPolicy.ContentType(command.FileName)
            : new FileExtensionContentTypeProvider().TryGetContentType("file" + extension, out var detected) ? detected : null;
        if (!extensions.Split(',').Any(value => value.Trim().Equals(extension, StringComparison.OrdinalIgnoreCase)) || contentType is null)
            return Result.Failure<CreateObjectResponseDto>(photo ? PhotoUploadErrors.Invalid :
                new Error("OBJECT_FILE_FORMAT", "El formato del archivo no está permitido. Selecciona un archivo compatible.", ErrorType.Validation));
        var code = Guid.NewGuid();
        var key = (photo ? "photos" : path.TrimEnd('/')) + "/" + code.ToString("D") + extension;
        var maxMb = photo ? 5 : command.MaxLength!.Value;
        var seconds = photo ? Math.Min(300, command.Expiration!.Value) : command.Expiration!.Value;
        var grant = await _store.GrantAsync(key, contentType, (long)maxMb * 1024 * 1024, seconds, cancellationToken);
        if (grant.IsFailure) return Result.Failure<CreateObjectResponseDto>(grant.Errors.ToArray());
        // MinIO se consulta antes de abrir la transacción que confirma registro e historial.
        return await _work.ExecuteAsync(async token =>
        {
            var date = DateTime.UtcNow; var user = _context.Username;
            var item = new Object { Code = code, ObjectName = key, FileName = command.FileName!, Path = photo ? "photos" : path,
                AllowedExtensions = extensions, MaxLength = maxMb, Expiration = seconds, Date = date,
                StateCode = Shared.Enums.ObjectState.PresignedUrl, User = user };
            await _objects.AddAsync(item, token);
            await _states.AddAsync(new ObjectState { ObjectId = item.Id, Date = date, StateCode = item.StateCode, User = user }, token);
            var response = _mapper.Map<CreateObjectResponseDto>(grant.Value);
            response.Headers["sanluis-object-code"] = code.ToString("D");
            return Result<CreateObjectResponseDto>.Success(response);
        }, cancellationToken);
    }
}
