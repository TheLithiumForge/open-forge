using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
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
        var result = presentation.Result;
        var payload = result.Result;
        var builder = new StringBuilder();
        if (presentation.Presentation.View == CliView.Compact)
        {
            builder.Append("Library inspect: id=")
                .Append(Escape(payload.Record.Id ?? "unavailable"))
                .Append("; record=")
                .Append(LibraryRecordViewStateConverter.ReadWireValue(payload.Record.State))
                .Append("; inventory=")
                .Append(LibraryInventoryViewStateConverter.ReadWireValue(payload.Source.State))
                .Append("; projection=")
                .Append(LibraryCoverageConverter.ReadWireValue(payload.Projection.State))
                .Append("; status=")
                .AppendLine(Status(result.Status));
        }
        else
        {
            builder.AppendLine("Open Forge library inspect");
            builder.AppendLine($"Workspace: {Escape(result.Workspace?.LexicalRoot ?? "unavailable")}");
            builder.AppendLine($"Library: {Escape(payload.Record.Id ?? "unavailable")}");
            builder.AppendLine(
                $"Record: {Escape(payload.Record.Path)} ({LibraryRecordViewStateConverter.ReadWireValue(payload.Record.State)})");
            builder.AppendLine(
                $"Source root: {Escape(payload.Record.SourceRoot ?? "unavailable")} ({LibrarySourceRootViewStateConverter.ReadWireValue(payload.Source.RootState)})");
            builder.AppendLine(
                $"Inventory: {LibraryInventoryViewStateConverter.ReadWireValue(payload.Source.State)} ({payload.Source.EligiblePaths.Length} eligible paths)");
            builder.AppendLine($"Projection: {LibraryCoverageConverter.ReadWireValue(payload.Projection.State)}");
        }

        foreach (var path in payload.Record.RegisteredPaths)
        {
            builder.Append("Registered: ")
                .Append(Escape(path.SourcePath))
                .Append(" -> ")
                .Append(Escape(path.DestinationPath))
                .Append("; source-id=")
                .Append(Escape(path.SourceId ?? "unavailable"))
                .Append("; expected=")
                .AppendLine(Escape(path.ExpectedRelativeLink ?? "unavailable"));
        }

        foreach (var path in payload.Source.EligiblePaths)
        {
            builder.AppendLine(
                $"Eligible: {Escape(path.SourcePath)} -> {Escape(path.DestinationPath)}; source-id={Escape(path.SourceId ?? "unavailable")}");
        }

        foreach (var comparison in payload.Projection.Comparisons)
        {
            builder.Append("Comparison: ")
                .Append(Escape(comparison.SourcePath))
                .Append(" -> ")
                .Append(Escape(comparison.DestinationPath))
                .Append("; source-id=")
                .Append(Escape(comparison.SourceId ?? "unavailable"))
                .Append("; relation=")
                .Append(LibraryComparisonRelationConverter.ReadWireValue(comparison.Relation))
                .Append("; observed=")
                .AppendLine(Escape(comparison.ObservedRelativeLink ?? "unavailable"));
        }

        foreach (var finding in payload.Findings)
        {
            builder.Append("Finding: ")
                .Append(LibraryInspectDefinitions.ReadFindingCode(finding.Code))
                .Append("; library=")
                .Append(Escape(finding.LibraryId ?? "unavailable"))
                .Append("; path=")
                .Append(Escape(finding.Path ?? "unavailable"))
                .Append("; ")
                .AppendLine(Escape(finding.Cause));
        }

        builder.AppendLine($"Status: {Status(result.Status)}");
        return builder.ToString().TrimEnd();
    }

    internal static string RenderJson(CliPresentationRequest<LibraryInspectResult> presentation)
    {
        Validate(presentation);
        var result = presentation.Result;
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

    private static string Status(CliSemanticStatus value)
        => CliStatusDefinitions.Read(value).MachineName;

    private static string Escape(string value)
        => value.Replace('\r', ' ').Replace('\n', ' ');
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
internal sealed partial class LibraryInspectJsonContext : JsonSerializerContext;
