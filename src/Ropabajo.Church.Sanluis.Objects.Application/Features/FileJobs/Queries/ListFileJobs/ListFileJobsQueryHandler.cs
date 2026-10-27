using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.ListFileJobs;

/// <summary>
/// Coordina el caso ListFileJobs mediante identidades verificadas y el ciclo de trabajos.
/// </summary>
public sealed class ListFileJobsQueryHandler : IRequestHandler<ListFileJobsQuery, Result<ListFileJobsResponseDto>>
{
    private readonly FileJobService _service;
    private readonly IFileJobActor _actor;
    private readonly IMapper _mapper;

    /// <summary>
    /// Inicializa los colaboradores del caso de uso.
    /// </summary>
    public ListFileJobsQueryHandler(FileJobService service, IFileJobActor actor, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _actor = actor ?? throw new ArgumentNullException(nameof(actor));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Ejecuta el caso y mapea únicamente su valor exitoso.
    /// </summary>
    public async Task<Result<ListFileJobsResponseDto>> Handle(ListFileJobsQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.ListAsync(_actor.GetRequester(query.ChurchCode), query.Cursor, query.PageSize, cancellationToken);
        if (result.IsFailure) return Result.Failure<ListFileJobsResponseDto>(result.Errors.ToArray());
        return _mapper.Map<ListFileJobsResponseDto>(result.Value);
    }
}
