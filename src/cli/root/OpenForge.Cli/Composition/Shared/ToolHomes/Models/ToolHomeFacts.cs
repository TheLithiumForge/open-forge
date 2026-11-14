using System.Collections.Immutable;

namespace OpenForge.Cli.Composition.Shared.ToolHomes.Models;

internal sealed class ToolClientRoots
{
    internal ToolClientRoots(ImmutableDictionary<string, string> directories, ImmutableDictionary<string, string> files,
        ImmutableDictionary<string, string> unavailable)
    {
        Directories = directories;
        Files = files;
        Unavailable = unavailable;
        Bindings = directories.AddRange(files);
    }

    internal ImmutableDictionary<string, string> Directories { get; }
    internal ImmutableDictionary<string, string> Files { get; }
    internal ImmutableDictionary<string, string> Unavailable { get; }
    internal ImmutableDictionary<string, string> Bindings { get; }
}

internal sealed record ToolHomeFacts
{
    public required string HomePath { get; init; }
    public required string BinPath { get; init; }
    public required string NativeHomePath { get; init; }
    public required ToolClientRoots Clients { get; init; }
    public required ImmutableDictionary<string, string> NativeEnvironment { get; init; }
}
