using System.Collections.Immutable;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Topology;

internal sealed class ExtensionUpdateTopologySnapshot
{
    internal ExtensionUpdateTopologySnapshot(ExtensionUpdateTopology topology)
    {
        IntendedTargetBytes = SnapshotBytes(topology.IntendedTargetBytes);
        GeneratedTargetBytes = SnapshotBytes(topology.GeneratedTargetBytes);
        ImmutableArray<ExtensionUpdateGeneratedRegion> regions = [.. topology.Regions];
        Regions = regions;
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

    internal IReadOnlyDictionary<string, ImmutableArray<byte>> GeneratedTargetBytes { get; }

    internal IReadOnlyList<ExtensionUpdateGeneratedRegion> Regions { get; }

    internal IReadOnlyDictionary<string, IReadOnlyList<GeneratedNavigationEntry>> GeneratedEntries { get; }

    internal IReadOnlySet<string> ProtectedPaths { get; }

    private static IReadOnlyDictionary<string, ImmutableArray<byte>> SnapshotBytes(IReadOnlyDictionary<string, byte[]> values)
        => new ReadOnlyDictionary<string, ImmutableArray<byte>>(values.ToDictionary(
            pair => pair.Key,
            pair => ImmutableArray.CreateRange(pair.Value),
            StringComparer.Ordinal));
}
