using System.Text;
using System.Text.Json;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests.Shared.MainRoot;

internal sealed class DefaultLayoutBaseline : IDisposable
{
    private const string ResourceName = "MainRoot.DefaultLayout";
    private const string FilesProperty = "files";
    private const string TextProperty = "text";
    private const string JsonProperty = "json";
    private readonly JsonDocument _document;

    internal DefaultLayoutBaseline()
    {
        using var stream = typeof(DefaultLayoutBaseline).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(ResourceName);
        _document = JsonDocument.Parse(stream);
    }

    internal Dictionary<string, byte[]> Files => _document.RootElement.GetProperty(FilesProperty).EnumerateObject()
        .ToDictionary(property => property.Name, property => FileBytes(property.Name, property.Value.GetBytesFromBase64()), StringComparer.Ordinal);
    internal byte[] Text => HostLines(_document.RootElement.GetProperty(TextProperty).GetString()!);
    internal JsonElement Json => _document.RootElement.GetProperty(JsonProperty);
    public void Dispose() => _document.Dispose();

    private static byte[] FileBytes(string path, byte[] bytes)
        => path is PublishedTokens.Settings.RelativePath or PublishedTokens.Ownership.RelativePath
            ? HostLines(Encoding.UTF8.GetString(bytes)) : bytes;

    // The Windows capture has host line endings in console output and generated JSON only.
    // Shipped files and all actual outputs remain byte-exact on every host.
    private static byte[] HostLines(string text) => Encoding.UTF8.GetBytes(text.Replace("\r\n", Environment.NewLine, StringComparison.Ordinal));
}
