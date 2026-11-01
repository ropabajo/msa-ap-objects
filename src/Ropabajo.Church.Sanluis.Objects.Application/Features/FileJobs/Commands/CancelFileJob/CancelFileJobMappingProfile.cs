using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CancelFileJob;

/// <summary>
/// Centraliza los mapeos del caso CancelFileJob.
/// </summary>
public sealed class CancelFileJobMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public CancelFileJobMappingProfile()
    {
        CreateMap<CancelFileJobRequestDto, CancelFileJobCommand>();
        CreateMap<FileJobSummary, CancelFileJobResponseDto>();
    }
}
