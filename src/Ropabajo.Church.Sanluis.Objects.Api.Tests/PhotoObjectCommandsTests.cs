using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Ropabajo.Churc.Sanluis.Framework.Authz.Authorization;
using Ropabajo.Churc.Sanluis.Framework.Integration;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;
using Ropabajo.Church.Sanluis.Objects.Application.Photos;
using Ropabajo.Church.Sanluis.Objects.Application.Shared.Enums;
using ObjectEntity = Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object;
using Xunit;

namespace Ropabajo.Church.Sanluis.Objects.Api.Tests;

public sealed class PhotoObjectCommandsTests
{
    [Theory]
    [InlineData("photo.JPG", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.webp", "image/webp")]
    public async Task Photo_reservations_enforce_the_server_allowlist_and_size_even_if_the_client_requests_more(string file, string mime)
    {
        var work = new Work(); ObjectEntity? saved = null; var history = 0;
        var store = Proxy<IObjectUploadStore>((method, args) =>
        {
            Assert.False(work.InTransaction); Assert.Equal("GrantAsync", method.Name);
            Assert.Equal(mime, args[1]); Assert.Equal((long)PhotoUploadPolicy.MaxBytes, args[2]); Assert.Equal(300, args[3]);
            Assert.StartsWith("photos/", (string)args[0]!);
            return Task.FromResult(Result<ObjectUploadGrant>.Success(new(new Dictionary<string, string> { ["url"] = "https://storage.test/bucket/" })));
        });
        var objects = Proxy<IObjectRepository>((method, args) =>
        {
            Assert.Equal("AddAsync", method.Name); Assert.True(work.InTransaction);
            saved = (ObjectEntity)args[0]!; saved.Id = 7; return Task.FromResult(saved);
        });
        var states = Proxy<IObjectStateRepository>((_, args) => { Assert.True(work.InTransaction); history++; return Task.FromResult((Ropabajo.Church.Sanluis.Objects.Domain.Entities.ObjectState)args[0]!); });
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<CreateObjectMappingProfile>(), NullLoggerFactory.Instance).CreateMapper();
        var handler = new CreateObjectCommandHandler(store, objects, states, Context(), work, mapper);
        var result = await handler.Handle(new CreateObjectCommand { Path = "/PHOTOS", FileName = file, MaxLength = 999, Expiration = 3600, AllowedExtensions = ".exe" }, default);
        Assert.True(result.IsSuccess); Assert.NotNull(saved); Assert.Equal(5, saved.MaxLength); Assert.Equal(PhotoUploadPolicy.Extensions, saved.AllowedExtensions);
        Assert.Equal(1, history); Assert.True(Guid.TryParse(result.Value.Headers["sanluis-object-code"], out _));
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Confirmation_does_not_write_state_or_history_if_the_backend_rejects_the_stored_photo(bool valid)
    {
        var work = new Work(); var writes = 0; var history = 0;
        var item = new ObjectEntity { Id = 7, Code = Guid.NewGuid(), Path = "photos", ObjectName = "photos/photo.jpeg", StateCode = ObjectState.PresignedUrl };
        var objects = Proxy<IObjectRepository>((method, _) => method.Name switch
        {
            "GetOneAsync" => Task.FromResult(item),
            "UpdateAsync" => CountWrite(),
            _ => throw new InvalidOperationException(method.Name)
        });
        Task<ObjectEntity> CountWrite() { Assert.True(work.InTransaction); writes++; return Task.FromResult(item); }
        var states = Proxy<IObjectStateRepository>((_, args) => { Assert.True(work.InTransaction); history++; return Task.FromResult((Ropabajo.Church.Sanluis.Objects.Domain.Entities.ObjectState)args[0]!); });
        var store = Proxy<IObjectUploadStore>((method, _) =>
        {
            Assert.Equal("VerifyPhotoAsync", method.Name); Assert.False(work.InTransaction);
            return Task.FromResult(valid ? Result<VerifiedPhoto>.Success(new(2, 2)) : Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Invalid));
        });
        var handler = new UploadObjectCommandHandler(objects, states, Context(), store, work);
        var result = await handler.Handle(new UploadObjectCommand { ObjectCode = item.Code.ToString("D") }, default);
        Assert.Equal(valid, result.IsSuccess); Assert.Equal(valid ? 1 : 0, writes); Assert.Equal(valid ? 1 : 0, history);
        Assert.Equal(valid ? ObjectState.Uploaded : ObjectState.PresignedUrl, item.StateCode);
    }
    private static ICurrentContextAccessor Context() => Proxy<ICurrentContextAccessor>((method, _) => method.Name == "get_Username" ? "fixture-user" : throw new InvalidOperationException());
    private static T Proxy<T>(Func<MethodInfo, object?[], object?> call) where T : class
    { var proxy = DispatchProxy.Create<T, ReservedExportPathsTests.Proxy>(); ((ReservedExportPathsTests.Proxy)(object)proxy).Call = call; return proxy; }
    private sealed class Work : IIntegrationWork
    {
        public bool InTransaction { get; private set; }
        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken token)
        { InTransaction = true; try { return await action(token); } finally { InTransaction = false; } }
        public void Enqueue<T>(Guid messageId, Guid correlationId, string kind, T message) => throw new NotSupportedException();
        public Task ReplayOrEnqueueAsync<T>(Guid messageId, Guid correlationId, string kind, T message, CancellationToken token) => throw new NotSupportedException();
    }
}
