using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ClaimFileJob;

/// <summary>
/// Centraliza los mapeos del caso ClaimFileJob.
/// </summary>
public sealed class ClaimFileJobMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public ClaimFileJobMappingProfile()
    {
        CreateMap<ClaimFileJobRequestDto, ClaimFileJobCommand>();
        CreateMap<FileJobWorkItem, ClaimFileJobResponseDto>();
    }
}
