using AutoMapper;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImportRows;

/// <summary>
/// Registra mapas propios de entrada y respuesta del caso.
/// </summary>
public sealed class ListCombinedImportRowsMappingProfile : Profile
{
    /// <summary>
    /// Configura los mapas sin copiar autoridad del cuerpo HTTP.
    /// </summary>
    public ListCombinedImportRowsMappingProfile()
    {
        CreateMap<ListCombinedImportRowsRequestDto, ListCombinedImportRowsQuery>();
        CreateMap<Ropabajo.Church.Sanluis.Objects.Domain.Imports.ImportRow, ListCombinedImportRowsItemResponseDto>()
            .ForMember(x => x.BulkLoadResultCode, x => x.MapFrom(y => y.RowCode))
            .ForMember(x => x.BulkLoadCode, x => x.MapFrom(y => y.LoadCode))
            .ForMember(x => x.StateCode, x => x.MapFrom(y => y.Status == "completed" ? "COMPLETADO" : y.Status.EndsWith("observed") ? "OBSERVADO" : "PROCESADO"))
            .ForMember(x => x.Observation, x => x.MapFrom(y => y.ErrorMessage))
            .ForMember(x => x.CanRetry, x => x.MapFrom(y => y.Status == "member_observed" || y.Status == "person_observed"));
    }
}
