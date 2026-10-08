namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;

internal sealed record FrameworkFrontmatterTransformResult
{
    private FrameworkFrontmatterTransformResult(
        FrameworkFrontmatterTransformState state,
        ReadOnlyMemory<byte>? bytes,
        string? cause)
    {
        State = state;
        Bytes = bytes;
        Cause = cause;
    }

    internal FrameworkFrontmatterTransformState State { get; }

    internal ReadOnlyMemory<byte>? Bytes { get; }

    internal string? Cause { get; }

    internal static FrameworkFrontmatterTransformResult Unchanged(ReadOnlyMemory<byte> bytes)
        => new(FrameworkFrontmatterTransformState.Unchanged, bytes, null);

    internal static FrameworkFrontmatterTransformResult Changed(ReadOnlyMemory<byte> bytes)
        => new(FrameworkFrontmatterTransformState.Changed, bytes, null);

    internal static FrameworkFrontmatterTransformResult Invalid(string cause)
        => new(FrameworkFrontmatterTransformState.Invalid, null, cause);
}
