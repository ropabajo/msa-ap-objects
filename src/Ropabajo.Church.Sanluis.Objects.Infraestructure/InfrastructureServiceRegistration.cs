using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ropabajo.Churc.Sanluis.Framework.RabbitMq;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories;
using System.Reflection;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure;

/// <summary>Registra la persistencia y las integraciones de Objects.</summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>Selecciona el dialecto SQL del ambiente sin abrir conexiones.</summary>
    /// <param name="services">Colección de servicios del aplicativo.</param>
    /// <param name="configuration">Configuración efectiva, con credenciales inyectadas.</param>
    /// <param name="environmentName">Ambiente ASP.NET Core; Local utiliza MySQL 5.7.44.</param>
    /// <returns>Colección con las dependencias de infraestructura registradas.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var serverVersion = string.Equals(environmentName, "Local", StringComparison.OrdinalIgnoreCase)
            ? new Version(5, 7, 44) : new Version(8, 0, 21);

        services.AddDbContext<DatabaseContext>(options =>
         options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
             new MySqlServerVersion(serverVersion)));

        services.AddRabbitMq(Assembly.GetExecutingAssembly());

        services.AddScoped(typeof(IBaseRepository<>), typeof(RepositoryBase<>));
        services.AddScoped<IObjectRepository, ObjectRepository>();
        services.AddScoped<IObjectStateRepository, ObjectStateRepository>();
        services.AddScoped<IBulkLoadRepository, BulkLoadRepository>();
        services.AddScoped<IBulkLoadStateRepository, BulkLoadStateRepository>();
        services.AddScoped<IBulkLoadResultRepository, BulkLoadResultRepository>();
        services.AddScoped<IFormatRepository, FormatRepository>();

        return services;
    }
}
