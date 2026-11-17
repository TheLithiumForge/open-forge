using System.Net;
using OpenForge.Cli.Composition.Shared.Tools.Npm;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Composition.Shared.Tools;

internal sealed class ToolHttpTransfer : IToolDownloadHost
{
    private const int BufferSize = 81920;
    private const int MaximumRedirects = 10;
    private static readonly HttpClient DefaultClient = new(new SocketsHttpHandler { AllowAutoRedirect = false, UseCookies = false });
    private readonly HttpClient _client;
    private readonly IProgress<ToolTransferProgress>? _progress;
    private readonly ToolNpmMetadataHost _metadata;

    internal ToolHttpTransfer(ToolNpmMetadataHost metadata, HttpClient? client = null, IProgress<ToolTransferProgress>? progress = null)
    {
        _metadata = metadata;
        _client = client ?? DefaultClient;
        _progress = progress;
    }

    public Task<ToolNpmMetadataResult> ReadNpmMetadataAsync(ToolNpmMetadataRequest request, CancellationToken cancellationToken)
        => _metadata.ReadAsync(request, cancellationToken);

    public async Task<ToolDownloadResult> DownloadAsync(ToolDownloadRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ToolArtifactSource.IsOfficial(request.Asset) || !ToolArtifactIntegrity.IsSha256(request.Asset.Sha256) || request.Asset.MaximumBytes <= 0)
            return new(null, null, "A download needs an official HTTPS source, a size limit and pinned SHA-256.");
        try
        {
            await TransferAsync(request, cancellationToken).ConfigureAwait(false);
            await ToolArtifactIntegrity.VerifyAsync(request.DestinationPath, request.Asset, cancellationToken).ConfigureAwait(false);
            return new(request.DestinationPath, new()
            {
                Kind = ToolVerificationKind.PinnedSha256,
                Source = request.Asset.Url.AbsoluteUri,
                Sha256 = request.Asset.Sha256,
            }, null);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or HttpRequestException or UnauthorizedAccessException or OperationCanceledException)
        {
            return new(null, null, "The artifact transfer or SHA-256 verification failed. Check the destination before retrying.");
        }
    }

    private async Task TransferAsync(ToolDownloadRequest request, CancellationToken token)
    {
        var address = request.Asset.Url;
        for (var redirect = 0; redirect <= MaximumRedirects; redirect++)
        {
            using var response = await _client.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
            if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect or HttpStatusCode.SeeOther
                or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
            {
                var location = response.Headers.Location ?? throw new IOException("The redirect has no location.");
                address = location.IsAbsoluteUri ? location : new Uri(address, location);
                if (!ToolArtifactSource.AllowsRedirect(request.Asset.Url, address)) throw new IOException("The redirect leaves the approved hosts.");
                continue;
            }
            response.EnsureSuccessStatusCode();
            var length = response.Content.Headers.ContentLength;
            if (length > request.Asset.MaximumBytes) throw new IOException("The artifact exceeds its size limit.");
            await using var source = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
            await using var target = new FileStream(request.DestinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            var buffer = new byte[BufferSize];
            long copied = 0;
            for (var count = await source.ReadAsync(buffer, token).ConfigureAwait(false); count != 0; count = await source.ReadAsync(buffer, token).ConfigureAwait(false))
            {
                if (count > request.Asset.MaximumBytes - copied) throw new IOException("The artifact exceeds its size limit.");
                await target.WriteAsync(buffer.AsMemory(0, count), token).ConfigureAwait(false);
                copied += count;
                _progress?.Report(new(copied, length));
            }
            if (length is { } expected && copied != expected) throw new IOException("The transfer length changed.");
            return;
        }
        throw new IOException("The artifact exceeded its redirect limit.");
    }
}
