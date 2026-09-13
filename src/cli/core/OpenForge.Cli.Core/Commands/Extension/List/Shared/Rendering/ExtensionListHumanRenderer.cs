using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;

internal static class ExtensionListHumanRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionListResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var style = CliHumanStyle.For(presentation);
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, "Extension list");
        foreach (var finding in result.Findings)
        {
            builder.AppendLine($"{style.Finding(finding.Status)}: {Text(finding.Cause)} [{ExtensionListDefinitions.ReadFindingCode(finding.Code)}]");
            if (finding.Subject is { } subject)
            {
                builder.AppendLine($"  {Text(subject)}");
            }
        }

        AppendSource(builder, result.Source, expanded);
        if (result.Selection.Installed)
        {
            var trust = result.LifecycleTrust is { } value ? ExtensionListJsonProjection.Trust(value) : "unavailable";
            builder.AppendLine($"Installed: coverage {ExtensionListJsonProjection.Coverage(result.InstalledCoverage)}; record {trust}");
            AppendEmpty(builder, result.Installed.Count, result.InstalledCoverage);
            foreach (var row in result.Installed)
            {
                builder.AppendLine($"  {Text(row.Id)}; version {Text(row.Version ?? "unknown")}; {ExtensionListJsonProjection.Trust(row.Trust)}; source {(row.SourceAvailable ? "available" : "unavailable")}");
                if (expanded)
                {
                    builder.AppendLine(CultureInfo.InvariantCulture, $"    Managed paths: {row.ManagedPathCount}");
                }
            }
        }

        if (result.Selection.Available)
        {
            builder.AppendLine($"Available: coverage {ExtensionListJsonProjection.Coverage(result.AvailableCoverage)}");
            AppendEmpty(builder, result.Available.Count, result.AvailableCoverage);
            foreach (var row in result.Available)
            {
                builder.AppendLine($"  {Text(row.Id)}; version {Text(row.Version)}");
                if (expanded)
                {
                    builder.AppendLine($"""
                            Name: {Text(row.Name)}
                            Description: {Text(row.Description)}
                        """);
                    builder.AppendLine(CultureInfo.InvariantCulture, $"    Packages: {row.PackageCount}; dependencies: {row.DependencyCount}");
                }
            }
        }

        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static void AppendSource(StringBuilder builder, ExtensionListSource? source, bool expanded)
    {
        if (source is null)
        {
            builder.AppendLine("Source: unavailable");
            return;
        }

        builder.AppendLine($"Source: {Text(source.Identity)}; {ExtensionListJsonProjection.SourceState(source.State)}");
        if (expanded && source.Kind is { } kind)
        {
            builder.AppendLine($"  Source kind: {ExtensionListJsonProjection.SourceKind(kind)}");
        }
    }

    private static void AppendEmpty(StringBuilder builder, int count, ExtensionListCoverage coverage)
    {
        if (count == 0)
        {
            builder.AppendLine(coverage == ExtensionListCoverage.Complete ? "  No packages." : "  No packages could be established from the available facts.");
        }
    }

    private static string Text(string value) => CliHumanText.Text(value);
}
