using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Detection;
using OpenForge.Cli.Core.Framework.Tools.Shared.Integration;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Observation;

/// <summary>The project settings as Tool commands see them. A usable read has a snapshot to plan the tools edit from.</summary>
internal sealed record ToolSettingsView(WorkspaceSettingsRead Read, string ArchivePath)
{
    internal bool Usable => Read.State is WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete && Read.Snapshot is not null;

    internal ImmutableArray<string> Tools => Usable ? Read.Document.Tools : [];
}

internal sealed record ToolIntegrationView(ToolIntegration Integration, ToolFileRead File, ToolIntegrationState State, string? Cause);

/// <summary>Reads what Tool commands decide from: settings, integration files, installed versions and requirements.</summary>
internal sealed class ToolObserver(ToolCatalogue catalogue, IToolProcessHost processes)
{
    internal ToolCatalogue Catalogue { get; } = catalogue;

    internal static async ValueTask<ToolSettingsView> ReadSettingsAsync(CliWorkspace workspace, CancellationToken cancellationToken)
    {
        var read = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace, cancellationToken).ConfigureAwait(false);
        var archive = WorkspaceLayout.Create(workspace, read.Document.MainRoot).MapPayloadPath(WorkspaceSettingsDefinitions.MemoryArchivePath);
        return new(read, archive);
    }

    internal static async ValueTask<ImmutableArray<ToolIntegrationView>> ReadIntegrationsAsync(
        ToolEntry entry, CliWorkspace workspace, string archivePath, CancellationToken cancellationToken)
    {
        var views = ImmutableArray.CreateBuilder<ToolIntegrationView>();
        foreach (var integration in entry.Integrations)
        {
            var file = await ToolFileReader.ReadAsync(workspace, integration.Path, cancellationToken).ConfigureAwait(false);
            if (file.Snapshot is null)
            {
                views.Add(new(integration, file, ToolIntegrationState.Conflict, file.Cause));
                continue;
            }
            var (state, cause) = ToolIntegrationEditor.Read(integration, file.Bytes, archivePath);
            views.Add(new(integration, file, state, cause));
        }
        return views.ToImmutable();
    }

    /// <summary>The installed version, an empty string for an installed tool that printed no version, or null when it isn't installed.</summary>
    internal async ValueTask<string?> DetectAsync(ToolEntry entry, CliWorkspace workspace, CancellationToken cancellationToken)
    {
        var result = await processes.RunAsync(new(entry.Detection, workspace.LexicalRoot, ToolDefinitions.ProbeTimeout), cancellationToken).ConfigureAwait(false);
        return result.Succeeded ? ToolVersionText.Read(result.OutputTail) ?? string.Empty : null;
    }

    internal async ValueTask<ImmutableArray<ToolRequirement>> MissingRequirementsAsync(ToolEntry entry, CliWorkspace workspace, CancellationToken cancellationToken)
    {
        var missing = ImmutableArray.CreateBuilder<ToolRequirement>();
        foreach (var requirement in ToolDefinitions.Requirements(entry.Method, Catalogue.Platform))
        {
            var probe = await processes.RunAsync(
                new(ToolDefinitions.Probe(requirement), workspace.LexicalRoot, ToolDefinitions.ProbeTimeout), cancellationToken).ConfigureAwait(false);
            if (!probe.Succeeded) missing.Add(requirement);
        }
        return missing.ToImmutable();
    }
}
