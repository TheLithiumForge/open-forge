using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;

internal static class FrameworkPayloadSelection
{
    internal static bool IncludesPath(string path, WorkspaceSettingsDocument settings)
        => !WorkspaceRemovals.IsPathRemoved(path, settings);

    internal static string? FindMissingRequiredAncestor(
        FrameworkPayload payload,
        WorkspaceSettingsDocument settings,
        GeneratedNavigationFormation formation)
    {
        var selectedEntrypoints = payload.Assets
            .Where(asset => IncludesPath(asset.Path, settings)
                && SourceFormClassifier.TryClassify(asset.Path, out var form)
                && SourceFormClassifier.IsEntrypoint(form))
            .ToArray();

        foreach (var excluded in payload.Assets.Where(asset => !IncludesPath(asset.Path, settings)))
        {
            if (formation.Sources.Any(source =>
                    string.Equals(source.Identity.CanonicalBasePath, excluded.Path, StringComparison.Ordinal)))
            {
                continue;
            }

            if (excluded.Path == FrameworkPayloadAsset.LoaderPath)
            {
                if (selectedEntrypoints.Length > 0)
                {
                    return excluded.Path;
                }

                continue;
            }

            if (!SourceFormClassifier.TryClassify(excluded.Path, out var excludedForm)
                || !SourceFormClassifier.IsEntrypoint(excludedForm))
            {
                continue;
            }

            var excludedDirectory = SourceLogicalPath.ReadParent(excluded.Path);
            var hasUnreachableDescendant = selectedEntrypoints.Any(asset =>
                SourceLogicalPath.ReadParent(asset.Path).StartsWith(
                    $"{excludedDirectory}/",
                    StringComparison.Ordinal)
                && formation.Topology.ReadAbsoluteDepth(asset.Path) is null);
            if (hasUnreachableDescendant)
            {
                return excluded.Path;
            }
        }

        return null;
    }
}
