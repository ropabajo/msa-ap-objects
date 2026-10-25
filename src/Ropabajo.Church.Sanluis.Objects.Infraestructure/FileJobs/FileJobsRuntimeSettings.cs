using Ropabajo.Churc.Sanluis.Framework.Files.Configuration;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs;

/// <summary>
/// Configura autenticación técnica y transporte sin incluirlos en la lógica de Application.
/// </summary>
public sealed class FileJobsRuntimeSettings
{
    /// <summary>
    /// Autoridad HTTPS que emite los JWT de generadores técnicos.
    /// </summary>
    /// <example>https://auth-dev.idnsanluis.org/realms/church</example>
    public string WorkerAuthority { get; set; } = string.Empty;

    /// <summary>
    /// Audiencia exclusiva de los endpoints de workers.
    /// </summary>
    /// <example>objects-files</example>
    public string WorkerAudience { get; set; } = "objects-files";

    /// <summary>
    /// Mapa cerrado de clientId de Keycloak hacia generador permitido.
    /// </summary>
    public Dictionary<string, string> WorkerClients { get; set; } = new();

    /// <summary>
    /// Transporte de la outbox con identidad de publicación restringida.
    /// </summary>
    public FileBrokerSettings Broker { get; set; } = new();
}
