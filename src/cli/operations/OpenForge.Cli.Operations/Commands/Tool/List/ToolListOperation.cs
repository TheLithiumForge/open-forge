using OpenForge.Cli.Core.Commands.Tool.List.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Models.Provider;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.List;

internal sealed class ToolListOperation(ToolCatalogueReader reader)
{
    internal async ValueTask<ToolListResult> ExecuteAsync(ToolListRequest request, CancellationToken token)
    {
        try { return await ReadAsync(request, token).ConfigureAwait(false); }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return new()
            {
                Workspace = request.Workspace,
                Status = CliSemanticStatus.Interrupted,
                Issues = [new(ToolReadIssueCode.Cancelled, CliSemanticStatus.Interrupted, "tool list", "The read was cancelled.")]
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new()
            {
                Workspace = request.Workspace,
                Status = CliSemanticStatus.Incomplete,
                Issues = [new(ToolReadIssueCode.ReadUnavailable, CliSemanticStatus.Incomplete, "tool list", "A required local observation couldn't be read.")]
            };
        }
    }

    private async ValueTask<ToolListResult> ReadAsync(ToolListRequest request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var catalogue = await reader.ReadAsync(request.Source, token).ConfigureAwait(false);
        var workspace = await ToolWorkspaceReader.ReadAsync(request.Workspace, token).ConfigureAwait(false);
        var issues = new List<ToolReadIssue>(workspace.Issues);
        if (catalogue.State != ToolSourceState.Complete)
            issues.Add(new(catalogue.State == ToolSourceState.Invalid ? ToolReadIssueCode.CatalogueInvalid : ToolReadIssueCode.CatalogueUnavailable,
                catalogue.State == ToolSourceState.Invalid ? CliSemanticStatus.Invalid : CliSemanticStatus.Incomplete,
                catalogue.Source, catalogue.Cause ?? "The catalogue isn't available."));
        var available = catalogue.Definitions.Where(definition => request.Role is null
                || request.Role == ToolVocabulary.CodeNavigationRole && definition.Manifest.Capabilities.Contains(ToolVocabulary.CodeGraphCapability, StringComparer.Ordinal))
            .OrderBy(definition => ToolVocabulary.OriginOrder(definition.Manifest.Origin))
            .ThenBy(definition => definition.Manifest.Id, StringComparer.Ordinal)
            .ThenBy(definition => definition.Manifest.Version, StringComparer.Ordinal)
            .Select(definition => new ToolAvailableRow(definition.Manifest.Id, definition.Manifest.Version, definition.Manifest.Origin,
                definition.Manifest.Capabilities, ToolNativeTarget.Current,
                definition.Manifest.Targets.GetValueOrDefault(ToolNativeTarget.Current)?.State ?? "unsupported",
                definition.Manifest.Admission, definition.Reviewed, definition.Blocker)).ToArray();
        return new()
        {
            Workspace = request.Workspace,
            Status = CliStatusDefinitions.Collapse(issues.Select(issue => issue.Status)),
            Available = available,
            Installed = workspace.Installed(request.Role),
            Catalogue = new(catalogue.Source, catalogue.State.ToString().ToLowerInvariant(), catalogue.Cause),
            Coverage = [.. workspace.Coverage, new("catalogue", catalogue.State == ToolSourceState.Complete ? ToolEvidenceState.Complete : ToolEvidenceState.Unavailable, catalogue.Cause)],
            RoleFilter = request.Role,
            Issues = [.. issues],
        };
    }
}
