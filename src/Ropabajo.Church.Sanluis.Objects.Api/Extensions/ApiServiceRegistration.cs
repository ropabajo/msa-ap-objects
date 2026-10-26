using Ropabajo.Churc.Sanluis.Framework.Authz;
using Ropabajo.Church.Sanluis.Objects.Api.Controllers;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs;

namespace Ropabajo.Church.Sanluis.Objects.Api.Extensions;

/// <summary>
/// Registra filtros HTTP y autenticación técnica de Objects.
/// </summary>
public static class ApiServiceRegistration
{
    /// <summary>
    /// Compone los adaptadores HTTP; el framework aplica las reglas comunes del JWT técnico.
    /// </summary>
    /// <param name="services">Colección de servicios del host.</param>
    /// <param name="configuration">Configuración efectiva del ambiente.</param>
    /// <param name="environment">Ambiente del host para seleccionar el contrato de importación.</param>
    /// <returns>Colección con los adaptadores de la API registrados.</returns>
    public static IServiceCollection AddObjectsApi(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddScoped<FileJobExceptionFilter>();
        services.AddControllers(options => options.Conventions.Add(new ImportControllerConvention(environment.IsEnvironment("Local"))));
        var runtime = configuration.GetSection("FileJobs").Get<FileJobsRuntimeSettings>() ?? new FileJobsRuntimeSettings();
        services.AddServiceAccountAuthentication("FileJobsWorker", configuration["Keycloak:Authority"] ?? string.Empty,
            runtime.WorkerAudience, runtime.WorkerClients.Keys);
        return services;
    }
}
