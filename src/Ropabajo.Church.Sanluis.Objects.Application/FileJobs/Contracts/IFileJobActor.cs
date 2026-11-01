using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;

/// <summary>
/// Obtiene identidades verificadas del transporte sin exponerlo en Application.
/// </summary>
public interface IFileJobActor
{
    /// <summary>
    /// Resuelve el usuario y su iglesia activa desde la sesión validada.
    /// </summary>
    FileJobRequester GetRequester(Guid churchCode);

    /// <summary>
    /// Resuelve el generador desde una identidad técnica de audiencia y cliente permitidos.
    /// </summary>
    string GetProducer();
}
