using AutoMapper;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateCombinedImport;

/// <summary>
/// Registra mapas propios de entrada y respuesta del caso.
/// </summary>
public sealed class CreateCombinedImportMappingProfile : Profile
{
    /// <summary>
    /// Configura los mapas sin copiar autoridad del cuerpo HTTP.
    /// </summary>
    public CreateCombinedImportMappingProfile()
    {
        CreateMap<CreateCombinedImportRequestDto, CreateCombinedImportCommand>();
        CreateMap<ImportLoadView, CreateCombinedImportResponseDto>()
            .ForMember(x => x.LoadCode, x => x.MapFrom(y => y.Metadata.LoadCode))
            .ForMember(x => x.Status, x => x.MapFrom(y => y.Metadata.Status));
    }
}
