using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateCombinedImport;

/// <summary>
/// Ejecuta el caso usando la identidad autenticada y el ámbito de iglesia de la ruta.
/// </summary>
public sealed class CreateCombinedImportCommandHandler : IRequestHandler<CreateCombinedImportCommand, Result<CreateCombinedImportResponseDto>>
{
    private readonly CombinedImportService _service;
    private readonly IImportActor _actor;
    private readonly IImportRepository _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Recibe los colaboradores del caso de importación.
    /// </summary>
    public CreateCombinedImportCommandHandler(CombinedImportService service, IImportActor actor, IImportRepository repository, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service)); _actor = actor ?? throw new ArgumentNullException(nameof(actor));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository)); _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc />
    public async Task<Result<CreateCombinedImportResponseDto>> Handle(CreateCombinedImportCommand command, CancellationToken cancellationToken)
    {
        var requester = _actor.GetRequester(command.ChurchCode);
        var result = await _service.CreateAsync(requester, command.RequestId, command.ObjectCode, command.FormatCode, command.Description, cancellationToken);
        if (result.IsFailure) return Result.Failure<CreateCombinedImportResponseDto>(result.Errors.ToArray());
        return _mapper.Map<CreateCombinedImportResponseDto>(result.Value);
    }
}
