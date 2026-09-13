using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Rendering;

internal static class IndexHumanRenderer
{
    internal static string Render(CliPresentationRequest<IndexResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var style = CliHumanStyle.For(presentation);
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, Summary(result));
        builder.AppendLine($"Mode: {IndexDefinitions.ReadMachineName(result.Mode)}");
        builder.AppendLine(CultureInfo.InvariantCulture,
            $"Regions: {result.Counts.Regions}; updates: {result.Counts.Updates}; already current: {result.Counts.Unchanged}; verified: {result.Counts.Verified}");
        if (presentation.Presentation.View == CliView.Expanded)
        {
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"Selection: {IndexDefinitions.ReadMachineName(result.Selection.Origin)} / {IndexDefinitions.ReadMachineName(result.Selection.Scope)} / {result.Selection.Sources.Count} sources");
        }

        foreach (var finding in result.Findings)
        {
            builder.AppendLine($"{style.Finding(finding.Status)}: {Text(finding.Cause)} [{IndexDefinitions.ReadMachineName(finding.Code)}]");
            if (finding.Source is { } source)
            {
                builder.AppendLine($"  {Text(source.Path)}");
            }

            foreach (var candidate in finding.Candidates)
            {
                builder.AppendLine($"  Candidate: {Text(candidate.Path)}");
            }
        }

        builder.AppendLine();
        foreach (var region in result.Regions.Where(region => region.Action == IndexRegionAction.Update))
        {
            if (result.Mode == IndexMode.DryRun)
            {
                builder.Append(IndexDiffRenderer.Render(region));
                builder.AppendLine();
            }
            else
            {
                builder.AppendLine(CultureInfo.InvariantCulture,
                    $"  {Text(region.Source.Path)}: {EntryCount(region.BeforeEntryCount)} -> {EntryCount(region.ExpectedEntryCount)} entries ({IndexDefinitions.ReadMachineName(region.Outcome)})");
            }
        }

        if (result.Mode == IndexMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }
        else if (result.Status is not (CliSemanticStatus.Complete or CliSemanticStatus.Attention))
        {
            AppendEffectSummary(builder, result.Regions);
        }
        else if (result.Counts.Updates == 0)
        {
            builder.AppendLine("No files changed.");
        }

        if (result.Recovery.State != IndexRecoveryState.NotRequired)
        {
            builder.AppendLine($"Recovery: {IndexDefinitions.ReadMachineName(result.Recovery.State)}");
            if (result.Recovery.ResidualPath is { } path)
            {
                builder.AppendLine($"  {Text(path)}");
            }
        }

        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static string Summary(IndexResult result)
        => result.Status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention when result.Counts.Updates == 0
                => "Generated Entries are up to date.",
            CliSemanticStatus.Complete or CliSemanticStatus.Attention when result.Mode == IndexMode.DryRun
                => "Preview of generated navigation changes",
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => "Generated Entries were updated.",
            _ => CliHumanText.Outcome("Generated navigation update", result.Status),
        };

    private static void AppendEffectSummary(StringBuilder builder, IReadOnlyList<IndexRegion> regions)
    {
        var affected = regions.Count(region => region.Outcome is IndexRegionOutcome.Applied
            or IndexRegionOutcome.Verified or IndexRegionOutcome.Unknown);
        if (affected == 0)
        {
            builder.AppendLine("No files changed.");
            return;
        }

        var uncertain = regions.Count(region => region.Outcome == IndexRegionOutcome.Unknown);
        if (uncertain == 0)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"{affected} files changed before the operation stopped.");
        }
        else
        {
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"{affected} files may have changed before the operation stopped; {uncertain} have an unknown final outcome.");
        }
    }

    private static string Text(string value) => CommandTextEscaping.Escape(value);

    private static string EntryCount(int? count)
        => count is { } value ? value.ToString(CultureInfo.InvariantCulture) : "unknown";
}
