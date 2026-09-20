using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Library.Detach.Models;

internal sealed record LibraryDetachData
{
    public required string Mode { get; init; }

    public required string? Id { get; init; }

    public required string? SourceFolder { get; init; }

    public required string? DestinationFolder { get; init; }

    public required bool RegistrationRemoved { get; init; }

    public required LibraryDetachDataPermissions Permissions { get; init; }

    public required IReadOnlyList<LibraryDetachDataEffect> Effects { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<LibraryDetachDataExpectedState>? ExpectedStates { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibraryDetachDataVerification? Verification { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LibraryDetachDataRecovery? Recovery { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<LibraryDetachDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetails { get; init; } = [];
}

internal sealed record LibraryDetachDataPermissions
{
    public required IReadOnlyList<string> Required { get; init; }

    public required IReadOnlyList<string> Missing { get; init; }

    public required string Decision { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }
}

internal sealed record LibraryDetachDataEffect
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Target { get; init; }

    [JsonIgnore]
    internal string Text { get; init; } = string.Empty;
}

internal sealed record LibraryDetachDataExpectedState
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required long? Length { get; init; }

    public required string? Sha256 { get; init; }

    public required string? RawRelativeTarget { get; init; }
}

internal sealed record LibraryDetachDataVerification
{
    public required string State { get; init; }

    public required string Registration { get; init; }
}

internal sealed record LibraryDetachDataRecovery
{
    public required string State { get; init; }

    public required string? Path { get; init; }

    public required IReadOnlyList<LibraryDetachDataResidual> Residuals { get; init; }
}

internal sealed record LibraryDetachDataResidual
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string State { get; init; }
}

internal sealed record LibraryDetachDataTextRow(string? Path, string Text);
