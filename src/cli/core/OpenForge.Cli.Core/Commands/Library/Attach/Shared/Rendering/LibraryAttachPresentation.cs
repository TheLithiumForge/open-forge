using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Presentation;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Shared.Rendering;

internal static class LibraryAttachPresentation
{
    internal static CliHelpContent CreateHelp() => new(
    [
        new CliHelpSection("Syntax", """
              open-forge library attach <library-id> <source-root> [--to <directory>] [--dry-run] [global options]
            """),
        new CliHelpSection("Source and destination", """
              Register a source directory contained in the workspace and create relative file links. Both paths are workspace-relative. --to defaults to the workspace root.
            """),
        new CliHelpSection("Preview and permissions", """
              Use --dry-run to inspect the complete plan without writing. Destinations outside .agents require the applicable workspace permission.
            """),
        new CliHelpSection("Examples", """
              open-forge library attach shared vendor/shared --dry-run
              open-forge library attach shared vendor/shared --to docs --dry-run
            """),
    ]);
    internal static string RenderHuman(CliPresentationRequest<LibraryAttachResult> presentation)
        => Render(presentation);
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

    private static string Render(CliPresentationRequest<LibraryAttachResult> presentation)
    {
        Validate(presentation);
        var result = presentation.Result;
        var payload = result.Result;
        var view = presentation.Presentation.View;
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, LibraryHumanText.Heading("Library attach", result.Status, payload.Identity.Mode));
        LibraryObservationHumanRenderer.AppendIdentity(builder, payload.Identity);
        foreach (var finding in payload.Findings)
        {
            LibraryHumanText.AppendFinding(builder, finding.Status,
                code: LibraryAttachDefinitions.ReadFindingCode(finding.Code), cause: finding.Cause, path: finding.Path);
            if (finding.LibraryId is { } id && id != payload.Identity.LibraryId)
            {
                builder.AppendLine($"  Library: {LibraryHumanText.Value(id)}");
            }
        }
        LibraryApplicationHumanRenderer.Append(builder, payload.Application, view);
        LibraryObservationHumanRenderer.AppendRecord(builder, payload.Record, view);
        LibraryObservationHumanRenderer.AppendSource(builder, payload.Source, view);
        LibraryObservationHumanRenderer.AppendProjection(builder, payload.Projection, view);
        LibraryPlanHumanRenderer.Append(builder, payload.Plan, payload.Record.Path, view);
        LibraryPermissionHumanRenderer.Append(builder, payload.Permissions, view);
        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
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
