using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using System.Text.Json.Serialization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;

internal static class LibraryInspectPresentation
{
    internal static CliHelpContent CreateHelp() => new(
    [
        new CliHelpSection("Syntax", """
              open-forge library inspect <library-id> [global options]
            """),
        new CliHelpSection("Inspection", """
              Scan all source files and destination links for one registered Library. The Library ID is its management ID, not an Open Forge source ID.
            """),
        new CliHelpSection("Examples", """
              open-forge library inspect shared
              open-forge library inspect shared --json
            """),
    ]);

    internal static string RenderHuman(CliPresentationRequest<LibraryInspectResult> presentation)
    {
        Validate(presentation);
        return LibraryInspectHumanRenderer.Render(presentation);
    }

    internal static string RenderJson(CliPresentationRequest<LibraryInspectResult> presentation)
    {
        Validate(presentation);
        var result = presentation.Result;
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, presentation.Result.Result),
                LibraryInspectJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(
            new LibraryInspectJsonDocument
            {
                SchemaVersion = 1,
                Command = result.Command,
                Status = result.Status,
                Workspace = result.Workspace is null
                    ? null
                    : new LibraryJsonWorkspace
                    {
                        Path = result.Workspace.LexicalRoot,
                        SelectedBy = WorkspaceSelection(result.Workspace.SelectedBy),
                    },
                Result = result.Result,
                Next = null,
            },
            LibraryInspectJsonContext.Default.LibraryInspectJsonDocument);
    }

    private static void Validate(CliPresentationRequest<LibraryInspectResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
    }

    private static string WorkspaceSelection(CliWorkspaceSelectionMethod value)
        => value switch
        {
            CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The workspace selection method is not defined."),
        };

}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    Converters = new[]
    {
        typeof(LibraryComparisonRelationConverter),
        typeof(LibraryCoverageConverter),
        typeof(LibraryInspectFindingCodeConverter),
        typeof(LibraryInventoryViewStateConverter),
        typeof(LibraryRecordViewStateConverter),
        typeof(LibrarySemanticStatusConverter),
        typeof(LibrarySourceRootViewStateConverter),
    })]
[JsonSerializable(typeof(LibraryInspectJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<LibraryInspectPayload>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class LibraryInspectJsonContext : JsonSerializerContext
{
    private static readonly Lazy<LibraryInspectJsonContext> CompactContext = new(CreateCompact);

    private static LibraryInspectJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static LibraryInspectJsonContext Compact => CompactContext.Value;
}
