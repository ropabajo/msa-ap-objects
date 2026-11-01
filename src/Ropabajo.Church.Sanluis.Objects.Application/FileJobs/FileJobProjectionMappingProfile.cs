using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs;

/// <summary>
/// Centraliza la proyección de persistencia hacia contratos técnicos sin datos privados.
/// </summary>
public sealed class FileJobProjectionMappingProfile : Profile
{
    /// <summary>
    /// Registra fechas UTC y archivos publicados del trabajo.
    /// </summary>
    public FileJobProjectionMappingProfile()
    {
        CreateMap<DateTime, DateTimeOffset>().ConvertUsing(value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
        CreateMap<FileJobArtifact, FileArtifactSummary>();
        CreateMap<FileJob, FileJobSummary>()
            .ForMember(value => value.Reused, options => options.Ignore())
            .ForMember(value => value.Artifacts, options => options.MapFrom(source => source.Artifacts.Where(artifact => artifact.Published)));
    }
}
