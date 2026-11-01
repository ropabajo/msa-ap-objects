using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ropabajo.Churc.Sanluis.Framework.Core.Results;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Api.Filters;

/// <summary>
/// Traduce fallos del límite de archivos sin exponer mensajes privados de infraestructura.
/// </summary>
/// <remarks>
/// Adaptador específico de Objects compartido por los controladores de trabajos e importación.
/// Conserva sus códigos HTTP y FILE_JOB_* sin registrar mensajes de proveedores, cuerpos o secretos.
/// No pertenece al framework común porque clasifica excepciones concretas de EF y MinIO.
/// </remarks>
public sealed class FileJobExceptionFilter : IExceptionFilter
{
    private readonly ILogger<FileJobExceptionFilter> _logger;

    /// <summary>
    /// Inicializa el registro seguro de fallos correlacionados.
    /// </summary>
    /// <param name="logger">Registro estructurado limitado al tipo de fallo y al identificador de traza.</param>
    public FileJobExceptionFilter(ILogger<FileJobExceptionFilter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Convierte un fallo no resuelto por el caso de uso en una respuesta HTTP segura.
    /// </summary>
    /// <param name="context">Excepción y contexto HTTP del endpoint de Objects.</param>
    /// <remarks>
    /// Una cancelación causada por el cliente se propaga sin sustituirla por un error 500.
    /// Los fallos de dependencias devuelven 503; una denegación local devuelve 403.
    /// Los mensajes originales permanecen fuera de la respuesta y del registro.
    /// </remarks>
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException && context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            return;
        }

        if (context.Exception is BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge })
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status413PayloadTooLarge,
                Title = "La solicitud supera el límite permitido."
            };
            context.Result = new ObjectResult(problem) { StatusCode = StatusCodes.Status413PayloadTooLarge };
        }
        else if (context.Exception is UnauthorizedAccessException)
        {
            context.Result = Result.Failure(new Error("FILE_JOB_FORBIDDEN", "No tienes permiso para este trabajo.", ErrorType.Forbidden))
                .ToActionResult(httpContext: context.HttpContext);
        }
        else if (IsDependencyFailure(context.Exception))
        {
            _logger.LogWarning(
                "File job dependency unavailable. TraceId={TraceId} FailureType={FailureType}",
                context.HttpContext.TraceIdentifier,
                context.Exception.GetType().Name);
            context.Result = Result.Failure(new Error("FILE_JOB_DEPENDENCY_UNAVAILABLE", "El servicio de archivos no está disponible por el momento.", ErrorType.Unavailable))
                .ToActionResult(httpContext: context.HttpContext);
        }
        else
        {
            _logger.LogError(
                "Unexpected file job failure. TraceId={TraceId} FailureType={FailureType}",
                context.HttpContext.TraceIdentifier,
                context.Exception.GetType().Name);
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "No se pudo completar la operación de archivos."
            };
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            context.Result = new ObjectResult(problem) { StatusCode = StatusCodes.Status500InternalServerError };
        }
        context.ExceptionHandled = true;
    }

    private static bool IsDependencyFailure(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is System.Data.Common.DbException or Microsoft.EntityFrameworkCore.DbUpdateException
                or Minio.Exceptions.MinioException or HttpRequestException or IOException or TaskCanceledException)
            {
                return true;
            }
        }

        return false;
    }
}
