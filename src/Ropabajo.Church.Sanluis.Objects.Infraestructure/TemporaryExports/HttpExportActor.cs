using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.TemporaryExports;

/// <summary>Obtiene el propietario únicamente del JWT validado y nunca de parámetros enviados por el cliente.</summary>
public sealed class HttpExportActor(IHttpContextAccessor accessor) : IExportActor
{
    /// <inheritdoc />
    public ExportScope GetScope(Guid churchCode)
    {
        var user = accessor.HttpContext?.User;
        var subject = user?.FindFirstValue("sub") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var issuer = user?.FindFirstValue("iss") ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Issuer;
        if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(subject) || churchCode == Guid.Empty)
            throw new ExportFlowException("missing", "Se requiere una sesión y una iglesia activa.");
        var owner = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(issuer + "\0" + subject))).ToLowerInvariant();
        return new(churchCode, owner);
    }
}
