using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusHumanRenderer
{
    internal static string Render(CliPresentationRequest<StatusResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, Installation(result.Facts.Installation.State));
        StatusFindingHumanRenderer.Append(builder, result);
        builder.AppendLine();
        builder.AppendLine("Context");
        AppendStartup(builder, result.Facts.Context);
        if (presentation.Presentation.View == CliView.Expanded)
        {
            AppendExpandedContext(builder, result.Facts.Context);
        }

        AppendStructure(builder, result.Facts.Structure, presentation.Presentation.View);
        StatusLifecycleHumanRenderer.Append(builder, result.Facts.Lifecycle, presentation.Presentation.View);
        StatusLibraryPresentation.Append(builder, result.Facts.Library);
        AppendRecovery(builder, result.Facts.Recovery);
        CliHumanText.AppendNext(builder, presentation);

        return builder.ToString().TrimEnd();
    }

    private static void AppendStartup(StringBuilder builder, StatusContext context)
    {
        AppendMeasurement(builder, "Shipped startup", context.Startup.Initial);
        AppendMeasurement(builder, "Startup", context.Startup.Current);
        AppendMeasurement(builder, "Difference", context.Startup.Difference);
        AppendMeasurement(builder, "Continuity (may load again)", context.Continuity);
        builder.AppendLine($"Startup share: {Percentage(context.StartupPercentage)}");
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

    private static void AppendStructure(StringBuilder builder, StatusStructure structure, CliView view)
    {
        builder.AppendLine();
        builder.AppendLine("Routes");
        builder.AppendLine($"Root categories: {Value(structure.RootCategories.Count)}");
        builder.AppendLine($"  Added: {List(structure.RootCategories.Added)}");
        builder.AppendLine($"  Removed: {List(structure.RootCategories.Removed)}");
        builder.AppendLine("Generated navigation:");
        foreach (var group in structure.GeneratedNavigation.GroupBy(target => target.State).OrderBy(group => group.Key))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"  {group.Count()} {NavigationState(group.Key)}");
        }

        foreach (var target in structure.GeneratedNavigation)
        {
            if (view == CliView.Expanded || target.State != OperationalGeneratedNavigationState.Current)
            {
                builder.AppendLine($"  {Text(target.Path)}: {NavigationState(target.State)}");
            }
        }

        if (structure.GeneratedNavigation.Count == 0)
        {
            builder.AppendLine("  none observed");
        }
    }

    private static string NavigationState(OperationalGeneratedNavigationState state)
        => state == OperationalGeneratedNavigationState.Changed ? "need updating" : StatusWireVocabulary.GeneratedNavigationState(state);

    private static void AppendRecovery(StringBuilder builder, StatusRecovery recovery)
    {
        builder.AppendLine();
        builder.AppendLine("Recovery");
        builder.AppendLine($"Verified recovery records: {Value(recovery.VerifiedFinals)}");
        builder.AppendLine($"Incomplete drafts: {Value(recovery.IncompleteDrafts)}");
        foreach (var candidate in recovery.Candidates)
        {
            builder.AppendLine(
                $"  {Text(candidate.Path)}: {StatusWireVocabulary.RecoveryKind(candidate.Kind)} / {StatusWireVocabulary.RecoveryIntegrity(candidate.Integrity)}");
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

    private static string Installation(OperationalInstallationState state) => state switch
    {
        OperationalInstallationState.Installed => "Open Forge is installed.",
        OperationalInstallationState.Uninstalled => "Open Forge is not installed.",
        OperationalInstallationState.Incomplete => "Open Forge installation could not be checked completely.",
        OperationalInstallationState.Blocked => "Open Forge installation checks are blocked.",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The installation state is not defined."),
    };

    internal static string Text(string value)
        => CliHumanText.Text(value);
}
