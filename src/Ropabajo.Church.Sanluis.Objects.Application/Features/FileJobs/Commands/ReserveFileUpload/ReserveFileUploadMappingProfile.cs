using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ReserveFileUpload;

/// <summary>
/// Centraliza los mapeos del caso ReserveFileUpload.
/// </summary>
public sealed class ReserveFileUploadMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public ReserveFileUploadMappingProfile()
    {
        CreateMap<ReserveFileUploadRequestDto, ReserveFileUploadCommand>();
        CreateMap<FileUploadGrant, ReserveFileUploadResponseDto>();
        CreateMap<ReserveFileUploadCommand, FileUploadRequest>();
    }
}
