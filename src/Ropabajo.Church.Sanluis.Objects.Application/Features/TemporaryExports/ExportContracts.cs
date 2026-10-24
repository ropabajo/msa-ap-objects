using System.ComponentModel.DataAnnotations;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;

/// <summary>Solicitud idempotente de un documento privado por usuario, iglesia y búsqueda.</summary>
/// <param name="RequestId">Identificador de la intención; repetirlo no crea otro archivo.</param>
/// <param name="Fingerprint">SHA-256 de los filtros normalizados y la versión del reporte.</param>
/// <param name="Kind">Familia del documento, por ejemplo members-v1.</param>
/// <param name="ForceRefresh">Solicita datos nuevos; un trabajo pendiente siempre se reutiliza.</param>
public sealed record ReserveExportRequest(Guid RequestId, string Fingerprint, string Kind, bool ForceRefresh = false);

/// <summary>Columna de un XLSX sin fórmulas ni macros.</summary>
/// <param name="Title">Etiqueta visible, por ejemplo Fecha de bautismo.</param>
/// <param name="Kind">text, date (yyyy-MM-dd) o integer.</param>
/// <param name="Width">Ancho de Excel, por ejemplo 24.</param>
public sealed record ExportColumn(string Title, string Kind = "text", double Width = 24);

/// <summary>Datos preparados por el MSA de negocio para el renderizador central.</summary>
/// <param name="Title">Encabezado, por ejemplo nombre de la iglesia.</param>
/// <param name="Columns">Definición tipada y ordenada de las columnas.</param>
/// <param name="Rows">Todas las filas filtradas; null representa una celda vacía.</param>
/// <param name="DataAsOf">Instante UTC en que se consultaron los datos.</param>
public sealed record ExportWorkbookSource(string Title, IReadOnlyList<ExportColumn> Columns,
    IReadOnlyList<IReadOnlyList<string?>> Rows, DateTimeOffset DataAsOf);

/// <summary>Estado público del documento; nunca incluye datos del padrón ni rutas de almacenamiento.</summary>
/// <param name="ExportId">Identificador estable de esta exportación.</param>
/// <param name="Status">pending, ready o failed.</param>
/// <param name="CreatedAt">Instante UTC de solicitud.</param>
/// <param name="GeneratedAt">Instante UTC de emisión, cuando está listo.</param>
/// <param name="ExpiresAt">Fin de disponibilidad; no invalida una copia ya descargada.</param>
/// <param name="DataAsOf">Fecha de los datos incluidos.</param>
/// <param name="FileName">Nombre seguro del XLSX.</param>
/// <param name="Reused">Indica que se recuperó un trabajo o archivo anterior.</param>
/// <param name="NeedsSource">Solo el productor necesita enviar datos; no se expone en Church UI.</param>
public sealed record TemporaryExportResponse(Guid ExportId, string Status, DateTimeOffset CreatedAt,
    DateTimeOffset? GeneratedAt, DateTimeOffset ExpiresAt, DateTimeOffset? DataAsOf, string FileName,
    bool Reused = false, bool NeedsSource = false);

/// <summary>Acceso temporal a un archivo existente, sin generar una exportación nueva.</summary>
/// <param name="Url">URL firmada de MinIO; no registrarla en logs.</param>
/// <param name="ExpiresAt">Vencimiento de la firma, siempre dentro de la vigencia del archivo.</param>
public sealed record ExportDownloadResponse(string Url, DateTimeOffset ExpiresAt);

/// <summary>Plazos y límites públicos del flujo, reflejados en appsettings.Local.json.</summary>
public sealed class TemporaryExportOptions
{
    /// <summary>Activa el flujo en Local. Ausente o false no inicia el reconciliador.</summary>
    public bool Enabled { get; set; }
    /// <summary>Horas disponibles desde la generación; por defecto 24.</summary>
    [Range(1, 168)] public int RetentionHours { get; set; } = 24;
    /// <summary>Segundos para reutilizar datos de una búsqueda, por defecto 300.</summary>
    [Range(1, 3600)] public int ReuseSeconds { get; set; } = 300;
    /// <summary>Vigencia máxima de una URL de descarga, por defecto 900 segundos.</summary>
    [Range(1, 3600)] public int LinkSeconds { get; set; } = 900;
    /// <summary>Plazo para recibir los datos tras reservar; por defecto 120 segundos.</summary>
    [Range(10, 600)] public int InputTimeoutSeconds { get; set; } = 120;
    /// <summary>Intervalo del reconciliador y limpieza del prefijo temporal.</summary>
    [Range(1, 300)] public int PollSeconds { get; set; } = 5;
    /// <summary>Cantidad de manifiestos revisados por recorrido del worker.</summary>
    [Range(1, 100)] public int BatchSize { get; set; } = 25;
    /// <summary>Máximo de intentos de generación antes de ofrecer un reintento al usuario.</summary>
    [Range(1, 10)] public int MaxAttempts { get; set; } = 3;
    /// <summary>Máximo de filas permitido; rechaza sin truncar los datos.</summary>
    [Range(1, 100000)] public int MaxRows { get; set; } = 10000;
    /// <summary>Máximo de columnas del libro.</summary>
    [Range(1, 100)] public int MaxColumns { get; set; } = 40;
    /// <summary>Máximo del JSON de datos almacenado, por defecto 10 MiB.</summary>
    [Range(1024, 20971520)] public int MaxSourceBytes { get; set; } = 10485760;
}
