using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Composition.Shared.ToolComposition;

internal static class ToolProjectManagerReader
{
    internal const string NpmLockfile = "package-lock.json";
    private static readonly IReadOnlyDictionary<string, ToolNpmClient> Lockfiles = new Dictionary<string, ToolNpmClient>(StringComparer.Ordinal)
    {
        [NpmLockfile] = ToolNpmClient.Npm,
        ["npm-shrinkwrap.json"] = ToolNpmClient.Npm,
        ["pnpm-lock.yaml"] = ToolNpmClient.Pnpm,
        ["yarn.lock"] = ToolNpmClient.YarnClassic,
        ["bun.lock"] = ToolNpmClient.Bun,
        ["bun.lockb"] = ToolNpmClient.Bun,
    };

    internal static IReadOnlyList<ToolNpmClient> Read(CliWorkspace workspace, CancellationToken token)
    {
        var resolver = new PhysicalPathResolver();
        return Lockfiles.Where(pair => NoFollowLeafObserver.Observe(resolver, workspace,
                Path.Combine(workspace.LexicalRoot, pair.Key), token).State == NoFollowLeafState.OrdinaryFile)
            .Select(pair => pair.Value).Distinct().ToArray();
    }
}
