using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Object = Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object;


namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

public partial class DatabaseContext : DbContext
{
    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BulkLoad> BulkLoads { get; set; }

    public virtual DbSet<BulkLoadState> BulkLoadStates { get; set; }

    public virtual DbSet<BulkLoadResult> BulkLoadResults { get; set; }

    public virtual DbSet<Format> Formats { get; set; }

    public virtual DbSet<Object> Objects { get; set; }

    public virtual DbSet<ObjectState> ObjectStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<BulkLoad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("carga_masiva");

            entity.HasIndex(e => e.Code, "CODIGO_CARGA_MASIVA_UNIQUE").IsUnique();

            entity.HasIndex(e => e.FormatId, "ID_FORMATO");

            entity.HasIndex(e => e.ObjectId, "ID_OBJETO");

            entity.Property(e => e.Id).HasColumnName("ID_CARGA_MASIVA");
            entity.Property(e => e.Code).HasColumnName("CODIGO_CARGA_MASIVA");
            entity.Property(e => e.StateCode)
                .HasMaxLength(40)
                .HasColumnName("CODIGO_ESTADO");
            entity.Property(e => e.FormatCode).HasColumnName("CODIGO_FORMATO");
            entity.Property(e => e.ObjectCode).HasColumnName("CODIGO_OBJETO");
            entity.Property(e => e.Description)
                .HasMaxLength(400)
                .HasColumnName("DESCRIPCION");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("FECHA");

            entity.Property(e => e.FormatId).HasColumnName("ID_FORMATO");
            entity.Property(e => e.ObjectId).HasColumnName("ID_OBJETO");

            entity.Property(e => e.FileName)
                .HasMaxLength(80)
                .HasColumnName("NOMBRE_ARCHIVO");
            entity.Property(e => e.ObjectName)
                .HasMaxLength(800)
                .HasColumnName("NOMBRE_OBJETO");
            entity.Property(e => e.Records).HasColumnName("NUMERO_REGISTROS");
            entity.Property(e => e.UploadedRecords).HasColumnName("NUMERO_REGISTROS_CARGADOS");
            entity.Property(e => e.ObservedRecords).HasColumnName("NUMERO_REGISTROS_OBSEVADOS");
            entity.Property(e => e.User)
                .HasMaxLength(80)
                .HasColumnName("USUARIO");

            entity.Property(e => e.Delete)
                .HasColumnType("bit(1)")
                .HasColumnName("ELIMINADO");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("USUARIO_CREACION");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("USUARIO_MODIFICACION");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("FECHA_CREACION");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("FECHA_MODIFICACION");
            entity.Property(e => e.CreatedIp)
              .HasMaxLength(40)
              .HasColumnName("IP_CREACION");
            entity.Property(e => e.LastModifiedIp)
                .HasMaxLength(40)
                .HasColumnName("IP_MODIFICACION");
        });

        modelBuilder.Entity<BulkLoadState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("carga_masiva_estado");

            entity.HasIndex(e => e.BulkLoadId, "ID_CARGA_MASIVA");

            entity.Property(e => e.Id).HasColumnName("ID_CARGA_MASIVA_ESTADO");
            entity.Property(e => e.StateCode)
                .HasMaxLength(40)
                .HasColumnName("CODIGO_ESTADO");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("FECHA");

            entity.Property(e => e.BulkLoadId).HasColumnName("ID_CARGA_MASIVA");

            entity.Property(e => e.User)
                .HasMaxLength(80)
                .HasColumnName("USUARIO");



        });

        modelBuilder.Entity<BulkLoadResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("carga_masiva_resultado");

            entity.HasIndex(e => e.BulkLoadId, "ID_CARGA_MASIVA");

            entity.Property(e => e.Id).HasColumnName("ID_CARGA_MASIVA_RESULTADO");
            entity.Property(e => e.StateCode)
                .HasMaxLength(40)
                .HasColumnName("CODIGO_ESTADO");


            entity.Property(e => e.Row)
                .HasMaxLength(8000)
                .HasColumnName("FILA");
            entity.Property(e => e.BulkLoadId).HasColumnName("ID_CARGA_MASIVA");

            entity.Property(e => e.RowNumber).HasColumnName("NUMERO_FILA");
            entity.Property(e => e.Observation)
                .HasMaxLength(400)
                .HasColumnName("OBSERVACION");

        });

        modelBuilder.Entity<Format>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("formato");

            entity.HasIndex(e => e.Code, "CODIGO_FORMATO_UNIQUE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID_FORMATO");
            entity.Property(e => e.Code).HasColumnName("CODIGO_FORMATO");

            entity.Property(e => e.Expiration).HasColumnName("EXPIRACION");
            entity.Property(e => e.AllowedExtensions)
                .HasMaxLength(40)
                .HasColumnName("EXTENSIONES_PERMITIDAS");


            entity.Property(e => e.MaxLength).HasColumnName("LONGITUD_MAXIMA");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.Template)
                .HasMaxLength(800)
                .HasColumnName("PLANTILLA");
            entity.Property(e => e.Path)
                .HasMaxLength(400)
                .HasColumnName("RUTA");

        });

        modelBuilder.Entity<Object>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("objeto");

            entity.HasIndex(e => e.Code, "CODIGO_OBJETO_UNIQUE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID_OBJETO");
            entity.Property(e => e.StateCode)
                .HasMaxLength(40)
                .HasColumnName("CODIGO_ESTADO");
            entity.Property(e => e.Code).HasColumnName("CODIGO_OBJETO");

            entity.Property(e => e.Expiration).HasColumnName("EXPIRACION");
            entity.Property(e => e.AllowedExtensions)
                .HasMaxLength(40)
                .HasColumnName("EXTENSIONES_PERMITIDAS");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("FECHA");

            entity.Property(e => e.MaxLength).HasColumnName("LONGITUD_MAXIMA");
            entity.Property(e => e.FileName)
                .HasMaxLength(80)
                .HasColumnName("NOMBRE_ARCHIVO");
            entity.Property(e => e.ObjectName)
                .HasMaxLength(800)
                .HasColumnName("NOMBRE_OBJETO");
            entity.Property(e => e.Path)
                .HasMaxLength(400)
                .HasColumnName("RUTA");
            entity.Property(e => e.User)
                .HasMaxLength(80)
                .HasColumnName("USUARIO");

        });

        modelBuilder.Entity<ObjectState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("objeto_estado");

            entity.HasIndex(e => e.ObjectId, "ID_OBJETO");

            entity.Property(e => e.Id).HasColumnName("ID_OBJETO_ESTADO");
            entity.Property(e => e.StateCode)
                .HasMaxLength(40)
                .HasColumnName("CODIGO_ESTADO");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("FECHA");

            entity.Property(e => e.ObjectId).HasColumnName("ID_OBJETO");

            entity.Property(e => e.User)
                .HasMaxLength(80)
                .HasColumnName("USUARIO");

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
