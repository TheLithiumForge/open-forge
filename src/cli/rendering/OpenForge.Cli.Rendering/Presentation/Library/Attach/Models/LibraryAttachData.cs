using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Library.Attach.Models;

internal sealed record LibraryAttachData
{
    public required string Mode { get; init; }

    public required string? Id { get; init; }

    public required string? SourceFolder { get; init; }

    public required string? DestinationFolder { get; init; }

    public required bool Recorded { get; init; }

    public required LibraryAttachDataPermissions Permissions { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<LibraryAttachDataLink>? Links { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibraryAttachDataInventory? Inventory { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<LibraryAttachDataExpectedState>? ExpectedStates { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibraryAttachDataVerification? Verification { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibraryAttachDataRecovery? Recovery { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<LibraryAttachDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextSummaryLines { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetailLines { get; init; } = [];

    [JsonIgnore]
    internal bool ShowNoChanges { get; init; }
}

internal sealed record LibraryAttachDataPermissions
{
    public required string Decision { get; init; }

    public required IReadOnlyList<string> Required { get; init; }

    public required IReadOnlyList<string> Missing { get; init; }

    public required bool Saved { get; init; }
}

internal sealed record LibraryAttachDataLink
{
    public required string Path { get; init; }

    public required string Target { get; init; }
}

internal sealed record LibraryAttachDataInventory
{
    public required int Eligible { get; init; }

    public required int Excluded { get; init; }
}

internal sealed record LibraryAttachDataExpectedState
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public long? Length { get; init; }

    public string? Sha256 { get; init; }

    public string? Target { get; init; }
}

internal sealed record LibraryAttachDataVerification
{
    public required string State { get; init; }

    public required int LinksCreated { get; init; }

    public required int LinksExpected { get; init; }

    public required bool Recorded { get; init; }
}

internal sealed record LibraryAttachDataRecovery
{
    public required string State { get; init; }

    public required string? Path { get; init; }

    public required IReadOnlyList<LibraryAttachDataRemaining> Remaining { get; init; }
}

internal sealed record LibraryAttachDataRemaining
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string State { get; init; }
}

internal sealed record LibraryAttachDataTextRow(string Path, string Wording);
