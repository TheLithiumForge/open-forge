using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;

internal static partial class RepairPresentation
{
    internal static string RenderHuman(CliPresentationRequest<RepairResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, result.Mode == RepairMode.DryRun
            ? "Preview of selected repairs"
            : "Open Forge repair");
        builder.AppendLine($"Mode: {RepairDefinitions.ReadMachineName(result.Mode)}");
        builder.AppendLine($"Selection: {RepairDefinitions.ReadMachineName(result.SelectionMode)}");
        builder.AppendLine($"Automatic: {result.Automatic.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Diagnosis coverage: {Coverage(result.Diagnosis)}");
        builder.AppendLine(
            $"Selected: {result.Counts.SelectedFindings.ToString(CultureInfo.InvariantCulture)} findings / "
            + $"{result.Counts.SelectedEffects.ToString(CultureInfo.InvariantCulture)} effects");
        builder.AppendLine($"Unselected: {result.Counts.UnselectedFindings.ToString(CultureInfo.InvariantCulture)} findings");
        builder.AppendLine(
            $"Remaining: {result.Counts.Remaining.ToString(CultureInfo.InvariantCulture)}; "
            + $"manual {result.Counts.Manual.ToString(CultureInfo.InvariantCulture)}; "
            + $"guided {result.Counts.Guided.ToString(CultureInfo.InvariantCulture)}; "
            + $"blocked {result.Counts.Blocked.ToString(CultureInfo.InvariantCulture)}");
        builder.AppendLine(CultureInfo.InvariantCulture,
            $"Repaired: {result.Counts.Repaired}; new findings: {result.Counts.NewFindings}; verified effects: {result.Counts.VerifiedEffects}");
        builder.AppendLine($"Affected paths: {Values(result.AffectedPaths)}");

        if (result.Selection is { Libraries: var libraries } && (!libraries.Selected.IsEmpty || !libraries.Unselected.IsEmpty)
            || result.LibraryExecution is not null)
        {
            RepairLibraryPresentation.Append(builder, result);
        }

        if (result.Plan is { } plan)
        {
            builder.AppendLine(
                $"Plan: {plan.Steps.Count.ToString(CultureInfo.InvariantCulture)} steps / "
                + $"{plan.Effects.Count.ToString(CultureInfo.InvariantCulture)} effects / "
                + $"{plan.NoOps.Count.ToString(CultureInfo.InvariantCulture)} no-ops");
            if (plan.Conflicts.Count != 0)
            {
                builder.AppendLine($"Conflicts: {plan.Conflicts.Count.ToString(CultureInfo.InvariantCulture)}");
            }

            if (expanded)
            {
                AppendSelection(builder, result);
            }

            AppendPlan(builder, plan, expanded);
        }
        else
        {
            builder.AppendLine("Plan: unavailable");
        }

        builder.AppendLine($"Preflight: {RepairDefinitions.ReadMachineName(result.Preflight.State)} / recovery {RepairDefinitions.ReadMachineName(result.Preflight.Recovery)}");
        builder.AppendLine($"Application: {RepairDefinitions.ReadMachineName(result.Application.State)} ({result.Application.AppliedEffects.ToString(CultureInfo.InvariantCulture)} effects)");
        if (result.Preflight.Cause is { } preflightCause)
        {
            builder.AppendLine($"  {Text(preflightCause)}");
        }

        if (result.Application.Cause is { } applicationCause)
        {
            builder.AppendLine($"  {Text(applicationCause)}");
        }
        builder.AppendLine(
            $"Verification: targets {RepairDefinitions.ReadMachineName(result.Verification.Targets)}; "
            + $"bytes {RepairDefinitions.ReadMachineName(result.Verification.ResultingBytes)}; "
            + $"post-conditions {RepairDefinitions.ReadMachineName(result.Verification.PostConditions)}");
        builder.AppendLine(
            $"Recovery: {RepairDefinitions.ReadMachineName(result.Recovery.State)}; "
            + $"residual {RepairDefinitions.ReadMachineName(result.Recovery.Residual)} "
            + $"({Text(result.Recovery.ResidualPath ?? "none")})");
        builder.AppendLine($"Post-diagnosis: {RepairDefinitions.ReadMachineName(result.PostDiagnosis.State)} / {Coverage(result.PostDiagnosis.Coverage)}");
        if (result.Mode == RepairMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        foreach (var finding in result.Findings)
        {
            builder.AppendLine($"{CliHumanText.Status(RepairDefinitions.ReadStatus(finding.Code)).ToUpperInvariant()}: {Text(finding.Cause)} [{RepairDefinitions.ReadMachineName(finding.Code)}]");
            if (finding.SourceCanonicalPath is { } source)
            {
                builder.AppendLine($"  {Text(source)}{FormatLocation(finding.Occurrence)}");
            }

            if (finding.Target is not null)
            {
                builder.AppendLine($"  Target: {FormatTarget(finding.Target)}");
            }
        }

        CliHumanText.AppendNext(builder, presentation);

        return builder.ToString().TrimEnd();
    }
}
