using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.Core;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Api.Controllers;
using Ropabajo.Church.Sanluis.Objects.Api.Extensions;
using Ropabajo.Church.Sanluis.Objects.Api.Realtime;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;
using Xunit;

public sealed class ImportProgressWebSocketTests
{
    private const string Origin = "https://portal.test";
    private static readonly Guid Church = Guid.NewGuid();

    private static string PathFor(Guid church) => $"/v2/churches/{church:D}/imports";

    [Fact]
    public async Task Ticket_requires_authentication_and_active_church_and_has_no_cache()
    {
        await using var app = await Start();
        using var http = app.GetTestClient();
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await http.PostAsync(PathFor(Church) + "/progress-ticket", null)).StatusCode
        );
        http.DefaultRequestHeaders.Add("Authorization", "Fixture");
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await http.PostAsync(PathFor(Guid.NewGuid()) + "/progress-ticket", null)).StatusCode
        );
        var response = await http.PostAsync(PathFor(Church) + "/progress-ticket", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Matches(
            "^[a-f0-9]{64}$",
            body.GetProperty("value").GetProperty("ticket").GetString()!
        );
        Assert.DoesNotContain("owner", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("replay")]
    [InlineData("origin")]
    [InlineData("church")]
    public async Task Ticket_cannot_be_reused_or_moved_to_another_origin_or_church(string scenario)
    {
        await using var app = await Start();
        var ticket = await Ticket(app);
        if (scenario == "replay")
        {
            using var first = await Connect(app, ticket);
            await Send(first, "{\"protocol\":\"json\",\"version\":1}");
            await Receive(first);
        }
        var client = app.GetTestServer().CreateWebSocketClient();
        client.ConfigureRequest = request =>
            request.Headers.Origin = scenario == "origin" ? "https://other.test" : Origin;
        var path = PathFor(scenario == "church" ? Guid.NewGuid() : Church);
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            using var socket = await client.ConnectAsync(
                new Uri($"ws://localhost{path}/progress?ticket={ticket}"),
                default
            );
        });
    }

    [Fact]
    public async Task Confirmed_versions_are_pushed_and_reconnection_receives_latest_snapshot()
    {
        await using var app = await Start();
        var reader = app.Services.GetRequiredService<Reader>();
        using var socket = await Connect(app, await Ticket(app));
        await Send(socket, "{\"protocol\":\"json\",\"version\":1}");
        Assert.Equal("{}", await Receive(socket));
        await Subscribe(socket, reader.Load);
        var initial = await Until(socket, 1);
        Assert.Equal(
            0,
            initial.GetProperty("arguments")[0][0].GetProperty("uploadedRecords").GetInt32()
        );
        reader.Version = 2;
        var update = await Until(socket, 1);
        Assert.Equal(2, update.GetProperty("arguments")[0][0].GetProperty("version").GetInt64());
        Assert.Equal(
            1,
            update.GetProperty("arguments")[0][0].GetProperty("uploadedRecords").GetInt32()
        );
        Assert.DoesNotContain("owner", update.ToString());
        using var next = await Connect(app, await Ticket(app));
        await Send(next, "{\"protocol\":\"json\",\"version\":1}");
        await Receive(next);
        await Subscribe(next, reader.Load);
        Assert.Equal(
            2,
            (await Until(next, 1)).GetProperty("arguments")[0][0].GetProperty("version").GetInt64()
        );
    }

    [Fact]
    public async Task Subscribe_ignores_browser_authority_and_rejects_foreign_load_without_partial_data()
    {
        await using var app = await Start();
        var reader = app.Services.GetRequiredService<Reader>();
        using var socket = await Connect(app, await Ticket(app));
        await Send(socket, "{\"protocol\":\"json\",\"version\":1}");
        await Receive(socket);
        await Send(
            socket,
            JsonSerializer.Serialize(
                new
                {
                    type = 1,
                    invocationId = "1",
                    target = "Subscribe",
                    arguments = new[]
                    {
                        new
                        {
                            loadCodes = new[] { reader.Load, Guid.NewGuid() },
                            requester = new { ownerHash = "other", churchCode = Guid.NewGuid() },
                        },
                    },
                }
            )
        );
        var completion = await Until(socket, 3);
        Assert.True(completion.TryGetProperty("error", out _));
        Assert.Equal("owner", reader.LastRequester!.OwnerHash);
        Assert.Equal(Church, reader.LastRequester.ChurchCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(2)]
    public async Task Empty_oversized_or_duplicate_subscriptions_do_not_read_persistence(int size)
    {
        await using var app = await Start();
        var reader = app.Services.GetRequiredService<Reader>();
        using var socket = await Connect(app, await Ticket(app));
        await Send(socket, "{\"protocol\":\"json\",\"version\":1}");
        await Receive(socket);
        var ids =
            size == 2
                ? new[] { reader.Load, reader.Load }
                : Enumerable.Range(0, size).Select(_ => Guid.NewGuid()).ToArray();
        await Send(
            socket,
            JsonSerializer.Serialize(
                new
                {
                    type = 1,
                    invocationId = "1",
                    target = "Subscribe",
                    arguments = new[] { new { loadCodes = ids } },
                }
            )
        );
        Assert.True((await Until(socket, 3)).TryGetProperty("error", out _));
        Assert.Null(reader.LastRequester);
    }

    private static Task Subscribe(WebSocket socket, Guid load) =>
        Send(
            socket,
            JsonSerializer.Serialize(
                new
                {
                    type = 1,
                    invocationId = "1",
                    target = "Subscribe",
                    arguments = new[] { new { loadCodes = new[] { load } } },
                }
            )
        );

    private static async Task Send(WebSocket socket, string message) =>
        await socket.SendAsync(
            Encoding.UTF8.GetBytes(message + "\u001e"),
            WebSocketMessageType.Text,
            true,
            default
        );

    private static async Task<string> Receive(WebSocket socket)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        var buffer = new byte[8192];
        var result = await socket.ReceiveAsync(buffer, timeout.Token);
        Assert.Equal(WebSocketMessageType.Text, result.MessageType);
        return Encoding.UTF8.GetString(buffer, 0, result.Count).TrimEnd('\u001e');
    }

    private static async Task<JsonElement> Until(WebSocket socket, int type)
    {
        for (var i = 0; i < 8; i++)
            foreach (
                var frame in (await Receive(socket)).Split(
                    '\u001e',
                    StringSplitOptions.RemoveEmptyEntries
                )
            )
            {
                var value = JsonSerializer.Deserialize<JsonElement>(frame);
                if (value.TryGetProperty("type", out var actual) && actual.GetInt32() == type)
                    return value;
            }
        throw new InvalidOperationException("Expected SignalR frame unavailable.");
    }

    private static async Task<string> Ticket(WebApplication app)
    {
        using var http = app.GetTestClient();
        http.DefaultRequestHeaders.Add("Authorization", "Fixture");
        http.DefaultRequestHeaders.Add("Origin", Origin);
        var response = await http.PostAsync(PathFor(Church) + "/progress-ticket", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("value")
            .GetProperty("ticket")
            .GetString()!;
    }

    private static Task<WebSocket> Connect(WebApplication app, string ticket)
    {
        var client = app.GetTestServer().CreateWebSocketClient();
        client.ConfigureRequest = request => request.Headers.Origin = Origin;
        return client.ConnectAsync(
            new Uri($"ws://localhost{PathFor(Church)}/progress?ticket={ticket}"),
            default
        );
    }

    private static async Task<WebApplication> Start()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Local" }
        );
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder
            .Services.AddAuthentication("fixture")
            .AddScheme<AuthenticationSchemeOptions, Authentication>("fixture", _ => { });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("IamAdmin", policy => policy.RequireAuthenticatedUser());
            options.AddPolicy(
                "ActiveChurchRoute",
                policy =>
                    policy.RequireAssertion(context =>
                        context.Resource is HttpContext http
                        && http.Request.RouteValues["churchCode"]?.ToString()
                            == Church.ToString("D")
                    )
            );
        });
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<Reader>();
        builder.Services.AddScoped<IMediatorBus, Bus>();
        builder.Services.AddScoped<IImportActor, Actor>();
        builder.Services.AddAutoMapper(config =>
        {
            config.AddProfile<GetImportProgressMappingProfile>();
            config.AddProfile<CreateImportProgressTicketMappingProfile>();
        });
        builder.Services.AddObjectsApi(
            new ConfigurationBuilder().Build(),
            new ImportFixtureEnvironment()
        );
        builder
            .Services.AddControllers(options => options.Filters.Add<ResultFilter>())
            .AddApplicationPart(typeof(ImportProgressController).Assembly);
        var app = builder.Build();
        app.UseImportProgress();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapImportProgress();
        await app.StartAsync();
        return app;
    }

    private sealed class Authentication(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
            Task.FromResult(
                Request.Headers.Authorization != "Fixture"
                    ? AuthenticateResult.NoResult()
                    : AuthenticateResult.Success(
                        new AuthenticationTicket(
                            new ClaimsPrincipal(
                                new ClaimsIdentity(
                                    [
                                        new Claim("sub", "owner"),
                                        new Claim(
                                            "exp",
                                            DateTimeOffset
                                                .UtcNow.AddMinutes(10)
                                                .ToUnixTimeSeconds()
                                                .ToString()
                                        ),
                                    ],
                                    "fixture"
                                )
                            ),
                            "fixture"
                        )
                    )
            );
    }

    private sealed class Actor : IImportActor
    {
        public ImportRequester GetRequester(Guid church) =>
            new(church, "owner", "subject", "test", "127.0.0.1");
    }

    private sealed class Reader : IImportProgressReader
    {
        public readonly Guid Load = Guid.NewGuid();
        public long Version = 1;
        public ImportRequester? LastRequester;

        public Task<IReadOnlyList<ImportProgressSnapshot>> ReadAsync(
            ImportRequester requester,
            IReadOnlyCollection<Guid> codes,
            CancellationToken cancellationToken
        )
        {
            LastRequester = requester;
            return Task.FromResult<IReadOnlyList<ImportProgressSnapshot>>(
                requester.OwnerHash == "owner"
                && requester.ChurchCode == Church
                && codes.Contains(Load)
                    ?
                    [
                        new(
                            Load,
                            Version == 1 ? "processing" : "completed",
                            "PROCESADO",
                            1,
                            Version == 1 ? 0 : 1,
                            0,
                            DateTime.UtcNow,
                            Version
                        ),
                    ]
                    : []
            );
        }
    }

    private sealed class Bus(
        Reader reader,
        IImportActor actor,
        IImportProgressTicketIssuer issuer,
        IMapper mapper
    ) : IMediatorBus
    {
        public async Task<T> SendAsync<T>(
            IRequest<T> request,
            CancellationToken cancellationToken = default
        )
        {
            object response = request switch
            {
                CreateImportProgressTicketCommand command =>
                    await new CreateImportProgressTicketCommandHandler(
                        actor,
                        issuer,
                        mapper
                    ).Handle(command, cancellationToken),
                GetImportProgressQuery query
                    when new GetImportProgressQueryValidator().Validate(query).IsValid =>
                    await new GetImportProgressQueryHandler(reader).Handle(
                        query,
                        cancellationToken
                    ),
                _ => Result.Failure<GetImportProgressResponseDto>(
                    new Error("INVALID", "Invalid selection", ErrorType.Validation)
                ),
            };
            return (T)response;
        }

        public Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
            where T : Command => throw new NotSupportedException();

        public Task RaiseAsync<T>(T @event, CancellationToken cancellationToken = default)
            where T : Event => throw new NotSupportedException();
    }
}
