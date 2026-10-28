using System.Security.Claims;
using System.Text.Json;
using Ropabajo.Churc.Sanluis.Framework.Realtime;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Api.Realtime;

internal sealed class ImportProgressTicketIssuer(
    IHttpContextAccessor accessor,
    ConnectionTicketStore tickets,
    TimeProvider clock
) : IImportProgressTicketIssuer
{
    private readonly IHttpContextAccessor _accessor =
        accessor ?? throw new ArgumentNullException(nameof(accessor));
    private readonly ConnectionTicketStore _tickets =
        tickets ?? throw new ArgumentNullException(nameof(tickets));
    private readonly TimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    internal const string RequesterClaim = "objects:import-progress-scope";

    internal static string Path(Guid church) => $"/v2/churches/{church:D}/imports/progress";

    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public ImportProgressTicket? Issue(ImportRequester requester)
    {
        var http = _accessor.HttpContext;
        if (http?.User.Identity?.IsAuthenticated != true)
            return null;
        var expiry = _clock.GetUtcNow().AddMinutes(5);
        if (long.TryParse(http.User.FindFirst("exp")?.Value, out var seconds))
        {
            var tokenExpiry = DateTimeOffset.FromUnixTimeSeconds(seconds);
            if (tokenExpiry < expiry)
                expiry = tokenExpiry;
        }
        var identity = new ClaimsIdentity(
            http.User.Claims.Where(x => x.Type != RequesterClaim),
            ConnectionTicketStore.Scheme
        );
        identity.AddClaim(new Claim(RequesterClaim, JsonSerializer.Serialize(requester, Json)));
        var grant = _tickets.Issue(
            new ClaimsPrincipal(identity),
            Path(requester.ChurchCode),
            http.Request.Headers.Origin.ToString(),
            expiry
        );
        return grant is null ? null : new ImportProgressTicket(grant.Value, grant.ExpiresAt);
    }
}
