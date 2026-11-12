using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;
using OpenForge.Cli.Core.Framework.Tools.Shared.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using YamlDotNet.Core;

namespace OpenForge.Cli.Core.Framework.Tools;

internal sealed class ToolCatalogueReader(ToolReleaseSupport? support = null)
{
    private const string EmbeddedPrefix = "OpenForge.Tools.Catalogue/";
    private const string CatalogueFileName = "catalogue.json";
    internal const string EmbeddedSource = "bundled";
    private readonly ToolReleaseSupport _support = support ?? ToolReleaseSupport.Production;

    internal async ValueTask<ToolCatalogueRead> ReadAsync(string? source, CancellationToken cancellationToken)
    {
        var identity = source ?? EmbeddedSource;
        var definitions = new List<ToolDefinition>();
        try
        {
            if (source is not null) identity = Path.GetFullPath(source);
            byte[] bytes;
            if (source is null)
            {
                bytes = await ReadEmbeddedAsync(CatalogueFileName, cancellationToken).ConfigureAwait(false);
            }
            else bytes = await ToolSourceFileReader.ReadAsync(identity,
                Path.GetDirectoryName(identity) ?? throw new JsonException("The tool source needs a containing directory."), cancellationToken).ConfigureAwait(false);
            var json = ToolDocumentDecoder.JsonBytes(bytes, identity);
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object) throw new JsonException("A tool source must contain an object.");
            if (!document.RootElement.TryGetProperty("tools", out _))
                definitions.Add(Definition(bytes, identity, true));
            else
            {
                var catalogue = JsonSerializer.Deserialize(json, ToolJsonContext.Default.ToolCatalogue) ?? throw new JsonException("The catalogue cannot be null.");
                Validate(catalogue);
                var root = source is null ? null : Path.GetDirectoryName(identity);
                foreach (var tool in catalogue.Tools.OrderBy(tool => tool.Id, StringComparer.Ordinal))
                    foreach (var version in tool.Versions.OrderBy(version => version.Version, StringComparer.Ordinal))
                    {
                        var path = root is null ? $"{EmbeddedSource}/{version.Path}" : Path.Combine(root, version.Path.Replace('/', Path.DirectorySeparatorChar));
                        var manifest = root is null
                            ? await ReadEmbeddedAsync(version.Path, cancellationToken).ConfigureAwait(false)
                            : await ToolSourceFileReader.ReadAsync(path, root, cancellationToken).ConfigureAwait(false);
                        if (Hash(manifest) != version.Sha256) throw new JsonException("The manifest SHA-256 doesn't match the catalogue.");
                        var definition = Definition(manifest, path, tool.DefaultVersion == version.Version);
                        if (definition.Manifest.Id != tool.Id || definition.Manifest.Version != version.Version) throw new JsonException("The manifest identity doesn't match its catalogue entry.");
                        definitions.Add(definition);
                    }
            }
            return new(ToolSourceState.Complete, identity, definitions, null);
        }
        catch (Exception exception) when (exception is JsonException or YamlException or DecoderFallbackException or ArgumentException)
        {
            return new(ToolSourceState.Invalid, identity, definitions, exception.Message);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new(ToolSourceState.Unavailable, identity, definitions, exception.Message);
        }
    }

    internal ToolDefinition Definition(byte[] bytes, string source, bool isDefault)
    {
        var json = ToolDocumentDecoder.JsonBytes(bytes, source);
        var manifest = JsonSerializer.Deserialize(json, ToolJsonContext.Default.ToolManifest) ?? throw new JsonException("The manifest cannot be null.");
        ToolManifestValidator.Validate(manifest);
        var hash = Hash(bytes);
        var blocker = _support.Blocker(manifest, hash);
        return new(manifest, source, hash, bytes.ToArray(), isDefault, blocker is null, blocker);
    }

    internal static ToolSelection Select(ToolCatalogueRead source, string id, string? version)
    {
        if (!ToolIdentity.IsToolId(id) || version is not null && !ToolIdentity.IsVersion(version)) return new(ToolSourceState.Invalid, null, "Invalid tool ID or exact version.");
        if (source.State != ToolSourceState.Complete) return new(source.State, null, source.Cause);
        var selected = source.Definitions.SingleOrDefault(definition => definition.Manifest.Id == id
            && (version is null ? definition.IsDefault : definition.Manifest.Version == version));
        if (selected is null) return new(ToolSourceState.Invalid, null, "The selected source has no matching tool pin.");
        if (!selected.Reviewed) return new(ToolSourceState.Blocked, selected, selected.Blocker);
        return new(ToolSourceState.Complete, selected, null);
    }

    internal static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static async ValueTask<byte[]> ReadEmbeddedAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = typeof(ToolCatalogueReader).Assembly.GetManifestResourceStream(EmbeddedPrefix + path)
            ?? throw new IOException("A required embedded Tool source is missing.");
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        return buffer.ToArray();
    }

    private static void Validate(ToolCatalogue catalogue)
    {
        ToolManifestValidator.Require(catalogue.Schema == ToolVocabulary.ManifestSchema, "catalogue schema must be 1");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tool in catalogue.Tools)
        {
            ToolManifestValidator.Require(tool is not null && ToolIdentity.IsToolId(tool.Id) && ids.Add(tool.Id), "invalid or duplicate catalogue ID");
            ToolManifestValidator.Require(tool.Versions is { Length: > 0 }, "catalogue versions cannot be empty");
            var versions = new HashSet<string>(StringComparer.Ordinal);
            foreach (var version in tool.Versions)
                ToolManifestValidator.Require(version is not null && ToolIdentity.IsVersion(version.Version) && versions.Add(version.Version)
                    && ToolIdentity.IsRelativePath(version.Path) && paths.Add(version.Path) && ToolIdentity.IsSha256(version.Sha256), "invalid or duplicate catalogue pin, path or digest");
            ToolManifestValidator.Require(versions.Contains(tool.DefaultVersion), "the default must name one listed version");
        }
    }
}
