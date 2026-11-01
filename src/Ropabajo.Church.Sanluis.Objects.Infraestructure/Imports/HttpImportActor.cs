using Microsoft.AspNetCore.Http;
using Ropabajo.Churc.Sanluis.Framework.Authz.Authorization;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Imports;

internal sealed class HttpImportActor(IFileJobActor actor, ICurrentContextAccessor context, IHttpContextAccessor http) : IImportActor
{
    public ImportRequester GetRequester(Guid churchCode)
    {
        var requester = actor.GetRequester(churchCode);
        if (!requester.Roles.Any(x => x is "ADMIN" or "USER_ADMIN")) throw new UnauthorizedAccessException("Import management role required.");
        var subject = context.KeycloakUserId;
        if (string.IsNullOrWhiteSpace(subject) || subject.Length > 50 || string.IsNullOrWhiteSpace(context.Username))
            throw new UnauthorizedAccessException("Import identity unavailable.");
        var ip = http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        return new ImportRequester(churchCode, requester.OwnerHash, subject, context.Username, ip.Length <= 40 ? ip : ip[..40]);
    }
}
