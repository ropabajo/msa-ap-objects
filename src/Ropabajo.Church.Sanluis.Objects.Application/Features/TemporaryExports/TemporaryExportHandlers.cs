using MediatR;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;

/// <summary>Reserva un documento bajo la identidad autenticada y la iglesia de la ruta.</summary>
/// <param name="ChurchCode">Iglesia validada por la política HTTP.</param>
/// <param name="Request">Intención idempotente del productor.</param>
public sealed record ReserveExportCommand(Guid ChurchCode, ReserveExportRequest Request) : IRequest<TemporaryExportResponse>;
/// <summary>Recibe los datos completos sin devolver archivos por HTTP.</summary>
/// <param name="ChurchCode">Iglesia activa.</param>
/// <param name="Id">Identidad reservada.</param>
/// <param name="Source">Datos sin credenciales del reporte.</param>
public sealed record SupplyExportCommand(Guid ChurchCode, Guid Id, ExportWorkbookSource Source) : IRequest<TemporaryExportResponse>;
/// <summary>Consulta el estado del trabajo propio.</summary>
/// <param name="ChurchCode">Iglesia activa.</param>
/// <param name="Id">Identidad del documento.</param>
public sealed record GetExportQuery(Guid ChurchCode, Guid Id) : IRequest<TemporaryExportResponse>;
/// <summary>Renueva el acceso al documento propio ya generado.</summary>
/// <param name="ChurchCode">Iglesia activa.</param>
/// <param name="Id">Identidad del documento.</param>
public sealed record GetExportDownloadQuery(Guid ChurchCode, Guid Id) : IRequest<ExportDownloadResponse>;

/// <summary>Adaptación de reserva al caso de uso central.</summary>
public sealed class ReserveExportHandler(TemporaryExportCoordinator exports, IExportActor actor) : IRequestHandler<ReserveExportCommand, TemporaryExportResponse>
{
    /// <inheritdoc />
    public Task<TemporaryExportResponse> Handle(ReserveExportCommand request, CancellationToken token) => exports.ReserveAsync(actor.GetScope(request.ChurchCode), request.Request, token);
}
/// <summary>Adaptación de entrega de datos al caso de uso central.</summary>
public sealed class SupplyExportHandler(TemporaryExportCoordinator exports, IExportActor actor) : IRequestHandler<SupplyExportCommand, TemporaryExportResponse>
{
    /// <inheritdoc />
    public Task<TemporaryExportResponse> Handle(SupplyExportCommand request, CancellationToken token) => exports.SupplyAsync(actor.GetScope(request.ChurchCode), request.Id, request.Source, token);
}
/// <summary>Adaptación de consulta al caso de uso central.</summary>
public sealed class GetExportHandler(TemporaryExportCoordinator exports, IExportActor actor) : IRequestHandler<GetExportQuery, TemporaryExportResponse>
{
    /// <inheritdoc />
    public Task<TemporaryExportResponse> Handle(GetExportQuery request, CancellationToken token) => exports.StatusAsync(actor.GetScope(request.ChurchCode), request.Id, token);
}
/// <summary>Adaptación de firma al caso de uso central.</summary>
public sealed class GetExportDownloadHandler(TemporaryExportCoordinator exports, IExportActor actor) : IRequestHandler<GetExportDownloadQuery, ExportDownloadResponse>
{
    /// <inheritdoc />
    public Task<ExportDownloadResponse> Handle(GetExportDownloadQuery request, CancellationToken token) => exports.DownloadAsync(actor.GetScope(request.ChurchCode), request.Id, token);
}
