using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Shared.Integration;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.List;

/// <summary>Reads settings, integration files and installed versions. It writes nothing and runs only detection commands.</summary>
internal sealed class ToolListOperation(ToolObserver observer)
{
    internal async ValueTask<ToolListResult> ExecuteAsync(ToolListRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var findings = new List<ToolFinding>();
            var settings = await ToolObserver.ReadSettingsAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
            if (!settings.Usable)
            {
                findings.Add(new(ToolFindingCode.SettingsUnavailable, CliSemanticStatus.Incomplete, Detail: settings.Read.Cause)
                {
                    Path = WorkspaceSettingsDefinitions.RelativePath,
                });
            }
            var rows = new List<ToolListEntry>();
            foreach (var entry in observer.Catalogue.Entries)
            {
                var integrations = await ToolObserver.ReadIntegrationsAsync(entry, request.Workspace, settings.ArchivePath, cancellationToken).ConfigureAwait(false);
                findings.AddRange(integrations.Where(view => view.State == ToolIntegrationState.Conflict).Select(view =>
                    new ToolFinding(ToolFindingCode.IntegrationConflict, CliSemanticStatus.Incomplete, entry.Id, view.Cause) { Path = view.Integration.Path }));
                var version = await observer.DetectAsync(entry, request.Workspace, cancellationToken).ConfigureAwait(false);
                rows.Add(new()
                {
                    Id = entry.Id,
                    Name = entry.Name,
                    Category = entry.Category,
                    Purpose = entry.Purpose,
                    Method = ToolDefinitions.MethodName(entry.Method),
                    Upstream = entry.Upstream.ToString(),
                    Licence = entry.Licence,
                    Configured = settings.Tools.Contains(entry.Id, StringComparer.Ordinal),
                    Integrated = integrations.All(view => view.State == ToolIntegrationState.Present),
                    Installed = version is not null,
                    Version = string.IsNullOrEmpty(version) ? null : version,
                });
            }
            return new()
            {
                Workspace = request.Workspace,
                Status = findings.Count == 0 ? CliSemanticStatus.Complete : CliSemanticStatus.Incomplete,
                Data = new(rows),
                Findings = findings,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new()
            {
                Workspace = request.Workspace,
                Status = CliSemanticStatus.Interrupted,
                Data = new([]),
                Findings = [new(ToolFindingCode.Interrupted, CliSemanticStatus.Interrupted)],
            };
        }
    }
}
