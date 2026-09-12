using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;

internal static class RouteInitHumanRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteInitResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return Render(
            presentation.Result,
            presentation.Presentation.View == CliView.Expanded);
    }

    private static string Render(RouteInitResult result, bool expanded)
    {
        var builder = new StringBuilder();
        builder.AppendLine(Summary(result));
        AppendIdentity(builder, result);
        builder.AppendLine($"Mode: {RouteInitDefinitions.ReadMachineName(result.Mode)}");
        builder.AppendLine($"Scaffold: {RouteInitDefinitions.ReadMachineName(result.Scaffold)}");
        builder.AppendLine($"Status: {Status(result.Status)}");
        builder.AppendLine(
            $"Plan: completeness={RouteInitDefinitions.ReadMachineName(result.Plan.Completeness)}, safety={RouteInitDefinitions.ReadMachineName(result.Plan.Safety)}");

        AppendEntrypointSummary(builder, result);
        AppendEffects(builder, result, expanded);
        AppendUnchanged(builder, result.UnchangedPaths);
        AppendDrafts(builder, result.Entrypoints);

        if (expanded)
        {
            AppendFramework(builder, result.Framework);
            AppendFindings(builder, result.Findings);
            builder.AppendLine(
                $"Lifecycle: {RouteInitDefinitions.ReadMachineName(result.Lifecycle.Action)} / {RouteInitDefinitions.ReadMachineName(result.Lifecycle.Outcome)}");
            builder.AppendLine(
                $"Recovery: {RouteInitDefinitions.ReadMachineName(result.Recovery.State)}{PathSuffix(result.Recovery.ResidualPath)}");
            builder.AppendLine(
                $"Verification: {RouteInitDefinitions.ReadMachineName(result.Verification)}");
        }
        else
        {
            AppendDirectFinding(builder, result);
        }

        if (result.Mode == RouteInitMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        AppendNext(builder, result.Next);
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
            return "The route was not initialized.";
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
        builder.AppendLine($"Workspace: {Value(result.Workspace?.LexicalRoot)}");
        builder.AppendLine($"Selected by: {SelectedBy(result.Workspace)}");
        builder.AppendLine(
            $"Target: {RouteTextEscaping.Escape(result.Target.Id ?? result.Target.Requested)}");
        builder.AppendLine($"Path: {Value(result.Target.Path)}");
    }

    private static void AppendEntrypointSummary(
        StringBuilder builder,
        RouteInitResult result)
    {
        var created = result.Entrypoints.Count(entrypoint => entrypoint.Outcome
            is RouteInitEntrypointOutcome.Planned
            or RouteInitEntrypointOutcome.Created);
        var generated = result.Effects.Count(effect => effect.Kind == RouteInitEffectKind.GeneratedRegion);
        var verb = result.Mode == RouteInitMode.DryRun
            ? "Would create"
            : "Created";
        var generatedVerb = result.Mode == RouteInitMode.DryRun
            ? "would update"
            : "updated";
        builder.AppendLine(
            CultureInfo.InvariantCulture,
            $"{verb} {created} entrypoints and {generatedVerb} {generated} generated regions.");
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
            var tense = result.Mode == RouteInitMode.DryRun
                ? "would"
                : "did";
            builder.AppendLine(
                $"  {RouteTextEscaping.Escape(effect.Path)}: {tense} {RouteInitDefinitions.ReadMachineName(effect.Action)} {RouteInitDefinitions.ReadMachineName(effect.Kind)} / {RouteInitDefinitions.ReadMachineName(effect.Outcome)} / residual={RouteInitDefinitions.ReadMachineName(effect.Residual)}");
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
                $"{FindingLabel(finding.Status)}: {RouteTextEscaping.Escape(finding.Cause)}");
            if (finding.Target is not null)
            {
                builder.AppendLine($"  Target: {RouteTextEscaping.Escape(finding.Target)}");
            }
        }
    }

    private static void AppendDirectFinding(
        StringBuilder builder,
        RouteInitResult result)
    {
        if (result.Status is not (
                CliSemanticStatus.Incomplete
                or CliSemanticStatus.Invalid
                or CliSemanticStatus.Blocked
                or CliSemanticStatus.Failed
                or CliSemanticStatus.Interrupted))
        {
            return;
        }

        var finding = result.Findings.FirstOrDefault(candidate => candidate.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        if (finding is null)
        {
            return;
        }

        builder.AppendLine(
            $"{FindingLabel(finding.Status)}: {RouteTextEscaping.Escape(finding.Cause)}");
        if (finding.Target is not null)
        {
            builder.AppendLine($"  Target: {RouteTextEscaping.Escape(finding.Target)}");
        }
    }

    private static void AppendNext(StringBuilder builder, CliNextAction? next)
    {
        if (next is null)
        {
            return;
        }

        var guidance = next.Command == "open-forge route update"
            ? "author each NeedsAuthoring entrypoint before relying on its description or tags."
            : LowerInitial(next.Reason);
        builder.AppendLine($"Next: {RouteTextEscaping.Escape(guidance)}");
    }

    private static string SelectedBy(CliWorkspace? workspace)
        => workspace?.SelectedBy switch
        {
            null => "unavailable",
            CliWorkspaceSelectionMethod.CurrentDirectory => "current directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "--workspace",
            _ => throw new ArgumentOutOfRangeException(
                nameof(workspace),
                workspace.SelectedBy,
                "The workspace selection method is not defined."),
        };

    private static string Value(string? value)
        => value is null ? "unavailable" : RouteTextEscaping.Escape(value);

    private static string PathSuffix(string? path)
        => path is null ? string.Empty : $" / {RouteTextEscaping.Escape(path)}";

    private static string Status(CliSemanticStatus status)
        => status == CliSemanticStatus.Attention
            ? "requires attention"
            : CliStatusDefinitions.Read(status).MachineName;

    private static string FindingLabel(CliSemanticStatus status)
        => status == CliSemanticStatus.Attention
            ? "Requires attention"
            : char.ToUpperInvariant(Status(status)[0]) + Status(status)[1..];

    private static string LowerInitial(string value)
        => char.ToLowerInvariant(value[0]) + value[1..];
}
