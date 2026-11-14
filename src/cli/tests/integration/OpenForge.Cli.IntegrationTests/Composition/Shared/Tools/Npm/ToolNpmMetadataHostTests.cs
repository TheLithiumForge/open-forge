using System.Collections;
using System.Net;
using System.Text;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Composition.Shared.Tools.Npm;
using OpenForge.Cli.Composition.Shared.Tools.Npm.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.Npm;

[Trait("Feature", "tool-npm-metadata"), Trait("Evidence", "Integration")]
public sealed class ToolNpmMetadataHostTests
{
    private const string Package = "@example/sample";
    private const string Entry = "entry.js";
    private const string Metadata = """{"name":"@example/sample","versions":{"2.4.3":{"version":"2.4.3","bin":{"sample":"entry.js"},"engines":{"node":">=20 <25"}},"2.4.4":{"version":"2.4.4","bin":"entry.js"}}}""";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Metadata uses the configured HTTPS registry and public package identity without installing a client")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadsConfiguredRegistry(bool custom)
    {
        using var temporary = TemporaryWorkspace.Create("npm-metadata");
        var before = Directory.EnumerateFileSystemEntries(temporary.Path).Order(StringComparer.Ordinal).ToArray();
        var variables = new Dictionary<string, string?>();
        if (custom) variables[ToolNpmMetadataPolicy.RegistryVariable] = "https://registry.example.test/packages/";
        ToolNpmMetadataConnection? connection = null;
        var queries = new List<Uri>();
        var host = Host(temporary, variables, selected =>
        {
            connection = selected;
            return new(new Responses((request, token) =>
            {
                Assert.True(token.CanBeCanceled);
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Null(request.Headers.Authorization);
                queries.Add(request.RequestUri ?? throw new InvalidOperationException());
                return Task.FromResult(Response(Metadata));
            }));
        });
        var result = await host.ReadNpmMetadataAsync(new(Channel(), temporary.Path), Token);
        Assert.Null(result.Failure);
        Assert.NotNull(result.Registry);
        Assert.NotNull(connection);
        Assert.Equal(new Uri(custom ? variables[ToolNpmMetadataPolicy.RegistryVariable] ?? throw new InvalidOperationException() : ToolNpmMetadataPolicy.DefaultRegistry), result.Registry);
        Assert.Equal(new Uri(result.Registry, Uri.EscapeDataString(Package)), Assert.Single(queries));
        Assert.Equal(new[] { "2.4.3", "2.4.4" }, result.Versions.Select(version => version.Version));
        Assert.All(result.Versions, version => Assert.Equal(Entry, version.EntryRelativePath));
        Assert.Equal(">=20 <25", result.Versions[0].NodeRange);
        Assert.Equal(before, Directory.EnumerateFileSystemEntries(temporary.Path).Order(StringComparer.Ordinal));
    }

    [Fact(DisplayName = "Explicit npm proxy configuration reaches the HTTP connection without changing the caller environment")]
    public async Task UsesConfiguredProxy()
    {
        using var temporary = TemporaryWorkspace.Create("npm-metadata");
        var proxy = new Uri("http://proxy.example.test:8080");
        var variables = new Dictionary<string, string?> { [ToolNpmMetadataPolicy.HttpsProxyVariable] = proxy.AbsoluteUri };
        ToolNpmMetadataConnection? observed = null;
        var host = Host(temporary, variables, connection => { observed = connection; return new(new Responses((_, _) => Task.FromResult(Response(Metadata)))); });
        var result = await host.ReadNpmMetadataAsync(new(Channel(), temporary.Path), Token);
        Assert.Null(result.Failure);
        Assert.NotNull(observed);
        Assert.NotNull(observed.Proxy);
        Assert.Equal(proxy, observed.Proxy.GetProxy(observed.Registry));
        Assert.Equal(proxy.AbsoluteUri, variables[ToolNpmMetadataPolicy.HttpsProxyVariable]);
    }

    [Fact(DisplayName = "Root's platform proxy keeps its own bypass policy for ordinary proxy environment settings")]
    public async Task KeepsPlatformProxy()
    {
        using var temporary = TemporaryWorkspace.Create("npm-metadata");
        var platform = new WebProxy(new Uri("http://proxy.example.test:8080"));
        var variables = new Dictionary<string, string?> { [ToolNpmMetadataPolicy.StandardProxyVariables[0]] = "http://proxy.example.test:8080", [ToolNpmMetadataPolicy.NoProxyVariable] = ".example.test" };
        var host = new ToolHttpTransfer(new ToolNpmMetadataHost(new() { UserHome = temporary.Path, Variables = variables, DefaultProxy = platform },
            connection => { Assert.Same(platform, connection.Proxy); return new(new Responses((_, _) => Task.FromResult(Response(Metadata)))); }));
        Assert.Null((await host.ReadNpmMetadataAsync(new(Channel(), temporary.Path), Token)).Failure);
    }

    [Fact(DisplayName = "Credential environment names block lookup without inspecting their values")]
    public async Task DoesNotReadCredentials()
    {
        using var temporary = TemporaryWorkspace.Create("npm-metadata");
        var credentials = new CredentialPresence();
        var calls = 0;
        var host = Host(temporary, credentials, _ => { calls++; throw new InvalidOperationException(); });
        var result = await host.ReadNpmMetadataAsync(new(Channel(), temporary.Path), Token);
        Assert.NotNull(result.Failure);
        Assert.Null(result.Registry);
        Assert.Empty(result.Versions);
        Assert.False(credentials.ValueRead);
        Assert.Equal(0, calls);
    }

    [Fact(DisplayName = "A nonempty npm configuration file blocks fallback without opening its contents")]
    public async Task DoesNotOpenConfiguration()
    {
        using var temporary = TemporaryWorkspace.Create("npm-metadata");
        var file = temporary.CreateFile(ToolNpmMetadataPolicy.ConfigFileName, "synthetic configuration that must stay unread");
        using var locked = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var calls = 0;
        var result = await Host(temporary, new Dictionary<string, string?>(), _ => { calls++; throw new InvalidOperationException(); })
            .ReadNpmMetadataAsync(new(Channel(), temporary.Path), Token);
        Assert.NotNull(result.Failure);
        Assert.Null(result.Registry);
        Assert.Equal(0, calls);
        Assert.True(locked.Length > 0);
    }

    [Theory(DisplayName = "Unsupported authentication and trust settings never fall back to the public registry")]
    [InlineData("registry-auth")]
    [InlineData("proxy-auth")]
    [InlineData("tls-disabled")]
    [InlineData("custom-ca")]
    public async Task BlocksUnsupportedConfiguration(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("npm-metadata");
        var pair = scenario switch
        {
            "registry-auth" => new KeyValuePair<string, string?>(ToolNpmMetadataPolicy.RegistryVariable, "https://synthetic:unread@registry.example.test/"),
            "proxy-auth" => new(ToolNpmMetadataPolicy.HttpsProxyVariable, "http://synthetic:unread@proxy.example.test/"),
            "tls-disabled" => new(ToolNpmMetadataPolicy.StrictSslVariable, "false"),
            "custom-ca" => new(ToolNpmMetadataPolicy.TrustVariables.First(), "unread"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var calls = 0;
        var result = await Host(temporary, new Dictionary<string, string?> { [pair.Key] = pair.Value }, _ => { calls++; throw new InvalidOperationException(); })
            .ReadNpmMetadataAsync(new(Channel(), temporary.Path), Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.Versions);
        Assert.Equal(0, calls);
    }

    [Theory(DisplayName = "Metadata bodies are bounded with and without a declared content length")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BoundsResponse(bool unknownLength)
    {
        using var temporary = TemporaryWorkspace.Create("npm-metadata");
        var content = Encoding.UTF8.GetBytes($"{Metadata[..^1]},\"padding\":\"{new string('x', ToolNpmMetadataHost.MaximumResponseBytes)}\"}}");
        var host = Host(temporary, new Dictionary<string, string?>(), _ => new(new Responses((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        { Content = unknownLength ? new UnknownLength(content) : new ByteArrayContent(content) }))));
        var result = await host.ReadNpmMetadataAsync(new(Channel(), temporary.Path), Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.Versions);
    }

    [Theory(DisplayName = "Malformed metadata and foreign redirects return no version facts")]
    [InlineData("malformed")]
    [InlineData("null-version")]
    [InlineData("foreign-package")]
    [InlineData("redirect")]
    public async Task RejectsInvalidResponse(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("npm-metadata");
        var host = Host(temporary, new Dictionary<string, string?>(), _ => new(new Responses((_, _) =>
        {
            var response = scenario switch
            {
                "malformed" => Response("{"),
                "null-version" => Response("""{"name":"@example/sample","versions":{"2.4.3":null}}"""),
                "foreign-package" => Response("""{"name":"other","versions":{}}"""),
                "redirect" => new(HttpStatusCode.Redirect),
                _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
            };
            if (scenario == "redirect") response.Headers.Location = new("https://foreign.example.test/");
            return Task.FromResult(response);
        })));
        var result = await host.ReadNpmMetadataAsync(new(Channel(), temporary.Path), Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.Versions);
    }

    [Fact(DisplayName = "Metadata cancellation stops the bounded HTTP read without writing files")]
    public async Task CancelsRead()
    {
        using var temporary = TemporaryWorkspace.Create("npm-metadata");
        var before = Directory.EnumerateFileSystemEntries(temporary.Path).Order(StringComparer.Ordinal).ToArray();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var host = Host(temporary, new Dictionary<string, string?>(), _ => new(new Responses(async (_, token) =>
        { await cancellation.CancelAsync(); await Task.Delay(Timeout.InfiniteTimeSpan, token); return Response(Metadata); })));
        var result = await host.ReadNpmMetadataAsync(new(Channel(), temporary.Path), cancellation.Token);
        Assert.NotNull(result.Failure);
        Assert.Equal(before, Directory.EnumerateFileSystemEntries(temporary.Path).Order(StringComparer.Ordinal));
    }

    private static ToolHttpTransfer Host(TemporaryWorkspace temporary, IReadOnlyDictionary<string, string?> variables, Func<ToolNpmMetadataConnection, HttpClient> connect)
        => new(new ToolNpmMetadataHost(new() { UserHome = temporary.Path, Variables = variables }, connect));
    private static ToolNpmChannel Channel() => new()
    {
        Id = "npm",
        Kind = ToolChannelKind.Npm,
        Package = Package,
        EntryRelativePath = Entry,
        MinimumClientVersions = new Dictionary<ToolNpmClient, string>(),
        Requirements = new() { Targets = [] }
    };
    private static HttpResponseMessage Response(string json) => new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class Responses(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => response(request, cancellationToken);
    }
    private sealed class UnknownLength(byte[] bytes) : HttpContent
    {
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => stream.WriteAsync(bytes).AsTask();
        protected override Task<Stream> CreateContentReadStreamAsync() => Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
    }
    private sealed class CredentialPresence : IReadOnlyDictionary<string, string?>
    {
        internal bool ValueRead { get; private set; }
        public IEnumerable<string> Keys => [ToolNpmMetadataPolicy.CredentialVariables.First()];
        public IEnumerable<string?> Values { get { ValueRead = true; throw new InvalidOperationException(); } }
        public int Count => 1;
        public string? this[string key] { get { ValueRead = true; throw new InvalidOperationException(); } }
        public bool ContainsKey(string key) => Keys.Contains(key);
        public bool TryGetValue(string key, out string? value) { ValueRead = true; throw new InvalidOperationException(); }
        public IEnumerator<KeyValuePair<string, string?>> GetEnumerator() { ValueRead = true; throw new InvalidOperationException(); }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
