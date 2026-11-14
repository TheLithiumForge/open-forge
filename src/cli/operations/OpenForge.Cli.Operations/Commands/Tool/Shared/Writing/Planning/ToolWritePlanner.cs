using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Selection;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Selection;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Planning;

internal sealed class ToolWritePlanner(ToolCatalogue catalogue, ToolWriteToolPlanner tools)
{
    internal async ValueTask<ToolWritePlan> PlanAsync(ToolWritePlanningInput input, CancellationToken cancellationToken)
    {
        List<ToolWriteFinding> findings = [];
        List<ToolWriteToolPlan> planned = [];
        List<ToolWriteManagerQuestion> questions = [];
        var usedNetwork = false;
        if (input.Records.PortableState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable
            || input.Records.MachineState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable
            || input.Records.WorkspaceMachineState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable
            || input.Records.RuntimeRecordsState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable
            || input.Records.PathRecordState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable)
        {
            findings.Add(new(ToolWriteFindingCode.RecordsUnavailable, CliSemanticStatus.Blocked,
                Cause: string.Join("\n", input.Records.Problems)));
            return Complete();
        }

        var portable = input.Records.Portable ?? new ToolPortableRecords
        {
            Settings = new Dictionary<string, ToolSetting>(),
            Lock = new Dictionary<string, ToolLockRecord>(),
        };
        if (input.Request.Kind == ToolWriteKind.Remove)
        {
            if (input.Request.Spec is not { } removal || !catalogue.Entries.TryGetValue(removal.Id, out var entry))
            {
                findings.Add(new(ToolWriteFindingCode.UnknownTool, CliSemanticStatus.Invalid, input.Request.Spec?.Id));
                return Complete();
            }
            planned.Add(await tools.PlanRemoveAsync(entry, input, cancellationToken).ConfigureAwait(false));
            AddNativeProblems(planned[^1]);
            return Complete();
        }

        var selection = new ToolWriteSelector(catalogue).Select(new()
        {
            Kind = input.Request.Kind == ToolWriteKind.Install ? ToolWriteSelectionKind.Install : ToolWriteSelectionKind.Update,
            Spec = input.Request.Spec,
            ChannelOverride = input.Request.Channel,
            Records = portable,
        });
        foreach (var issue in selection.Issues)
        {
            var code = issue.Code switch
            {
                ToolWriteSelectionIssueCode.NamedSpecRequired => ToolWriteFindingCode.InvalidInput,
                ToolWriteSelectionIssueCode.UnknownId => ToolWriteFindingCode.UnknownTool,
                ToolWriteSelectionIssueCode.UnknownChannel => ToolWriteFindingCode.ChannelInvalid,
                ToolWriteSelectionIssueCode.UpdateRequiresRecord => ToolWriteFindingCode.UpdateRequiresRecord,
                _ => throw new ArgumentOutOfRangeException(nameof(issue)),
            };
            findings.Add(new(code, CliSemanticStatus.Invalid, issue.ToolId));
        }
        if (findings.Count != 0) return Complete();

        var managers = selection.Tools.Count == 0
            ? []
            : await tools.DetectManagersAsync(input.Request.Workspace.LexicalRoot, cancellationToken).ConfigureAwait(false);
        foreach (var selected in selection.Tools)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await tools.PlanInstallAsync(selected, input, managers, catalogue.NodeRuntimes, cancellationToken).ConfigureAwait(false);
            usedNetwork |= result.UsedNetwork;
            if (result.Manager is { } question) questions.Add(question);
            findings.AddRange(result.Findings);
            if (result.Tool is not { } tool) continue;
            planned.Add(tool);
            AddNativeProblems(tool);
            foreach (var change in tool.Config?.Changes ?? [])
            {
                if (change.Disposition == ToolChangeDisposition.Conflict)
                    findings.Add(new(ToolWriteFindingCode.ConfigConflict, CliSemanticStatus.Blocked, selected.Entry.Identity.Id, Cause: change.Conflict));
            }
            foreach (var change in tool.Config?.Removals ?? [])
            {
                if (change.Disposition == ToolChangeDisposition.Conflict)
                    findings.Add(new(ToolWriteFindingCode.ConfigConflict, CliSemanticStatus.Blocked, selected.Entry.Identity.Id, Cause: change.Conflict));
            }
        }
        return Complete();

        void AddNativeProblems(ToolWriteToolPlan tool)
        {
            if (!tool.NoOp && tool.Profile is { } profile
                && !ToolWriteMinimumVersion.IsSatisfied(input.Facts.OpenForgeVersion, profile.MinimumOpenForgeVersion))
                findings.Add(new(ToolWriteFindingCode.OpenForgeVersionUnsupported, CliSemanticStatus.Blocked,
                    tool.Entry.Identity.Id, Cause: $"{input.Facts.OpenForgeVersion} < {profile.MinimumOpenForgeVersion}"));
            if (!tool.NoOp && tool.Context is null)
                findings.Add(new(ToolWriteFindingCode.NativePlanBlocked, CliSemanticStatus.Blocked, tool.Entry.Identity.Id));
            foreach (var problem in tool.Native?.Problems ?? [])
                findings.Add(new(ToolWriteFindingCode.NativePlanBlocked, CliSemanticStatus.Blocked, tool.Entry.Identity.Id, Cause: problem));
        }
        ToolWritePlan Complete() => new()
        {
            Input = input,
            Tools = planned,
            Findings = findings,
            ManagerQuestions = questions,
            UsedNetwork = usedNetwork,
        };
    }
}
