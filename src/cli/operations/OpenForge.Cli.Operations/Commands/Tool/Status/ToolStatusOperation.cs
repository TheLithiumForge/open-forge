using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Status;

internal sealed class ToolStatusOperation
{
    internal async ValueTask<ToolStatusResult> ExecuteAsync(ToolStatusRequest request, CancellationToken token)
    {
        try { return await ReadAsync(request, token).ConfigureAwait(false); }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return new()
            {
                Workspace = request.Workspace,
                Status = CliSemanticStatus.Interrupted,
                Issues = [new(ToolReadIssueCode.Cancelled, CliSemanticStatus.Interrupted, "tool status", "The read was cancelled.")]
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new()
            {
                Workspace = request.Workspace,
                Status = CliSemanticStatus.Incomplete,
                Issues = [new(ToolReadIssueCode.ReadUnavailable, CliSemanticStatus.Incomplete, "tool status", "A required local observation couldn't be read.")]
            };
        }
    }

    private async ValueTask<ToolStatusResult> ReadAsync(ToolStatusRequest request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var workspace = await ToolWorkspaceReader.ReadAsync(request.Workspace, token).ConfigureAwait(false);
        var issues = new List<ToolReadIssue>(workspace.Issues);
        var installed = workspace.Installed();
        if (request.Id is not null && workspace.Coverage[0].State == ToolEvidenceState.Complete && installed.All(row => row.Id != request.Id))
            issues.Add(new(ToolReadIssueCode.ToolNotInstalled, CliSemanticStatus.Invalid, request.Id, "The tool isn't registered in this workspace."));
        var rows = new List<ToolStatusRow>();
        foreach (var row in installed.Where(row => request.Id is null || row.Id == request.Id))
        {
            workspace.Lock.Document.Tools.TryGetValue(row.Id, out var receipt);
            var observed = await ToolPassiveStatusReader.ReadAsync(request.Workspace, workspace.Settings.Document, row, receipt, token).ConfigureAwait(false);
            rows.Add(observed);
            AddIssue(issues, row.Id, observed.Installation, (ToolReadIssueCode.StoreUnavailable, ToolReadIssueCode.InstalledFileMissing, ToolReadIssueCode.InstalledFileChanged));
            AddIssue(issues, row.Id, observed.Configuration, (ToolReadIssueCode.HostUnavailable, ToolReadIssueCode.MemberChanged, ToolReadIssueCode.MemberChanged));
            AddIssue(issues, row.Id, observed.Guidance, (ToolReadIssueCode.UsageSkillUnavailable, ToolReadIssueCode.UsageSkillMissing, ToolReadIssueCode.UsageSkillChanged));
            if (observed.Guidance.State == ToolEvidenceState.Excluded)
                issues.Add(new(ToolReadIssueCode.PathExcluded, CliSemanticStatus.Attention, row.Id, observed.Guidance.Cause ?? "The usage Skill is excluded."));
            if (observed.Index.State == ToolEvidenceState.Unavailable)
                issues.Add(new(ToolReadIssueCode.IndexUnavailable, CliSemanticStatus.Incomplete, row.Id, observed.Index.Cause ?? "The native index observation is unavailable."));
            else if (row.Roles.Length > 0 && observed.Index.State == ToolEvidenceState.NeedsIndex)
                issues.Add(new(ToolReadIssueCode.IndexMissing, CliSemanticStatus.Attention, row.Id, "The active provider needs an index."));
            if (row.Roles.Length > 0 && observed.Limits.Length > 0)
                issues.Add(new(ToolReadIssueCode.NativeLimits, CliSemanticStatus.Attention, row.Id, string.Join(" ", observed.Limits)));
        }
        return new()
        {
            Workspace = request.Workspace,
            Status = CliStatusDefinitions.Collapse(issues.Select(issue => issue.Status)),
            Tools = [.. rows],
            Coverage = workspace.Coverage,
            Issues = [.. issues],
        };
    }

    private static void AddIssue(List<ToolReadIssue> issues, string id, ToolObservation fact,
        (ToolReadIssueCode Unavailable, ToolReadIssueCode Missing, ToolReadIssueCode Changed) codes)
    {
        if (fact.State == ToolEvidenceState.Unavailable)
            issues.Add(new(codes.Unavailable, CliSemanticStatus.Incomplete, id, fact.Cause ?? "The passive observation is unavailable."));
        else if (fact.State == ToolEvidenceState.Missing)
            issues.Add(new(codes.Missing, CliSemanticStatus.Attention, id, fact.Cause ?? "The recorded file is missing."));
        else if (fact.State is ToolEvidenceState.Changed or ToolEvidenceState.Drifted)
            issues.Add(new(codes.Changed, CliSemanticStatus.Attention, id, fact.Cause ?? "The recorded value has changed."));
    }

}
