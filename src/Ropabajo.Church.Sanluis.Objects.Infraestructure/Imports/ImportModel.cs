using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Domain.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Imports;

internal static class ImportModel
{
    public static void AddImports(this ModelBuilder model)
    {
        model.Entity<ImportLoad>(e =>
        {
            e.ToTable("importacion_carga"); e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID_IMPORTACION_CARGA").ValueGeneratedOnAdd();
            e.Property(x => x.LoadCode).HasColumnName("CODIGO_CARGA").HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.ChurchCode).HasColumnName("CODIGO_IGLESIA").HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.OwnerHash).HasColumnName("HUELLA_SOLICITANTE").HasMaxLength(64);
            e.Property(x => x.ScopeJson).HasColumnName("AMBITO_JSON").HasColumnType("longtext");
            e.Property(x => x.InitialMessageId).HasColumnName("CODIGO_MENSAJE_INICIAL").HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.Status).HasColumnName("ESTADO").HasMaxLength(32);
            e.Property(x => x.ErrorCode).HasColumnName("CODIGO_ERROR").HasMaxLength(100);
            e.Property(x => x.UpdatedAt).HasColumnName("FECHA_MODIFICACION");
            e.Property(x => x.Version).HasColumnName("VERSION").IsConcurrencyToken();
            e.HasIndex(x => new { x.ChurchCode, x.OwnerHash }).HasDatabaseName("IX_CARGA_IGLESIA_SOLICITANTE");
            e.HasIndex(x => x.LoadCode).IsUnique().HasDatabaseName("UQ_IMPORTACION_CARGA_CODIGO");
        });
        model.Entity<ImportRow>(e =>
        {
            e.ToTable("importacion_fila"); e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID_IMPORTACION_FILA").ValueGeneratedOnAdd();
            e.Property(x => x.RowCode).HasColumnName("CODIGO_FILA").HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.LoadCode).HasColumnName("CODIGO_CARGA").HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.RowNumber).HasColumnName("NUMERO_FILA");
            e.Property(x => x.PersonRequestId).HasColumnName("CODIGO_SOLICITUD_PERSONA").HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.MemberRequestId).HasColumnName("CODIGO_SOLICITUD_MIEMBRO").HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.PersonCode).HasColumnName("CODIGO_PERSONA").HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.MemberCode).HasColumnName("CODIGO_MIEMBRO").HasConversion<string>().HasMaxLength(36);
            e.Property(x => x.PersonRequestJson).HasColumnName("SOLICITUD_PERSONA_JSON").HasColumnType("longtext");
            e.Property(x => x.MemberRequestJson).HasColumnName("SOLICITUD_MIEMBRO_JSON").HasColumnType("longtext");
            e.Property(x => x.Status).HasColumnName("ESTADO").HasMaxLength(32);
            e.Property(x => x.ErrorCode).HasColumnName("CODIGO_ERROR").HasMaxLength(100);
            e.Property(x => x.ErrorMessage).HasColumnName("MENSAJE_ERROR").HasMaxLength(400);
            e.Property(x => x.UpdatedAt).HasColumnName("FECHA_MODIFICACION");
            e.Property(x => x.Version).HasColumnName("VERSION").IsConcurrencyToken();
            e.HasIndex(x => new { x.LoadCode, x.RowNumber }).IsUnique().HasDatabaseName("UQ_FILA_CARGA_NUMERO");
            e.HasIndex(x => new { x.Status, x.UpdatedAt }).HasDatabaseName("IX_FILA_ESTADO_MODIFICACION");
            e.HasIndex(x => x.RowCode).IsUnique().HasDatabaseName("UQ_IMPORTACION_FILA_CODIGO");
        });
    }
}
