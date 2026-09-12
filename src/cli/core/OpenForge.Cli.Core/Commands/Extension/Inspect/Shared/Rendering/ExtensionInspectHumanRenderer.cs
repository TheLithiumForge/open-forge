using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectHumanRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionInspectResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, "Extension inspection");
        builder.AppendLine($"""
            Package: {Value(result.Subject.Id ?? result.Subject.Supplied)}; {ExtensionInspectWireVocabulary.SubjectState(result.Subject.State)}
            Source: {Value(result.Source.Identity ?? result.Source.Supplied)}; {ExtensionInspectWireVocabulary.SourceState(result.Source.State)}
            Installed: {ExtensionInspectWireVocabulary.InstalledState(result.Installed.State)}; version {Value(result.Installed.Package?.Version)}
            Available: {ExtensionInspectWireVocabulary.AvailableState(result.Available.State)}; version {Value(result.Available.Package?.Version)}
            Installation record: {ExtensionInspectWireVocabulary.LifecycleReadState(result.Lifecycle.ReadState)}; {ExtensionInspectWireVocabulary.LifecycleTrust(result.Lifecycle.Trust)}; coverage {ExtensionInspectWireVocabulary.Coverage(result.Lifecycle.Coverage)}
            Comparison: {ComparisonMode(result.Comparison.Mode)}; {ExtensionInspectWireVocabulary.ComparisonState(result.Comparison.State)}
            Paths: {ExtensionHumanText.Count(result.Counts.CurrentPaths)} current; {ExtensionHumanText.Count(result.Counts.IntendedPaths)} intended; {ExtensionHumanText.Count(result.Counts.UnchangedPaths)} unchanged
            """);
        ExtensionInspectFindingHumanRenderer.Append(builder, result.Findings.Where(finding => finding.Path is null));
        ExtensionInspectFindingHumanRenderer.AppendCandidates(builder, result.Subject.Candidates);
        ExtensionInspectPathsHumanRenderer.Append(builder, presentation);
        ExtensionInspectDetailsHumanRenderer.AppendDependencies(builder, presentation);
        if (expanded)
        {
            ExtensionInspectDetailsHumanRenderer.AppendPackage(builder, result);
        }

        builder.AppendLine($"Generated navigation: {ExtensionInspectWireVocabulary.GeneratedState(result.Generated.State)}; derived content");
        var absentRegions = result.Generated.Regions.Count(region => region.State == ExtensionInspectGeneratedRegionState.Absent);
        if (!expanded && absentRegions > 0)
        {
            builder.AppendLine($"  No generated region in {ExtensionHumanText.Count(absentRegions)} checked files.");
        }

        foreach (var region in result.Generated.Regions)
        {
            if (!expanded && region.State == ExtensionInspectGeneratedRegionState.Absent)
            {
                continue;
            }

            builder.AppendLine($"  {Value(region.Path)}: {ExtensionInspectWireVocabulary.GeneratedRegionState(region.State)}");
            if (expanded)
            {
                builder.AppendLine($"    Markers: {Value(region.StartMarker)} / {Value(region.EndMarker)}; marker lines retained: {region.MarkerLinesRetained.ToString().ToLowerInvariant()}");
            }
        }

        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static string ComparisonMode(ExtensionInspectComparisonMode value) => value switch
    {
        ExtensionInspectComparisonMode.None => "not compared",
        ExtensionInspectComparisonMode.InstalledOnly => "installed package only",
        ExtensionInspectComparisonMode.AvailableOnly => "selected package only",
        ExtensionInspectComparisonMode.ThreeWay => "installed baseline, current workspace and selected package",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The comparison mode is not defined."),
    };

    private static string Value(string? value) => ExtensionHumanText.Value(value);
}
