using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Observation;

// Status, Doctor and Tool Status check the tools that settings declare or the lock records.
internal static class ToolSelection
{
    internal static IReadOnlyList<string> DeclaredOrLocked(ToolPortableRecords? portable)
        => (portable?.Settings.Keys ?? []).Union(portable?.Lock.Keys ?? [], StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
}
