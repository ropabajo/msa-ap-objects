using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.GetFileDownload;

/// <summary>
/// Centraliza los mapeos del caso GetFileDownload.
/// </summary>
public sealed class GetFileDownloadMappingProfile : Profile
{
    /// <summary>
    /// Registra contratos HTTP, MediatR y respuesta propios del caso.
    /// </summary>
    public GetFileDownloadMappingProfile()
    {
        CreateMap<GetFileDownloadRequestDto, GetFileDownloadCommand>();
        CreateMap<FileDownload, GetFileDownloadResponseDto>();
    }
}
