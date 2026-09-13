using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Presentation;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Shared.Rendering;

internal static class LibrarySyncPresentation
{
    internal static CliHelpContent CreateHelp() => new(
    [
        new CliHelpSection("Syntax", """
              open-forge library sync <library-id> [--dry-run] [global options]
            """),
        new CliHelpSection("Write policy", """
              Reconcile a registered Library with its complete source inventory. Use --dry-run to preview all changes without writing.
            """),
        new CliHelpSection("Examples", """
              open-forge library sync shared --dry-run
              open-forge library sync shared
            """),
    ]);
    internal static string RenderHuman(CliPresentationRequest<LibrarySyncResult> presentation)
        => Render(presentation);
    internal static string RenderJson(CliPresentationRequest<LibrarySyncResult> presentation)
    {
        Validate(presentation);
        var result = presentation.Result;
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, presentation.Result.Result),
                LibrarySyncJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(new LibrarySyncJsonDocument
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
        }, LibrarySyncJsonContext.Default.LibrarySyncJsonDocument);
    }

    private static string Render(CliPresentationRequest<LibrarySyncResult> presentation)
    {
        Validate(presentation);
        var result = presentation.Result;
        var payload = result.Result;
        var view = presentation.Presentation.View;
        var style = CliHumanStyle.For(presentation);
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, LibraryHumanText.Heading("Library sync", result.Status, payload.Identity.Mode));
        LibraryObservationHumanRenderer.AppendIdentity(builder, payload.Identity);
        foreach (var finding in payload.Findings)
        {
            LibraryHumanText.AppendFinding(builder, style.Finding(finding.Status),
                code: LibrarySyncDefinitions.ReadFindingCode(finding.Code), cause: finding.Cause, path: finding.Path);
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

    private static void Validate(CliPresentationRequest<LibrarySyncResult> presentation)
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
        typeof(LibrarySyncFindingCodeConverter),
        typeof(LibraryVerificationStateConverter),
    })]
[JsonSerializable(typeof(LibrarySyncJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<LibrarySyncPayload>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class LibrarySyncJsonContext : JsonSerializerContext
{
    private static readonly Lazy<LibrarySyncJsonContext> CompactContext = new(CreateCompact);

    private static LibrarySyncJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static LibrarySyncJsonContext Compact => CompactContext.Value;
}
