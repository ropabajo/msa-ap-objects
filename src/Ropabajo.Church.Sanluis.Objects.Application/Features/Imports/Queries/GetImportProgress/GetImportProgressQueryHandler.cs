using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;

/// <summary>
/// Comprueba propiedad e iglesia antes de entregar contadores.
/// </summary>
public sealed class GetImportProgressQueryHandler(IImportProgressReader reader)
    : IRequestHandler<GetImportProgressQuery, Result<GetImportProgressResponseDto>>
{
    private readonly IImportProgressReader _reader =
        reader ?? throw new ArgumentNullException(nameof(reader));

    /// <inheritdoc />
    public async Task<Result<GetImportProgressResponseDto>> Handle(
        GetImportProgressQuery query,
        CancellationToken cancellationToken
    )
    {
        var rows = await _reader.ReadAsync(query.Requester, query.LoadCodes, cancellationToken);
        return rows.Count == query.LoadCodes.Length
            ? new GetImportProgressResponseDto { Items = rows }
            : Result.Failure<GetImportProgressResponseDto>(
                new Error(
                    ImportValidationCodes.NotFound,
                    ImportValidationMessages.NotFound,
                    ErrorType.NotFound
                )
            );
    }
}
