using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.Core;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Api.Controllers;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;
using Ropabajo.Church.Sanluis.Objects.Application.Photos;
using Xunit;

namespace Ropabajo.Church.Sanluis.Objects.Api.Tests;

public sealed class PhotoObjectHttpContractTests
{
    [Fact]
    public async Task Http_keeps_headers_and_empty_success_bodies_and_preserves_validation_with_the_real_result_filter()
    {
        var builder = WebApplication.CreateBuilder(); builder.WebHost.UseTestServer(); builder.Logging.ClearProviders();
        builder.Services.AddAuthentication("fixture").AddScheme<AuthenticationSchemeOptions, FixtureAuthentication>("fixture", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddControllers(options => options.Filters.Add<ResultFilter>()).AddApplicationPart(typeof(ObjectController).Assembly);
        builder.Services.AddScoped<INotificationHandler<Header>, HeaderHandler>();
        builder.Services.AddScoped<INotificationHandler<Notification>, NotificationHandler>();
        builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<CreateObjectMappingProfile>(); cfg.AddProfile<UploadObjectMappingProfile>(); });
        var code = Guid.NewGuid(); var reject = false; UploadObjectCommand? confirmed = null;
        var bus = DispatchProxy.Create<IMediatorBus, ReservedExportPathsTests.Proxy>();
        ((ReservedExportPathsTests.Proxy)(object)bus).Call = (method, args) => args[0] switch
        {
            CreateObjectCommand => Task.FromResult(Result<CreateObjectResponseDto>.Success(new() { Headers = new()
                { ["url"] = "https://storage.test/bucket/", ["sanluis-object-code"] = code.ToString("D"), ["x-form-data-policy"] = "fixture-policy" } })),
            UploadObjectCommand command => Confirm(command),
            _ => throw new InvalidOperationException(method.Name)
        };
        Task<Result<UploadObjectResponseDto>> Confirm(UploadObjectCommand command)
        {
            confirmed = command;
            return Task.FromResult(reject ? Result.Failure<UploadObjectResponseDto>(PhotoUploadErrors.Invalid)
                : Result<UploadObjectResponseDto>.Success(new() { Uploaded = true }));
        }
        builder.Services.AddSingleton(bus);
        await using var app = builder.Build(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync();
        using var client = app.GetTestClient();
        using var created = await client.PostAsJsonAsync("/v1/sanluis-objects", new { path = "photos", fileName = "foto.jpeg", allowedExtensions = ".jpg,.jpeg,.png,.webp", maxLength = 5, expiration = 300 });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(code.ToString("D"), created.Headers.GetValues("sanluis-object-code").Single());
        Assert.Contains("x-form-data-policy", string.Join(',', created.Headers.GetValues("Access-Control-Expose-Headers")));
        Assert.Equal("", await created.Content.ReadAsStringAsync());
        using var success = await client.PatchAsync($"/v1/sanluis-objects/{code:D}/state/uploaded", null);
        Assert.Equal(HttpStatusCode.OK, success.StatusCode); Assert.Equal("", await success.Content.ReadAsStringAsync());
        Assert.Equal(code.ToString("D"), confirmed!.ObjectCode);
        reject = true;
        using var failure = await client.PatchAsync($"/v1/sanluis-objects/{code:D}/state/uploaded", null);
        Assert.Equal(HttpStatusCode.BadRequest, failure.StatusCode);
        using var body = JsonDocument.Parse(await failure.Content.ReadAsStringAsync());
        Assert.Equal("PHOTO_FILE_INVALID", body.RootElement.GetProperty("errors")[0].GetProperty("code").GetString());
        Assert.Contains("Selecciona otra foto", body.RootElement.GetProperty("errors")[0].GetProperty("message").GetString());
    }
    private sealed class FixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "isolated-fixture")], "fixture")), "fixture")));
    }
}
