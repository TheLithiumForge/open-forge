using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.List.Models.Presentation;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
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
        var result = presentation.Result;
        var builder = new StringBuilder();
        if (presentation.Presentation.View == CliView.Compact)
        {
            builder.Append("Library list: record=")
                .Append(LibraryRecordViewStateConverter.ReadWireValue(result.Result.Record.State))
                .Append("; inventory=")
                .Append(LibraryListInventoryStateConverter.ReadWireValue(result.Result.Inventory))
                .Append("; coverage=")
                .Append(LibraryCoverageConverter.ReadWireValue(result.Result.Coverage))
                .Append("; libraries=")
                .Append(result.Result.Libraries.Length)
                .Append("; status=")
                .AppendLine(Status(result.Status));
        }
        else
        {
            builder.AppendLine("Open Forge library list");
            builder.AppendLine($"Workspace: {Escape(result.Workspace?.LexicalRoot ?? "unavailable")}");
            builder.AppendLine(
                $"Record: {Escape(result.Result.Record.Path)} ({LibraryRecordViewStateConverter.ReadWireValue(result.Result.Record.State)}; libraries={ReadCount(result.Result.Record.LibraryCount)})");
            builder.AppendLine($"Inventory: {LibraryListInventoryStateConverter.ReadWireValue(result.Result.Inventory)}");
            builder.AppendLine($"Coverage: {LibraryCoverageConverter.ReadWireValue(result.Result.Coverage)}");
            builder.AppendLine($"Libraries: {result.Result.Libraries.Length}");
        }

        foreach (var library in result.Result.Libraries)
        {
            builder.AppendLine(
                $"Library: {Escape(library.Id)}; source-root={Escape(library.SourceRoot)}; state={LibrarySourceRootViewStateConverter.ReadWireValue(library.SourceRootState)}");
            foreach (var path in library.Paths)
            {
                builder.Append("Path: ")
                    .Append(Escape(path.SourcePath))
                    .Append(" -> ")
                    .Append(Escape(path.DestinationPath))
                    .Append("; source-id=")
                    .Append(Escape(path.SourceId ?? "unavailable"))
                    .Append("; expected=")
                    .Append(Escape(path.ExpectedRelativeLink ?? "unavailable"))
                    .Append("; observed=")
                    .Append(Escape(path.ObservedRelativeLink ?? "unavailable"))
                    .Append("; link=")
                    .AppendLine(LibraryLinkViewStateConverter.ReadWireValue(path.State));
            }
        }

        foreach (var finding in result.Result.Findings)
        {
            builder.Append("Finding: ")
                .Append(LibraryListDefinitions.ReadFindingCode(finding.Code))
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

    internal static string RenderJson(CliPresentationRequest<LibraryListResult> presentation)
    {
        Validate(presentation);
        var result = presentation.Result;
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

    private static string Status(CliSemanticStatus value)
        => CliStatusDefinitions.Read(value).MachineName;

    private static string ReadCount(int? value)
        => value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "unknown";

    private static string Escape(string value)
        => value.Replace('\r', ' ').Replace('\n', ' ');
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
internal sealed partial class LibraryListJsonContext : JsonSerializerContext;
