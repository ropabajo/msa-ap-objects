using AutoMapper;
using Ropabajo.Church.Sanluis.Objects.Application.Photos;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;
/// <summary>
/// Mapas propios de la reserva de archivos.
/// </summary>
public sealed class CreateObjectMappingProfile : Profile
{
    /// <summary>
    /// Registra el contrato de entrada y las cabeceras producidas por almacenamiento.
    /// </summary>
    public CreateObjectMappingProfile()
    {
        CreateMap<CreateObjectRequestDto, CreateObjectCommand>();
        CreateMap<ObjectUploadGrant, CreateObjectResponseDto>();
    }
}
