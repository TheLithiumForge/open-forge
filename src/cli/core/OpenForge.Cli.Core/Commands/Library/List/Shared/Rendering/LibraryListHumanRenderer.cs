using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Library.List.Shared.Rendering;

internal static class LibraryListHumanRenderer
{
    internal static string Render(CliPresentationRequest<LibraryListResult> presentation)
    {
        var payload = presentation.Result.Result;
        var style = CliHumanStyle.For(presentation);
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, Heading(payload.Record));
        var count = payload.Record.LibraryCount?.ToString(CultureInfo.InvariantCulture) ?? "unavailable";
        var inventory = payload.Inventory == LibraryListInventoryState.NotRequested
            ? "not scanned (library list checks registered links only)" : LibraryListInventoryStateConverter.ReadWireValue(payload.Inventory);
        builder.AppendLine($"""
            Record: {LibraryHumanText.State(payload.Record.State)} ({LibraryHumanText.Value(payload.Record.Path)})
            Registered Libraries: {count}
            Source inventory: {inventory}
            Checks: {LibraryHumanText.State(payload.Coverage)}
            """);
        foreach (var finding in payload.Findings)
        {
            LibraryHumanText.AppendFinding(builder, style.Finding(finding.Status),
                code: LibraryListDefinitions.ReadFindingCode(finding.Code), cause: finding.Cause, path: finding.Path);
            if (finding.LibraryId is { } id)
            {
                builder.AppendLine($"  Library: {LibraryHumanText.Value(id)}");
            }
        }
        foreach (var library in payload.Libraries)
        {
            builder.AppendLine();
            builder.AppendLine($"""
                Library: {LibraryHumanText.Value(library.Id)}
                  Source root: {LibraryHumanText.Value(library.SourceRoot)} ({LibraryHumanText.State(library.SourceRootState)})
                  Destination root: {LibraryHumanText.Value(library.DestinationRoot)}
                """);
            foreach (var path in library.Paths)
            {
                builder.AppendLine($"  {LibraryHumanText.Value(path.SourcePath)} -> {LibraryHumanText.Value(path.DestinationPath)}; {LibraryHumanText.State(path.State)}; ID {LibraryHumanText.Value(path.SourceId)}");
                if (presentation.Presentation.View == CliView.Expanded)
                {
                    builder.AppendLine($"""
                            Expected target: {LibraryHumanText.Value(path.ExpectedRelativeLink)}
                            Observed target: {LibraryHumanText.Value(path.ObservedRelativeLink)}
                        """);
                }
            }
        }
        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static string Heading(LibraryListRecordView record)
    {
        if (record.State == LibraryRecordViewState.Missing || (record.State == LibraryRecordViewState.Complete && record.LibraryCount == 0))
        {
            return "No Libraries are registered.";
        }
        if (record.State == LibraryRecordViewState.Complete && record.LibraryCount is { } count)
        {
            var label = count == 1 ? "Library" : "Libraries";
            return string.Create(CultureInfo.InvariantCulture, $"{count} {label} registered.");
        }
        return "Library registration could not be checked completely.";
    }
}
