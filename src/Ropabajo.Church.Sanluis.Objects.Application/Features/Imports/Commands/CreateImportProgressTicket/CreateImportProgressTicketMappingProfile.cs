using AutoMapper;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;

/// <summary>
/// Mapas exclusivos de la solicitud y entrega técnica.
/// </summary>
public sealed class CreateImportProgressTicketMappingProfile : Profile
{
    /// <summary>
    /// Conserva la ruta autorizada y excluye datos de sesión del transporte.
    /// </summary>
    public CreateImportProgressTicketMappingProfile()
    {
        CreateMap<CreateImportProgressTicketRequestDto, CreateImportProgressTicketCommand>();
        CreateMap<ImportProgressTicket, CreateImportProgressTicketResponseDto>();
    }
}
