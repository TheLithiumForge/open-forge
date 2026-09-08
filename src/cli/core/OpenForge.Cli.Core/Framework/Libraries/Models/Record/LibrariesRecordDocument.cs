
namespace OpenForge.Cli.Core.Framework.Libraries.Models.Record;

internal sealed record LibrariesRecordDocument
{
    public required int SchemaVersion { get; init; }
    public required LibraryRecordDocument[] Libraries { get; init; }
}

internal sealed record LibraryRecordDocument
{
    public required string Id { get; init; }
    public required string SourceRoot { get; init; }
    public required string[] Paths { get; init; }
}
