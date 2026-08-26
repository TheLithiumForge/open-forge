using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;

internal static class ExtensionListHumanRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionListResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return presentation.Presentation.View switch
        {
            CliView.Compact => RenderCompact(presentation.Result),
            CliView.Expanded => RenderExpanded(presentation.Result),
            _ => throw new ArgumentOutOfRangeException(nameof(presentation), presentation.Presentation.View, "The Extension List view is not defined."),
        };
    }

    private static string RenderExpanded(ExtensionListResult result)
    {
        var builder = new StringBuilder();
        var workspace = result.Workspace is null ? "unavailable" : Escape(result.Workspace.LexicalRoot);
        builder.AppendLine($"""
            Open Forge extension list
            Workspace: {workspace}
            """);
        AppendSource(builder, result.Source);
        if (result.Selection.Installed)
        {
            var trust = result.LifecycleTrust is null
                ? "unavailable"
                : ExtensionListJsonProjection.Trust(result.LifecycleTrust.Value);
            builder.AppendLine(
                $"Installed (coverage: {ExtensionListJsonProjection.Coverage(result.InstalledCoverage)}; trust: {trust})");
            if (result.Installed.Count == 0)
            {
                builder.AppendLine("- none established");
            }
            else
            {
                foreach (var row in result.Installed)
                {
                    var version = row.Version is null ? string.Empty : $" {Escape(row.Version)}";
                    var availability = row.SourceAvailable ? "available" : "unavailable";
                    builder.AppendLine(
                        CultureInfo.InvariantCulture,
                        $"- {Escape(row.Id)}{version}; {ExtensionListJsonProjection.Trust(row.Trust)}; {row.ManagedPathCount} managed paths; source {availability}");
                }
            }
        }

        if (result.Selection.Available)
        {
            builder.AppendLine($"Available (coverage: {ExtensionListJsonProjection.Coverage(result.AvailableCoverage)})");
            if (result.Available.Count == 0)
            {
                builder.AppendLine("- none established");
            }
            else
            {
                foreach (var row in result.Available)
                {
                    var packageNoun = row.PackageCount == 1 ? "package" : "packages";
                    var dependencyNoun = row.DependencyCount == 1 ? "dependency" : "dependencies";
                    builder.AppendLine(
                        CultureInfo.InvariantCulture,
                        $"- {Escape(row.Id)} {Escape(row.Version)}; {row.PackageCount} {packageNoun}; {row.DependencyCount} {dependencyNoun}");
                }
            }
        }

        AppendFindings(builder, result.Findings);
        AppendCompletion(builder, result);
        return builder.ToString().TrimEnd();
    }

    private static string RenderCompact(ExtensionListResult result)
    {
        var builder = new StringBuilder();
        object installed = result.Selection.Installed ? result.Installed.Count : "not-requested";
        object available = result.Selection.Available ? result.Available.Count : "not-requested";
        var source = result.Source is null ? "unavailable" : ExtensionListJsonProjection.SourceState(result.Source.State);
        builder.AppendLine(
            CultureInfo.InvariantCulture,
            $"Extension list: installed={installed}; available={available}; source={source}; status={ReadHumanStatus(result.Status)}");
        foreach (var row in result.Installed)
        {
            builder.AppendLine($"Installed: {Escape(row.Id)}; {ExtensionListJsonProjection.Trust(row.Trust)}");
        }

        foreach (var row in result.Available)
        {
            builder.AppendLine($"Available: {Escape(row.Id)} {Escape(row.Version)}");
        }

        AppendFindings(builder, result.Findings);
        if (result.Next is not null)
        {
            builder.AppendLine($"Next: {result.Next.Command}");
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendSource(StringBuilder builder, ExtensionListSource? source)
    {
        if (source is null)
        {
            builder.AppendLine("Source: unavailable");
            return;
        }

        var kind = source.Kind is null
            ? string.Empty
            : $"; {ExtensionListJsonProjection.SourceKind(source.Kind.Value)}";
        builder.AppendLine($"Source: {Escape(source.Identity)}{kind}; {ExtensionListJsonProjection.SourceState(source.State)}");
    }

    private static void AppendFindings(
        StringBuilder builder,
        IEnumerable<ExtensionListFinding> findings)
    {
        foreach (var finding in findings)
        {
            var subject = finding.Subject is null ? string.Empty : $"; {Escape(finding.Subject)}";
            builder.AppendLine(
                $"Finding: {ExtensionListDefinitions.ReadFindingCode(finding.Code)}{subject}; {Escape(finding.Cause)}");
        }
    }

    private static void AppendCompletion(StringBuilder builder, ExtensionListResult result)
    {
        builder.AppendLine($"Status: {ReadHumanStatus(result.Status)}");
        if (result.Next is not null)
        {
            builder.AppendLine($"Next: {result.Next.Command}");
        }
    }

    private static string ReadHumanStatus(CliSemanticStatus status)
        => status == CliSemanticStatus.Attention
            ? "requires attention"
            : CliStatusDefinitions.Read(status).MachineName;

    private static string Escape(string value) => ExtensionListTextEscaping.Escape(value);
}
