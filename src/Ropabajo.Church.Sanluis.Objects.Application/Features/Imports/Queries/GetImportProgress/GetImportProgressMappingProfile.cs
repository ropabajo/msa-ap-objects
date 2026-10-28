using AutoMapper;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;

/// <summary>Enlaza únicamente los identificadores proporcionados por el navegador.</summary>
public sealed class GetImportProgressMappingProfile : Profile
{
    /// <summary>La autoridad se asigna después desde la sesión validada.</summary>
    public GetImportProgressMappingProfile()
    {
        CreateMap<GetImportProgressRequestDto, GetImportProgressQuery>()
            .ForMember(x => x.Requester, x => x.Ignore());
    }
}
