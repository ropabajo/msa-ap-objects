namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs;

/// <summary>
/// Define los plazos y límites públicos de los trabajos de archivos.
/// </summary>
public sealed class FileJobsSettings
{
    /// <summary>
    /// Habilita el mecanismo solo después de preparar SQL y las identidades técnicas.
    /// </summary>
    /// <example>false</example>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Ambiente lógico que se incorpora a todos los ámbitos y mensajes.
    /// </summary>
    /// <example>development</example>
    public string Environment { get; set; } = "development";

    /// <summary>
    /// Vigencia del permiso del intento; se renueva mediante heartbeat.
    /// </summary>
    /// <example>120</example>
    public int LeaseSeconds { get; set; } = 120;

    /// <summary>
    /// Tiempo máximo de ejecución de un intento.
    /// </summary>
    /// <example>900</example>
    public int RunSeconds { get; set; } = 900;

    /// <summary>
    /// Tiempo máximo para resolver un trabajo aceptado.
    /// </summary>
    /// <example>3600</example>
    public int JobDeadlineSeconds { get; set; } = 3600;

    /// <summary>
    /// Vigencia máxima de la firma de descarga.
    /// </summary>
    /// <example>900</example>
    public int LinkSeconds { get; set; } = 900;

    /// <summary>
    /// Vigencia máxima de una política de subida.
    /// </summary>
    /// <example>600</example>
    public int UploadSeconds { get; set; } = 600;

    /// <summary>
    /// Disponibilidad del Excel desde generación.
    /// </summary>
    /// <example>24</example>
    public int RetentionHours { get; set; } = 24;

    /// <summary>
    /// Antigüedad máxima de datos reutilizables.
    /// </summary>
    /// <example>300</example>
    public int ReuseSeconds { get; set; } = 300;

    /// <summary>
    /// Cantidad máxima total de intentos de generación.
    /// </summary>
    /// <example>3</example>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Espera base del primer reintento; el siguiente espera cuatro veces este valor.
    /// </summary>
    /// <example>30</example>
    public int RetryDelaySeconds { get; set; } = 30;


    /// <summary>
    /// Máximo de bytes por archivo generado.
    /// </summary>
    /// <example>20971520</example>
    public long MaxBytes { get; set; } = 20971520;

    /// <summary>
    /// Cantidad máxima de trabajos activos por usuario e iglesia.
    /// </summary>
    /// <example>3</example>
    public int MaxActivePerOwner { get; set; } = 3;

    /// <summary>
    /// Intervalo de despacho y recuperación.
    /// </summary>
    /// <example>5</example>
    public int PollSeconds { get; set; } = 5;

    /// <summary>
    /// Tamaño de lote del dispatcher y reconciliador.
    /// </summary>
    /// <example>25</example>
    public int BatchSize { get; set; } = 25;
}
