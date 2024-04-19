using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using System.Reflection;

namespace Ropabajo.Church.Sanluis.Objects.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediator(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
