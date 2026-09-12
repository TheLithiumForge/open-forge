using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Presentation;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Shared.Rendering;

internal static class LibraryAttachPresentation
{
    internal static CliHelpContent CreateHelp() => new([new CliHelpSection("Library", LibraryAttachDefinitions.Command.Description)]);
    internal static string RenderHuman(CliPresentationRequest<LibraryAttachResult> presentation)
        => Render(presentation, "attach");
    internal static string RenderJson(CliPresentationRequest<LibraryAttachResult> presentation)
    {
        Validate(presentation);
        var result = presentation.Result;
        return JsonSerializer.Serialize(new LibraryAttachJsonDocument
        {
            SchemaVersion = 1,
            Command = result.Command,
            Status = result.Status,
            Workspace = Workspace(result.Workspace),
            Result = result.Result,
            Next = result.Next is null ? null : new LibraryJsonNext
            {
                Command = result.Next.Command,
                Reason = result.Next.Reason,
            },
        }, LibraryAttachJsonContext.Default.LibraryAttachJsonDocument);
    }

    private static string Render(CliPresentationRequest<LibraryAttachResult> presentation, string leaf)
    {
        Validate(presentation);
        var result = presentation.Result;
        var builder = new StringBuilder();
        builder.Append("Library ").Append(leaf).Append(": id=")
            .Append(Escape(result.Result.Identity.LibraryId ?? "unavailable"))
            .Append("; mode=").Append(LibraryModeConverter.ReadWireValue(result.Result.Identity.Mode))
            .Append("; plan=").Append(LibraryPlanStateConverter.ReadWireValue(result.Result.Plan.State))
            .Append("; status=").AppendLine(CliStatusDefinitions.Read(result.Status).MachineName);
        foreach (var finding in result.Result.Findings)
        {
            builder.Append("Finding: ").Append(LibraryAttachDefinitions.ReadFindingCode(finding.Code))
                .Append("; path=").Append(Escape(finding.Path ?? "unavailable"))
                .Append("; ").AppendLine(Escape(finding.Cause));
        }

        builder.Append("Status: ").Append(CliStatusDefinitions.Read(result.Status).MachineName);
        return builder.ToString();
    }

    private static LibraryJsonWorkspace? Workspace(CliWorkspace? workspace)
        => workspace is null ? null : new LibraryJsonWorkspace
        {
            Path = workspace.LexicalRoot,
            SelectedBy = workspace.SelectedBy switch
            {
                CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                _ => throw new ArgumentOutOfRangeException(nameof(workspace), workspace.SelectedBy, "The workspace selection is not defined."),
            },
        };

    private static void Validate(CliPresentationRequest<LibraryAttachResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
    }

    private static string Escape(string value) => value.Replace('\r', ' ').Replace('\n', ' ');
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    Converters = new[]
    {
        typeof(LibraryApplicationStateConverter),
        typeof(LibraryAttachFindingCodeConverter),
        typeof(LibraryCollisionKindConverter),
        typeof(LibraryComparisonRelationConverter),
        typeof(LibraryExclusionKindConverter),
        typeof(LibraryExpectedStateKindConverter),
        typeof(LibraryLinkEffectKindConverter),
        typeof(LibraryLinkViewStateConverter),
        typeof(LibraryModeConverter),
        typeof(LibraryMutationInventoryStateConverter),
        typeof(LibraryMutationRecordStateConverter),
        typeof(LibraryOwnershipKindConverter),
        typeof(LibraryPlanStateConverter),
        typeof(LibraryRecordEffectConverter),
        typeof(LibraryRecordPublicationStateConverter),
        typeof(LibraryRecoveryStateConverter),
        typeof(LibraryResidualKindConverter),
        typeof(LibraryResidualStateConverter),
        typeof(LibrarySemanticStatusConverter),
        typeof(LibrarySourceRootViewStateConverter),
        typeof(LibraryVerificationStateConverter),
    })]
[JsonSerializable(typeof(LibraryAttachJsonDocument))]
internal sealed partial class LibraryAttachJsonContext : JsonSerializerContext;
