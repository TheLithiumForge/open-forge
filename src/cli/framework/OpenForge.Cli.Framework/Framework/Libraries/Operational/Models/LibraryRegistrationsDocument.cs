
namespace OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

internal sealed record LibraryRegistrationsDocument
{
    public required int SchemaVersion { get; init; }
    public required LibraryRegistrationDocument[] Libraries { get; init; }
}

internal sealed record LibraryRegistrationDocument
{
    public required string Id { get; init; }
    public required string SourceRoot { get; init; }
    public required string DestinationRoot { get; init; }
    public required string[] Paths { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public bool GitIgnore { get; init; }
}
