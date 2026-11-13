using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Inspect.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Models.Provider;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Inspect;

internal sealed class ToolInspectOperation(ToolCatalogueReader reader)
{
    internal async ValueTask<ToolInspectResult> ExecuteAsync(ToolInspectRequest request, CancellationToken token)
    {
        try { return await ReadAsync(request, token).ConfigureAwait(false); }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return new()
            {
                Workspace = request.Workspace,
                Status = CliSemanticStatus.Interrupted,
                Issues = [new(ToolReadIssueCode.Cancelled, CliSemanticStatus.Interrupted, "tool inspect", "The read was cancelled.")]
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new()
            {
                Workspace = request.Workspace,
                Status = CliSemanticStatus.Incomplete,
                Issues = [new(ToolReadIssueCode.ReadUnavailable, CliSemanticStatus.Incomplete, "tool inspect", "A required local observation couldn't be read.")]
            };
        }
    }

    private async ValueTask<ToolInspectResult> ReadAsync(ToolInspectRequest request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var catalogue = await reader.ReadAsync(request.Source, token).ConfigureAwait(false);
        var selection = ToolCatalogueReader.Select(catalogue, request.Id, request.Version);
        var workspace = await ToolWorkspaceReader.ReadAsync(request.Workspace, token).ConfigureAwait(false);
        var issues = new List<ToolReadIssue>(workspace.Issues);
        if (selection.State != ToolSourceState.Complete) issues.Add(new(selection.State switch
        {
            ToolSourceState.Invalid => ToolReadIssueCode.DefinitionInvalid,
            ToolSourceState.Blocked => ToolReadIssueCode.PinNotSupported,
            _ => ToolReadIssueCode.CatalogueUnavailable,
        }, selection.State switch
        {
            ToolSourceState.Invalid => CliSemanticStatus.Invalid,
            ToolSourceState.Blocked => CliSemanticStatus.Blocked,
            _ => CliSemanticStatus.Incomplete,
        }, request.Id, selection.Cause ?? "The selected definition isn't available."));
        var manifest = selection.Definition?.Manifest;
        var requirements = new List<string>();
        if (selection.Cause is { } blocker) requirements.Add(blocker);
        if (manifest is not null)
        {
            requirements.AddRange(manifest.Checks);
            if (manifest.AdmissionReason is { } reason) requirements.Add(reason);
            if (manifest.Targets.GetValueOrDefault(ToolNativeTarget.Current) is { Reason: { } targetReason }) requirements.Add(targetReason);
        }
        return new()
        {
            Workspace = request.Workspace,
            Status = CliStatusDefinitions.Collapse(issues.Select(issue => issue.Status)),
            Id = request.Id,
            Version = manifest?.Version,
            Definition = manifest is null ? null : JsonSerializer.SerializeToElement(manifest, ToolJsonContext.Default.ToolManifest),
            Target = ToolNativeTarget.Current,
            Installed = Installed(workspace, request.Id),
            Telemetry = workspace.Coverage[0].State == ToolEvidenceState.Unavailable
                ? null : workspace.Settings.Document.Tools.GetValueOrDefault(request.Id)?.Telemetry ?? false,
            Requirements = [.. requirements],
            Limitations = manifest is null ? [] : [manifest.Runtime.Notes, manifest.Index.Freshness, manifest.Qualification.Accuracy],
            Instructions = new(null, null, null, null, "A verified installation and local store bindings are needed to resolve external commands."),
            Issues = [.. issues],
            Coverage = workspace.Coverage,
        };
    }

    private static ToolInstalledFact? Installed(ToolWorkspaceRead workspace, string id)
    {
        if (workspace.Coverage[0].State == ToolEvidenceState.Unavailable)
            return new(null, new(ToolEvidenceState.Unavailable, workspace.Settings.Cause));
        var row = workspace.Installed().SingleOrDefault(row => row.Id == id);
        return row is null ? null : new(row, row.Verification);
    }

}
