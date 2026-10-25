using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Persistence;

/// <summary>
/// Contexto independiente de tareas de archivos; en desarrollo utiliza db_object en el servidor DEV.
/// </summary>
public sealed class FileJobsDbContext : DbContext
{
    /// <summary>
    /// Inicializa el contexto con la conexión específica de trabajos.
    /// </summary>
    public FileJobsDbContext(DbContextOptions<FileJobsDbContext> options) : base(options ?? throw new ArgumentNullException(nameof(options))) { }

    /// <summary>
    /// Trabajos persistentes y su ámbito.
    /// </summary>
    public DbSet<FileJob> Jobs => Set<FileJob>();

    /// <summary>
    /// Reservas y archivos publicados por intento.
    /// </summary>
    public DbSet<FileJobArtifact> Artifacts => Set<FileJobArtifact>();

    /// <summary>
    /// Identidades de intención y su respuesta estable.
    /// </summary>
    public DbSet<FileJobIntent> Intents => Set<FileJobIntent>();

    /// <summary>
    /// Registros únicos de reutilización.
    /// </summary>
    public DbSet<FileJobSlot> Slots => Set<FileJobSlot>();

    /// <summary>
    /// Mensajes pendientes o confirmados por el broker.
    /// </summary>
    public DbSet<FileJobOutbox> Outbox => Set<FileJobOutbox>();

    /// <summary>
    /// Transiciones de estado sin contenido del reporte.
    /// </summary>
    public DbSet<FileJobAudit> Audit => Set<FileJobAudit>();

    /// <summary>
    /// Configura unicidad, relaciones, tipos y concurrencia para MySQL.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jobs = modelBuilder.Entity<FileJob>();
        jobs.HasKey(value => value.Sequence).HasName("PK_TAREA_ARCHIVO"); jobs.Property(value => value.Sequence).ValueGeneratedOnAdd();
        jobs.HasAlternateKey(value => value.JobId).HasName("CODIGO_TAREA_ARCHIVO_UNIQUE"); jobs.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        jobs.Property(value => value.Environment).HasMaxLength(32).IsRequired(); jobs.Property(value => value.ChurchCode).HasConversion<string>().HasMaxLength(36);
        jobs.Property(value => value.OwnerHash).HasMaxLength(64).IsRequired(); jobs.Property(value => value.ReportType).HasMaxLength(100).IsRequired();
        jobs.Property(value => value.Producer).HasMaxLength(64).IsRequired(); jobs.Property(value => value.Fingerprint).HasMaxLength(64).IsRequired();
        jobs.Property(value => value.ParametersJson).HasColumnType("longtext").IsRequired(); jobs.Property(value => value.Status).HasMaxLength(32).IsRequired();
        jobs.Property(value => value.FailureCode).HasMaxLength(100); jobs.Property(value => value.LeaseHash).HasMaxLength(64);
        jobs.Property(value => value.AttemptId).HasConversion<string>().HasMaxLength(36); jobs.Property(value => value.Version).IsConcurrencyToken();
        jobs.HasIndex(value => new { value.Environment, value.ChurchCode, value.OwnerHash, value.Sequence }).HasDatabaseName("IDX_TAREA_ARCHIVO_AMBITO");
        jobs.HasIndex(value => new { value.Environment, value.Status, value.LeaseUntil }).HasDatabaseName("IDX_TAREA_ARCHIVO_ESTADO_PERMISO");
        var artifacts = modelBuilder.Entity<FileJobArtifact>();
        artifacts.HasKey(value => value.Id).HasName("PK_TAREA_ARCHIVO_RESULTADO");
        artifacts.Property(value => value.Id).ValueGeneratedOnAdd();
        artifacts.HasAlternateKey(value => value.ArtifactId).HasName("CODIGO_ARCHIVO_UNIQUE");
        artifacts.Property(value => value.ArtifactId).HasConversion<string>().HasMaxLength(36).ValueGeneratedNever();
        artifacts.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        artifacts.Property(value => value.AttemptId).HasConversion<string>().HasMaxLength(36);
        artifacts.Property(value => value.Name).HasMaxLength(50).IsRequired(); artifacts.Property(value => value.FileName).HasMaxLength(200).IsRequired();
        artifacts.Property(value => value.StorageKey).HasMaxLength(500).IsRequired(); artifacts.Property(value => value.ContentType).HasMaxLength(150).IsRequired();
        artifacts.Property(value => value.Sha256).HasMaxLength(64).IsRequired(); artifacts.HasIndex(value => new { value.JobId, value.AttemptId, value.Name }).IsUnique().HasDatabaseName("RESULTADO_ARCHIVO_INTENTO_NOMBRE_UNIQUE");
        artifacts.HasOne<FileJob>().WithMany(value => value.Artifacts).HasForeignKey(value => value.JobId).HasPrincipalKey(value => value.JobId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_RESULTADO_ARCHIVO_TAREA");
        var intents = modelBuilder.Entity<FileJobIntent>();
        intents.HasKey(value => value.Id).HasName("PK_TAREA_ARCHIVO_SOLICITUD");
        intents.Property(value => value.Id).ValueGeneratedOnAdd();
        intents.HasIndex(value => new { value.Scope, value.RequestId }).IsUnique().HasDatabaseName("SOLICITUD_ARCHIVO_AMBITO_UNIQUE");
        intents.Property(value => value.Scope).HasMaxLength(64); intents.Property(value => value.RequestId).HasConversion<string>().HasMaxLength(36);
        intents.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36); intents.Property(value => value.Fingerprint).HasMaxLength(64).IsRequired();
        var slots = modelBuilder.Entity<FileJobSlot>(); slots.HasKey(value => value.Id).HasName("PK_TAREA_ARCHIVO_REUTILIZACION");
        slots.Property(value => value.Id).ValueGeneratedOnAdd();
        slots.HasIndex(value => value.Fingerprint).IsUnique().HasDatabaseName("REUTILIZACION_ARCHIVO_HUELLA_UNIQUE");
        slots.Property(value => value.Fingerprint).HasMaxLength(64); slots.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        slots.Property(value => value.Version).IsConcurrencyToken();
        var outbox = modelBuilder.Entity<FileJobOutbox>(); outbox.HasKey(value => value.Id).HasName("PK_TAREA_ARCHIVO_MENSAJE");
        outbox.Property(value => value.Id).ValueGeneratedOnAdd();
        outbox.HasIndex(value => value.MessageId).IsUnique().HasDatabaseName("CODIGO_MENSAJE_UNIQUE");
        outbox.Property(value => value.MessageId).HasConversion<string>().HasMaxLength(36); outbox.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        outbox.Property(value => value.Payload).HasColumnType("longtext").IsRequired(); outbox.Property(value => value.DispatchId).HasConversion<string>().HasMaxLength(36);
        outbox.Property(value => value.Version).IsConcurrencyToken(); outbox.HasIndex(value => new { value.PublishedAt, value.AvailableAt, value.DispatchUntil }).HasDatabaseName("IDX_MENSAJE_ARCHIVO_DESPACHO");
        var audit = modelBuilder.Entity<FileJobAudit>(); audit.HasKey(value => value.Id).HasName("PK_TAREA_ARCHIVO_AUDITORIA");
        audit.Property(value => value.Id).ValueGeneratedOnAdd();
        audit.HasIndex(value => value.AuditId).IsUnique().HasDatabaseName("CODIGO_AUDITORIA_UNIQUE");
        audit.Property(value => value.AuditId).HasConversion<string>().HasMaxLength(36); audit.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        audit.Property(value => value.AttemptId).HasConversion<string>().HasMaxLength(36); audit.Property(value => value.Status).HasMaxLength(32).IsRequired();
        audit.Property(value => value.Code).HasMaxLength(100); audit.HasIndex(value => new { value.JobId, value.OccurredAt }).HasDatabaseName("IDX_AUDITORIA_ARCHIVO_TAREA_FECHA");
    }
}
