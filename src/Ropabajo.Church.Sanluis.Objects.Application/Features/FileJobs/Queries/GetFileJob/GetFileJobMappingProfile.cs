using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.GetFileJob;

/// <summary>
/// Centraliza los mapeos del caso GetFileJob.
/// </summary>
public sealed class GetFileJobMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public GetFileJobMappingProfile()
    {
        CreateMap<GetFileJobRequestDto, GetFileJobQuery>();
        CreateMap<FileJobSummary, GetFileJobResponseDto>();
    }
}
