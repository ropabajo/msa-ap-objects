using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

/// <summary>
/// Comprueba el destino SQL del ambiente Local antes de registrar el acceso a datos.
/// </summary>
public static class LocalDatabaseConfiguration
{
    /// <summary>
    /// Exige db_object del servidor DEV en Local; conserva los demás ambientes.
    /// </summary>
    /// <param name="configuration">Configuración efectiva, incluidas las variables del runtime.</param>
    /// <param name="environmentName">Nombre del ambiente ASP.NET Core.</param>
    public static void Validate(IConfiguration configuration, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);
        if (!string.Equals(environmentName, "Local", StringComparison.OrdinalIgnoreCase)) return;

        var connection = new MySqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection") ?? string.Empty);
        if (!string.Equals(connection.Database, "db_object", StringComparison.Ordinal)
            || !IsDevelopmentEndpoint(connection) || string.IsNullOrWhiteSpace(connection.UserID) || string.IsNullOrWhiteSpace(connection.Password))
            throw new InvalidOperationException("Objects Local requiere db_object en 161.132.54.155:3306 o su alias mysql-local, con credenciales del runtime de desarrollo.");
    }

    internal static bool IsDevelopmentEndpoint(MySqlConnectionStringBuilder connection)
        => connection.Port == 3306 && (string.Equals(connection.Server, "161.132.54.155", StringComparison.OrdinalIgnoreCase)
            || string.Equals(connection.Server, "mysql-local.church-dev.svc.cluster.local", StringComparison.OrdinalIgnoreCase));
}
