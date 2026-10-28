using Microsoft.AspNetCore.Http.Connections;
using Ropabajo.Churc.Sanluis.Framework.Realtime;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Api.Realtime;

/// <summary>Compone transporte y observación de contadores, sin SQL adicional ni otro broker.</summary>
public static class ImportProgressRegistration
{
    /// <summary>Registra el canal exclusivamente en el contrato Local de importación.</summary>
    public static IServiceCollection AddImportProgress(
        this IServiceCollection services,
        IHostEnvironment environment
    )
    {
        if (!environment.IsEnvironment("Local"))
            return services;
        services.AddConnectionTickets();
        services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 4096;
            options.MaximumParallelInvocationsPerClient = 1;
            options.EnableDetailedErrors = false;
        });
        services.AddSingleton<ImportProgressConnections>();
        services.AddTransient<ImportProgressHub>(provider => new ImportProgressHub(
            provider.GetRequiredService<ImportProgressConnections>(),
            provider.GetRequiredService<Ropabajo.Churc.Sanluis.Framework.Mediator.IMediatorBus>(),
            provider.GetRequiredService<AutoMapper.IMapper>()
        ));
        services.AddScoped<IImportProgressTicketIssuer, ImportProgressTicketIssuer>();
        services.AddHostedService<ImportProgressPublisher>();
        return services;
    }

    /// <summary>Habilita el reconocimiento del upgrade antes del middleware de autenticación.</summary>
    public static WebApplication UseImportProgress(this WebApplication app)
    {
        if (app.Environment.IsEnvironment("Local"))
            app.UseWebSockets();
        return app;
    }

    /// <summary>Expone únicamente WebSocket; la sesión se cierra al vencer su autenticación.</summary>
    public static WebApplication MapImportProgress(this WebApplication app)
    {
        if (app.Environment.IsEnvironment("Local"))
            app.MapHub<ImportProgressHub>(
                "/v2/churches/{churchCode:guid}/imports/progress",
                options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                    options.CloseOnAuthenticationExpiration = true;
                }
            );
        return app;
    }
}
