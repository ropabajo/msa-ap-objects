namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs;

/// <summary>
/// Configura autenticación técnica y transporte sin incluirlos en la lógica de Application.
/// </summary>
public sealed class FileJobsRuntimeSettings
{
    /// <summary>
    /// Audiencia exclusiva de los endpoints de workers.
    /// </summary>
    /// <example>objects-files</example>
    public string WorkerAudience { get; set; } = "objects-files";

    /// <summary>
    /// Mapa cerrado de clientId de Keycloak hacia generador permitido.
    /// </summary>
    public Dictionary<string, string> WorkerClients { get; set; } = new();

}
