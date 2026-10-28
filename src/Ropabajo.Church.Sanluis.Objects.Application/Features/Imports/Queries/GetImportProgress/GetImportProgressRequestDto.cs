namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;

/// <summary>Identificadores visibles en la página; el navegador no decide usuario ni iglesia.</summary>
public sealed class GetImportProgressRequestDto
{
    /// <summary>Códigos públicos, sin duplicados; máximo cincuenta.</summary>
    public Guid[] LoadCodes { get; set; } = [];
}
