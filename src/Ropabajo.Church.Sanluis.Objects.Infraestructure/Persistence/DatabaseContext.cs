using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Domain;
using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public virtual DbSet<Departament> Departamentos { get; set; }

        public virtual DbSet<District> Districts { get; set; }

        public virtual DbSet<Province> Provinces { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .UseCollation("utf8mb4_unicode_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Departament>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("departamento");

                entity.HasIndex(e => e.Code, "CODIGO_DEPARTAMENTO_UNIQUE").IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID_DEPARTAMENTO");
                entity.Property(e => e.Abbreviation)
                    .HasMaxLength(7)
                    .HasColumnName("ABREVIATURA");
                entity.Property(e => e.Code).HasColumnName("CODIGO_DEPARTAMENTO");
                entity.Property(e => e.IneiCode)
                    .HasMaxLength(10)
                    .HasColumnName("CODIGO_DEPARTAMENTO_INEI");
                entity.Property(e => e.ReniecCode)
                    .HasMaxLength(10)
                    .HasColumnName("CODIGO_DEPARTAMENTO_RENIEC");
                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .HasColumnName("DESCRIPCION");
                entity.Property(e => e.Delete)
                    .HasColumnType("bit(1)")
                    .HasColumnName("ELIMINADO");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_CREACION");
                entity.Property(e => e.LastModifiedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_MODIFICACION");
                entity.Property(e => e.CreatedIp)
                    .HasMaxLength(15)
                    .HasColumnName("IP_CREACION");
                entity.Property(e => e.LastModifiedIp)
                    .HasMaxLength(15)
                    .HasColumnName("IP_MODIFICACION");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(20)
                    .HasColumnName("USUARIO_CREACION");
                entity.Property(e => e.LastModifiedBy)
                    .HasMaxLength(20)
                    .HasColumnName("USUARIO_MODIFICACION");
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("distrito");

                entity.HasIndex(e => e.Code, "CODIGO_DISTRITO_UNIQUE").IsUnique();

                entity.HasIndex(e => e.DepartamentId, "ID_DEPARTAMENTO");

                entity.HasIndex(e => e.ProvinceId, "ID_PROVINCIA");

                entity.Property(e => e.Id).HasColumnName("ID_DISTRITO");
                entity.Property(e => e.Abbreviation)
                    .HasMaxLength(7)
                    .HasColumnName("ABREVIATURA");
                entity.Property(e => e.DepartamentCode).HasColumnName("CODIGO_DEPARTAMENTO");
                entity.Property(e => e.Code).HasColumnName("CODIGO_DISTRITO");
                entity.Property(e => e.IneiCode)
                    .HasMaxLength(10)
                    .HasColumnName("CODIGO_DISTRITO_INEI");
                entity.Property(e => e.ReniecCode)
                    .HasMaxLength(10)
                    .HasColumnName("CODIGO_DISTRITO_RENIEC");
                entity.Property(e => e.ProvinceCode).HasColumnName("CODIGO_PROVINCIA");
                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .HasColumnName("DESCRIPCION");
                entity.Property(e => e.Delete)
                    .HasColumnType("bit(1)")
                    .HasColumnName("ELIMINADO");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_CREACION");
                entity.Property(e => e.LastModifiedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_MODIFICACION");
                entity.Property(e => e.DepartamentId).HasColumnName("ID_DEPARTAMENTO");
                entity.Property(e => e.ProvinceId).HasColumnName("ID_PROVINCIA");
                entity.Property(e => e.CreatedIp)
                    .HasMaxLength(15)
                    .HasColumnName("IP_CREACION");
                entity.Property(e => e.LastModifiedIp)
                    .HasMaxLength(15)
                    .HasColumnName("IP_MODIFICACION");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(20)
                    .HasColumnName("USUARIO_CREACION");
                entity.Property(e => e.LastModifiedBy)
                    .HasMaxLength(20)
                    .HasColumnName("USUARIO_MODIFICACION");

                entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Districts)
                    .HasForeignKey(d => d.DepartamentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("distrito_ibfk_1");

                entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.Districts)
                    .HasForeignKey(d => d.ProvinceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("distrito_ibfk_2");
            });

            modelBuilder.Entity<Province>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("provincia");

                entity.HasIndex(e => e.Code, "CODIGO_PROVINCIA_UNIQUE").IsUnique();

                entity.HasIndex(e => e.DepartamentId, "ID_DEPARTAMENTO");

                entity.Property(e => e.Id).HasColumnName("ID_PROVINCIA");
                entity.Property(e => e.Abbreviation)
                    .HasMaxLength(7)
                    .HasColumnName("ABREVIATURA");
                entity.Property(e => e.DepartamentCode).HasColumnName("CODIGO_DEPARTAMENTO");
                entity.Property(e => e.Code).HasColumnName("CODIGO_PROVINCIA");
                entity.Property(e => e.IneiCode)
                    .HasMaxLength(10)
                    .HasColumnName("CODIGO_PROVINCIA_INEI");
                entity.Property(e => e.ReniecCode)
                    .HasMaxLength(10)
                    .HasColumnName("CODIGO_PROVINCIA_RENIEC");
                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .HasColumnName("DESCRIPCION");
                entity.Property(e => e.Delete)
                    .HasColumnType("bit(1)")
                    .HasColumnName("ELIMINADO");
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_CREACION");
                entity.Property(e => e.LastModifiedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_MODIFICACION");
                entity.Property(e => e.DepartamentId).HasColumnName("ID_DEPARTAMENTO");
                entity.Property(e => e.CreatedIp)
                    .HasMaxLength(15)
                    .HasColumnName("IP_CREACION");
                entity.Property(e => e.LastModifiedIp)
                    .HasMaxLength(15)
                    .HasColumnName("IP_MODIFICACION");
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(20)
                    .HasColumnName("USUARIO_CREACION");
                entity.Property(e => e.LastModifiedBy)
                    .HasMaxLength(20)
                    .HasColumnName("USUARIO_MODIFICACION");

                entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Provinces)
                    .HasForeignKey(d => d.DepartamentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("provincia_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now;
                        entry.Entity.CreatedBy = "XXX"; // TODO: Get UserName
                        entry.Entity.CreatedIp = "YYY"; // TODO: Get IP remote
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.Now;
                        entry.Entity.LastModifiedBy = "AAA"; // TODO: Get UserName
                        entry.Entity.LastModifiedIp = "BBB"; // TODO: Get IP remote
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}