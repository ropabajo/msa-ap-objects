using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Domain.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Imports;

internal static class ImportModel
{
    public static void AddImports(this ModelBuilder model)
    {
        model.Entity<ImportLoad>(e =>
        {
            e.ToTable("importacion_carga"); e.HasKey(x => x.LoadCode);
            e.Property(x => x.LoadCode).HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.ChurchCode).HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.InitialMessageId).HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.OwnerHash).HasMaxLength(64); e.Property(x => x.ScopeJson).HasColumnType("longtext");
            e.Property(x => x.Status).HasMaxLength(32); e.Property(x => x.ErrorCode).HasMaxLength(100);
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasIndex(x => new { x.ChurchCode, x.OwnerHash });
        });
        model.Entity<ImportRow>(e =>
        {
            e.ToTable("importacion_fila"); e.HasKey(x => x.RowCode);
            e.Property(x => x.RowCode).HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.LoadCode).HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.PersonRequestId).HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.MemberRequestId).HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.PersonCode).HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.MemberCode).HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.PersonRequestJson).HasColumnType("longtext"); e.Property(x => x.MemberRequestJson).HasColumnType("longtext");
            e.Property(x => x.Status).HasMaxLength(32); e.Property(x => x.ErrorCode).HasMaxLength(100); e.Property(x => x.ErrorMessage).HasMaxLength(400);
            e.Property(x => x.Version).IsConcurrencyToken(); e.HasIndex(x => new { x.LoadCode, x.RowNumber }).IsUnique();
            e.HasIndex(x => new { x.Status, x.UpdatedAt });
        });
    }
}
