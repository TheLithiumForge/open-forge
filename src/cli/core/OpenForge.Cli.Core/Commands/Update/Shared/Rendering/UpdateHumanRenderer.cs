using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static partial class UpdateHumanRenderer
{
    internal static string Render(CliPresentationRequest<UpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var style = CliHumanStyle.For(presentation);
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, Summary(result));
        builder.AppendLine($"Mode: {UpdateDefinitions.ReadMachineName(result.Mode)}; force: {MachineBoolean(result.Force)}; prune: {MachineBoolean(result.Prune)}; automatic: {MachineBoolean(result.Automatic)}");
        AppendSource(builder, result.Source, expanded);
        AppendFindings(builder, result.Findings, style);
        AppendEffects(builder, result, expanded);
        AppendGeneratedNavigation(builder, result.GeneratedNavigation);
        AppendLifecycle(builder, result.Lifecycle);
        AppendRecovery(builder, result.Recovery);
        builder.AppendLine(
            $"Verification: {UpdateDefinitions.ReadMachineName(result.Verification)}");
        if (result.Mode == UpdateMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        CliHumanText.AppendNext(builder, presentation);

        return builder.ToString().TrimEnd();
    }

    private static void AppendSource(StringBuilder builder, UpdateSource? source, bool expanded)
    {
        if (source is null)
        {
            builder.AppendLine("Source: unavailable");
            return;
        }

        builder.AppendLine(CultureInfo.InvariantCulture,
            $"Source: embedded Framework; {source.AssetCount} assets");
        if (expanded)
        {
            builder.AppendLine($"  ID: {Value(source.Id)}; version: {Value(source.Version)}; inventory fingerprint: {Value(source.InventoryFingerprint)}");
        }
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
                => CliHumanText.Outcome("Framework update", result.Status),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.Status,
                "The Update status is not defined."),
        };

    private static string MachineBoolean(bool value) => value ? "true" : "false";

}
