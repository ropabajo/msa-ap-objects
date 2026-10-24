using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>Traduce fallos del flujo a HTTP sin exponer URLs firmadas ni mensajes del proveedor.</summary>
public sealed class TemporaryExportExceptionFilter(ILogger<TemporaryExportExceptionFilter> logger) : IExceptionFilter
{
    /// <inheritdoc />
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException && context.HttpContext.RequestAborted.IsCancellationRequested) return;
        var known = context.Exception as ExportFlowException;
        var oversized = context.Exception is Microsoft.AspNetCore.Http.BadHttpRequestException { StatusCode: 413 };
        var status = oversized ? 413 : known?.Code switch { "missing" or "disabled" => 404, "expired" => 410, "busy" or "pending" => 409, "invalid" => 422, _ => 503 };
        if (known is null) logger.LogWarning("Temporary export unavailable. FailureType={FailureType}", context.Exception.GetType().Name);
        context.Result = new ObjectResult(new { message = oversized ? "Los datos superan el tamaño permitido. Ajusta los filtros." : known?.Message ?? "La exportación no está disponible por el momento. Puedes reintentar." }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
