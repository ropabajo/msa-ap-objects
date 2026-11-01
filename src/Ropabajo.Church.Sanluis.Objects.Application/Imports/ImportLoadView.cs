using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Ropabajo.Church.Sanluis.Objects.Domain.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Une la carga existente con su seguimiento autorizado para mapear respuestas.
/// </summary>
/// <param name="Load">Carga con conteos y datos del archivo.</param>
/// <param name="Metadata">Ámbito y estado del flujo combinado.</param>
public sealed record ImportLoadView(BulkLoad Load, ImportLoad Metadata);
