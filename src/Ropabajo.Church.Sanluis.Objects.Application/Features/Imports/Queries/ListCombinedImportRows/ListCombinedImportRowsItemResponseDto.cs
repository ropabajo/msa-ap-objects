namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImportRows;

/// <summary>
/// Elemento funcional de la página autorizada.
/// </summary>
public sealed class ListCombinedImportRowsItemResponseDto
{
    /// <summary>
    /// Código público de fila.
    /// </summary>
    /// <example>16c927f9-c4b4-40cc-9de2-5c6ce9d2c1c4</example>
    public Guid BulkLoadResultCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Código público de carga.
    /// </summary>
    /// <example>8431e4c1-4a16-46ab-8932-e02851c073dd</example>
    public Guid BulkLoadCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Número real de fila en Excel.
    /// </summary>
    /// <example>2</example>
    public int RowNumber { get; set; } = 0;

    /// <summary>
    /// Estado general de la fila.
    /// </summary>
    /// <example>OBSERVADO</example>
    public string StateCode { get; set; } = string.Empty;

    /// <summary>
    /// Paso funcional de la fila.
    /// </summary>
    /// <example>member_observed</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Persona confirmada cuando existe.
    /// </summary>
    /// <example>8cbddf7f-a8ab-46a0-a832-373f454357cd</example>
    public Guid? PersonCode { get; set; } = null;

    /// <summary>
    /// Miembro confirmado cuando existe.
    /// </summary>
    /// <example>90ac6b0a-3306-459e-a00a-bab20b844d5b</example>
    public Guid? MemberCode { get; set; } = null;

    /// <summary>
    /// Observación segura para corregir la fila.
    /// </summary>
    /// <example>El campo Estado del Miembro no tiene un valor válido.</example>
    public string? Observation { get; set; } = null;

    /// <summary>
    /// Permite recuperar un paso observado sin volver a subir el archivo.
    /// </summary>
    /// <example>true</example>
    public bool CanRetry { get; set; } = false;

}
