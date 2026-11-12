using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;

/// <summary>

/// Solicita una autorización de carga conservando el contrato público existente.

/// </summary>
public sealed class CreateObjectRequestDto
{
    /// <summary>
    /// Carpeta del archivo.
    /// </summary>
    /// <example>photos</example>
    [Required(ErrorMessage = ObjectUploadValidationMessages.PathRequired)]
    [MaxLength(400, ErrorMessage = ObjectUploadValidationMessages.PathMaxLength)]
    public string? Path { get; set; }

    /// <summary>

    /// Formatos solicitados por el consumidor; el servidor conserva su propia lista para fotos.

    /// </summary>
    /// <example>.jpg,.jpeg,.png,.webp</example>
    [Required(ErrorMessage = ObjectUploadValidationMessages.ExtensionsRequired)]
    [MaxLength(40, ErrorMessage = ObjectUploadValidationMessages.ExtensionsMaxLength)]
    public string? AllowedExtensions { get; set; }

    /// <summary>

    /// Tamaño solicitado en MB; para fotos el servidor aplica como máximo 5 MB.

    /// </summary>
    /// <example>5</example>
    [Required(ErrorMessage = ObjectUploadValidationMessages.MaxLengthRequired)]
    [Range(1, int.MaxValue, ErrorMessage = ObjectUploadValidationMessages.MaxLengthPositive)]
    public int? MaxLength { get; set; }

    /// <summary>

    /// Vigencia solicitada en segundos; las fotos tienen como máximo cinco minutos.

    /// </summary>
    /// <example>300</example>
    [Required(ErrorMessage = ObjectUploadValidationMessages.ExpirationRequired)]
    [Range(1, int.MaxValue, ErrorMessage = ObjectUploadValidationMessages.ExpirationPositive)]
    public int? Expiration { get; set; }

    /// <summary>

    /// Nombre del archivo seleccionado.

    /// </summary>
    /// <example>postulante.jpeg</example>
    [Required(ErrorMessage = ObjectUploadValidationMessages.FileNameRequired)]
    [MaxLength(80, ErrorMessage = ObjectUploadValidationMessages.FileNameMaxLength)]
    public string? FileName { get; set; }

}
