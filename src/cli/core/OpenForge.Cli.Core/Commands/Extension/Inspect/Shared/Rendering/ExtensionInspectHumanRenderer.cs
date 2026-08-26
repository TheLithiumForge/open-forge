using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectHumanRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionInspectResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return presentation.Presentation.View switch
        {
            CliView.Compact => RenderCompact(presentation.Result),
            CliView.Expanded => RenderExpanded(presentation.Result),
            _ => throw new ArgumentOutOfRangeException(nameof(presentation), presentation.Presentation.View, "The Extension Inspect view is not defined."),
        };
    }

    private static string RenderCompact(ExtensionInspectResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            CultureInfo.InvariantCulture,
            $"Extension inspect: id={Value(result.Subject.Id)}; source={ExtensionInspectJsonProjection.SourceState(result.Source.State)}; comparison={ExtensionInspectJsonProjection.ComparisonMode(result.Comparison.Mode)}; status={HumanStatus(result.Status)}");
        builder.AppendLine(
            CultureInfo.InvariantCulture,
            $"Installed: {ExtensionInspectJsonProjection.InstalledState(result.Installed.State)}; available: {ExtensionInspectJsonProjection.AvailableState(result.Available.State)}; findings: {result.Findings.Count}");
        foreach (var finding in result.Findings)
        {
            builder.AppendLine($"Finding: {ExtensionInspectDefinitions.ReadFindingCode(finding.Code)}; {Escape(finding.Cause)}");
        }

        if (result.Next is not null)
        {
            builder.AppendLine($"Next: {Escape(result.Next.Command)}");
        }

        return builder.ToString().TrimEnd();
    }

    private static string RenderExpanded(ExtensionInspectResult result)
    {
        var builder = new StringBuilder();
        var workspace = result.Workspace is null ? "unavailable" : Escape(result.Workspace.LexicalRoot);
        builder.AppendLine($"Open Forge extension inspect {Value(result.Subject.Id)}");
        builder.AppendLine($"Workspace: {workspace}");
        builder.AppendLine(
            $"Source: {Value(result.Source.Identity)}; {ExtensionInspectJsonProjection.SourceState(result.Source.State)}");
        builder.AppendLine(
            $"Lifecycle: {ExtensionInspectJsonProjection.LifecycleReadState(result.Lifecycle.ReadState)}; trust: {ExtensionInspectJsonProjection.LifecycleTrust(result.Lifecycle.Trust)}; coverage: {ExtensionInspectJsonProjection.Coverage(result.Lifecycle.Coverage)}");
        builder.AppendLine(
            $"Installed: {ExtensionInspectJsonProjection.InstalledState(result.Installed.State)}{InstalledVersion(result.Installed)}");
        builder.AppendLine(
            $"Available: {ExtensionInspectJsonProjection.AvailableState(result.Available.State)}{AvailableVersion(result.Available)}");
        builder.AppendLine(
            CultureInfo.InvariantCulture,
            $"Dependencies: {result.Dependencies.Declared.Count} declared; {result.Dependencies.Resolved.Count} package(s) in the resolved closure");
        builder.AppendLine(
            CultureInfo.InvariantCulture,
            $"Paths: {result.PathFacts.Declared.Count} declared; {result.PathFacts.Current.Count} current; {result.Available.Package?.Payload.Count ?? 0} intended");
        builder.AppendLine(
            $"Comparison: {ExtensionInspectJsonProjection.ComparisonMode(result.Comparison.Mode)}; {Count(result.Counts.ChangedPaths)} changed");
        builder.AppendLine("Generated: derived navigation, not package-owned authored bytes");
        foreach (var region in result.Generated.Regions)
        {
            builder.AppendLine(
                $"Generated region: {Escape(region.Path)}; {ExtensionInspectJsonProjection.GeneratedRegionState(region.State)}");
        }

        foreach (var finding in result.Findings)
        {
            var subject = finding.Subject is null ? string.Empty : $"; {Escape(finding.Subject)}";
            var path = finding.Path is null ? string.Empty : $"; {Escape(finding.Path)}";
            builder.AppendLine(
                $"Finding: {ExtensionInspectDefinitions.ReadFindingCode(finding.Code)}{subject}{path}; {Escape(finding.Cause)}");
        }

        builder.AppendLine($"Status: {HumanStatus(result.Status)}");
        if (result.Next is not null)
        {
            builder.AppendLine($"Next: {Escape(result.Next.Command)}");
        }

        return builder.ToString().TrimEnd();
    }

    private static string InstalledVersion(ExtensionInspectInstalled installed)
        => installed.Package?.Version is { } version ? $"; version: {Escape(version)}" : string.Empty;

    private static string AvailableVersion(ExtensionInspectAvailable available)
        => available.Package?.Version is { } version ? $"; version: {Escape(version)}" : string.Empty;

    private static string Count(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "unknown";

    private static string Value(string? value) => value is null ? "unavailable" : Escape(value);

    private static string HumanStatus(CliSemanticStatus status)
        => status == CliSemanticStatus.Attention
            ? "requires attention"
            : CliStatusDefinitions.Read(status).MachineName;

    private static string Escape(string value) => ExtensionInspectTextEscaping.Escape(value);
}
