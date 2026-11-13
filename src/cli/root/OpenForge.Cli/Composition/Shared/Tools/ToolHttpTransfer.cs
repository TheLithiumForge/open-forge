using System.Buffers;
using System.Collections.Frozen;
using System.Net;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;

namespace OpenForge.Cli.Composition.Shared.Tools;

internal sealed class ToolHttpTransfer
{
    private const int BufferSize = 81920;
    private static readonly SocketsHttpHandler Handler = new() { AllowAutoRedirect = false, UseCookies = false };
    private static readonly HttpClient Client = new(Handler);
    private readonly IReadOnlySet<string> _redirectHosts;
    internal ToolHttpTransfer(IReadOnlySet<string>? redirectHosts = null) => _redirectHosts = (redirectHosts ?? new HashSet<string>()).ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    internal async ValueTask DownloadAsync(ToolDownloadRequest request, CancellationToken token)
    {
        if (request.Source.Scheme != Uri.UriSchemeHttps || request.Source.UserInfo.Length != 0 || request.Bytes <= 0)
            throw new IOException("A Tool download requires an exact HTTPS descriptor.");
        var address = request.Source;
        for (var redirect = 0; redirect <= Handler.MaxAutomaticRedirections; redirect++)
        {
            using var response = await Client.GetAsync(address, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
            if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect or HttpStatusCode.SeeOther
                or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
            {
                var location = response.Headers.Location ?? throw new IOException("The artifact redirect has no location.");
                address = location.IsAbsoluteUri ? location : new Uri(address, location);
                if (address.Scheme != Uri.UriSchemeHttps || address.UserInfo.Length != 0
                    || !string.Equals(address.Host, request.Source.Host, StringComparison.OrdinalIgnoreCase) && !_redirectHosts.Contains(address.Host))
                    throw new IOException("The artifact redirected outside the approved HTTPS hosts.");
                continue;
            }
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength is { } size && size != request.Bytes) throw new IOException("The artifact length differs from its descriptor.");
            await using var source = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
            await using var target = new FileStream(request.Destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                long copied = 0;
                for (var count = await source.ReadAsync(buffer, token).ConfigureAwait(false); count != 0; count = await source.ReadAsync(buffer, token).ConfigureAwait(false))
                {
                    copied += count;
                    if (copied > request.Bytes) throw new IOException("The artifact exceeds its declared length.");
                    await target.WriteAsync(buffer.AsMemory(0, count), token).ConfigureAwait(false);
                }
                if (copied != request.Bytes) throw new IOException("The artifact is shorter than its declared length.");
                return;
            }
            finally { ArrayPool<byte>.Shared.Return(buffer); }
        }
        throw new IOException("The artifact exceeded the HTTP redirect limit.");
    }
}
