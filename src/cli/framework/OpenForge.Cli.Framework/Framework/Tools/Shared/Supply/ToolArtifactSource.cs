using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;
using OpenForge.Cli.Core.Framework.Tools.Models.Supply;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal static class ToolArtifactSource
{
    internal const string GitHubHost = "github.com";
    internal const string NodeHost = "nodejs.org";
    internal const string RegistryHost = "registry.npmjs.org";

    internal static Uri Resolve(ToolManifest manifest, ToolArtifact artifact)
        => new(artifact.Url ?? $"{manifest.DownloadBase.TrimEnd('/')}/{artifact.File}", UriKind.Absolute);

    internal static bool IsOfficial(ToolManifest manifest, ToolArtifact artifact)
    {
        var source = Resolve(manifest, artifact);
        if (!source.IsDefaultPort) return false;
        if (source.Host == GitHubHost)
        {
            var repository = new Uri(manifest.Source.Repository);
            var prefix = $"{repository.AbsolutePath.TrimEnd('/')}/releases/download/{Uri.EscapeDataString(manifest.Source.Tag)}/";
            return repository.Host == GitHubHost && source.AbsolutePath.StartsWith(prefix, StringComparison.Ordinal)
                && source.AbsolutePath.Length > prefix.Length;
        }
        if (manifest.Runtime.Kind != "locked-package") return false;
        if (source.Host == RegistryHost) return true;
        var nodePrefix = $"/dist/v{manifest.Runtime.Version}/";
        return source.Host == NodeHost && ToolIdentity.IsVersion(manifest.Runtime.Version)
            && source.AbsolutePath.StartsWith(nodePrefix, StringComparison.Ordinal) && source.AbsolutePath.Length > nodePrefix.Length;
    }
}
