using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.TemporaryExports;

/// <summary>Bloqueo asesor MySQL entre réplicas; no crea tablas ni utiliza ACL nuevas de Redis.</summary>
public sealed class MySqlExportLeaseFactory(IConfiguration configuration, IOptions<MinIoOptions> storage) : IExportLeaseFactory
{
    /// <inheritdoc />
    public async Task<IAsyncDisposable?> TryAcquireAsync(string key, CancellationToken token)
    {
        // A dedicated, non-pooled connection releases the lock even on shutdown or broken cleanup.
        var builder = new MySqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Export lease database connection is not configured.")) { Pooling = false };
        var connection = new MySqlConnection(builder.ConnectionString);
        try
        {
            await connection.OpenAsync(token);
            var identity = storage.Value.Endpoint + ":" + storage.Value.BucketName + ":" + key;
            var name = "exports-v1:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..53];
            await using var command = new MySqlCommand("SELECT GET_LOCK(@name, 0)", connection) { CommandTimeout = 5 };
            command.Parameters.AddWithValue("@name", name);
            var result = await command.ExecuteScalarAsync(token);
            if (result is not null && result != DBNull.Value && Convert.ToInt32(result) == 1) return new Lease(connection);
            await connection.DisposeAsync(); return null;
        }
        catch { await connection.DisposeAsync(); throw; }
    }
    private sealed class Lease(MySqlConnection connection) : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => connection.DisposeAsync();
    }
}
