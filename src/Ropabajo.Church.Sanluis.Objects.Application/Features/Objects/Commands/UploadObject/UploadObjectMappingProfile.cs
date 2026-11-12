using AutoMapper;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;
/// <summary>
/// Enlaza el identificador de ruta con el comando de confirmación.
/// </summary>
public sealed class UploadObjectMappingProfile : Profile
{
    /// <summary>
    /// Registra el mapa local de entrada.
    /// </summary>
    public UploadObjectMappingProfile() => CreateMap<UploadObjectRequestDto, UploadObjectCommand>();
}
