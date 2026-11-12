using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;
/// <summary>
/// Identificador enlazado exclusivamente desde la ruta de confirmación.
/// </summary>
public sealed class UploadObjectRequestDto
{
    /// <summary>
    /// Identificador público del archivo reservado.
    /// </summary>
    /// <example>4352279a-d37b-4f80-8bd8-42e018d7a98a</example>
    [Required(ErrorMessage = ObjectUploadValidationMessages.ObjectCodeRequired)]
    [RegularExpression(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", ErrorMessage = ObjectUploadValidationMessages.ObjectCodeInvalid)]
    public string? ObjectCode { get; set; }
}
