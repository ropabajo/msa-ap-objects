using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>Aplica el límite configurado antes de leer el cuerpo de datos del reporte.</summary>
public sealed class TemporaryExportRequestLimitFilter(IOptions<TemporaryExportOptions> settings) : IResourceFilter
{
    /// <inheritdoc />
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var limit = settings.Value.MaxSourceBytes;
        var feature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (feature is not null && !feature.IsReadOnly) feature.MaxRequestBodySize = limit;
        if (context.HttpContext.Request.ContentLength > limit)
            context.Result = new ObjectResult(new { message = "Los datos de la exportación superan el tamaño permitido. Ajusta los filtros." }) { StatusCode = 413 };
    }
    /// <inheritdoc />
    public void OnResourceExecuted(ResourceExecutedContext context) { }
}
