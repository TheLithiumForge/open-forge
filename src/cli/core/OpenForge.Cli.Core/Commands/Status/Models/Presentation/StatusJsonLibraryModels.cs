namespace OpenForge.Cli.Core.Commands.Status.Models.Presentation;

internal sealed record StatusJsonLibrary
{
    public required string State { get; init; }
    public required StatusJsonLibraryRecord Record { get; init; }
    public required StatusJsonLibraryRegistration[] Records { get; init; }
    public required StatusJsonLibraryCounts Counts { get; init; }
}

internal sealed record StatusJsonLibraryRecord(string Path, string State);

internal sealed record StatusJsonLibraryRegistration
{
    public required string Id { get; init; }
    public required string SourceRoot { get; init; }
    public required string DestinationRoot { get; init; }
    public required string SourceRootState { get; init; }
    public required string SourceAvailability { get; init; }
    public required StatusJsonLibraryRegisteredLinks RegisteredLinks { get; init; }
}

internal sealed record StatusJsonLibraryRegisteredLinks
{
    public required StatusJsonIntegerValue Registered { get; init; }
    public required StatusJsonLibraryLinkCounts Counts { get; init; }
    public required StatusJsonLibraryLink[] Links { get; init; }
}

internal sealed record StatusJsonLibraryLink(string SourcePath, string DestinationPath, string ExpectedRelativeLink, string? SourceId, string State);

internal sealed record StatusJsonLibraryCounts
{
    public required StatusJsonIntegerValue Registered { get; init; }
    public required StatusJsonIntegerValue Current { get; init; }
    public required StatusJsonIntegerValue Missing { get; init; }
    public required StatusJsonIntegerValue Changed { get; init; }
    public required StatusJsonIntegerValue Blocked { get; init; }
    public required StatusJsonIntegerValue Unavailable { get; init; }
}

internal sealed record StatusJsonLibraryLinkCounts
{
    public required StatusJsonIntegerValue Current { get; init; }
    public required StatusJsonIntegerValue Missing { get; init; }
    public required StatusJsonIntegerValue Changed { get; init; }
    public required StatusJsonIntegerValue Blocked { get; init; }
    public required StatusJsonIntegerValue Unavailable { get; init; }
}
