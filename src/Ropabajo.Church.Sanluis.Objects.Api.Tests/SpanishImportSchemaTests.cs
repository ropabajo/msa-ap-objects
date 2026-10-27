using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Ropabajo.Churc.Sanluis.Framework.Integration;
using Ropabajo.Church.Sanluis.Objects.Domain.Imports;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;
using Xunit;

/// <summary>Exercises the versioned SQL and real EF mappings only in the disposable CI database.</summary>
public sealed class SpanishImportSchemaTests : IAsyncLifetime
{
    private MySqlConnection _connection = null!;
    private string _connectionString = string.Empty;
    private string _sqlRoot = string.Empty;
    private readonly Guid _message = Guid.NewGuid();
    private readonly string _database = "spanish_import_test_" + Guid.NewGuid().ToString("N");
    private static readonly string[] Journal = ["integracion_entrada", "integracion_salida"];

    public async Task InitializeAsync()
    {
        var options = new MySqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FILE_JOBS_TEST_MYSQL")
            ?? throw new InvalidOperationException("The isolated MySQL fixture is required."));
        if (options.Server != "127.0.0.1" || options.Port != 13316)
            throw new InvalidOperationException("Only the disposable local fixture is permitted.");
        var init = Environment.GetEnvironmentVariable("OBJECTS_SCHEMA_INIT_SQL")
            ?? throw new InvalidOperationException("The canonical SQL checkout is required.");
        _sqlRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(init)!, "../.."));
        options.Database = string.Empty;
        await using (var admin = new MySqlConnection(options.ConnectionString))
        {
            await admin.OpenAsync();
            await using var create = new MySqlCommand($"CREATE DATABASE `{_database}` CHARACTER SET utf8mb4", admin);
            await create.ExecuteNonQueryAsync();
        }
        options.Database = _database; _connectionString = options.ConnectionString;
        _connection = new MySqlConnection(_connectionString); await _connection.OpenAsync();
    }

    public async Task DisposeAsync()
    {
        await Execute($"DROP DATABASE `{_database}`");
        await _connection.DisposeAsync();
    }

    [Theory]
    [InlineData("mysql-db-person", false, false)]
    [InlineData("mysql-db-church", false, true)]
    [InlineData("mysql-db-object", true, false)]
    public async Task Canonical_migration_preserves_rows_public_codes_uniqueness_and_can_be_reversed(
        string folder, bool imports, bool replica)
    {
        var old = File.ReadAllText(Path.Combine(_sqlRoot, folder, "db-migrations/20261026-persons-members-import.sql"));
        if (imports) old = old[..old.IndexOf("CREATE TABLE IF NOT EXISTS formato_campo", StringComparison.Ordinal)];
        await Script(old);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("{}"))).ToLowerInvariant();
        await Execute($$"""
            INSERT INTO integracion_entrada VALUES ('{{_message:D}}', 'schema-test', '{{hash}}', '2026-10-27 00:00:00.123456');
            INSERT INTO integracion_salida VALUES (42, UUID(), UUID(), '{{_message:D}}', 'bulk.row.outcome.v1',
              '{"nota":"Niño de prueba"}', '2026-10-27 00:00:00.123456', NULL,
              '2026-10-27 00:00:01.654321', 2, NULL, NULL, NULL, 7);
            """);
        if (imports) await Execute("""
            SET @load=UUID(); SET @person=UUID();
            INSERT INTO importacion_carga VALUES (@load, UUID(), REPEAT('a',64), '{}', UUID(), 'pending', NULL, '2026-10-27 00:00:00.123456', 3);
            INSERT INTO importacion_fila VALUES (UUID(), @load, 2, UUID(), NULL, @person, NULL, '{}', NULL, 'person_pending', NULL, 'Prueba ñ', '2026-10-27 00:00:00.123456', 4);
            """);
        if (replica) await Execute("INSERT INTO importacion_replica_persona VALUES (UUID(), 638970336001234567)");
        var tables = Journal.Concat(imports ? ["importacion_carga", "importacion_fila"] : Array.Empty<string>())
            .Concat(replica ? ["importacion_replica_persona"] : Array.Empty<string>()).ToArray();
        var oldColumns = new Dictionary<string, string[]>();
        foreach (var table in tables)
            oldColumns[table] = (await Rows($"SELECT COLUMN_NAME FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='{table}' ORDER BY ordinal_position"))
                .Select(x => x[0]!).ToArray();
        var before = await Snapshot(oldColumns);
        var directory = Path.Combine(_sqlRoot, folder, "db-migrations");
        var up = File.ReadAllText(Path.Combine(directory, "20261027-integration-columnas-espanol.up.sql"));
        var down = File.ReadAllText(Path.Combine(directory, "20261027-integration-columnas-espanol.down.sql"));
        await Script(up);
        var translated = new Dictionary<string, string[]>();
        foreach (var table in tables)
        {
            var columns = (await Rows($"SELECT COLUMN_NAME FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='{table}' ORDER BY ordinal_position"))
                .Select(x => x[0]!).ToArray();
            foreach (var column in columns) Assert.Matches("^[A-Z][A-Z0-9_]*$", column);
            Assert.Equal("ID_" + table.ToUpperInvariant(), columns[0]);
            var key = Assert.Single(await Rows($"SELECT column_name,extra FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='{table}' AND column_key='PRI'"));
            Assert.Equal(columns[0], key[0]); Assert.Contains("auto_increment", key[1]);
            translated[table] = table == "integracion_salida" ? columns : columns.Skip(1).ToArray();
            var copyColumns = columns.Where(x => x != columns[0]).Select(x => "`" + x + "`");
            var list = string.Join(',', copyColumns);
            var duplicate = await Assert.ThrowsAsync<MySqlException>(() => Execute($"INSERT INTO `{table}` ({list}) SELECT {list} FROM `{table}` LIMIT 1"));
            Assert.Equal(1062, duplicate.Number);
        }
        Assert.Equal(before, await Snapshot(translated));
        await using (var context = Context())
        {
            var receipt = await context.Set<InboxReceipt>().AsNoTracking().SingleAsync();
            Assert.True(receipt.Id > 0); Assert.Equal(_message, receipt.MessageId);
            var message = await context.Set<OutboxMessage>().AsNoTracking().SingleAsync();
            Assert.Equal(42, message.Sequence); Assert.Equal(_message, message.CausationId);
            Assert.Contains("Niño", message.Payload);
            if (imports)
            {
                Assert.True((await context.Set<ImportLoad>().SingleAsync()).Id > 0);
                Assert.True((await context.Set<ImportRow>().SingleAsync()).Id > 0);
            }
        }
        await Script(down); Assert.Equal(before, await Snapshot(oldColumns));
        await Script(up); Assert.Equal(before, await Snapshot(translated));
        // A re-delivery after migration does not execute the business callback again.
        await using (var context = Context())
        {
            var work = new IntegrationTransaction<DatabaseContext>(context); var called = false;
            Assert.False(await work.ReceiveAsync(_message, "schema-test", "{}", _ => { called = true; return Task.CompletedTask; }, default));
            Assert.False(called);
        }
        var afterDuplicate = await Snapshot(translated);
        // Domain and outgoing writes still roll back together if the operation aborts.
        await using (var context = Context())
        {
            var work = new IntegrationTransaction<DatabaseContext>(context);
            await Assert.ThrowsAsync<InvalidOperationException>(() => work.ReceiveAsync(Guid.NewGuid(), "schema-test", "{}", async token =>
            {
                work.Enqueue(Guid.NewGuid(), Guid.NewGuid(), "bulk.row.outcome.v1", new { nota = "rollback" });
                await context.SaveChangesAsync(token);
                throw new InvalidOperationException("Synthetic transaction abort.");
            }, default));
        }
        Assert.Equal(afterDuplicate, await Snapshot(translated));
    }

    private DatabaseContext Context() => new(new DbContextOptionsBuilder<DatabaseContext>()
        .UseMySql(_connectionString, new MySqlServerVersion(new Version(5, 7, 44))).Options, new HttpContextAccessor());

    private async Task<string> Snapshot(Dictionary<string, string[]> columns)
    {
        var values = new Dictionary<string, List<string?[]>>();
        foreach (var (table, names) in columns)
            values[table] = await Rows($"SELECT {string.Join(',', names.Select(x => "`" + x + "`"))} FROM `{table}` ORDER BY `{names[0]}`");
        return JsonSerializer.Serialize(values);
    }

    private async Task<List<string?[]>> Rows(string sql)
    {
        await using var command = new MySqlCommand(sql, _connection);
        await using var reader = await command.ExecuteReaderAsync(); var result = new List<string?[]>();
        while (await reader.ReadAsync())
        {
            var row = new string?[reader.FieldCount];
            for (var i = 0; i < row.Length; i++) row[i] = reader.IsDBNull(i) ? null
                : reader.GetValue(i) is DateTime time ? time.ToString("O", CultureInfo.InvariantCulture)
                : Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture);
            result.Add(row);
        }
        return result;
    }

    private async Task Execute(string sql)
    {
        await using var command = new MySqlCommand(sql, _connection); await command.ExecuteNonQueryAsync();
    }

    private async Task Script(string sql)
    {
        var statements = string.Join('\n', sql.Replace("\r\n", "\n").Split('\n').Where(x => !x.TrimStart().StartsWith("--")));
        foreach (var statement in statements.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            if (!statement.StartsWith("USE ", StringComparison.OrdinalIgnoreCase)) await Execute(statement);
    }
}
