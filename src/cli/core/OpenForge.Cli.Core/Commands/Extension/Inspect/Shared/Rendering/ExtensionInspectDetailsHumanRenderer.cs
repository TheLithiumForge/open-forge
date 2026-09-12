using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectDetailsHumanRenderer
{
    internal static void AppendDependencies(StringBuilder builder, CliPresentationRequest<ExtensionInspectResult> presentation)
    {
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var comparison = result.Comparison.Dependencies;
        builder.AppendLine($"Dependency resolution: {ExtensionInspectWireVocabulary.DependencyState(result.Dependencies.State)}; resolved packages: {ExtensionHumanText.Count(result.Counts.Dependencies)}");
        builder.AppendLine($"  Comparison: {ExtensionInspectWireVocabulary.DependencyComparisonState(comparison.State)}; {ExtensionInspectWireVocabulary.DependencyRelation(comparison.Relation)}");
        if (expanded || (comparison.State == ExtensionInspectDependencyComparisonState.Available
            && comparison.Relation != ExtensionInspectDependencyRelation.Equal))
        {
            builder.AppendLine($"""
              Installed baseline: {ObservedDependencies(comparison.Baseline, comparison.State)}
              Current workspace: {ObservedDependencies(comparison.Current, comparison.State)}
              Selected package: {ObservedDependencies(comparison.Intended, comparison.State)}
            """);
        }

        foreach (var package in result.Dependencies.Resolved)
        {
            builder.AppendLine($"  {Value(package.Id)}; version {Value(package.Version)}; {ExtensionInspectWireVocabulary.DependencyPackageState(package.State)}");
            if (expanded)
            {
                builder.AppendLine($"    Source: {Value(package.Source)}");
            }
        }

        if (expanded)
        {
            builder.AppendLine($"  Resolution order: {ExtensionHumanText.Values(result.Dependencies.Order)}");
            foreach (var edge in result.Dependencies.Declared)
            {
                builder.AppendLine($"  {Value(edge.From)} requires {Value(edge.To)}");
            }
        }
    }

    internal static void AppendPackage(StringBuilder builder, ExtensionInspectResult result)
    {
        builder.AppendLine($"""
            Record: {Value(result.Lifecycle.DocumentPath)}
              Workspace match: {ExtensionInspectWireVocabulary.WorkspaceBinding(result.Lifecycle.WorkspaceBinding)}
              Fingerprint policy: {Value(result.Lifecycle.FingerprintPolicy)}
            """);
        builder.AppendLine($"""
            Comparison coverage:
              Installed baseline: {ExtensionInspectWireVocabulary.ComparisonSideState(result.Comparison.Baseline.State)}
              Current workspace: {ExtensionInspectWireVocabulary.ComparisonSideState(result.Comparison.Current.State)}
              Selected package: {ExtensionInspectWireVocabulary.ComparisonSideState(result.Comparison.Intended.State)}
            """);
        if (result.Installed.Package is { } installed)
        {
            builder.AppendLine($"""
                Installed package: {Value(installed.Id)}
                  Recorded source: {Value(installed.Source)}
                  Dependencies: {ExtensionHumanText.Values(installed.Dependencies)}
                """);
        }

        if (result.Available.Package is { } available)
        {
            builder.AppendLine($"""
                Selected package: {Value(available.Id)}
                  Name: {Value(available.Name)}
                  Description: {Value(available.Description)}
                  Manifest: {Value(available.ManifestPath)}
                  Dependencies: {ExtensionHumanText.Values(available.Dependencies)}
                """);
        }
    }

    private static string ObservedDependencies(IReadOnlyList<string> values, ExtensionInspectDependencyComparisonState state)
    {
        if (values.Count == 0 && state != ExtensionInspectDependencyComparisonState.Available)
        {
            return "not established";
        }

        return ExtensionHumanText.Values(values);
    }

    private static string Value(string? value) => ExtensionHumanText.Value(value);
}
