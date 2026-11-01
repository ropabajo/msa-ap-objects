using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CompleteFileJob;

/// <summary>
/// Centraliza los mapeos del caso CompleteFileJob.
/// </summary>
public sealed class CompleteFileJobMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public CompleteFileJobMappingProfile()
    {
        CreateMap<CompleteFileJobRequestDto, CompleteFileJobCommand>();
        CreateMap<FileJobSummary, CompleteFileJobResponseDto>();
        CreateMap<CompleteFileJobCommand, FileCompletion>();
    }
}
