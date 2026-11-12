using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;
/// <summary>
/// Confirma una carga; las fotos deben superar la verificación del contenido almacenado.
/// </summary>
public sealed class UploadObjectCommand : IRequest<Result<UploadObjectResponseDto>>
{
    /// <summary>
    /// Identificador público del archivo reservado.
    /// </summary>
    /// <example>4352279a-d37b-4f80-8bd8-42e018d7a98a</example>
    public string? ObjectCode { get; set; }
}
