using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

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
        var builder = new StringBuilder();
        builder.AppendLine(Summary(result));
        builder.AppendLine($"Workspace: {Value(result.Workspace?.LexicalRoot)}");
        builder.AppendLine($"Selected by: {SelectedBy(result.Workspace)}");
        builder.AppendLine($"Target: {Value(result.Target.Id ?? result.Target.Requested)}");
        builder.AppendLine($"Path: {Value(result.Target.Path)}");
        builder.AppendLine($"Description: {Value(result.Metadata.Description)}");
        builder.AppendLine($"Mode: {RouteCreateDefinitions.ReadMachineName(result.Mode)}");
        builder.AppendLine($"Status: {Status(result.Status)}");
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
        if (expanded)
        {
            foreach (var finding in result.Findings)
            {
                builder.AppendLine($"{FindingLabel(finding.Status)}: {Value(finding.Cause)}");
            }

            builder.AppendLine(
                $"Recovery: {RouteCreateDefinitions.ReadMachineName(result.Recovery.State)}{PathSuffix(result.Recovery.ResidualPath)}");
            builder.AppendLine(
                $"Verification: {RouteCreateDefinitions.ReadMachineName(result.Verification)}");
        }
        else if (result.Status == CliSemanticStatus.Attention)
        {
            builder.AppendLine(
                $"Recovery: {RouteCreateDefinitions.ReadMachineName(result.Recovery.State)}{PathSuffix(result.Recovery.ResidualPath)}");
        }

        if (result.Mode == Models.Request.RouteCreateMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        if (result.Next is { } next)
        {
            builder.AppendLine($"Next: {Value(next.Command)} — {Value(next.Reason)}");
        }

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
            return "The routed file was not created.";
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
        var name = Status(status);
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private static string Value(string? value)
        => value is null ? "unavailable" : RouteCreateTextEscaping.Escape(value);

    private static string PathSuffix(string? path)
        => path is null
            ? string.Empty
            : $" / {RouteCreateTextEscaping.Escape(path)}";

    private static string Status(CliSemanticStatus status)
        => status == CliSemanticStatus.Attention
            ? "requires attention"
            : CliStatusDefinitions.Read(status).MachineName;
}
