using System.Collections.Immutable;
using System.CommandLine;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;

namespace OpenForge.Cli.Core.Commands.Stripping.Models.Binding;

/// <summary>The selection flags uninstall and export both declare.</summary>
internal sealed record StripSelectionSymbols
{
    public required Option<string[]> Strip { get; init; }
    public required Option<string?> Flatten { get; init; }
    public required Option<string[]> RemoveEdited { get; init; }
    public required Option<bool> DryRun { get; init; }
}

/// <summary>The bound selection flags, or the input problem that stops binding.</summary>
internal sealed record StripSelection
{
    public ImmutableArray<StripPart> Parts { get; init; } = [];
    public string? Flatten { get; init; }
    public ImmutableArray<string> RemoveEdited { get; init; } = [];
    public bool DryRun { get; init; }
    public string? Problem { get; init; }
}
