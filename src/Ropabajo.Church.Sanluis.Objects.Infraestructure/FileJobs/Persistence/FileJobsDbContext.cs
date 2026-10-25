using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Persistence;

/// <summary>
/// Contexto exclusivo de metadatos de archivos, separado de la base db_object compartida.
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
        jobs.ToTable("file_jobs"); jobs.HasKey(value => value.Sequence); jobs.Property(value => value.Sequence).ValueGeneratedOnAdd();
        jobs.HasAlternateKey(value => value.JobId); jobs.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        jobs.Property(value => value.Environment).HasMaxLength(32).IsRequired(); jobs.Property(value => value.ChurchCode).HasConversion<string>().HasMaxLength(36);
        jobs.Property(value => value.OwnerHash).HasMaxLength(64).IsRequired(); jobs.Property(value => value.ReportType).HasMaxLength(100).IsRequired();
        jobs.Property(value => value.Producer).HasMaxLength(64).IsRequired(); jobs.Property(value => value.Fingerprint).HasMaxLength(64).IsRequired();
        jobs.Property(value => value.ParametersJson).HasColumnType("longtext").IsRequired(); jobs.Property(value => value.Status).HasMaxLength(32).IsRequired();
        jobs.Property(value => value.FailureCode).HasMaxLength(100); jobs.Property(value => value.LeaseHash).HasMaxLength(64);
        jobs.Property(value => value.AttemptId).HasConversion<string>().HasMaxLength(36); jobs.Property(value => value.Version).IsConcurrencyToken();
        jobs.HasIndex(value => new { value.Environment, value.ChurchCode, value.OwnerHash, value.Sequence });
        jobs.HasIndex(value => new { value.Environment, value.Status, value.LeaseUntil });
        var artifacts = modelBuilder.Entity<FileJobArtifact>();
        artifacts.ToTable("file_job_artifacts"); artifacts.HasKey(value => value.ArtifactId);
        artifacts.Property(value => value.ArtifactId).HasConversion<string>().HasMaxLength(36).ValueGeneratedNever();
        artifacts.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        artifacts.Property(value => value.AttemptId).HasConversion<string>().HasMaxLength(36);
        artifacts.Property(value => value.Name).HasMaxLength(50).IsRequired(); artifacts.Property(value => value.FileName).HasMaxLength(200).IsRequired();
        artifacts.Property(value => value.StorageKey).HasMaxLength(500).IsRequired(); artifacts.Property(value => value.ContentType).HasMaxLength(150).IsRequired();
        artifacts.Property(value => value.Sha256).HasMaxLength(64).IsRequired(); artifacts.HasIndex(value => new { value.JobId, value.AttemptId, value.Name }).IsUnique();
        artifacts.HasOne<FileJob>().WithMany(value => value.Artifacts).HasForeignKey(value => value.JobId).HasPrincipalKey(value => value.JobId).OnDelete(DeleteBehavior.Cascade);
        var intents = modelBuilder.Entity<FileJobIntent>();
        intents.ToTable("file_job_intents"); intents.HasKey(value => new { value.Scope, value.RequestId });
        intents.Property(value => value.Scope).HasMaxLength(64); intents.Property(value => value.RequestId).HasConversion<string>().HasMaxLength(36);
        intents.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36); intents.Property(value => value.Fingerprint).HasMaxLength(64).IsRequired();
        var slots = modelBuilder.Entity<FileJobSlot>(); slots.ToTable("file_job_slots"); slots.HasKey(value => value.Fingerprint);
        slots.Property(value => value.Fingerprint).HasMaxLength(64); slots.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        slots.Property(value => value.Version).IsConcurrencyToken();
        var outbox = modelBuilder.Entity<FileJobOutbox>(); outbox.ToTable("file_job_outbox"); outbox.HasKey(value => value.MessageId);
        outbox.Property(value => value.MessageId).HasConversion<string>().HasMaxLength(36); outbox.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        outbox.Property(value => value.Payload).HasColumnType("longtext").IsRequired(); outbox.Property(value => value.DispatchId).HasConversion<string>().HasMaxLength(36);
        outbox.Property(value => value.Version).IsConcurrencyToken(); outbox.HasIndex(value => new { value.PublishedAt, value.AvailableAt, value.DispatchUntil });
        var audit = modelBuilder.Entity<FileJobAudit>(); audit.ToTable("file_job_audit"); audit.HasKey(value => value.AuditId);
        audit.Property(value => value.AuditId).HasConversion<string>().HasMaxLength(36); audit.Property(value => value.JobId).HasConversion<string>().HasMaxLength(36);
        audit.Property(value => value.AttemptId).HasConversion<string>().HasMaxLength(36); audit.Property(value => value.Status).HasMaxLength(32).IsRequired();
        audit.Property(value => value.Code).HasMaxLength(100); audit.HasIndex(value => new { value.JobId, value.OccurredAt });
    }
}
