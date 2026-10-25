namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

/// <summary>
/// Conserva la reserva y verificación de un archivo del intento.
/// </summary>
public sealed class FileJobArtifact
{
    /// <summary>
    /// Identificador del archivo.
    /// </summary>
    /// <example>6d2e9eb4-24d6-4335-8297-29968401bdf7</example>
    public Guid ArtifactId { get; set; }

    /// <summary>
    /// Trabajo al que pertenece el archivo.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Intento que posee la subida.
    /// </summary>
    /// <example>fa8e9db0-c458-48ba-81e2-d54d94655983</example>
    public Guid AttemptId { get; set; }

    /// <summary>
    /// Nombre lógico exigido por el reporte.
    /// </summary>
    /// <example>workbook</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nombre seguro de descarga.
    /// </summary>
    /// <example>miembros-20261025.xlsx</example>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Clave interna limitada al prefijo de trabajos.
    /// </summary>
    /// <example>file-jobs/v1/development/iglesia/trabajo/intento/archivo.xlsx</example>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>
    /// Tipo MIME exigido por la política de subida.
    /// </summary>
    /// <example>application/vnd.openxmlformats-officedocument.spreadsheetml.sheet</example>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad exacta de bytes que se verificará.
    /// </summary>
    /// <example>24576</example>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Hash esperado del archivo terminado.
    /// </summary>
    /// <example>dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd</example>
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>
    /// Indica que el conjunto completo fue verificado y publicado.
    /// </summary>
    /// <example>false</example>
    public bool Published { get; set; }

    /// <summary>
    /// Fecha UTC de reserva del objeto.
    /// </summary>
    /// <example>2026-10-25T12:00:07Z</example>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Fecha UTC en que se confirmó la eliminación física del objeto.
    /// </summary>
    /// <example>2026-10-26T12:01:00Z</example>
    public DateTime? DeletedAt { get; set; }
}

