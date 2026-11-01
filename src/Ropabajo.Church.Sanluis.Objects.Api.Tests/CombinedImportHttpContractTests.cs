using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ropabajo.Churc.Sanluis.Framework.Core;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Api.Controllers;
using Ropabajo.Church.Sanluis.Objects.Api.Extensions;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateCombinedImport;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.RetryCombinedImportRow;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImportRows;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImports;
using Xunit;

public sealed class CombinedImportHttpContractTests
{
    [Fact]
    public async Task Accepted_commands_keep_route_authority_and_the_documented_envelope()
    {
        var bus = new ProbeBus
        {
            Respond = message => message switch
            {
                CreateCombinedImportCommand command => Result<CreateCombinedImportResponseDto>.Success(
                    new() { LoadCode = command.RequestId, Status = "pending" }),
                RetryCombinedImportRowCommand => Result<RetryCombinedImportRowResponseDto>.Success(
                    new() { Accepted = true }),
                _ => throw new InvalidOperationException()
            }
        };
        await using var app = await StartAsync(bus);
        using var client = app.GetTestClient();
        var church = Guid.NewGuid();
        var load = Guid.NewGuid();
        var row = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var path = $"/v2/churches/{church:D}/imports";

        var created = await client.PostAsJsonAsync(path, new
        {
            churchCode = Guid.NewGuid(),
            requestId,
            objectCode = Guid.NewGuid(),
            formatCode = Guid.NewGuid(),
            description = "Carga de prueba"
        });
        Assert.Equal(HttpStatusCode.Accepted, created.StatusCode);
        var command = Assert.IsType<CreateCombinedImportCommand>(bus.LastRequest);
        Assert.Equal(church, command.ChurchCode);
        Assert.Equal(requestId, command.RequestId);
        using var createBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        Assert.True(createBody.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(requestId, createBody.RootElement.GetProperty("value").GetProperty("loadCode").GetGuid());
        Assert.Equal("pending", createBody.RootElement.GetProperty("value").GetProperty("status").GetString());

        var retried = await client.PostAsJsonAsync($"{path}/{load:D}/rows/{row:D}/retry", new
        {
            churchCode = Guid.NewGuid(),
            loadCode = Guid.NewGuid(),
            rowCode = Guid.NewGuid(),
            requestId
        });
        Assert.Equal(HttpStatusCode.Accepted, retried.StatusCode);
        var retry = Assert.IsType<RetryCombinedImportRowCommand>(bus.LastRequest);
        Assert.Equal(church, retry.ChurchCode);
        Assert.Equal(load, retry.LoadCode);
        Assert.Equal(row, retry.RowCode);
        Assert.Equal(requestId, retry.RequestId);
        using var retryBody = JsonDocument.Parse(await retried.Content.ReadAsStringAsync());
        Assert.True(retryBody.RootElement.GetProperty("value").GetProperty("accepted").GetBoolean());

        var createDescription = Descriptions(app).Single(description =>
            description.HttpMethod == "POST" && description.ActionDescriptor.RouteValues["action"] == "Create");
        Assert.Equal(typeof(Result<CreateCombinedImportResponseDto>),
            Assert.Single(createDescription.SupportedResponseTypes, response => response.StatusCode == 202).Type);
        Assert.DoesNotContain(createDescription.SupportedResponseTypes, response => response.StatusCode == 200);
    }

    [Fact]
    public async Task Queries_keep_route_scope_filters_pagination_and_empty_pages()
    {
        var bus = new ProbeBus
        {
            Respond = message => message switch
            {
                ListCombinedImportsQuery => Result<ListCombinedImportsResponseDto>.Success(new()),
                ListCombinedImportRowsQuery => Result<ListCombinedImportRowsResponseDto>.Success(new()),
                _ => throw new InvalidOperationException()
            }
        };
        await using var app = await StartAsync(bus);
        using var client = app.GetTestClient();
        var church = Guid.NewGuid();
        var load = Guid.NewGuid();
        var format = Guid.NewGuid();
        var path = $"/v2/churches/{church:D}/imports";

        var page = await client.GetAsync($"{path}?churchCode={Guid.NewGuid():D}&filterFormatCode={format:D}&pageNumber=2&pageSize=3");
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var query = Assert.IsType<ListCombinedImportsQuery>(bus.LastRequest);
        Assert.Equal(church, query.ChurchCode);
        Assert.Equal(format, query.FilterFormatCode);
        Assert.Equal(2, query.PageNumber);
        Assert.Equal(3, query.PageSize);
        using var body = JsonDocument.Parse(await page.Content.ReadAsStringAsync());
        Assert.Empty(body.RootElement.GetProperty("value").GetProperty("items").EnumerateArray());
        Assert.Equal(0, body.RootElement.GetProperty("value").GetProperty("total").GetInt32());

        var rows = await client.GetAsync($"{path}/{load:D}/rows?churchCode={Guid.NewGuid():D}&loadCode={Guid.NewGuid():D}&stateCode=OBSERVADO&pageNumber=4&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, rows.StatusCode);
        var rowQuery = Assert.IsType<ListCombinedImportRowsQuery>(bus.LastRequest);
        Assert.Equal(church, rowQuery.ChurchCode);
        Assert.Equal(load, rowQuery.LoadCode);
        Assert.Equal("OBSERVADO", rowQuery.StateCode);
        Assert.Equal(4, rowQuery.PageNumber);
        Assert.Equal(5, rowQuery.PageSize);
    }

    [Fact]
    public async Task Expected_failures_and_model_validation_keep_their_real_http_status()
    {
        var bus = new ProbeBus
        {
            Respond = _ => Result.Failure<CreateCombinedImportResponseDto>(
                new Error("IMPORT_INTENT_CONFLICT", "La solicitud ya fue utilizada.", ErrorType.Conflict))
        };
        await using var app = await StartAsync(bus);
        using var client = app.GetTestClient();
        var path = $"/v2/churches/{Guid.NewGuid():D}/imports";
        var failed = await client.PostAsJsonAsync(path, new
        {
            requestId = Guid.NewGuid(),
            objectCode = Guid.NewGuid(),
            formatCode = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.Conflict, failed.StatusCode);
        Assert.Equal("application/problem+json", failed.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await failed.Content.ReadAsStringAsync());
        Assert.Equal("IMPORT_INTENT_CONFLICT", problem.RootElement.GetProperty("errors")[0].GetProperty("code").GetString());
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));

        var sends = bus.Sends;
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync(path + "?pageNumber=0")).StatusCode);
        Assert.Equal(sends, bus.Sends);
    }

    [Theory]
    [InlineData("dependency", HttpStatusCode.ServiceUnavailable)]
    [InlineData("forbidden", HttpStatusCode.Forbidden)]
    [InlineData("too-large", HttpStatusCode.RequestEntityTooLarge)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task Relocated_exception_filter_keeps_safe_responses(string failure, HttpStatusCode expected)
    {
        const string privateDetail = "private-provider-detail-not-for-response";
        Exception exception = failure switch
        {
            "dependency" => new InvalidOperationException(privateDetail, new IOException(privateDetail)),
            "forbidden" => new UnauthorizedAccessException(privateDetail),
            "too-large" => new Microsoft.AspNetCore.Http.BadHttpRequestException(privateDetail, 413),
            _ => new InvalidOperationException(privateDetail)
        };
        var bus = new ProbeBus { Respond = _ => throw exception };
        await using var app = await StartAsync(bus);
        using var client = app.GetTestClient();
        var response = await client.GetAsync($"/v2/churches/{Guid.NewGuid():D}/imports");
        Assert.Equal(expected, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(privateDetail, text);
        using var body = JsonDocument.Parse(text);
        Assert.Equal((int)expected, body.RootElement.GetProperty("status").GetInt32());
    }

    private static IEnumerable<ApiDescription> Descriptions(WebApplication app) =>
        app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
            .ApiDescriptionGroups.Items.SelectMany(group => group.Items)
            .Where(description => description.ActionDescriptor.RouteValues["controller"] == "CombinedImport");

    private static async Task<WebApplication> StartAsync(ProbeBus bus)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        // These fixtures test transport, not IAM. No user token or remote dependency is used.
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ActiveChurchRoute", policy => policy.RequireAssertion(_ => true));
            options.AddPolicy("IamAdmin", policy => policy.RequireAssertion(_ => true));
        });
        builder.Services.AddSingleton<IMediatorBus>(bus);
        builder.Services.AddAutoMapper(config =>
        {
            config.AddProfile<CreateCombinedImportMappingProfile>();
            config.AddProfile<RetryCombinedImportRowMappingProfile>();
            config.AddProfile<ListCombinedImportsMappingProfile>();
            config.AddProfile<ListCombinedImportRowsMappingProfile>();
        });
        builder.Services.AddObjectsApi(new ConfigurationBuilder().Build(), new ImportFixtureEnvironment());
        builder.Services.AddControllers(options => options.Filters.Add<ResultFilter>())
            .AddApplicationPart(typeof(CombinedImportController).Assembly);
        var app = builder.Build();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        return app;
    }

    private sealed class ProbeBus : IMediatorBus
    {
        public Func<object, object> Respond = _ => throw new InvalidOperationException();
        public object? LastRequest;
        public int Sends;

        public Task<T> SendAsync<T>(IRequest<T> request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            Sends++;
            return Task.FromResult((T)Respond(request));
        }

        public Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : Command =>
            throw new NotSupportedException();

        public Task RaiseAsync<T>(T @event, CancellationToken cancellationToken = default) where T : Event =>
            throw new NotSupportedException();
    }
}
