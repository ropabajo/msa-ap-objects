using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ropabajo.Churc.Sanluis.Framework.RabbitMq;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories;
using System.Reflection;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure;

/// <summary>
/// Registra persistencia e integraciones de Objects con configuración explícita.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Valida el destino Local y configura las dependencias sin abrir conexiones.
    /// </summary>
    /// <param name="services">Colección de servicios del aplicativo.</param>
    /// <param name="configuration">Configuración efectiva del ambiente.</param>
    /// <param name="environmentName">Nombre del ambiente ASP.NET Core.</param>
    /// <returns>Colección con las dependencias de infraestructura registradas.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        LocalDatabaseConfiguration.Validate(configuration, environmentName);
        var serverVersion = string.Equals(environmentName, "Local", StringComparison.OrdinalIgnoreCase)
            ? new Version(5, 7, 44) : new Version(8, 0, 21);

        services.AddDbContext<DatabaseContext>(options =>
         options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
             new MySqlServerVersion(serverVersion)));

        services.AddRabbitMq(Assembly.GetExecutingAssembly());
        services.AddFileJobs(configuration);

        services.AddScoped(typeof(IBaseRepository<>), typeof(RepositoryBase<>));
        services.AddScoped<IObjectRepository, ObjectRepository>();
        services.AddScoped<IObjectStateRepository, ObjectStateRepository>();
        services.AddScoped<IBulkLoadRepository, BulkLoadRepository>();
        services.AddScoped<IBulkLoadStateRepository, BulkLoadStateRepository>();
        services.AddScoped<IBulkLoadResultRepository, BulkLoadResultRepository>();
        services.AddScoped<IFormatRepository, FormatRepository>();

        services.AddOptions<Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports.TemporaryExportOptions>()
            .Bind(configuration.GetSection("TemporaryExports")).ValidateDataAnnotations().ValidateOnStart();
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports.IExportActor, TemporaryExports.HttpExportActor>();
        services.AddSingleton<Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports.IExportObjectStore, TemporaryExports.MinioExportObjectStore>();
        services.AddScoped<Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports.IExportLeaseFactory, TemporaryExports.MySqlExportLeaseFactory>();
        services.AddScoped<Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports.ITemporaryWorkbookWriter, TemporaryExports.TemporaryWorkbookWriter>();
        services.AddScoped<Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports.TemporaryExportCoordinator>();
        services.AddHostedService<TemporaryExports.TemporaryExportWorker>();

        return services;
    }
}
