using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;

internal static class FrameworkPayloadSelection
{
    private static readonly WorkspaceLayout DefaultLayout = WorkspaceLayout.Create(
        new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory),
        WorkspaceLayout.DefaultMainRoot);

    internal static bool IncludesPath(string path, WorkspaceSettingsDocument settings)
        => IncludesPath(DefaultLayout, path, settings);

    internal static bool IncludesPath(WorkspaceLayout layout, string packagePath, WorkspaceSettingsDocument settings)
        => IncludesInstalledPath(layout, layout.MapPayloadPath(packagePath), settings);

    internal static bool IncludesInstalledPath(WorkspaceLayout layout, string path, WorkspaceSettingsDocument settings)
    {
        if (WorkspaceRemovals.IsPathRemoved(layout, path, settings))
        {
            return false;
        }

        foreach (var removed in settings.RemovedFiles)
        {
            if (SourceFormClassifier.Matches(layout, removed, SourceDocumentForm.Skill)
                && path.StartsWith($"{SourceLogicalPath.ReadParent(removed)}/", StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    internal static string? FindMissingRequiredAncestor(
        FrameworkPayload payload,
        WorkspaceSettingsDocument settings,
        GeneratedNavigationFormation formation)
        => FindMissingRequiredAncestor(DefaultLayout, payload, settings, formation);

    internal static string? FindMissingRequiredAncestor(
        WorkspaceLayout layout,
        FrameworkPayload payload,
        WorkspaceSettingsDocument settings,
        GeneratedNavigationFormation formation)
    {
        var selectedEntrypoints = payload.Assets
            .Select(asset => layout.MapPayloadPath(asset.Path))
            .Where(path => IncludesInstalledPath(layout, path, settings)
                && SourceFormClassifier.TryClassify(layout, path, out var form)
                && SourceFormClassifier.IsEntrypoint(form))
            .ToArray();

        foreach (var excluded in payload.Assets.Select(asset => layout.MapPayloadPath(asset.Path))
                     .Where(path => !IncludesInstalledPath(layout, path, settings)))
        {
            if (formation.Sources.Any(source =>
                    string.Equals(source.Identity.CanonicalBasePath, excluded, StringComparison.Ordinal)))
            {
                continue;
            }

            if (excluded == layout.LoaderPath)
            {
                if (selectedEntrypoints.Length > 0)
                {
                    return excluded;
                }

                continue;
            }

            if (!SourceFormClassifier.TryClassify(layout, excluded, out var excludedForm)
                || !SourceFormClassifier.IsEntrypoint(excludedForm))
            {
                continue;
            }

            var excludedDirectory = SourceLogicalPath.ReadParent(excluded);
            var hasUnreachableDescendant = selectedEntrypoints.Any(path =>
                SourceLogicalPath.ReadParent(path).StartsWith(
                    $"{excludedDirectory}/",
                    StringComparison.Ordinal)
                && formation.Topology.ReadAbsoluteDepth(path) is null);
            if (hasUnreachableDescendant)
            {
                return excluded;
            }
        }

        return null;
    }
}
