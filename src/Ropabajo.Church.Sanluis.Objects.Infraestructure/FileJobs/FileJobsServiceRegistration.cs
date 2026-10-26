using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.RabbitMq;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Producers.FileJobs;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Authentication;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Hosting;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Storage;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs;

/// <summary>
/// Registra trabajos con conexión aislada y activación explícita del ambiente.
/// </summary>
public static class FileJobsServiceRegistration
{
    /// <summary>
    /// Configura puertos y hospedaje sin ejecutar DDL ni abrir conexiones desde constructores.
    /// </summary>
    public static IServiceCollection AddFileJobs(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var section = configuration.GetSection("FileJobs");
        var settings = section.Get<FileJobsSettings>() ?? new FileJobsSettings();
        var runtime = section.Get<FileJobsRuntimeSettings>() ?? new FileJobsRuntimeSettings();
        var connection = configuration.GetConnectionString("FileJobs") ?? string.Empty;
        services.AddOptions<FileJobsSettings>().Bind(section)
            .Validate(value => value.Environment is "development" or "production" && value.LeaseSeconds >= 30 && value.RunSeconds >= value.LeaseSeconds
                && value.JobDeadlineSeconds >= value.RunSeconds && value.JobDeadlineSeconds <= 86400
                && value.LinkSeconds is >= 1 and <= 3600 && value.UploadSeconds is >= 60 and <= 3600 && value.MaxAttempts is >= 1 and <= 10
                && value.RetentionHours is >= 1 and <= 168 && value.ReuseSeconds is >= 1 and <= 3600
                && value.MaxBytes is >= 1024 and <= 104857600 && value.MaxActivePerOwner is >= 1 and <= 20
                && value.PollSeconds is >= 1 and <= 300 && value.BatchSize is >= 1 and <= 100 && value.RetryDelaySeconds is >= 1 and <= 600,
                "FileJobs contains invalid public limits.").ValidateOnStart();
        services.AddOptions<FileJobsRuntimeSettings>().Bind(section);
        if (settings.Enabled)
        {
            if (!Uri.TryCreate(runtime.WorkerAuthority, UriKind.Absolute, out var authority) || authority.Scheme != "https"
                || !string.IsNullOrEmpty(authority.UserInfo) || runtime.WorkerAudience != "objects-files"
                || runtime.WorkerClients.Count == 0 || runtime.WorkerClients.Any(client => string.IsNullOrWhiteSpace(client.Key) || client.Value != "church"))
                throw new InvalidOperationException("FileJobs requires an explicit HTTPS worker authority, audience and allowed client map.");
            var sql = new MySqlConnectionStringBuilder(connection);
            var requiredDatabase = settings.Environment == "development" ? "db_object" : "db_file_jobs_prod";
            if (!string.Equals(sql.Database, requiredDatabase, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(sql.UserID) || string.IsNullOrWhiteSpace(sql.Password))
                throw new InvalidOperationException("ConnectionStrings:FileJobs must use the configured metadata database and dedicated credentials.");
            if (settings.Environment == "development")
            {
                var applicationSql = new MySqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection") ?? string.Empty);
                if (!LocalDatabaseConfiguration.IsDevelopmentEndpoint(sql) || !string.Equals(applicationSql.Database, sql.Database, StringComparison.Ordinal)
                    || !string.Equals(applicationSql.Server, sql.Server, StringComparison.OrdinalIgnoreCase) || applicationSql.Port != sql.Port)
                    throw new InvalidOperationException("FileJobs development requires db_object on the same 3306 endpoint as DefaultConnection, using 161.132.54.155 or its mysql-local alias.");
            }
            if (string.IsNullOrWhiteSpace(runtime.Broker.HostName) || string.IsNullOrWhiteSpace(runtime.Broker.VirtualHost)
                || runtime.Broker.Port is < 1 or > 65535 || string.IsNullOrWhiteSpace(runtime.Broker.UserName) || string.IsNullOrWhiteSpace(runtime.Broker.Password)
                || runtime.Broker.Exchange != settings.Environment + ".file-jobs.v1")
                throw new InvalidOperationException("FileJobs:Broker requires dedicated credentials and the environment exchange.");
            services.AddDbContextFactory<FileJobsDbContext>(options => options.UseMySql(connection, new MySqlServerVersion(new Version(5, 7, 44))));
            services.AddScoped<IFileJobStore, SqlFileJobStore>();
        }
        else services.AddScoped<IFileJobStore, DisabledFileJobStore>();
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<IFileJobActor, HttpFileJobActor>();
        services.AddSingleton<IFileArtifactStore, MinioFileArtifactStore>();
        services.AddScoped<FileJobService>();
        services.AddRabbitMqProducer<GenerateFileCommand, FileJobProducer>(section.GetSection("Broker"));
        services.AddHostedService<FileJobsDispatcher>();
        return services;
    }
}
