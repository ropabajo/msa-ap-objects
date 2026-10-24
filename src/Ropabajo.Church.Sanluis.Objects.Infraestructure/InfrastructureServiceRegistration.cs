using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ropabajo.Churc.Sanluis.Framework.RabbitMq;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories;
using System.Reflection;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        IConfiguration configuration;
        using (var serviceProvider = services.BuildServiceProvider())
        {
            configuration = serviceProvider.GetService<IConfiguration>();
        }

        services.AddDbContext<DatabaseContext>(options =>
         options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
             new MySqlServerVersion(new Version(8, 0, 21))));

        services.AddRabbitMq(Assembly.GetExecutingAssembly());

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
