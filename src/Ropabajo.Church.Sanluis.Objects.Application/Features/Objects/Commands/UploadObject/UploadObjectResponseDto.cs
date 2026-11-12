namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;
/// <summary>
/// Confirmación interna; el endpoint conserva su éxito 200 sin cuerpo.
/// </summary>
public sealed class UploadObjectResponseDto
{
    /// <summary>
    /// Indica que el archivo puede asociarse a su registro de destino.
    /// </summary>
    /// <example>true</example>
    public bool Uploaded { get; set; }
}
