using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;

/// <summary>Prepara acceso únicamente después de verificar la sesión y su iglesia.</summary>
public sealed class CreateImportProgressTicketCommandHandler(
    IImportActor actor,
    IImportProgressTicketIssuer issuer,
    IMapper mapper
)
    : IRequestHandler<
        CreateImportProgressTicketCommand,
        Result<CreateImportProgressTicketResponseDto>
    >
{
    private readonly IImportActor _actor = actor ?? throw new ArgumentNullException(nameof(actor));
    private readonly IImportProgressTicketIssuer _issuer =
        issuer ?? throw new ArgumentNullException(nameof(issuer));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    /// <inheritdoc />
    public Task<Result<CreateImportProgressTicketResponseDto>> Handle(
        CreateImportProgressTicketCommand command,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var grant = _issuer.Issue(_actor.GetRequester(command.ChurchCode));
        return Task.FromResult(
            grant is null
                ? Result.Failure<CreateImportProgressTicketResponseDto>(
                    new Error(
                        "IMPORT_PROGRESS_UNAVAILABLE",
                        "No se pudo iniciar el seguimiento. Vuelve a intentarlo.",
                        ErrorType.Unavailable
                    )
                )
                : Result<CreateImportProgressTicketResponseDto>.Success(
                    _mapper.Map<CreateImportProgressTicketResponseDto>(grant)
                )
        );
    }
}
