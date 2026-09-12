using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static partial class RouteUpdateHumanRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteUpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, Summary(result));
        if (result.Target.Id is { } id)
        {
            builder.AppendLine($"ID: {Value(id)}");
        }
        else
        {
            builder.AppendLine($"Target: {Value(result.Target.Requested)}");
        }

        builder.AppendLine($"Path: {Value(result.Target.Path)}");
        builder.AppendLine($"Mode: {RouteUpdateDefinitions.ReadMachineName(result.Mode)}");
        var completeness = RouteUpdateDefinitions.ReadMachineName(result.Plan.Completeness);
        var safety = RouteUpdateDefinitions.ReadMachineName(result.Plan.Safety);
        var body = RouteUpdateDefinitions.ReadMachineName(result.Plan.Body);
        builder.AppendLine(
            $"Plan: completeness={completeness}, safety={safety}, body={body}");
        if (result.Template is { } template)
        {
            builder.AppendLine(
                $"Template: {Value(template.Id ?? template.Requested)} ({Value(template.Path)}) / {RouteUpdateDefinitions.ReadMachineName(template.Decision)}");
        }

        AppendPatch(builder, result);
        AppendEffects(builder, result);
        AppendUnchanged(builder, result.UnchangedPaths);
        if (result.Plan.Body == RouteUpdateBodyState.AuthoredBodyProtected
            && !result.Findings.Any(finding => finding.Code == RouteUpdateFindingCode.TemplateBodyProtected))
        {
            builder.AppendLine("Template body not applied: the target already has authored body content.");
        }

        foreach (var finding in result.Findings)
        {
            var cause = finding.Code == RouteUpdateFindingCode.TemplateBodyProtected
                ? "Template body not applied: the target already has authored body content."
                : Value(finding.Cause);
            builder.AppendLine($"{CliHumanText.Status(finding.Status).ToUpperInvariant()}: {cause} [{RouteUpdateDefinitions.ReadMachineName(finding.Code)}]");
            if (finding.Target is { } target)
            {
                builder.AppendLine($"  {Value(target)}");
            }
        }

        builder.AppendLine($"Recovery: {RouteUpdateDefinitions.ReadMachineName(result.Recovery.State)}{PathSuffix(result.Recovery.ResidualPath)}");
        builder.AppendLine($"Verification: {RouteUpdateDefinitions.ReadMachineName(result.Verification)}");

        if (result.Mode == Models.Request.RouteUpdateMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        CliHumanText.AppendNext(builder, presentation);

        return builder.ToString().TrimEnd();
    }

    private static string Summary(RouteUpdateResult result)
    {
        if (IsError(result.Status))
        {
            return CliHumanText.Outcome("Route Update", result.Status);
        }

        if (result.Status == CliSemanticStatus.Attention)
        {
            return "The routed source requires attention.";
        }

        if (result.Effects.IsEmpty)
        {
            return "The routed source is up to date.";
        }

        return result.Mode == Models.Request.RouteUpdateMode.DryRun
            ? "The routed source would be updated."
            : "The routed source was updated.";
    }

    private static bool IsError(CliSemanticStatus status)
        => status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted;

    private static string Value(string? value)
        => value is null ? "unavailable" : RouteTextEscaping.Escape(value);

    private static string PathSuffix(string? path)
        => path is null
            ? string.Empty
            : $" / {RouteTextEscaping.Escape(path)}";

}
