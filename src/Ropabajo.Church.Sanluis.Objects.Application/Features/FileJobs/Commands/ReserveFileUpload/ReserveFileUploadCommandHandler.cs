using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ReserveFileUpload;

/// <summary>
/// Coordina el caso ReserveFileUpload mediante identidades verificadas y el ciclo de trabajos.
/// </summary>
public sealed class ReserveFileUploadCommandHandler : IRequestHandler<ReserveFileUploadCommand, Result<ReserveFileUploadResponseDto>>
{
    private readonly FileJobService _service;
    private readonly IFileJobActor _actor;
    private readonly IMapper _mapper;

    /// <summary>
    /// Inicializa los colaboradores del caso de uso.
    /// </summary>
    public ReserveFileUploadCommandHandler(FileJobService service, IFileJobActor actor, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _actor = actor ?? throw new ArgumentNullException(nameof(actor));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Ejecuta el caso y mapea únicamente su valor exitoso.
    /// </summary>
    public async Task<Result<ReserveFileUploadResponseDto>> Handle(ReserveFileUploadCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.UploadAsync(_actor.GetProducer(), command.JobId, command.AttemptId, command.LeaseToken, _mapper.Map<FileUploadRequest>(command), cancellationToken);
        if (result.IsFailure) return Result.Failure<ReserveFileUploadResponseDto>(result.Errors.ToArray());
        return _mapper.Map<ReserveFileUploadResponseDto>(result.Value);
    }
}
