using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;

/// <summary>

/// Solicita una autorización de carga conservando el contrato público existente.

/// </summary>
public sealed class CreateObjectCommand : IRequest<Result<CreateObjectResponseDto>>
{
    /// <summary>
    /// Carpeta del archivo.
    /// </summary>
    /// <example>photos</example>
    public string? Path { get; set; }

    /// <summary>

    /// Formatos solicitados por el consumidor; el servidor conserva su propia lista para fotos.

    /// </summary>
    /// <example>.jpg,.jpeg,.png,.webp</example>
    public string? AllowedExtensions { get; set; }

    /// <summary>

    /// Tamaño solicitado en MB; para fotos el servidor aplica como máximo 5 MB.

    /// </summary>
    /// <example>5</example>
    public int? MaxLength { get; set; }

    /// <summary>

    /// Vigencia solicitada en segundos; las fotos tienen como máximo cinco minutos.

    /// </summary>
    /// <example>300</example>
    public int? Expiration { get; set; }

    /// <summary>

    /// Nombre del archivo seleccionado.

    /// </summary>
    /// <example>postulante.jpeg</example>
    public string? FileName { get; set; }

}
