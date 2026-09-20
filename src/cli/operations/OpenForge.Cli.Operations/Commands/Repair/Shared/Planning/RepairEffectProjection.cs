using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal static class RepairEffectProjection
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static FileStateSnapshot Project(
        FileStateSnapshot expected,
        IReadOnlyList<RepairChange> changes)
    {
        var bytes = expected.Bytes.ToArray();
        foreach (var change in changes.OrderByDescending(value => value.Occurrence.ByteOffset))
        {
            var offset = checked((int)change.Occurrence.ByteOffset);
            var length = checked((int)change.Occurrence.ByteLength);
            var replacement = StrictUtf8.GetBytes(change.IntendedDestination);
            bytes = [.. bytes[..offset], .. replacement, .. bytes[(offset + length)..]];
        }

        return FileStateSnapshot.File(
            expected.LogicalPath,
            expected.PhysicalPath
                ?? throw new InvalidOperationException("A Repair file effect requires physical identity."),
            bytes);
    }

    internal static bool SpanMatches(RepairableReferenceInput input)
    {
        if (!input.ObservedFileState.HasBytes)
        {
            return false;
        }

        var expected = StrictUtf8.GetBytes(input.ExpectedDestination);
        var offset = input.Occurrence.ByteOffset;
        return offset >= 0
            && offset <= int.MaxValue
            && input.Occurrence.ByteLength == expected.Length
            && offset + expected.Length <= input.ObservedFileState.Bytes.Length
            && input.ObservedFileState.Bytes
                .AsSpan(checked((int)offset), expected.Length)
                .SequenceEqual(expected);
    }

    internal static bool HasCommonExpectedState(IReadOnlyList<RepairableReferenceInput> inputs)
        => inputs.Skip(1).All(input =>
            input.ObservedFileState.Expectation == inputs[0].ObservedFileState.Expectation);

    internal static bool HasOverlap(IReadOnlyList<RepairableReferenceInput> inputs)
    {
        var ordered = inputs.OrderBy(input => input.Occurrence.ByteOffset).ToArray();
        return ordered.Zip(ordered.Skip(1)).Any(pair =>
            pair.First.Occurrence.ByteOffset + pair.First.Occurrence.ByteLength
                > pair.Second.Occurrence.ByteOffset);
    }
}
