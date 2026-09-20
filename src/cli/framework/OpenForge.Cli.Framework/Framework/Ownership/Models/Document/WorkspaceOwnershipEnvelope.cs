using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Ownership.Models.Document;

/// <summary>
/// The wire shape of <c>.agents/open-forge.lock.json</c>.
///
/// Unknown members are accepted rather than refused. A key this release has not
/// heard of means a newer one wrote the file, and refusing it would turn the
/// lock into the gate the state-file decision removed. Every member is optional
/// on read for the same reason: a truncated or partial file yields the entries
/// it does carry instead of nothing.
/// </summary>
internal sealed class WorkspaceOwnershipEnvelope
{
    [JsonPropertyName(WorkspaceOwnershipDefinitions.SchemaProperty)]
    [JsonPropertyOrder(-2)]
    public string? Schema { get; init; }

    [JsonPropertyOrder(-1)]
    public int SchemaVersion { get; init; }

    public FrameworkOwnershipEntry? Framework { get; init; }

    public ExtensionOwnershipEntry[]? Extensions { get; init; }

    public LibraryOwnershipEntry[]? Libraries { get; init; }
}

internal sealed class OwnedSourceEntry
{
    public string? Id { get; init; }

    public string? Version { get; init; }
}

internal sealed class OwnedRegionEntry
{
    public string? Path { get; init; }

    public string? Region { get; init; }
}

internal sealed class FrameworkOwnershipEntry
{
    public OwnedSourceEntry? Source { get; init; }

    public string[]? Paths { get; init; }

    public OwnedRegionEntry[]? Regions { get; init; }
}

internal sealed class ExtensionOwnershipEntry
{
    public string? Id { get; init; }

    public string? Version { get; init; }

    public string? Source { get; init; }

    public string[]? Dependencies { get; init; }

    public string[]? Paths { get; init; }

    public OwnedRegionEntry[]? Regions { get; init; }
}

internal sealed class LibraryOwnershipEntry
{
    public string? Id { get; init; }

    public string? SourceRoot { get; init; }

    public string? DestinationRoot { get; init; }

    public string[]? Paths { get; init; }
}
