using AutoMapper;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImports;

/// <summary>
/// Registra mapas propios de entrada y respuesta del caso.
/// </summary>
public sealed class ListCombinedImportsMappingProfile : Profile
{
    /// <summary>
    /// Configura los mapas sin copiar autoridad del cuerpo HTTP.
    /// </summary>
    public ListCombinedImportsMappingProfile()
    {
        CreateMap<ListCombinedImportsRequestDto, ListCombinedImportsQuery>();
        CreateMap<ImportLoadView, ListCombinedImportsItemResponseDto>()
            .IncludeMembers(x => x.Load, x => x.Metadata);
        CreateMap<Ropabajo.Church.Sanluis.Objects.Domain.Entities.BulkLoad, ListCombinedImportsItemResponseDto>(MemberList.None)
            .ForMember(x => x.BulkLoadCode, x => x.MapFrom(y => y.Code));
        CreateMap<Ropabajo.Church.Sanluis.Objects.Domain.Imports.ImportLoad, ListCombinedImportsItemResponseDto>(MemberList.None)
            .ForMember(x => x.UpdatedAt, x => x.MapFrom(y => DateTime.SpecifyKind(y.UpdatedAt, DateTimeKind.Utc)));
    }
}
