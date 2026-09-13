using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;

internal static class RouteCreateHumanRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteCreateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var style = CliHumanStyle.For(presentation);
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, Summary(result));
        builder.AppendLine($"Target: {Value(result.Target.Id ?? result.Target.Requested)}");
        builder.AppendLine($"Path: {Value(result.Target.Path)}");
        builder.AppendLine($"Description: {Value(result.Metadata.Description)}");
        builder.AppendLine($"Mode: {RouteCreateDefinitions.ReadMachineName(result.Mode)}");
        builder.AppendLine(
            $"Plan: completeness={RouteCreateDefinitions.ReadMachineName(result.Plan.Completeness)}, safety={RouteCreateDefinitions.ReadMachineName(result.Plan.Safety)}");

        if (result.Template is { } template)
        {
            builder.AppendLine($"Template: {Value(template.Id)} ({Value(template.Path)})");
        }

        AppendEffects(
            builder,
            result,
            showChanges: expanded || result.Mode == Models.Request.RouteCreateMode.DryRun);
        AppendUnchanged(builder, result.UnchangedPaths);
        foreach (var finding in result.Findings)
        {
            builder.AppendLine($"{style.Finding(finding.Status)}: {Value(finding.Cause)} [{RouteCreateDefinitions.ReadMachineName(finding.Code)}]");
            if (finding.Target is { } target)
            {
                builder.AppendLine($"  {Value(target)}");
            }
        }

        builder.AppendLine($"Recovery: {RouteCreateDefinitions.ReadMachineName(result.Recovery.State)}{PathSuffix(result.Recovery.ResidualPath)}");
        builder.AppendLine($"Verification: {RouteCreateDefinitions.ReadMachineName(result.Verification)}");

        if (result.Mode == Models.Request.RouteCreateMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        CliHumanText.AppendNext(builder, presentation);

        return builder.ToString().TrimEnd();
    }

    private static string Summary(RouteCreateResult result)
    {
        if (result.Status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted)
        {
            return CliHumanText.Outcome("Route Create", result.Status);
        }

        if (result.Mode == Models.Request.RouteCreateMode.DryRun)
        {
            return "The routed file would be created.";
        }

        if (result.Status == CliSemanticStatus.Attention)
        {
            return "The verified Route Create result requires attention.";
        }

        return result.Effects.Length == 0
            ? "The routed file already matches the requested content."
            : "The routed file was created.";
    }

    private static void AppendEffects(
        StringBuilder builder,
        RouteCreateResult result,
        bool showChanges)
    {
        if (result.Effects.Length == 0)
        {
            builder.AppendLine("No files changed.");
            return;
        }

        builder.AppendLine("Effects:");
        foreach (var effect in result.Effects)
        {
            builder.AppendLine(
                $"  {Value(effect.Path)}: {RouteCreateDefinitions.ReadMachineName(effect.Action)} {RouteCreateDefinitions.ReadMachineName(effect.Kind)} / {RouteCreateDefinitions.ReadMachineName(effect.Outcome)} / residual={RouteCreateDefinitions.ReadMachineName(effect.Residual)}");
            if (showChanges)
            {
                builder.AppendLine($"    Before: {Value(effect.Change.Before)}");
                builder.AppendLine($"    Expected: {Value(effect.Change.Expected)}");
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
            builder.AppendLine($"  {Value(path)}");
        }
    }

    private static string Value(string? value)
        => value is null ? "unavailable" : RouteTextEscaping.Escape(value);

    private static string PathSuffix(string? path)
        => path is null
            ? string.Empty
            : $" / {RouteTextEscaping.Escape(path)}";

}
