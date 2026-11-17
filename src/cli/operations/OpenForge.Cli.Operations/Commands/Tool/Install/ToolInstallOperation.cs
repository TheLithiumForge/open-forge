using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Application;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Install;

/// <summary>
/// Named Install installs a missing tool, runs its own project setup and adds Open Forge's changes. Bare Install only
/// installs the project's configured tools that are missing on this machine.
/// </summary>
internal sealed class ToolInstallOperation(ToolObserver observer, ToolSteps runner, ToolFileApplication files, ToolInteraction interaction)
{
    internal async ValueTask<ToolWriteResult> ExecuteAsync(ToolWriteRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            if (request.Id is not { } id) return await InstallMissingAsync(request, cancellationToken).ConfigureAwait(false);
            return await InstallAsync(request, observer.Catalogue.Find(id) ?? throw new InvalidOperationException("Binding admits only catalogue IDs."),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ToolResults.Stop(request, new(ToolFindingCode.Interrupted, CliSemanticStatus.Interrupted, request.Id));
        }
    }

    private async ValueTask<ToolWriteResult> InstallAsync(ToolWriteRequest request, ToolEntry entry, CancellationToken cancellationToken)
    {
        var plan = await ToolEditPlanner.PlanAsync(entry, request.Workspace, removal: false, cancellationToken).ConfigureAwait(false);
        if (plan.Conflict is { } conflict) return ToolResults.Stop(request, conflict);
        var version = await observer.DetectAsync(entry, request.Workspace, cancellationToken).ConfigureAwait(false);
        var planned = ToolSteps.Plan(ToolWire.Install, entry.Install, version is null ? ToolWire.Planned : ToolWire.Skipped)
            .Concat(entry.Setup is { } setup ? ToolSteps.Plan(ToolWire.Setup, setup) : []).ToList();
        var tool = new ToolWriteTool
        {
            Id = entry.Id,
            Name = entry.Name,
            InstalledVersion = version,
            Steps = [.. planned.Select(item => item.Step)],
            Changes = plan.Changes,
        };
        if (version is null && await MissingAsync(entry, request, cancellationToken).ConfigureAwait(false) is { Count: > 0 } missing)
            return ToolResults.Create(request, CliSemanticStatus.Blocked, [tool], missing);
        if (request.Preview) return ToolResults.Create(request, CliSemanticStatus.Complete, [tool]);
        var consent = await ToolResults.AskAsync(request, interaction, ToolResults.Create(request, CliSemanticStatus.Complete, [tool]), cancellationToken)
            .ConfigureAwait(false);
        if (consent != ToolConsent.Accepted) return ToolResults.Unconsented(request, consent, [tool]);

        var (ran, failure) = await runner.RunAsync(planned, entry.Id, request.Workspace, cancellationToken).ConfigureAwait(false);
        tool = tool with { Steps = ran };
        if (failure is not null)
            return ToolResults.Create(request, CliSemanticStatus.Failed, [tool with { Changes = ToolResults.WithOutcomes(tool.Changes, 0, failed: false) }], [failure]);

        // The tool's own setup can touch the workspace, so Open Forge plans its edits again from what is there now.
        var fresh = await ToolEditPlanner.PlanAsync(entry, request.Workspace, removal: false, cancellationToken).ConfigureAwait(false);
        if (fresh.Conflict is { } later)
            return ToolResults.Create(request, CliSemanticStatus.Failed, [tool with { Changes = ToolResults.WithOutcomes(tool.Changes, 0, failed: false) }],
                [later with { Status = CliSemanticStatus.Failed }]);
        var applied = await files.ApplyAsync(request.Workspace, request.Kind, fresh.Edits, cancellationToken).ConfigureAwait(false);
        tool = tool with { Changes = ToolResults.WithOutcomes(fresh.Changes, applied.Applied, applied.Failure is not null) };
        if (applied.Failure is not { } code) return ToolResults.Create(request, CliSemanticStatus.Complete, [tool]);
        var status = code == ToolFindingCode.Interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed;
        return ToolResults.Create(request, status, [tool], [new(code, status, entry.Id, applied.Cause)]);
    }

    private async ValueTask<ToolWriteResult> InstallMissingAsync(ToolWriteRequest request, CancellationToken cancellationToken)
    {
        var settings = await ToolObserver.ReadSettingsAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
        if (!settings.Usable)
            return ToolResults.Stop(request, new(ToolFindingCode.SettingsUnavailable, CliSemanticStatus.Blocked, Detail: settings.Read.Cause)
            {
                Path = WorkspaceSettingsDefinitions.RelativePath,
            });
        var tools = new List<ToolWriteTool>();
        var planned = new List<(ToolEntry Entry, List<ToolPlannedStep> Steps)>();
        var findings = new List<ToolFinding>();
        foreach (var entry in observer.Catalogue.Entries.Where(entry => settings.Tools.Contains(entry.Id, StringComparer.Ordinal)))
        {
            var version = await observer.DetectAsync(entry, request.Workspace, cancellationToken).ConfigureAwait(false);
            var steps = ToolSteps.Plan(ToolWire.Install, entry.Install, version is null ? ToolWire.Planned : ToolWire.Skipped).ToList();
            tools.Add(new() { Id = entry.Id, Name = entry.Name, InstalledVersion = version, Steps = [.. steps.Select(item => item.Step)] });
            if (version is not null) continue;
            planned.Add((entry, steps));
            findings.AddRange(await MissingAsync(entry, request, cancellationToken).ConfigureAwait(false));
        }
        if (findings.Count != 0) return ToolResults.Create(request, CliSemanticStatus.Blocked, tools, findings);
        if (planned.Count == 0 || request.Preview) return ToolResults.Create(request, CliSemanticStatus.Complete, tools);
        var consent = await ToolResults.AskAsync(request, interaction, ToolResults.Create(request, CliSemanticStatus.Complete, tools), cancellationToken)
            .ConfigureAwait(false);
        if (consent != ToolConsent.Accepted) return ToolResults.Unconsented(request, consent, tools);

        ToolFinding? failure = null;
        foreach (var (entry, steps) in planned)
        {
            var index = tools.FindIndex(tool => tool.Id == entry.Id);
            if (failure is not null)
            {
                tools[index] = tools[index] with { Steps = [.. tools[index].Steps.Select(step => step with { Outcome = ToolWire.NotStarted })] };
                continue;
            }
            var (ran, failed) = await runner.RunAsync(steps, entry.Id, request.Workspace, cancellationToken).ConfigureAwait(false);
            tools[index] = tools[index] with { Steps = ran };
            failure = failed;
        }
        return failure is null
            ? ToolResults.Create(request, CliSemanticStatus.Complete, tools)
            : ToolResults.Create(request, CliSemanticStatus.Failed, tools, [failure]);
    }

    private async ValueTask<List<ToolFinding>> MissingAsync(ToolEntry entry, ToolWriteRequest request, CancellationToken cancellationToken)
        => [.. (await observer.MissingRequirementsAsync(entry, request.Workspace, cancellationToken).ConfigureAwait(false))
            .Select(requirement => new ToolFinding(ToolFindingCode.RequirementMissing, CliSemanticStatus.Blocked, entry.Id,
                ToolDefinitions.RequirementName(requirement)))];
}
