namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

/// <summary>
/// Registra un comando pendiente dentro de la misma transacción que el trabajo.
/// </summary>
public sealed class FileJobOutbox
{
    /// <summary>
    /// Identificador estable del mensaje pendiente.
    /// </summary>
    /// <example>2bd69423-e6c1-4fb3-9bf3-66c82ca13724</example>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Trabajo que debe generar el consumidor.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Comando pequeño serializado, sin parámetros privados ni filas.
    /// </summary>
    /// <example>{"messageType":"files.generate.v1"}</example>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Fecha UTC de creación de la outbox.
    /// </summary>
    /// <example>2026-10-25T12:00:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha UTC desde la cual puede despacharse.
    /// </summary>
    /// <example>2026-10-25T12:00:00Z</example>
    public DateTime AvailableAt { get; set; }

    /// <summary>
    /// Fecha UTC de confirmación del broker.
    /// </summary>
    /// <example>2026-10-25T12:00:01Z</example>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Propietario temporal del despacho.
    /// </summary>
    /// <example>4608cf17-18ba-4636-bb1a-ec425f18e880</example>
    public Guid? DispatchId { get; set; }

    /// <summary>
    /// Vencimiento UTC del lease de despacho.
    /// </summary>
    /// <example>2026-10-25T12:01:00Z</example>
    public DateTime? DispatchUntil { get; set; }

    /// <summary>
    /// Cantidad de intentos de publicación.
    /// </summary>
    /// <example>1</example>
    public int Attempts { get; set; }

    /// <summary>
    /// Versión de concurrencia de la outbox.
    /// </summary>
    /// <example>1</example>
    public long Version { get; set; }
    /// <summary>
    /// Fecha UTC de descarte de un mensaje cuyo trabajo ya terminó.
    /// </summary>
    /// <example>2026-10-25T13:00:00Z</example>
    public DateTime? DiscardedAt { get; set; }
}
