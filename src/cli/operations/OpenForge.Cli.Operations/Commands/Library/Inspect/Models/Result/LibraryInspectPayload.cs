
namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;

internal sealed record LibraryInspectPayload
{
    public required LibraryInspectRecordView Record { get; init; }

    public required LibraryInspectSourceView Source { get; init; }

    public required LibraryInspectProjectionView Projection { get; init; }

    public required LibraryInspectFinding[] Findings { get; init; }
}
