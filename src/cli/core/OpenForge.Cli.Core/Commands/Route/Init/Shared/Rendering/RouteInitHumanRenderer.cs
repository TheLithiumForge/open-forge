using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;

internal static class RouteInitHumanRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteInitResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, Summary(result));
        AppendIdentity(builder, result);
        builder.AppendLine($"Mode: {RouteInitDefinitions.ReadMachineName(result.Mode)}");
        builder.AppendLine($"Scaffold: {RouteInitDefinitions.ReadMachineName(result.Scaffold)}");
        builder.AppendLine(
            $"Plan: completeness={RouteInitDefinitions.ReadMachineName(result.Plan.Completeness)}, safety={RouteInitDefinitions.ReadMachineName(result.Plan.Safety)}");

        AppendEntrypointSummary(builder, result);
        AppendEffects(builder, result, expanded);
        AppendUnchanged(builder, result.UnchangedPaths);
        AppendDrafts(builder, result.Entrypoints);

        if (expanded)
        {
            AppendFramework(builder, result.Framework);
        }

        AppendFindings(builder, result.Findings);
        builder.AppendLine($"Lifecycle: {RouteInitDefinitions.ReadMachineName(result.Lifecycle.Action)} / {RouteInitDefinitions.ReadMachineName(result.Lifecycle.Outcome)}");
        builder.AppendLine($"Recovery: {RouteInitDefinitions.ReadMachineName(result.Recovery.State)}{PathSuffix(result.Recovery.ResidualPath)}");
        builder.AppendLine($"Verification: {RouteInitDefinitions.ReadMachineName(result.Verification)}");

        if (result.Mode == RouteInitMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static string Summary(RouteInitResult result)
    {
        if (result.Status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted
            or CliSemanticStatus.Incomplete)
        {
            return CliHumanText.Outcome("Route Init", result.Status);
        }

        if (result.Mode == RouteInitMode.DryRun)
        {
            return "The route would be initialized.";
        }

        return IsNoOp(result)
            ? "The route is initialized."
            : "The route was initialized.";
    }

    private static bool IsNoOp(RouteInitResult result)
        => result.Effects.Length == 0
            || result.Effects.All(effect => effect.Outcome == RouteInitEffectOutcome.NotStarted)
                && result.UnchangedPaths.Length > 0;

    private static void AppendIdentity(StringBuilder builder, RouteInitResult result)
    {
        builder.AppendLine(
            $"Target: {RouteTextEscaping.Escape(result.Target.Id ?? result.Target.Requested)}");
        builder.AppendLine($"Path: {Value(result.Target.Path)}");
    }

    private static void AppendEntrypointSummary(
        StringBuilder builder,
        RouteInitResult result)
    {
        var outcomes = result.Entrypoints.GroupBy(entrypoint => entrypoint.Outcome)
            .OrderBy(group => group.Key)
            .Select(group => string.Create(CultureInfo.InvariantCulture,
                $"{group.Count()} {RouteInitDefinitions.ReadMachineName(group.Key)}"));
        var generated = result.Effects.Count(effect => effect.Kind == RouteInitEffectKind.GeneratedRegion);
        builder.AppendLine(CultureInfo.InvariantCulture,
            $"Entrypoints: {result.Entrypoints.Length}; generated regions: {generated}");
        if (result.Entrypoints.Length != 0)
        {
            builder.AppendLine($"  {string.Join(", ", outcomes)}");
        }
    }

    private static void AppendEffects(
        StringBuilder builder,
        RouteInitResult result,
        bool expanded)
    {
        if (result.Effects.Length == 0)
        {
            builder.AppendLine(
                CultureInfo.InvariantCulture,
                $"Checked {result.Entrypoints.Length} entrypoints. No files changed.");
            return;
        }

        builder.AppendLine("Effects:");
        foreach (var effect in result.Effects)
        {
            builder.AppendLine(
                $"  {RouteTextEscaping.Escape(effect.Path)}: {RouteInitDefinitions.ReadMachineName(effect.Action)} {RouteInitDefinitions.ReadMachineName(effect.Kind)} / {RouteInitDefinitions.ReadMachineName(effect.Outcome)} / residual={RouteInitDefinitions.ReadMachineName(effect.Residual)}");
            if (effect.SourceAssetPath is not null)
            {
                builder.AppendLine(
                    $"    Source: {RouteTextEscaping.Escape(effect.SourceAssetPath)}");
            }

            if (effect.Change is not null
                && (expanded || result.Mode == RouteInitMode.DryRun))
            {
                builder.AppendLine(
                    $"    Before: {Value(effect.Change.Before)}");
                builder.AppendLine(
                    $"    Expected: {RouteTextEscaping.Escape(effect.Change.Expected)}");
            }
        }
    }

    private static void AppendUnchanged(
        StringBuilder builder,
        IReadOnlyList<string> unchangedPaths)
    {
        if (unchangedPaths.Count == 0)
        {
            return;
        }

        builder.AppendLine("Unchanged:");
        foreach (var path in unchangedPaths)
        {
            builder.AppendLine($"  {RouteTextEscaping.Escape(path)}");
        }
    }

    private static void AppendDrafts(
        StringBuilder builder,
        IReadOnlyList<RouteInitEntrypoint> entrypoints)
    {
        foreach (var entrypoint in entrypoints.Where(IsDraft))
        {
            builder.AppendLine($"Draft: {RouteTextEscaping.Escape(entrypoint.Path)}");
        }
    }

    private static bool IsDraft(RouteInitEntrypoint entrypoint)
        => entrypoint.Metadata?.Tags.Contains("NeedsAuthoring", StringComparer.Ordinal) == true;

    private static void AppendFramework(
        StringBuilder builder,
        RouteInitFramework? framework)
    {
        if (framework is null)
        {
            return;
        }

        builder.AppendLine(
            $"Framework inventory: {RouteTextEscaping.Escape(framework.InventoryFingerprint)}");
        foreach (var segment in framework.Segments)
        {
            builder.AppendLine(
                $"  {RouteTextEscaping.Escape(segment.Path)}: {RouteInitDefinitions.ReadMachineName(segment.Role)}{PathSuffix(segment.SourceAssetPath)}");
        }
    }

    private static void AppendFindings(
        StringBuilder builder,
        IReadOnlyList<RouteInitFinding> findings)
    {
        foreach (var finding in findings)
        {
            builder.AppendLine(
                $"{CliHumanText.Status(finding.Status).ToUpperInvariant()}: {RouteTextEscaping.Escape(finding.Cause)} [{RouteInitDefinitions.ReadMachineName(finding.Code)}]");
            if (finding.Target is not null)
            {
                builder.AppendLine($"  Target: {RouteTextEscaping.Escape(finding.Target)}");
            }
        }
    }

    private static string Value(string? value)
        => value is null ? "unavailable" : RouteTextEscaping.Escape(value);

    private static string PathSuffix(string? path)
        => path is null ? string.Empty : $" / {RouteTextEscaping.Escape(path)}";

}
