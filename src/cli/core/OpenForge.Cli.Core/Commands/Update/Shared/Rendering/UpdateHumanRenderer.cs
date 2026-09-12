using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static partial class UpdateHumanRenderer
{
    internal static string Render(CliPresentationRequest<UpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var builder = new StringBuilder();
        builder.AppendLine(Summary(result));
        AppendIdentity(builder, result);
        AppendSource(builder, result.Source);
        AppendComparisons(builder, result.Comparisons, expanded);
        AppendGeneratedNavigation(builder, result.GeneratedNavigation);
        AppendEffects(builder, result.Effects);
        AppendLifecycle(builder, result.Lifecycle);
        AppendRecovery(
            builder,
            result.Recovery,
            expanded || result.Recovery.State is UpdateRecoveryState.Retained or UpdateRecoveryState.Unknown);
        AppendFindings(builder, result.Findings, result.Findings.Count != 0);
        builder.AppendLine(
            $"Verification: {UpdateDefinitions.ReadMachineName(result.Verification)}");
        if (result.Mode == UpdateMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        if (result.Next is { } next)
        {
            builder.AppendLine($"Next: {Value(next.Command)} — {Value(next.Reason)}");
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendIdentity(StringBuilder builder, UpdateResult result)
    {
        builder.AppendLine(string.Create(
            CultureInfo.InvariantCulture,
            $"""
            Workspace: {Value(result.Workspace?.LexicalRoot)}
            Selected by: {SelectedBy(result.Workspace)}
            Flags: mode={UpdateDefinitions.ReadMachineName(result.Mode)}, force={MachineBoolean(result.Force)}, prune={MachineBoolean(result.Prune)}, automatic={MachineBoolean(result.Automatic)}
            Status: {Status(result.Status)}
            """).Replace("\n", Environment.NewLine, StringComparison.Ordinal));
    }

    private static void AppendSource(StringBuilder builder, UpdateSource? source)
    {
        if (source is null)
        {
            builder.AppendLine("Source: unavailable");
            return;
        }

        builder.AppendLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Source: {Value(source.Id)} / version={Value(source.Version)} / inventory={Value(source.InventoryFingerprint)} / assets={source.AssetCount}"));
    }

    private static string Summary(UpdateResult result)
        => result.Status switch
        {
            CliSemanticStatus.Complete when result.Mode == UpdateMode.DryRun
                => "The managed Framework update is ready to apply.",
            CliSemanticStatus.Complete => "The managed Framework is up to date.",
            CliSemanticStatus.Attention => "The managed Framework update requires attention.",
            CliSemanticStatus.Incomplete => "The managed Framework update is incomplete.",
            CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted
                => "The managed Framework was not updated.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.Status,
                "The Update status is not defined."),
        };

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

    private static string MachineBoolean(bool value) => value ? "true" : "false";

    private static string Status(CliSemanticStatus status)
        => status == CliSemanticStatus.Attention
            ? "requires attention"
            : CliStatusDefinitions.Read(status).MachineName;
}
