using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.HeartbeatFileJob;

/// <summary>
/// Centraliza los mapeos del caso HeartbeatFileJob.
/// </summary>
public sealed class HeartbeatFileJobMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public HeartbeatFileJobMappingProfile()
    {
        CreateMap<HeartbeatFileJobRequestDto, HeartbeatFileJobCommand>();
        CreateMap<FileJobSummary, HeartbeatFileJobResponseDto>();
    }
}
