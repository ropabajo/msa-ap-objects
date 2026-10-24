using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.TemporaryExports;
using Xunit;
namespace Ropabajo.Church.Sanluis.Objects.Api.Tests;

public class MinioExportObjectStoreTests
{
    [Fact]
    public async Task Real_sdk_reads_lists_deletes_and_signs_only_the_temporary_prefix()
    {
        using var server = new S3ProtocolFixture();
        using var store = new MinioExportObjectStore(Options.Create(new MinIoOptions
        {
            Endpoint = server.Endpoint,
            UseSsl = false,
            BucketName = "fixture-exports",
            AccessKey = "fixture-user",
            SecretKey = "fixture-password"
        }),
            Options.Create(new TemporaryExportOptions { Enabled = true, MaxSourceBytes = 1024 }));
        var root = TemporaryExportCoordinator.Prefix + "jobs/fixture/";
        Assert.Null(await store.ReadAsync(root + "missing.json", default));
        var content = Encoding.UTF8.GetBytes("{\"status\":\"pending\"}");
        await store.WriteAsync(root + "manifest.json", content, "application/json", default);
        Assert.Equal(content, await store.ReadAsync(root + "manifest.json", default));
        Assert.Contains(root + "manifest.json", await store.ListAsync(TemporaryExportCoordinator.Prefix, default));
        var url = await store.SignAsync(root + "document.xlsx", "members-v1.xlsx", 900, DateTimeOffset.UtcNow, default);
        Assert.Contains("X-Amz-Signature=", url);
        Assert.Contains("X-Amz-Expires=900", url);
        Assert.Contains("attachment", Uri.UnescapeDataString(url));
        await store.WriteAsync(root + "oversized.json", new byte[1025], "application/json", default);
        await Assert.ThrowsAnyAsync<Exception>(() => store.ReadAsync(root + "oversized.json", default));
        await store.DeleteAsync(root + "manifest.json", default);
        Assert.False(await store.ExistsAsync(root + "manifest.json", default));
        foreach (var outside in new[] { "photos/existing.jpg", "reports/elections/existing.pdf", root + "../escape.json" })
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.ExistsAsync(outside, default));
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.DeleteAsync(outside, default));
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.SignAsync(outside, "file.xlsx", 900, DateTimeOffset.UtcNow, default));
        }
    }
    private sealed class S3ProtocolFixture : IDisposable
    {
        private readonly HttpListener listener = new();
        private readonly CancellationTokenSource stop = new();
        private readonly Task loop;
        private readonly ConcurrentDictionary<string, byte[]> objects = new();
        public string Endpoint { get; }
        public S3ProtocolFixture()
        {
            var tcp = new TcpListener(IPAddress.Loopback, 0); tcp.Start();
            var port = ((IPEndPoint)tcp.LocalEndpoint).Port; tcp.Stop();
            Endpoint = "127.0.0.1:" + port; listener.Prefixes.Add("http://" + Endpoint + "/"); listener.Start();
            loop = Task.Run(async () =>
            {
                while (!stop.IsCancellationRequested)
                {
                    HttpListenerContext context;
                    try { context = await listener.GetContextAsync().WaitAsync(stop.Token); }
                    catch (Exception) when (stop.IsCancellationRequested) { return; }
                    var request = context.Request; var response = context.Response;
                    var path = request.Url!.AbsolutePath; var key = Uri.UnescapeDataString(path).Replace("/fixture-exports/", "");
                    if (request.QueryString.AllKeys.Contains("location"))
                        await Xml(response, "<LocationConstraint xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">us-east-1</LocationConstraint>");
                    else if (request.QueryString["list-type"] == "2")
                    {
                        XNamespace ns = "http://s3.amazonaws.com/doc/2006-03-01/";
                        var keys = objects.Keys.Where(k => k.StartsWith(request.QueryString["prefix"] ?? "")).Order().ToArray();
                        var xml = new XElement(ns + "ListBucketResult", new XElement(ns + "Name", "fixture-exports"),
                            new XElement(ns + "Prefix", request.QueryString["prefix"] ?? ""), new XElement(ns + "KeyCount", keys.Length),
                            new XElement(ns + "MaxKeys", 1000), new XElement(ns + "IsTruncated", false),
                            keys.Select(k => new XElement(ns + "Contents", new XElement(ns + "Key", k), new XElement(ns + "LastModified", "2026-10-24T12:00:00.000Z"),
                                new XElement(ns + "ETag", "\"fixture\""), new XElement(ns + "Size", objects[k].Length), new XElement(ns + "StorageClass", "STANDARD"))));
                        await Xml(response, xml.ToString());
                    }
                    else if (request.HttpMethod == "PUT")
                    {
                        using var body = new MemoryStream(); await request.InputStream.CopyToAsync(body);
                        objects[key] = body.ToArray(); response.Headers["ETag"] = "\"fixture\"";
                    }
                    else if (request.HttpMethod == "DELETE") { objects.TryRemove(key, out _); response.StatusCode = 204; }
                    else if (objects.TryGetValue(key, out var data))
                    {
                        response.ContentLength64 = data.Length; response.Headers["ETag"] = "\"fixture\"";
                        response.Headers["Last-Modified"] = DateTime.UtcNow.ToString("R"); response.ContentType = "application/octet-stream";
                        if (request.HttpMethod != "HEAD") await response.OutputStream.WriteAsync(data);
                    }
                    else if (path.Trim('/') == "fixture-exports") response.StatusCode = 200;
                    else response.StatusCode = 404;
                    response.Close();
                }
            });
        }
        private static async Task Xml(HttpListenerResponse response, string xml)
        { var bytes = Encoding.UTF8.GetBytes(xml); response.ContentType = "application/xml"; response.ContentLength64 = bytes.Length; await response.OutputStream.WriteAsync(bytes); }
        public void Dispose() { stop.Cancel(); listener.Close(); loop.GetAwaiter().GetResult(); stop.Dispose(); }
    }
}
