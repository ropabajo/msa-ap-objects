namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;
/// <summary>
/// Autorización transportada por las cabeceras HTTP existentes; no agrega un cuerpo de éxito nuevo.
/// </summary>
public sealed class CreateObjectResponseDto
{
    /// <summary>
    /// Cabeceras de carga que se exponen por CORS al navegador.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();
}
