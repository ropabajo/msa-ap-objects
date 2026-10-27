using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.ListFileJobs;

/// <summary>
/// Centraliza los mapeos del caso ListFileJobs.
/// </summary>
public sealed class ListFileJobsMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public ListFileJobsMappingProfile()
    {
        CreateMap<ListFileJobsRequestDto, ListFileJobsQuery>();
        CreateMap<FileJobPage, ListFileJobsResponseDto>();
    }
}
