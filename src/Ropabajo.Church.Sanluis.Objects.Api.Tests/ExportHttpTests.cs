using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ropabajo.Church.Sanluis.Objects.Api.Controllers;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.TemporaryExports;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Xunit;
namespace Ropabajo.Church.Sanluis.Objects.Api.Tests;

public class ExportHttpTests
{
    private static readonly Guid Church = Guid.Parse("8cbddf7f-a8ab-46a0-a832-373f454357cd");
    private static string Path(Guid? church = null) => $"/v1/church/{church ?? Church}/exports";
    [Fact]
    public async Task Routes_require_authentication_and_active_church_and_never_return_binary_workbooks()
    {
        var fixture = new Fixture(); await using var app = await Host(fixture);
        var client = app.GetTestClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(Path(), new ReserveExportRequest(Guid.NewGuid(), new string('a', 64), "members-v1"))).StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", "user-a");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync(Path(Guid.NewGuid()), new ReserveExportRequest(Guid.NewGuid(), new string('a', 64), "members-v1"))).StatusCode);
        var requestId = Guid.NewGuid();
        var response = await client.PostAsJsonAsync(Path(), new ReserveExportRequest(requestId, new string('a', 64), "members-v1"));
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode); Assert.Equal("application/json", response.Content.Headers.ContentType!.MediaType);
        var job = (await response.Content.ReadFromJsonAsync<TemporaryExportResponse>())!;
        Assert.Equal(job.ExportId, (await (await client.PostAsJsonAsync(Path(), new ReserveExportRequest(requestId, new string('a', 64), "members-v1"))).Content.ReadFromJsonAsync<TemporaryExportResponse>())!.ExportId);
        Assert.Equal(HttpStatusCode.Accepted, (await client.PutAsJsonAsync(Path() + $"/{job.ExportId}/source", fixture.Source())).StatusCode);
        fixture.Options.MaxSourceBytes = 1024;
        var oversized = await client.PutAsync(Path() + $"/{job.ExportId}/source", new StringContent(new string(' ', 1025), System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversized.StatusCode);
        await app.Services.GetRequiredService<TemporaryExportCoordinator>().ReconcileAsync(null, default);
        var ready = await client.GetFromJsonAsync<TemporaryExportResponse>(Path() + $"/{job.ExportId}");
        Assert.Equal("ready", ready!.Status);
        var file = await client.PostAsJsonAsync(Path() + $"/{job.ExportId}/download", new { });
        Assert.Equal(HttpStatusCode.OK, file.StatusCode); Assert.Equal("application/json", file.Content.Headers.ContentType!.MediaType);
        Assert.StartsWith("https://minio.example.invalid/", (await file.Content.ReadFromJsonAsync<ExportDownloadResponse>())!.Url);
        client.DefaultRequestHeaders.Authorization = new("Bearer", "user-b");
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(Path() + $"/{job.ExportId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync(Path() + $"/{job.ExportId}/download", new { })).StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", "user-a"); fixture.Clock.Advance(TimeSpan.FromHours(24));
        Assert.Equal(HttpStatusCode.Gone, (await client.PostAsJsonAsync(Path() + $"/{job.ExportId}/download", new { })).StatusCode);
    }
    private static async Task<WebApplication> Host(Fixture fixture)
    {
        var builder = WebApplication.CreateBuilder(); builder.WebHost.UseTestServer(); builder.Logging.ClearProviders();
        builder.Services.AddControllers().AddApplicationPart(typeof(TemporaryExportController).Assembly);
        builder.Services.AddMediator(typeof(ReserveExportCommand).Assembly);
        builder.Services.AddSingleton<TemporaryExportCoordinator>(fixture.Service);
        builder.Services.AddSingleton<IOptions<TemporaryExportOptions>>(Options.Create(fixture.Options));
        builder.Services.AddScoped<TemporaryExportRequestLimitFilter>();
        builder.Services.AddHttpContextAccessor(); builder.Services.AddScoped<IExportActor, HttpExportActor>();
        builder.Services.AddScoped<TemporaryExportExceptionFilter>();
        builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuth>("test", _ => { });
        builder.Services.AddAuthorization(options => options.AddPolicy("ActiveChurchRoute", policy => policy.RequireAuthenticatedUser()
            .RequireAssertion(context => context.Resource is HttpContext http &&
                string.Equals(http.Request.RouteValues["churchCode"]?.ToString(), Church.ToString(), StringComparison.OrdinalIgnoreCase))));
        var app = builder.Build(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync(); return app;
    }
    private sealed class TestAuth(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = Request.Headers.Authorization.ToString();
            if (token is not ("Bearer user-a" or "Bearer user-b")) return Task.FromResult(AuthenticateResult.NoResult());
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity([
                new Claim("sub",token[7..]),new Claim("iss","fixture-issuer")], "test")), "test")));
        }
    }
}
