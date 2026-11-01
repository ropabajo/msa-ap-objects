using AutoMapper;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.RetryCombinedImportRow;

/// <summary>
/// Registra mapas propios de entrada y respuesta del caso.
/// </summary>
public sealed class RetryCombinedImportRowMappingProfile : Profile
{
    /// <summary>
    /// Configura los mapas sin copiar autoridad del cuerpo HTTP.
    /// </summary>
    public RetryCombinedImportRowMappingProfile()
    {
        CreateMap<RetryCombinedImportRowRequestDto, RetryCombinedImportRowCommand>();
    }
}
