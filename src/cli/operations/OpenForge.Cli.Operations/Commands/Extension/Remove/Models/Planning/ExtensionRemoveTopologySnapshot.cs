using System.Collections.Immutable;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;

internal sealed class ExtensionRemoveTopologySnapshot
{
    internal ExtensionRemoveTopologySnapshot(ExtensionRemoveTopology topology)
    {
        IntendedTargetBytes = new ReadOnlyDictionary<string, ImmutableArray<byte>>(
            topology.IntendedTargetBytes.ToDictionary(
                pair => pair.Key,
                pair => ImmutableArray.CreateRange(pair.Value),
                StringComparer.Ordinal));
        GeneratedEntries = new ReadOnlyDictionary<string, IReadOnlyList<GeneratedNavigationEntry>>(
            topology.GeneratedEntries.ToDictionary(
                pair => pair.Key,
                pair =>
                {
                    ImmutableArray<GeneratedNavigationEntry> entries = [.. pair.Value];
                    return (IReadOnlyList<GeneratedNavigationEntry>)entries;
                },
                StringComparer.Ordinal));
        ProtectedPaths = topology.ProtectedPaths.ToImmutableHashSet(StringComparer.Ordinal);
    }

    internal IReadOnlyDictionary<string, ImmutableArray<byte>> IntendedTargetBytes { get; }

    internal IReadOnlyDictionary<string, IReadOnlyList<GeneratedNavigationEntry>> GeneratedEntries { get; }

    internal IReadOnlySet<string> ProtectedPaths { get; }
}
