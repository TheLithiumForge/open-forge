using System.Text;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;

internal static class LibraryInspectHumanRenderer
{
    internal static string Render(CliPresentationRequest<LibraryInspectResult> presentation)
    {
        var payload = presentation.Result.Result;
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, $"Library inspection: {CliHumanText.Status(presentation.Result.Status)}.");
        var inventory = payload.Source.State == LibraryInventoryViewState.NotStarted
            ? "not scanned" : LibraryInventoryViewStateConverter.ReadWireValue(payload.Source.State);
        builder.AppendLine($"""
            Library: {LibraryHumanText.Value(payload.Record.Id)}
            Record: {LibraryHumanText.State(payload.Record.State)} ({LibraryHumanText.Value(payload.Record.Path)})
            Source root: {LibraryHumanText.Value(payload.Record.SourceRoot)} ({LibraryHumanText.State(payload.Source.RootState)})
            Destination root: {LibraryHumanText.Value(payload.Record.DestinationRoot)}
            Source inventory: {inventory}
            Link comparison: {LibraryHumanText.State(payload.Projection.State)}
            """);
        foreach (var finding in payload.Findings)
        {
            LibraryHumanText.AppendFinding(builder, finding.Status,
                code: LibraryInspectDefinitions.ReadFindingCode(finding.Code), cause: finding.Cause, path: finding.Path);
            if (finding.LibraryId is { } id && id != payload.Record.Id)
            {
                builder.AppendLine($"  Library: {LibraryHumanText.Value(id)}");
            }
        }
        LibraryInspectPathsHumanRenderer.Append(builder, presentation);
        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }
}
