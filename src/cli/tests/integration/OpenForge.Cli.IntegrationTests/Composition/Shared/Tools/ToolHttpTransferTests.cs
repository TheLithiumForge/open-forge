using System.Net;
using System.Security.Cryptography;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Composition.Shared.Tools.Npm;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.Tools;

[Trait("Feature", "tool-download"), Trait("Evidence", "Integration")]
public sealed class ToolHttpTransferTests
{
    private static readonly byte[] Payload = [1, 127, 255];

    [Theory(DisplayName = "Downloads verify pinned bytes and retain honest integrity evidence")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task VerifiesTransfer(bool mismatch)
    {
        using var temporary = TemporaryWorkspace.Create("tool-download");
        using var handler = new Responses(_ => new(HttpStatusCode.OK) { Content = new ByteArrayContent(Payload) });
        using var client = new HttpClient(handler);
        var asset = Asset();
        if (mismatch) asset = asset with { Sha256 = new string('0', SHA256.HashSizeInBytes * 2) };
        var result = await Host(temporary, client).DownloadAsync(new(asset, Destination(temporary)), TestContext.Current.CancellationToken);
        Assert.Equal(mismatch, result.Failure is not null);
        if (!mismatch)
        {
            Assert.NotNull(result.Integrity);
            Assert.Equal(ToolVerificationKind.PinnedSha256, result.Integrity.Kind);
            Assert.Equal(asset.Sha256, result.Integrity.Sha256);
            Assert.NotNull(result.VerifiedPath);
            Assert.Equal(Payload, await File.ReadAllBytesAsync(result.VerifiedPath, TestContext.Current.CancellationToken));
        }
        Assert.Equal(1, handler.Calls);
    }

    [Theory(DisplayName = "Downloads block oversized content and unapproved redirects")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RejectsUnsafeResponse(bool redirect)
    {
        using var temporary = TemporaryWorkspace.Create("tool-download");
        using var handler = new Responses(_ => redirect ? Redirect("https://example.com/unapproved") : new(HttpStatusCode.OK) { Content = new ByteArrayContent(Payload) });
        using var client = new HttpClient(handler);
        var result = await Host(temporary, client).DownloadAsync(new(Asset() with { MaximumBytes = 1 }, Destination(temporary)), TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.Null(result.VerifiedPath);
        Assert.Equal(1, handler.Calls);
        Assert.False(File.Exists(temporary.Combine("download")));
    }

    [Fact(DisplayName = "Invalid descriptors cause no HTTP request or destination write")]
    public async Task RejectsUnpinnedDownload()
    {
        using var temporary = TemporaryWorkspace.Create("tool-download");
        using var handler = new Responses(_ => throw new InvalidOperationException());
        using var client = new HttpClient(handler);
        var result = await Host(temporary, client).DownloadAsync(new(Asset() with { Sha256 = null }, Destination(temporary)), TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.Equal(0, handler.Calls);
        Assert.False(File.Exists(temporary.Combine("download")));
    }

    private static ToolHttpTransfer Host(TemporaryWorkspace temporary, HttpClient client)
        => new(new ToolNpmMetadataHost(new() { UserHome = temporary.Path, Variables = new Dictionary<string, string?>() }), client);

    private static string Destination(TemporaryWorkspace temporary)
    {
        var path = temporary.CreateFile("download");
        File.Delete(path);
        return path;
    }

    private static ToolReleaseAsset Asset() => new()
    {
        Version = "2.4.3",
        Target = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64 },
        Url = new("https://github.com/example/sample/releases/download/v2.4.3/sample.zip"),
        Sha256 = Convert.ToHexStringLower(SHA256.HashData(Payload)),
        Archive = ToolArchiveKind.File,
        StripComponents = 0,
        EntryRelativePath = "sample",
        MaximumBytes = 100,
    };

    private static HttpResponseMessage Redirect(string location)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Redirect);
        response.Headers.Location = new(location);
        return response;
    }

    private sealed class Responses(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        internal int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(response(request));
        }
    }
}
