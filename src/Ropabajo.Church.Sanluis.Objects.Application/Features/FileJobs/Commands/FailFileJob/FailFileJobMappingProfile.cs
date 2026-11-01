using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.FailFileJob;

/// <summary>
/// Centraliza los mapeos del caso FailFileJob.
/// </summary>
public sealed class FailFileJobMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public FailFileJobMappingProfile()
    {
        CreateMap<FailFileJobRequestDto, FailFileJobCommand>();
        CreateMap<FileJobSummary, FailFileJobResponseDto>();
        CreateMap<FailFileJobCommand, FileFailure>();
    }
}
