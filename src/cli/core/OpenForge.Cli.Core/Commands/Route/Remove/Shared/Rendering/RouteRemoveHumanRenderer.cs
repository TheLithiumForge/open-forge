using System.Text;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;

internal static partial class RouteRemoveHumanRenderer
{
    internal static string Render(CliPresentationRequest<RouteRemoveResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, Summary(result));
        AppendIdentity(builder, result);
        AppendSubject(builder, result.Subject);
        AppendOwnership(builder, result.Ownership, expanded);
        AppendReferences(builder, result.References);
        AppendGeneratedNavigation(builder, result.GeneratedNavigation);
        AppendEffects(builder, result.Effects);
        AppendUnchanged(builder, result.UnchangedPaths);
        AppendRecovery(builder, result.Recovery);
        AppendFindings(builder, result.Findings);
        builder.AppendLine($"Verification: {RouteRemoveDefinitions.ReadMachineName(result.Verification)}");
        if (result.Mode == RouteRemoveMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        CliHumanText.AppendNext(builder, presentation);

        return builder.ToString().TrimEnd();
    }

    private static void AppendIdentity(StringBuilder builder, RouteRemoveResult result)
    {
        builder.AppendLine($"Source: {Value(result.Source.Id ?? result.Source.Requested)}");
        builder.AppendLine($"Source path: {Value(result.Source.Path)}");
        builder.AppendLine($"Subject: {Optional(result.Subject.Kind, RouteRemoveDefinitions.ReadMachineName)}");
        builder.AppendLine($"Mode: {RouteRemoveDefinitions.ReadMachineName(result.Mode)}");
        builder.AppendLine(
            $"Plan: completeness={RouteRemoveDefinitions.ReadMachineName(result.Plan.Completeness)}, safety={RouteRemoveDefinitions.ReadMachineName(result.Plan.Safety)}");
    }

    private static string Summary(RouteRemoveResult result)
    {
        if (IsError(result.Status))
        {
            return CliHumanText.Outcome("Route Remove", result.Status);
        }

        if (result.Status == CliSemanticStatus.Attention)
        {
            return "The verified Route Remove result requires attention.";
        }

        var noun = result.Subject.Kind == RouteRemoveSubjectKind.Category
            ? "category"
            : "file";
        return result.Mode == RouteRemoveMode.DryRun
            ? $"The routed {noun} would be removed."
            : $"The routed {noun} was removed.";
    }

    private static bool IsError(CliSemanticStatus status)
        => status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted;

    private static string Optional<T>(T? value, Func<T, string> map)
        where T : struct
        => value is { } established ? map(established) : "unavailable";

    private static string Value(string? value)
        => value is null ? "unavailable" : RouteTextEscaping.Escape(value);

}
