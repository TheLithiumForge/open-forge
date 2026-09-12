using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Rendering;

internal static class IndexHumanRenderer
{
    private const string UnknownEntryCount = "unknown";

    internal static string Render(CliPresentationRequest<IndexResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return RenderResult(
            presentation.Result,
            expanded: presentation.Presentation.View == CliView.Expanded);
    }

    private static string RenderResult(IndexResult result, bool expanded)
    {
        if (result.Status is not (CliSemanticStatus.Complete or CliSemanticStatus.Attention))
        {
            return RenderNonSuccess(result, expanded);
        }

        if (result.Mode == IndexMode.DryRun)
        {
            return RenderDryRun(result, expanded);
        }

        return RenderApplication(result, expanded);
    }

    private static string RenderApplication(IndexResult result, bool expanded)
    {
        if (result.Counts.Updates == 0)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"""
                Generated Entries are up to date.
                Checked {result.Counts.Regions} regions. No files changed.
                """).ReplaceLineEndings();
        }

        var builder = new StringBuilder();
        builder.Append(string.Create(
            CultureInfo.InvariantCulture,
            $"""
            Generated Entries were updated.
            Checked {result.Counts.Regions} regions: {result.Counts.Updates} updated, {result.Counts.Unchanged} already up to date.
            All {result.Counts.Verified} regions match the routed sources.{Environment.NewLine}
            """).ReplaceLineEndings());
        foreach (var region in result.Regions.Where(region => region.Action == IndexRegionAction.Update))
        {
            builder.AppendLine(
                CultureInfo.InvariantCulture,
                $"  {CommandTextEscaping.Escape(region.Source.Path)}: {EntryCount(region.BeforeEntryCount)} -> {EntryCount(region.ExpectedEntryCount)} entries ({Outcome(region.Outcome)})");
        }

        if (expanded)
        {
            AppendSelection(builder, result.Selection);
        }

        AppendRecovery(builder, result.Recovery);
        AppendFindings(builder, result.Findings);
        AppendNext(builder, result.Next);
        return builder.ToString().TrimEnd();
    }

    private static string RenderDryRun(IndexResult result, bool expanded)
    {
        if (result.Counts.Updates == 0)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"""
                Generated Entries are up to date.
                Checked {result.Counts.Regions} regions. No files changed (--dry-run).
                """).ReplaceLineEndings();
        }

        var builder = new StringBuilder();
        builder.Append(string.Create(
            CultureInfo.InvariantCulture,
            $"""
            Generated Entries would be updated.
            Checked {result.Counts.Regions} regions: {result.Counts.Updates} need updates, {result.Counts.Unchanged} are up to date.{Environment.NewLine}{Environment.NewLine}
            """).ReplaceLineEndings());
        foreach (var region in result.Regions.Where(region => region.Action == IndexRegionAction.Update))
        {
            builder.Append(IndexDiffRenderer.Render(region));
            builder.AppendLine();
        }

        builder.AppendLine("No files changed (--dry-run).");
        if (expanded)
        {
            AppendSelection(builder, result.Selection);
        }

        AppendFindings(builder, result.Findings);
        AppendNext(builder, result.Next);
        return builder.ToString().TrimEnd();
    }

    private static string RenderNonSuccess(IndexResult result, bool expanded)
    {
        var builder = new StringBuilder();
        builder.Append(
            """
            Generated Entries were not updated.


            """.ReplaceLineEndings());
        AppendFindings(builder, result.Findings);
        if (expanded)
        {
            AppendSelection(builder, result.Selection);
        }

        AppendEffectSummary(builder, result.Regions);
        AppendRecovery(builder, result.Recovery);
        AppendNext(builder, result.Next);
        return builder.ToString().TrimEnd();
    }

    private static void AppendEffectSummary(
        StringBuilder builder,
        IReadOnlyList<IndexRegion> regions)
    {
        var affected = regions.Count(region => region.Outcome is IndexRegionOutcome.Applied
            or IndexRegionOutcome.Verified
            or IndexRegionOutcome.Unknown);
        if (affected == 0)
        {
            builder.AppendLine("No files changed.");
            return;
        }

        var uncertain = regions.Count(region => region.Outcome == IndexRegionOutcome.Unknown);
        if (uncertain == 0)
        {
            builder.AppendLine(
                CultureInfo.InvariantCulture,
                $"{affected} files changed before the operation stopped.");
            return;
        }

        builder.AppendLine(
            CultureInfo.InvariantCulture,
            $"{affected} files may have changed before the operation stopped; {uncertain} have an unknown final outcome.");
    }

    private static void AppendSelection(StringBuilder builder, IndexSelection selection)
    {
        builder.AppendLine(
            CultureInfo.InvariantCulture,
            $"Selection: {IndexDefinitions.ReadMachineName(selection.Origin)} / {IndexDefinitions.ReadMachineName(selection.Scope)} / {selection.Sources.Count} sources");
    }

    private static void AppendRecovery(StringBuilder builder, IndexRecovery recovery)
    {
        if (recovery.State == IndexRecoveryState.NotRequired)
        {
            return;
        }

        builder.AppendLine($"Recovery: {IndexDefinitions.ReadMachineName(recovery.State)}");
        if (recovery.ResidualPath is not null)
        {
            builder.AppendLine($"  {CommandTextEscaping.Escape(recovery.ResidualPath)}");
        }
    }

    private static void AppendFindings(StringBuilder builder, IReadOnlyList<IndexFinding> findings)
    {
        foreach (var finding in findings)
        {
            var label = finding.Status == CliSemanticStatus.Attention
                ? "Requires attention"
                : Capitalize(CliStatusDefinitions.Read(finding.Status).MachineName);
            builder.AppendLine($"{label}: {CommandTextEscaping.Escape(finding.Cause)}");
            if (finding.Source is not null)
            {
                builder.AppendLine($"  {CommandTextEscaping.Escape(finding.Source.Path)}");
            }

            foreach (var candidate in finding.Candidates)
            {
                builder.AppendLine($"  {CommandTextEscaping.Escape(candidate.Path)}");
            }
        }
    }

    private static void AppendNext(StringBuilder builder, CliNextAction? next)
    {
        if (next is null)
        {
            return;
        }

        builder.AppendLine(
            $"""
            Next: {CommandTextEscaping.Escape(next.Command)}
            {CommandTextEscaping.Escape(next.Reason)}
            """.ReplaceLineEndings());
    }

    private static string Capitalize(string value) => char.ToUpperInvariant(value[0]) + value[1..];

    private static string Outcome(IndexRegionOutcome outcome)
        => IndexDefinitions.ReadMachineName(outcome);

    private static string EntryCount(int? count)
        => count is { } value
            ? string.Create(CultureInfo.InvariantCulture, $"{value}")
            : UnknownEntryCount;
}
