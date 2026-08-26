namespace OpenForge.Cli.Core.Commands.References.Models.Presentation;

internal sealed class ReferencesJsonIncomingSelection
{
    public required string Mode { get; init; }

    public required ReferencesJsonSelectorOccurrence[] Supplied { get; init; }

    public required ReferencesJsonSelectorResolution[] Resolved { get; init; }

    public required ReferencesJsonIdentity[] EffectiveSources { get; init; }

    public required ReferencesJsonSourceLayerEvidence[] InspectedSources { get; init; }
}

internal sealed class ReferencesJsonSelectorOccurrence
{
    public required string Role { get; init; }

    public required string Value { get; init; }
}

internal sealed class ReferencesJsonSelectorResolution
{
    public required string Role { get; init; }

    public required int Occurrence { get; init; }

    public required string Supplied { get; init; }

    public required string Form { get; init; }

    public required string Resolution { get; init; }

    public required ReferencesJsonIdentity? Source { get; init; }

    public required string? Expansion { get; init; }

    public required ReferencesJsonIdentity[] Candidates { get; init; }
}

internal sealed class ReferencesJsonSourceLayerEvidence
{
    public required ReferencesJsonIdentity Source { get; init; }

    public required string Layer { get; init; }

    public required string Path { get; init; }
}

internal sealed class ReferencesJsonIdentity
{
    public required string Id { get; init; }

    public required string Path { get; init; }
}
