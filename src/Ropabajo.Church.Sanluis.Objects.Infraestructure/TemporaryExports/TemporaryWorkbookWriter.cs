using System.Globalization;
using ClosedXML.Excel;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.TemporaryExports;

/// <summary>Renderiza XLSX genérico sin fórmulas, macros, credenciales ni datos de negocio adicionales.</summary>
public sealed class TemporaryWorkbookWriter : ITemporaryWorkbookWriter
{
    /// <inheritdoc />
    public byte[] Write(ExportWorkbookSource source, DateTimeOffset generatedAt, CancellationToken token)
    {
        using var book = new XLWorkbook(); var sheet = book.AddWorksheet("Exportación"); var columns = source.Columns.Count;
        sheet.Cell(1, 1).Value = source.Title; sheet.Range(1, 1, 1, columns).Merge().Style.Font.SetBold();
        sheet.Cell(2, 1).Value = $"Datos consultados: {source.DataAsOf.ToOffset(TimeSpan.FromHours(-5)):dd/MM/yyyy HH:mm:ss} (Perú) · Emitido: {generatedAt.ToOffset(TimeSpan.FromHours(-5)):dd/MM/yyyy HH:mm:ss}";
        sheet.Range(2, 1, 2, columns).Merge();
        for (var column = 0; column < columns; column++)
        {
            sheet.Cell(4, column + 1).Value = source.Columns[column].Title;
            sheet.Column(column + 1).Width = source.Columns[column].Width;
            if (source.Columns[column].Kind == "date") sheet.Column(column + 1).Style.DateFormat.Format = "dd/MM/yyyy";
        }
        for (var row = 0; row < source.Rows.Count; row++)
        {
            token.ThrowIfCancellationRequested();
            for (var column = 0; column < columns; column++)
            {
                var value = source.Rows[row][column]; if (value is null) continue;
                var cell = sheet.Cell(row + 5, column + 1);
                switch (source.Columns[column].Kind)
                {
                    case "date": cell.Value = DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToDateTime(TimeOnly.MinValue); break;
                    case "integer": cell.Value = long.Parse(value, CultureInfo.InvariantCulture); break;
                    default: cell.Value = value; break; // Never use FormulaA1, including values beginning with '='.
                }
            }
        }
        sheet.Range(4, 1, Math.Max(4, source.Rows.Count + 4), columns).CreateTable().Theme = XLTableTheme.TableStyleMedium2;
        sheet.SheetView.FreezeRows(4);
        using var output = new MemoryStream(); book.SaveAs(output); token.ThrowIfCancellationRequested(); return output.ToArray();
    }
}
