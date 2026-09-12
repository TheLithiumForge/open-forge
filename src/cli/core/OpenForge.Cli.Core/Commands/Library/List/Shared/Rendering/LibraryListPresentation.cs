using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using System.Text.Json.Serialization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.List.Models.Presentation;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Library.List.Shared.Rendering;

internal static class LibraryListPresentation
{
    internal static CliHelpContent CreateHelp() => new(
    [
        new CliHelpSection("Inspection", """
              List registered Libraries and their link status without scanning all source files.
            """),
        new CliHelpSection("Examples", """
              open-forge library list
              open-forge library list --json
            """),
    ]);

    internal static string RenderHuman(CliPresentationRequest<LibraryListResult> presentation)
    {
        Validate(presentation);
        return LibraryListHumanRenderer.Render(presentation);
    }

    internal static string RenderJson(CliPresentationRequest<LibraryListResult> presentation)
    {
        Validate(presentation);
        var result = presentation.Result;
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, presentation.Result.Result),
                LibraryListJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(
            new LibraryListJsonDocument
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
            LibraryListJsonContext.Default.LibraryListJsonDocument);
    }

    private static void Validate(CliPresentationRequest<LibraryListResult> presentation)
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
        typeof(LibraryCoverageConverter),
        typeof(LibraryLinkViewStateConverter),
        typeof(LibraryListFindingCodeConverter),
        typeof(LibraryListInventoryStateConverter),
        typeof(LibraryRecordViewStateConverter),
        typeof(LibrarySemanticStatusConverter),
        typeof(LibrarySourceRootViewStateConverter),
    })]
[JsonSerializable(typeof(LibraryListJsonDocument))]
[JsonSerializable(typeof(CliCompactJsonDocument<LibraryListPayload>), TypeInfoPropertyName = "CompactDocument")]
internal sealed partial class LibraryListJsonContext : JsonSerializerContext
{
    private static readonly Lazy<LibraryListJsonContext> CompactContext = new(CreateCompact);

    private static LibraryListJsonContext CreateCompact()
        => new(new System.Text.Json.JsonSerializerOptions(Default.Options) { WriteIndented = false });

    internal static LibraryListJsonContext Compact => CompactContext.Value;
}
