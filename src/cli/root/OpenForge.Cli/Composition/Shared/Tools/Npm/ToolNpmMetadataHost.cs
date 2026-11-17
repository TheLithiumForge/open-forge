using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using OpenForge.Cli.Composition.Shared.Tools.Npm.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Composition.Shared.Tools.Npm;

internal sealed class ToolNpmMetadataHost(ToolNpmMetadataEnvironment environment, Func<ToolNpmMetadataConnection, HttpClient>? connect = null)
{
    internal const int MaximumResponseBytes = 8 * 1024 * 1024;
    internal const int MaximumRedirects = 3;
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    internal async Task<ToolNpmMetadataResult> ReadAsync(ToolNpmMetadataRequest request, CancellationToken token)
    {
        Uri? registry = null;
        try
        {
            token.ThrowIfCancellationRequested();
            var connection = new ToolNpmMetadataPolicy(environment).Read(request.WorkspacePath);
            registry = connection.Registry;
            using var client = connect is null ? Connect(connection) : connect(connection);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(Timeout);
            var address = new Uri(registry, Uri.EscapeDataString(request.Channel.Package));
            for (var redirect = 0; redirect <= MaximumRedirects; redirect++)
            {
                using var query = new HttpRequestMessage(HttpMethod.Get, address);
                query.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.npm.install-v1+json"));
                using var response = await client.SendAsync(query, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
                if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect or HttpStatusCode.SeeOther
                    or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
                {
                    var location = response.Headers.Location ?? throw new IOException("The npm metadata redirect has no destination.");
                    var next = location.IsAbsoluteUri ? location : new Uri(address, location);
                    if (next.Scheme != registry.Scheme || next.Host != registry.Host || next.Port != registry.Port || next.UserInfo.Length != 0 || next.Query.Length != 0 || next.Fragment.Length != 0)
                        throw new IOException("Npm metadata redirected outside its configured registry.");
                    address = next;
                    continue;
                }
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength > MaximumResponseBytes) throw new IOException("Npm metadata exceeds its response limit.");
                await using var source = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
                using var bytes = new MemoryStream();
                var buffer = new byte[81920];
                for (var count = await source.ReadAsync(buffer, timeout.Token).ConfigureAwait(false); count != 0; count = await source.ReadAsync(buffer, timeout.Token).ConfigureAwait(false))
                {
                    if (count > MaximumResponseBytes - bytes.Length) throw new IOException("Npm metadata exceeds its response limit.");
                    await bytes.WriteAsync(buffer.AsMemory(0, count), timeout.Token).ConfigureAwait(false);
                }
                return new(ToolNpmMetadataReader.Read(bytes.ToArray(), request.Channel), registry, null);
            }
            throw new IOException("Npm metadata exceeded its redirect limit.");
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or HttpRequestException or JsonException or InvalidOperationException or ArgumentException
            or UnauthorizedAccessException or OperationCanceledException)
        {
            return new([], registry, "Npm metadata couldn't be read with the configured registry, proxy and trust settings. Use a verified native client for unsupported configuration.");
        }
    }

    private static HttpClient Connect(ToolNpmMetadataConnection connection)
        => new(new SocketsHttpHandler { AllowAutoRedirect = false, UseCookies = false, UseProxy = connection.Proxy is not null, Proxy = connection.Proxy });
}
