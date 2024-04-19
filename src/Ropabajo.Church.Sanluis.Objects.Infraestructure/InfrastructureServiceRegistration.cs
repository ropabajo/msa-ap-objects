using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure
{
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

            services.AddScoped(typeof(IBaseRepository<>), typeof(RepositoryBase<>));
            services.AddScoped<IObjectRepository, ObjectRepository>();
            services.AddScoped<IObjectStateRepository, ObjectStateRepository>();
            services.AddScoped<IBulkLoadRepository, BulkLoadRepository>();
            services.AddScoped<IBulkLoadStateRepository, BulkLoadStateRepository>();
            services.AddScoped<IFormatRepository, FormatRepository>(); 
            return services;
        }
    }
}
