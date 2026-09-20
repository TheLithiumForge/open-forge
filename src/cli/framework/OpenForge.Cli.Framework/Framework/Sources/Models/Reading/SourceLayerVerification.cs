using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Reading;

internal enum SourceLayerVerificationState
{
    Verified,
    Missing,
    Unsafe,
    Unavailable,
    Changed,
    Cancelled,
}

internal sealed class SourceLayerVerification
{
    internal SourceLayerVerification(
        SourceLayer layer,
        SourceLayerVerificationState state,
        string? currentPhysicalPath,
        FilesystemFailure? failure)
    {
        ArgumentNullException.ThrowIfNull(layer);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The source layer verification state is not defined.");
        }

        var normalizedCurrentPhysicalPath = NormalizeOptionalPhysicalPath(currentPhysicalPath);
        if (state is SourceLayerVerificationState.Verified or SourceLayerVerificationState.Changed
            && normalizedCurrentPhysicalPath is null)
        {
            throw new ArgumentException("A verified or changed source layer requires its current physical path.", nameof(currentPhysicalPath));
        }

        if (state != SourceLayerVerificationState.Unavailable && failure is not null)
        {
            throw new ArgumentException("Only an unavailable source layer verification can carry a failure.", nameof(failure));
        }

        Layer = layer;
        State = state;
        CurrentPhysicalPath = normalizedCurrentPhysicalPath;
        Failure = failure;
    }

    internal SourceLayer Layer { get; }

    internal SourceLayerVerificationState State { get; }

    internal string? CurrentPhysicalPath { get; }

    internal FilesystemFailure? Failure { get; }

    private static string? NormalizeOptionalPhysicalPath(string? path)
    {
        if (path is null)
        {
            return null;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var normalized = Path.GetFullPath(path);
        if (!Path.IsPathRooted(path)
            || !string.Equals(normalized, path, StringComparison.Ordinal))
        {
            throw new ArgumentException("A current physical path must be absolute and normalized.", nameof(path));
        }

        return normalized;
    }
}
