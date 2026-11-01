using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CreateFileJob;

/// <summary>
/// Centraliza los mapeos del caso CreateFileJob.
/// </summary>
public sealed class CreateFileJobMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public CreateFileJobMappingProfile()
    {
        CreateMap<CreateFileJobRequestDto, CreateFileJobCommand>();
        CreateMap<FileJobSummary, CreateFileJobResponseDto>();
    }
}
