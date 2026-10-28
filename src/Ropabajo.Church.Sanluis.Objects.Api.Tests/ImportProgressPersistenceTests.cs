using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Ropabajo.Church.Sanluis.Objects.Domain.Imports;
using Ropabajo.Church.Sanluis.Objects.Infraestructure;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;
using Xunit;

public sealed class ImportProgressPersistenceTests
{
    [Fact]
    public async Task Reader_projects_only_owned_church_loads_and_ignores_deleted_and_unrequested_loads()
    {
        var sql = new MySqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("FILE_JOBS_TEST_MYSQL")
                ?? throw new InvalidOperationException("Disposable MySQL required.")
        );
        if (sql.Server != "127.0.0.1" || sql.Port != 13316)
            throw new InvalidOperationException("Only the isolated fixture is allowed.");
        sql.Database = "progress_test_" + Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseMySql(sql.ConnectionString, new MySqlServerVersion(new Version(5, 7, 44)))
            .Options;
        await using var context = new DatabaseContext(options, new HttpContextAccessor());
        try
        {
            await context.Database.EnsureCreatedAsync();
            var format = new Format
            {
                Code = Guid.NewGuid(),
                Name = "Fixture",
                Path = "fixture",
                AllowedExtensions = "xlsx",
                Template = "fixture.xlsx",
            };
            var document = new Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object
            {
                Code = Guid.NewGuid(),
                ObjectName = "fixture",
                FileName = "fixture.xlsx",
                Path = "fixture",
                AllowedExtensions = "xlsx",
                StateCode = "CARGADO",
                User = "fixture",
            };
            context.Formats.Add(format);
            context.Objects.Add(document);
            await context.SaveChangesAsync();
            var church = Guid.NewGuid();
            var own = Guid.NewGuid();
            var foreignOwner = Guid.NewGuid();
            var foreignChurch = Guid.NewGuid();
            var deleted = Guid.NewGuid();
            foreach (
                var (code, owner, scope, remove) in new[]
                {
                    (own, "owner", church, false),
                    (foreignOwner, "other", church, false),
                    (foreignChurch, "owner", Guid.NewGuid(), false),
                    (deleted, "owner", church, true),
                }
            )
            {
                context.BulkLoads.Add(
                    new BulkLoad
                    {
                        FormatId = format.Id,
                        FormatCode = format.Code,
                        ObjectId = document.Id,
                        ObjectCode = document.Code,
                        Code = code,
                        StateCode = "PROCESADO",
                        User = "fixture",
                        Delete = remove,
                        Records = 3,
                        UploadedRecords = 2,
                        ObservedRecords = 1,
                    }
                );
                context
                    .Set<ImportLoad>()
                    .Add(
                        new ImportLoad
                        {
                            LoadCode = code,
                            ChurchCode = scope,
                            OwnerHash = owner,
                            Status = "observed",
                            ScopeJson = "{}",
                            InitialMessageId = Guid.NewGuid(),
                            Version = 8,
                            UpdatedAt = new DateTime(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc),
                        }
                    );
            }
            await context.SaveChangesAsync();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddInfrastructure(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:DefaultConnection"] = sql.ConnectionString,
                        }
                    )
                    .Build(),
                "Test"
            );
            services.AddScoped(_ => new DatabaseContext(options, new HttpContextAccessor()));
            await using var provider = services.BuildServiceProvider();
            await using var serviceScope = provider.CreateAsyncScope();
            var reader = serviceScope.ServiceProvider.GetRequiredService<IImportProgressReader>();
            var rows = await reader.ReadAsync(
                new(church, "owner", "subject", "fixture", "127.0.0.1"),
                [own, foreignOwner, foreignChurch, deleted],
                default
            );
            var row = Assert.Single(rows);
            Assert.Equal(own, row.BulkLoadCode);
            Assert.Equal(2, row.UploadedRecords);
            Assert.Equal(8, row.Version);
            Assert.Equal(DateTimeKind.Utc, row.UpdatedAt.Kind);
            Assert.Empty(
                await reader.ReadAsync(
                    new(church, "owner", "subject", "fixture", "127.0.0.1"),
                    [Guid.NewGuid()],
                    default
                )
            );
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}
