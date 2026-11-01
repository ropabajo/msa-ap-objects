using System.Reflection;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.Authz.Authorization;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Queries.GetObjectPresignedUrl;
using Xunit;
namespace Ropabajo.Church.Sanluis.Objects.Api.Tests;

public class ReservedExportPathsTests
{
    [Theory]
    [InlineData("temporary-exports/v1/jobs/another-user/document.xlsx")]
    [InlineData("temporary-exports%2Fv1%2Fjobs%2Fother%2Fdocument.xlsx")]
    public async Task Generic_signer_cannot_bypass_owner_or_expiration_checks(string path)
    {
        var notifications = 0;
        var bus = Stub<IMediatorBus>((method, _) =>
        {
            Assert.Equal("RaiseAsync", method.Name); notifications++; return Task.CompletedTask;
        });
        var handler = new GetObjectPresignedUrlHandler(bus, new Settings());
        var result = await handler.Handle(new GetObjectPresignedUrlQuery { Template = "minio.example.invalid/church-dev/" + path }, default);
        Assert.Null(result); Assert.Equal(1, notifications);
    }
    [Theory]
    [InlineData("temporary-exports")]
    [InlineData("/temporary-exports/v1/jobs")]
    [InlineData("temporary-exports%2Fv1")]
    [InlineData("photos/../temporary-exports/v1")]
    public async Task Generic_upload_cannot_overwrite_temporary_documents_or_metadata(string path)
    {
        var notifications = 0;
        var bus = Stub<IMediatorBus>((method, _) => { Assert.Equal("RaiseAsync", method.Name); notifications++; return Task.CompletedTask; });
        var handler = new CreateObjectCommandHandler(bus, new Settings(),
            Stub<IObjectRepository>(NoCall), Stub<IObjectStateRepository>(NoCall), Stub<ICurrentContextAccessor>(NoCall));
        await handler.Handle(new CreateObjectCommand { Path = path }, default);
        Assert.Equal(1, notifications);
    }
    private static object? NoCall(MethodInfo method, object?[] args) => throw new InvalidOperationException("Unexpected downstream call: " + method.Name);
    private sealed class Settings : IOptionsSnapshot<MinIoOptions>
    {
        public MinIoOptions Value { get; } = new() { Endpoint = "minio.example.invalid", UseSsl = true, BucketName = "church-dev", AccessKey = "fixture-user", SecretKey = "fixture-password" };
        public MinIoOptions Get(string? name) => Value;
    }
    private static T Stub<T>(Func<MethodInfo, object?[], object?> call) where T : class
    { var proxy = DispatchProxy.Create<T, Proxy>(); ((Proxy)(object)proxy).Call = call; return proxy; }
    public class Proxy : DispatchProxy
    {
        public Func<MethodInfo, object?[], object?> Call { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Call(method!, args!);
    }
}
