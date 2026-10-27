using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CreateFileJob;

/// <summary>
/// Coordina el caso CreateFileJob mediante identidades verificadas y el ciclo de trabajos.
/// </summary>
public sealed class CreateFileJobCommandHandler : IRequestHandler<CreateFileJobCommand, Result<CreateFileJobResponseDto>>
{
    private readonly FileJobService _service;
    private readonly IFileJobActor _actor;
    private readonly IMapper _mapper;

    /// <summary>
    /// Inicializa los colaboradores del caso de uso.
    /// </summary>
    public CreateFileJobCommandHandler(FileJobService service, IFileJobActor actor, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _actor = actor ?? throw new ArgumentNullException(nameof(actor));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Ejecuta el caso y mapea únicamente su valor exitoso.
    /// </summary>
    public async Task<Result<CreateFileJobResponseDto>> Handle(CreateFileJobCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(_actor.GetRequester(command.ChurchCode), command.RequestId, command.ReportType, command.ContractVersion, command.Parameters, command.ForceRefresh, cancellationToken);
        if (result.IsFailure) return Result.Failure<CreateFileJobResponseDto>(result.Errors.ToArray());
        return _mapper.Map<CreateFileJobResponseDto>(result.Value);
    }
}
