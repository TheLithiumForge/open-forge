using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal static class ToolArtifactSource
{
    internal const string GitHubHost = "github.com";
    internal const string NodeHost = "nodejs.org";
    internal static readonly IReadOnlySet<string> GitHubRedirectHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "release-assets.githubusercontent.com", "objects.githubusercontent.com",
    };

    internal static bool IsOfficial(ToolReleaseAsset asset)
    {
        var source = asset.Url;
        if (!IsHttps(source) || ToolVersionService.ReadStable(asset.Version) is null) return false;
        var nodePrefix = $"/dist/v{asset.Version}/";
        if (source.Host == NodeHost) return source.AbsolutePath.StartsWith(nodePrefix, StringComparison.Ordinal) && source.AbsolutePath.Length > nodePrefix.Length;
        if (source.Host != GitHubHost) return false;
        var segments = source.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length == 6 && segments[2] == "releases" && segments[3] == "download" && segments[4] == $"v{asset.Version}";
    }

    internal static bool IsHttps(Uri uri)
        => uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeHttps && uri.IsDefaultPort && uri.UserInfo.Length == 0 && uri.Query.Length == 0 && uri.Fragment.Length == 0;

    internal static bool AllowsRedirect(Uri original, Uri destination)
        => destination.IsAbsoluteUri && destination.Scheme == Uri.UriSchemeHttps && destination.IsDefaultPort && destination.UserInfo.Length == 0
            && (destination.Host == original.Host || original.Host == GitHubHost && GitHubRedirectHosts.Contains(destination.Host));
}
