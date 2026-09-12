using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;

internal static class RepairPresentation
{
    internal static CliHelpContent CreateHelp()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge repair [--automatic] [--relink <source-location> <expected-destination> <target-path>]... [--dry-run] [global options]"),
            new CliHelpSection(
                "Selection",
                string.Join(
                    " ",
                    [
                        "  Run repair interactively to choose from the available repairs.",
                        "--automatic selects currently verified safe corrections;",
                        "--relink selects one current occurrence and contained target.",
                        "Guided candidates are never selected automatically.",
                    ])),
            new CliHelpSection(
                "Catalogue",
                """
                  Repair can correct path spelling, case, encoding, and unique fragments while preserving the target. You can also explicitly relink a missing target to a candidate reported by Doctor.
                It can also complete selected incomplete Library operations when current evidence proves the correction is safe.
                """),
            new CliHelpSection(
                "Library recovery",
                """
                  Library recovery requires a matching current-v1 recovery record, verified link identity without following the link, and current permission to change links outside .agents.
                It adds no recovery syntax, never follows or mutates source targets, and does not restore or widen permission grants.
                """),
            new CliHelpSection(
                "Preview and safety",
                string.Join(
                    " ",
                    [
                        "  --dry-run previews the complete selected plan and writes nothing.",
                        "Every eventual replacement carries complete expected and intended file states,",
                        "exact verification, and external recovery attribution.",
                    ])),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command. --view is ignored with --json."),
            new CliHelpSection("Results and streams", ResultsAndStreams()),
            new CliHelpSection(
                "Notes",
                "  Repair is stateless and local. It does not author content, change route or generated navigation, clean recovery artifacts, or invoke another command as a subprocess."),
        ]);

    internal static string RenderHuman(CliPresentationRequest<RepairResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var builder = new StringBuilder();
        if (result.Selection is { Libraries: var libraries } && (!libraries.Selected.IsEmpty || !libraries.Unselected.IsEmpty)
            || result.LibraryExecution is not null)
        {
            RepairLibraryPresentation.Append(builder, result);
        }

        builder.AppendLine("Open Forge repair");
        builder.AppendLine($"Workspace: {result.Workspace?.LexicalRoot ?? "unavailable"}");
        builder.AppendLine($"Mode: {RepairDefinitions.ReadMachineName(result.Mode)}");
        builder.AppendLine($"Selection: {RepairDefinitions.ReadMachineName(result.SelectionMode)}");
        builder.AppendLine($"Automatic: {result.Automatic.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Application: {(result.Mode == RepairMode.DryRun ? "preview" : RepairDefinitions.ReadMachineName(result.Application.State))}");
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
        builder.AppendLine($"Affected paths: {Values(result.AffectedPaths)}");

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
                AppendPlan(builder, plan);
            }
        }
        else
        {
            builder.AppendLine("Plan: unavailable");
        }

        builder.AppendLine($"Preflight: {RepairDefinitions.ReadMachineName(result.Preflight.State)} / recovery {RepairDefinitions.ReadMachineName(result.Preflight.Recovery)}");
        builder.AppendLine($"Application state: {RepairDefinitions.ReadMachineName(result.Application.State)} ({result.Application.AppliedEffects.ToString(CultureInfo.InvariantCulture)} effects)");
        builder.AppendLine(
            $"Verification: targets {RepairDefinitions.ReadMachineName(result.Verification.Targets)}; "
            + $"bytes {RepairDefinitions.ReadMachineName(result.Verification.ResultingBytes)}; "
            + $"post-conditions {RepairDefinitions.ReadMachineName(result.Verification.PostConditions)}");
        builder.AppendLine(
            $"Recovery: {RepairDefinitions.ReadMachineName(result.Recovery.State)}; "
            + $"residual {RepairDefinitions.ReadMachineName(result.Recovery.Residual)} "
            + $"({result.Recovery.ResidualPath ?? "none"})");
        builder.AppendLine($"Post-diagnosis: {RepairDefinitions.ReadMachineName(result.PostDiagnosis.State)} / {Coverage(result.PostDiagnosis.Coverage)}");
        if (result.Mode == RepairMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        builder.AppendLine($"Findings: {result.Findings.Count.ToString(CultureInfo.InvariantCulture)}");
        if (expanded)
        {
            foreach (var finding in result.Findings)
            {
                builder.AppendLine($"  {RepairDefinitions.ReadMachineName(finding.Code)}: {finding.Cause}");
                if (finding.SourceCanonicalPath is not null)
                {
                    builder.AppendLine($"    Source: {finding.SourceCanonicalPath}{FormatLocation(finding.Occurrence)}");
                }

                if (finding.Target is not null)
                {
                    builder.AppendLine($"    Target: {FormatTarget(finding.Target)}");
                }
            }
        }

        builder.AppendLine($"Status: {CliStatusDefinitions.Read(result.Status).MachineName}");
        if (result.Next is { } next)
        {
            builder.AppendLine($"Next: {next.Command}");
            builder.AppendLine($"  Reason: {next.Reason}");
        }

        return builder.ToString().TrimEnd();
    }

    internal static string? RenderDiagnostic(CliPresentationRequest<RepairResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"status={CliStatusDefinitions.Read(presentation.Result.Status).MachineName}; findings={presentation.Result.Findings.Count}; affected-paths={presentation.Result.AffectedPaths.Count}");
    }

    internal static CliRendererSet<RepairResult> CreateRenderers()
        => new(RenderHuman, RepairJsonRenderer.Render);

    private static void AppendSelection(StringBuilder builder, RepairResult result)
    {
        if (result.Selection is null)
        {
            builder.AppendLine("Selection details: unavailable");
            return;
        }

        builder.AppendLine($"Selection details: {RepairDefinitions.ReadMachineName(result.Selection.Mode)}");
        foreach (var selected in result.Selection.Selected)
        {
            var proposal = selected.Proposal;
            builder.AppendLine(
                $"  selected {RepairDefinitions.ReadMachineName(proposal.Member)}: "
                + $"{proposal.SourceCanonicalPath}{FormatLocation(proposal.Occurrence)} "
                + $"-> {FormatTarget(selected.Resolution.Target)} "
                + $"({Origins(selected.Origins)})");
        }

        foreach (var proposal in result.Selection.Unselected)
        {
            builder.AppendLine(
                $"  unselected {RepairDefinitions.ReadMachineName(proposal.Member)}: "
                + $"{proposal.SourceCanonicalPath}{FormatLocation(proposal.Occurrence)} "
                + $"-> {(proposal.Target is null ? "unresolved" : FormatTarget(proposal.Target))}");
            if (proposal.Candidates is { } candidates)
            {
                builder.AppendLine(
                    $"    candidates: {RepairDefinitions.ReadMachineName(candidates.Cardinality)} "
                    + $"({candidates.Items.Count.ToString(CultureInfo.InvariantCulture)})");
                foreach (var candidate in candidates.Items)
                {
                    builder.AppendLine(
                        $"      {FormatTarget(candidate.Target)}"
                        + (candidate.RecommendedForReview ? " [recommended for review]" : "")
                        + $" — {Evidence(candidate)}");
                }
            }
        }
    }

    private static void AppendPlan(StringBuilder builder, RepairPlan plan)
    {
        foreach (var step in plan.Steps)
        {
            builder.AppendLine(
                $"  step {step.Ordinal.ToString(CultureInfo.InvariantCulture)}: "
                + $"{RepairDefinitions.ReadMachineName(step.Outcome)} / "
                + $"{RepairDefinitions.ReadMachineName(step.Proposal.Member)} / "
                + $"{Origins(step.Origins)}");
        }

        foreach (var effect in plan.Effects)
        {
            builder.AppendLine($"  effect: replace {effect.SourceCanonicalPath} ({effect.Changes.Count.ToString(CultureInfo.InvariantCulture)} destination literals)");
            foreach (var change in effect.Changes)
            {
                builder.AppendLine(
                    $"    {FormatLocation(change.Occurrence)} {change.ExpectedDestination} "
                    + $"-> {change.IntendedDestination} "
                    + $"({RepairDefinitions.ReadMachineName(change.CatalogueMember)})");
            }
        }

        foreach (var noOp in plan.NoOps)
        {
            builder.AppendLine($"  no-op: {noOp.SourceCanonicalPath}{FormatLocation(noOp.Occurrence)} retains {noOp.Destination}");
        }

        foreach (var conflict in plan.Conflicts)
        {
            builder.AppendLine($"  conflict {RepairDefinitions.ReadMachineName(conflict.Kind)}: {conflict.Cause}");
        }
    }

    private static string Coverage(RepairDiagnosisCoverage coverage)
        => string.Join(
            ", ",
            $"workspace/path={RepairDefinitions.ReadMachineName(coverage.WorkspaceAndPath)}",
            $"route/heading={RepairDefinitions.ReadMachineName(coverage.RouteAndHeading)}",
            $"local-references={RepairDefinitions.ReadMachineName(coverage.LocalReferences)}",
            $"selected-scope={RepairDefinitions.ReadMachineName(coverage.SelectedScope)}");

    private static string FormatLocation(SourceLocation? location)
        => location is null
            ? string.Empty
            : string.Create(
                CultureInfo.InvariantCulture,
                $"@{location.Line}:{location.Column} [{location.ByteOffset}..{location.ByteOffset + location.ByteLength})");

    private static string FormatTarget(RepairTargetSelection target)
        => target.TargetFragment is null
            ? target.CanonicalTargetPath
            : $"{target.CanonicalTargetPath}#{target.TargetFragment}";

    private static string Origins(IReadOnlyList<RepairSelectionOrigin> origins)
        => string.Join(
            ", ",
            origins.Select(RepairDefinitions.ReadMachineName));

    private static string Evidence(RepairCandidate candidate)
        => string.Join(
            "; ",
            candidate.Evidence.Select(evidence =>
                evidence.Location is null
                    ? $"{RepairDefinitions.ReadMachineName(evidence.Kind)}={evidence.Value}"
                    : $"{RepairDefinitions.ReadMachineName(evidence.Kind)}={evidence.Value}{FormatLocation(evidence.Location)}"));

    private static string Values(IReadOnlyList<string> values)
        => values.Count == 0 ? "none" : string.Join(", ", values);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            string.Join(
                " ",
                [
                    $"  JSON writes one schema-version-{RepairDefinitions.SchemaVersion} envelope to stdout for every semantic status.",
                    "Verbose diagnostics use bounded stderr.",
                ]),
        };
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var definition = CliStatusDefinitions.Read(status);
            lines.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"  {definition.MachineName}: exit {definition.Disposition.ExitCode} and human {Stream(definition.Disposition.HumanOutputTarget)}."));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Stream(CliOutputTarget target)
        => target switch
        {
            CliOutputTarget.StandardOutput => "stdout",
            CliOutputTarget.StandardError => "stderr",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "The output target is not defined."),
        };
}
