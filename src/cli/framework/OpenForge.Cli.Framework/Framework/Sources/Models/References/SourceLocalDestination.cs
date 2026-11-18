namespace OpenForge.Cli.Core.Framework.Sources.Models.References;

internal enum SourceLocalDestinationKind
{
    External,
    Local,
    Unresolved,
}

/// <summary>
/// The lexical meaning of one authored destination: an external URI, a contained
/// workspace path with its raw fragment suffix, or a destination that cannot be resolved.
/// </summary>
internal sealed record SourceLocalDestination
{
    internal static SourceLocalDestination External { get; } = new() { Kind = SourceLocalDestinationKind.External };

    internal static SourceLocalDestination Unresolved { get; } = new() { Kind = SourceLocalDestinationKind.Unresolved };

    internal required SourceLocalDestinationKind Kind { get; init; }

    internal string? CanonicalPath { get; init; }

    internal string? LexicalPath { get; init; }

    internal string Suffix { get; init; } = string.Empty;

    internal bool IsFragmentOnly { get; init; }
}
