using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;

internal static partial class RepairPresentation
{
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
                + $"{Text(proposal.SourceCanonicalPath)}{FormatLocation(proposal.Occurrence)} "
                + $"-> {FormatTarget(selected.Resolution.Target)} "
                + $"({Origins(selected.Origins)})");
        }

        foreach (var proposal in result.Selection.Unselected)
        {
            builder.AppendLine(
                $"  unselected {RepairDefinitions.ReadMachineName(proposal.Member)}: "
                + $"{Text(proposal.SourceCanonicalPath)}{FormatLocation(proposal.Occurrence)} "
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

    private static void AppendPlan(StringBuilder builder, RepairPlan plan, bool expanded)
    {
        if (expanded)
        {
            foreach (var step in plan.Steps)
            {
                builder.AppendLine(
                    $"  step {step.Ordinal.ToString(CultureInfo.InvariantCulture)}: "
                    + $"{RepairDefinitions.ReadMachineName(step.Outcome)} / "
                    + $"{RepairDefinitions.ReadMachineName(step.Proposal.Member)} / "
                    + $"{Origins(step.Origins)}");
            }
        }

        foreach (var effect in plan.Effects)
        {
            builder.AppendLine($"  effect: replace {Text(effect.SourceCanonicalPath)} ({effect.Changes.Count.ToString(CultureInfo.InvariantCulture)} destination literals)");
            foreach (var change in effect.Changes)
            {
                builder.AppendLine(
                    $"    {FormatLocation(change.Occurrence)} {Text(change.ExpectedDestination)} "
                    + $"-> {Text(change.IntendedDestination)} "
                    + $"({RepairDefinitions.ReadMachineName(change.CatalogueMember)})");
            }
        }

        foreach (var noOp in plan.NoOps)
        {
            builder.AppendLine($"  no-op: {Text(noOp.SourceCanonicalPath)}{FormatLocation(noOp.Occurrence)} retains {Text(noOp.Destination)}");
        }

        foreach (var conflict in plan.Conflicts)
        {
            builder.AppendLine($"  conflict {RepairDefinitions.ReadMachineName(conflict.Kind)}: {Text(conflict.Cause)}");
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
                $":{location.Line}:{location.Column}");

    private static string FormatTarget(RepairTargetSelection target)
        => target.TargetFragment is null
            ? Text(target.CanonicalTargetPath)
            : $"{Text(target.CanonicalTargetPath)}#{Text(target.TargetFragment)}";

    private static string Origins(IReadOnlyList<RepairSelectionOrigin> origins)
        => string.Join(
            ", ",
            origins.Select(RepairDefinitions.ReadMachineName));

    private static string Evidence(RepairCandidate candidate)
        => string.Join(
            "; ",
            candidate.Evidence.Select(evidence =>
                evidence.Location is null
                    ? $"{RepairDefinitions.ReadMachineName(evidence.Kind)}={Text(evidence.Value)}"
                    : $"{RepairDefinitions.ReadMachineName(evidence.Kind)}={Text(evidence.Value)}{FormatLocation(evidence.Location)}"));

    private static string Values(IReadOnlyList<string> values)
        => values.Count == 0 ? "none" : string.Join(", ", values.Select(Text));

    private static string Text(string value) => CommandTextEscaping.Escape(value);
}
