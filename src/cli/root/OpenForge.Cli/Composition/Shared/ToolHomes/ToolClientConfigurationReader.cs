using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Composition.Shared.ToolHomes;

internal static class ToolClientConfigurationReader
{
    internal const int MaximumConfigurationBytes = 8 * 1024 * 1024;
    internal const string RovoConfiguration = ".rovodev/config.yml";
    internal const string RovoMcp = "mcp";
    internal const string RovoMcpPath = "mcpConfigPath";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static string Rovo(string home)
    {
        var configuration = ToolHomePaths.Join(home, RovoConfiguration);
        if (Read(configuration) is { } text)
        {
            var yaml = new YamlDocumentParser().Parse(text);
            if (yaml.State != YamlDocumentState.Complete || yaml.Root is { Mapping: null }
                || yaml.Root?.ContainsAlias == true || yaml.Root?.ContainsUnsupportedMapping == true)
                throw new IOException("The native Rovo configuration is unavailable or ambiguous.");
            if (yaml.Root is { } root && root.TryGetMappingValue(RovoMcp, out var mcp) && mcp is not null && mcp.TryGetMappingValue(RovoMcpPath, out var value))
            {
                if (value?.Scalar is not { } scalar || string.IsNullOrWhiteSpace(scalar.Value) || scalar.Value.Contains('\n') || !scalar.IsPlainImplicit && !scalar.IsQuotedImplicit)
                    throw new IOException("The native Rovo MCP path requires a scalar path.");
                var path = ToolHomeEnvironment.Expand(scalar.Value, home)
                    ?? throw new IOException("The native Rovo MCP path must be absolute or home-relative.");
                var lexicalRoot = ToolHomePaths.Join(home, ".rovodev");
                var rootPath = ToolHomePaths.Resolve(lexicalRoot, directory: true);
                var contained = new PhysicalPathResolver().ResolveCandidate(lexicalRoot, rootPath, path);
                if (contained.State is not (PhysicalPathState.Contained or PhysicalPathState.Missing))
                    throw new IOException("The native Rovo MCP path crosses its client directory.");
                path = ToolHomePaths.Resolve(path, directory: false);
                if (!PhysicalContainment.Contains(rootPath, path) || PhysicalIdentityTracker.PathComparer.Equals(rootPath, path))
                    throw new IOException("The native Rovo MCP path must stay below its client directory.");
                return path;
            }
        }
        return ToolHomePaths.Choose(home, ".rovodev/mcp.json", ".rovodev/mcp_config.json");
    }

    internal static string? Read(string path)
    {
        path = ToolHomePaths.Resolve(path, directory: false);
        if (!File.Exists(path)) return null;
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length > MaximumConfigurationBytes) throw new IOException("The native client configuration exceeds its read bound.");
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        if (stream.ReadByte() != -1) throw new IOException("The native client configuration changed during its read.");
        return StrictUtf8.GetString(bytes);
    }
}
