namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Contadores confirmados de una carga; nunca incluye filas, mensajes o datos de Persona.
/// </summary>
/// <param name="BulkLoadCode">Código público de carga.</param>
/// <param name="Status">Estado durable del procesamiento.</param>
/// <param name="StateCode">Estado de bandeja compatible con el listado.</param>
/// <param name="Records">Total de filas del archivo.</param>
/// <param name="UploadedRecords">Filas confirmadas por Church, tras Persona y Miembro.</param>
/// <param name="ObservedRecords">Filas con observaciones pendientes.</param>
/// <param name="UpdatedAt">Última confirmación persistida en UTC.</param>
/// <param name="Version">Versión monotónica; permite descartar mensajes atrasados.</param>
public sealed record ImportProgressSnapshot(
    Guid BulkLoadCode,
    string Status,
    string StateCode,
    int Records,
    int UploadedRecords,
    int ObservedRecords,
    DateTime UpdatedAt,
    long Version
);
