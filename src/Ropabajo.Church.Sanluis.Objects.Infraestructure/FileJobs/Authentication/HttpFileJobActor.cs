using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.Authz.Authorization;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Authentication;

/// <summary>
/// Obtiene ámbitos de usuario y generador exclusivamente de autenticación verificada.
/// </summary>
public sealed class HttpFileJobActor : IFileJobActor
{
    private readonly IHttpContextAccessor _http;
    private readonly ICurrentContextAccessor _context;
    private readonly FileJobsRuntimeSettings _settings;

    /// <summary>
    /// Inicializa los accesores de identidades sin leer datos del body.
    /// </summary>
    public HttpFileJobActor(IHttpContextAccessor http, ICurrentContextAccessor context, IOptions<FileJobsRuntimeSettings> settings)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _settings = (settings ?? throw new ArgumentNullException(nameof(settings))).Value;
    }

    /// <inheritdoc />
    public FileJobRequester GetRequester(Guid churchCode)
    {
        var user = _http.HttpContext?.User;
        var subject = user?.FindFirstValue("sub") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var issuer = user?.FindFirstValue("iss") ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Issuer;
        if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(issuer)
            || churchCode == Guid.Empty || !Guid.TryParse(_context.ChurchCode, out var activeChurch) || activeChurch != churchCode)
            throw new UnauthorizedAccessException("File job scope is not authorized.");
        var owner = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(issuer + "\0" + subject))).ToLowerInvariant();
        return new FileJobRequester(churchCode, owner, _context.EffectiveRoles.ToArray());
    }

    /// <inheritdoc />
    public string GetProducer()
    {
        var user = _http.HttpContext?.User;
        var clientId = user?.FindFirstValue("azp");
        if (user?.Identity?.IsAuthenticated != true || clientId is null || !_settings.WorkerClients.TryGetValue(clientId, out var producer))
            throw new UnauthorizedAccessException("File generator identity is not authorized.");
        return producer;
    }
}
