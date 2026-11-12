using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;

internal sealed record ToolWorkspaceRead(WorkspaceSettingsRead Settings, WorkspaceOwnershipRead Lock,
    ToolCoverage[] Coverage, ToolReadIssue[] Issues)
{
    internal ToolInstalledRow[] Installed(string? role = null)
    {
        var rows = new List<ToolInstalledRow>();
        foreach (var (id, setting) in Settings.Document.Tools.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            Lock.Document.Tools.TryGetValue(id, out var receipt);
            string[]? capabilities = null;
            if (receipt is not null)
                capabilities = new ToolCatalogueReader().Definition(receipt.ManifestBytes, "receipt." + receipt.ManifestFormat, false).Manifest.Capabilities;
            if (role is not null && (role != ToolVocabulary.CodeNavigationRole
                || capabilities is not null && !capabilities.Contains(ToolVocabulary.CodeGraphCapability, StringComparer.Ordinal))) continue;
            var verification = new ToolObservation(ToolEvidenceState.Matching);
            if (Lock.State != WorkspaceOwnershipReadState.Complete || receipt is null)
                verification = new(ToolEvidenceState.Unavailable, Lock.Cause ?? "A matching tool receipt isn't recorded.");
            else if (receipt.Version != setting.Version)
                verification = new(ToolEvidenceState.Changed, "The settings pin differs from the receipt pin.");
            rows.Add(new(id, setting.Version, setting.Roles, capabilities, verification));
        }
        return [.. rows];
    }
}
