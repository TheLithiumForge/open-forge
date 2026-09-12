using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static partial class RouteUpdateHumanRenderer
{
    internal static string Render(
        CliPresentationRequest<RouteUpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var builder = new StringBuilder();
        builder.AppendLine(Summary(result));
        builder.AppendLine($"Workspace: {Value(result.Workspace?.LexicalRoot)}");
        builder.AppendLine($"Selected by: {SelectedBy(result.Workspace)}");
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
        builder.AppendLine($"Status: {Status(result.Status)}");
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

        AppendChanged(builder, result);
        AppendEffects(
            builder,
            result,
            showPreview: true);
        AppendUnchanged(builder, result.UnchangedPaths);
        if (result.Plan.Body == RouteUpdateBodyState.AuthoredBodyProtected)
        {
            builder.AppendLine(
                "Template body not applied: the target already has authored body content.");
        }

        if (expanded)
        {
            foreach (var finding in result.Findings)
            {
                if (finding.Code == RouteUpdateFindingCode.TemplateBodyProtected)
                {
                    continue;
                }

                builder.AppendLine($"{FindingLabel(finding.Status)}: {Value(finding.Cause)}");
            }

            builder.AppendLine(
                $"Recovery: {RouteUpdateDefinitions.ReadMachineName(result.Recovery.State)}{PathSuffix(result.Recovery.ResidualPath)}");
            builder.AppendLine(
                $"Verification: {RouteUpdateDefinitions.ReadMachineName(result.Verification)}");
        }
        else if (result.Status == CliSemanticStatus.Attention)
        {
            builder.AppendLine(
                $"Recovery: {RouteUpdateDefinitions.ReadMachineName(result.Recovery.State)}{PathSuffix(result.Recovery.ResidualPath)}");
        }

        if (result.Mode == Models.Request.RouteUpdateMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        if (result.Next is { } next)
        {
            builder.AppendLine($"Next: {Value(next.Command)} — {Value(next.Reason)}");
        }

        return builder.ToString().TrimEnd();
    }

    private static string Summary(RouteUpdateResult result)
    {
        if (IsError(result.Status))
        {
            var primary = result.Findings.First(finding => finding.Status == result.Status);
            return $"Route Update did not update {Value(result.Target.Id ?? result.Target.Requested)}: {Value(primary.Cause)}";
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

    private static string FindingLabel(CliSemanticStatus status)
    {
        if (status == CliSemanticStatus.Attention)
        {
            return "Attention";
        }

        var name = Status(status);
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private static string Value(string? value)
        => value is null ? "unavailable" : RouteTextEscaping.Escape(value);

    private static string PathSuffix(string? path)
        => path is null
            ? string.Empty
            : $" / {RouteTextEscaping.Escape(path)}";

    private static string Status(CliSemanticStatus status)
        => status == CliSemanticStatus.Attention
            ? "requires attention"
            : CliStatusDefinitions.Read(status).MachineName;
}
