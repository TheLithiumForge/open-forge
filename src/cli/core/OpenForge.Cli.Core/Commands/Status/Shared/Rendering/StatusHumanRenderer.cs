using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusHumanRenderer
{
    internal static string Render(CliPresentationRequest<StatusResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var builder = new StringBuilder();
        builder.AppendLine("Open Forge status");
        builder.AppendLine($"Workspace: {Text(result.Workspace?.LexicalRoot ?? "unavailable")}");
        builder.AppendLine($"Selected by: {Selection(result)}");
        builder.AppendLine($"Result: {Status(result.Status)}");
        builder.AppendLine(
            $"Installation: {StatusWireVocabulary.InstallationState(result.Facts.Installation.State)}");
        if (result.Facts.Installation.State == OperationalInstallationState.Uninstalled)
        {
            builder.AppendLine("Open Forge is not installed.");
        }

        builder.AppendLine();
        AppendStartup(builder, result.Facts.Context);
        if (presentation.Presentation.View == CliView.Expanded)
        {
            AppendExpandedContext(builder, result.Facts.Context);
        }

        AppendStructure(builder, result.Facts.Structure);
        StatusLifecycleHumanRenderer.Append(builder, result.Facts.Lifecycle);
        StatusLibraryPresentation.Append(builder, result.Facts.Library);
        AppendRecovery(builder, result.Facts.Recovery);
        AppendFindings(builder, result.Findings);
        if (result.Next is { } next)
        {
            builder.AppendLine($"Next: {Text(next.Command)}");
            if (presentation.Presentation.View == CliView.Expanded)
            {
                builder.AppendLine(Text(next.Reason));
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendStartup(StringBuilder builder, StatusContext context)
    {
        builder.AppendLine("Startup context");
        AppendMeasurement(builder, "Initial (shipped)", context.Startup.Initial);
        AppendMeasurement(builder, "Current workspace", context.Startup.Current);
        AppendMeasurement(builder, "Difference", context.Startup.Difference);
        AppendMeasurement(builder, "Continuity context (may load again)", context.Continuity);
        builder.AppendLine($"Startup percentage: {Percentage(context.StartupPercentage)}");
    }

    private static void AppendExpandedContext(StringBuilder builder, StatusContext context)
    {
        AppendMeasurement(builder, "Total available context", context.TotalAvailable);
        builder.AppendLine("Largest continuity sources");
        foreach (var source in context.ContinuitySources.Take(3))
        {
            builder.AppendLine($"  {Text(source.SourceId)}: {source.Utf8Bytes.ToString(CultureInfo.InvariantCulture)} bytes");
        }

        if (context.ContinuitySources.Count == 0)
        {
            builder.AppendLine("  none");
        }
    }

    private static void AppendStructure(StringBuilder builder, StatusStructure structure)
    {
        builder.AppendLine($"Root categories: {Value(structure.RootCategories.Count)}");
        builder.AppendLine($"  Added: {List(structure.RootCategories.Added)}");
        builder.AppendLine($"  Removed: {List(structure.RootCategories.Removed)}");
        builder.AppendLine("Generated navigation:");
        foreach (var target in structure.GeneratedNavigation)
        {
            builder.AppendLine($"  {Text(target.Path)}: {StatusWireVocabulary.GeneratedNavigationState(target.State)}");
        }

        if (structure.GeneratedNavigation.Count == 0)
        {
            builder.AppendLine("  none");
        }
    }

    private static void AppendRecovery(StringBuilder builder, StatusRecovery recovery)
    {
        builder.AppendLine($"Verified recovery finals: {Value(recovery.VerifiedFinals)}");
        builder.AppendLine($"Incomplete recovery drafts: {Value(recovery.IncompleteDrafts)}");
        foreach (var candidate in recovery.Candidates)
        {
            builder.AppendLine(
                $"  {Text(candidate.Path)}: {StatusWireVocabulary.RecoveryKind(candidate.Kind)} / {StatusWireVocabulary.RecoveryIntegrity(candidate.Integrity)}");
        }
    }

    private static void AppendFindings(StringBuilder builder, IReadOnlyList<StatusFinding> findings)
    {
        foreach (var finding in findings)
        {
            builder.AppendLine($"{Status(finding.Status)}: {Text(finding.Cause)}");
            if (finding.Subject is { } subject)
            {
                builder.AppendLine($"  {Text(subject)}");
            }
        }
    }

    private static void AppendMeasurement(StringBuilder builder, string label, StatusMeasurement measurement)
        => builder.AppendLine(
            $"  {label}: {Value(measurement.Files)} files, {Value(measurement.Characters)} characters, {Value(measurement.Utf8Bytes)} bytes, ~{Value(measurement.EstimatedTokens)} tokens");

    private static string Value(StatusIntegerValue value)
        => value.State == OperationalValueState.Available
            ? value.Value?.ToString(CultureInfo.InvariantCulture) ?? "unavailable"
            : StatusWireVocabulary.ValueState(value.State);

    private static string Percentage(StatusDecimalValue value)
    {
        if (value.State != OperationalValueState.Available)
        {
            return StatusWireVocabulary.ValueState(value.State);
        }

        return value.Value is { } percentage
            ? $"{percentage.ToString("0.##", CultureInfo.InvariantCulture)}%"
            : "unavailable";
    }

    private static string List(IReadOnlyList<string> values)
        => values.Count == 0 ? "none" : string.Join(", ", values.Select(Text));

    private static string Selection(StatusResult result)
        => result.Workspace is { } workspace
            ? StatusWireVocabulary.WorkspaceSelection(workspace.SelectedBy)
            : "unavailable";

    private static string Status(CliSemanticStatus status)
        => status == CliSemanticStatus.Attention
            ? "requires attention"
            : CliStatusDefinitions.Read(status).MachineName;

    private static string Text(string value)
        => string.Concat(value.Select(character => char.IsControl(character) ? '\uFFFD' : character));
}
