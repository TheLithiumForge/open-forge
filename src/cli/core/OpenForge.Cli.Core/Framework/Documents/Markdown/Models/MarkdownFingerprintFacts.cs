namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal enum MarkdownFingerprintState
{
    Semantic,
    ExactBytes,
    Unavailable,
}

internal enum MarkdownFingerprintRegionState
{
    Valid,
    Absent,
    Invalid,
    Ambiguous,
    Unavailable,
}

internal sealed record MarkdownFingerprintRegion
{
    internal MarkdownFingerprintRegion(
        MarkdownFingerprintRegionState state,
        string? startMarker,
        string? endMarker,
        int? startByteOffset,
        int? endByteOffset,
        int? excludedInteriorByteLength,
        bool markerLinesRetained)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Markdown fingerprint region state is not defined.");
        }

        if (state == MarkdownFingerprintRegionState.Valid
            && (startMarker is null
                || endMarker is null
                || startByteOffset is null
                || endByteOffset is null
                || excludedInteriorByteLength is null
                || startByteOffset < 0
                || endByteOffset < startByteOffset
                || excludedInteriorByteLength != endByteOffset - startByteOffset
                || !markerLinesRetained))
        {
            throw new ArgumentException("A valid Markdown fingerprint region requires retained markers and exact offsets.", nameof(state));
        }

        if (state != MarkdownFingerprintRegionState.Valid
            && (startByteOffset is not null
                || endByteOffset is not null
                || excludedInteriorByteLength is not (0 or null)
                || markerLinesRetained))
        {
            throw new ArgumentException("Only a valid Markdown fingerprint region can retain offsets or marker lines.", nameof(state));
        }

        State = state;
        StartMarker = startMarker;
        EndMarker = endMarker;
        StartByteOffset = startByteOffset;
        EndByteOffset = endByteOffset;
        ExcludedInteriorByteLength = excludedInteriorByteLength;
        MarkerLinesRetained = markerLinesRetained;
    }

    internal MarkdownFingerprintRegionState State { get; }

    internal string? StartMarker { get; }

    internal string? EndMarker { get; }

    internal int? StartByteOffset { get; }

    internal int? EndByteOffset { get; }

    internal int? ExcludedInteriorByteLength { get; }

    internal bool MarkerLinesRetained { get; }

    internal static MarkdownFingerprintRegion Absent()
        => new(MarkdownFingerprintRegionState.Absent, null, null, null, null, 0, false);

    internal static MarkdownFingerprintRegion Invalid(MarkdownFingerprintRegionState state)
        => new(state, null, null, null, null, null, false);
}

internal sealed record MarkdownFingerprintFacts
{
    internal MarkdownFingerprintFacts(
        MarkdownFingerprintState state,
        string policy,
        string? sha256,
        MarkdownFingerprintRegion region,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Markdown fingerprint state is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(policy);
        ArgumentNullException.ThrowIfNull(region);
        if (sha256 is not null
            && (sha256.Length != 64
                || sha256.Any(character => character is < '0' or > '9' and < 'a' or > 'f')))
        {
            throw new ArgumentException("A Markdown fingerprint hash must be lowercase SHA-256.", nameof(sha256));
        }

        if (state is MarkdownFingerprintState.Semantic or MarkdownFingerprintState.ExactBytes
            && sha256 is null)
        {
            throw new ArgumentException("A readable Markdown fingerprint requires a hash.", nameof(sha256));
        }

        Policy = policy;
        State = state;
        Sha256 = sha256;
        Region = region;
        Cause = cause;
    }

    internal string Policy { get; }

    internal MarkdownFingerprintState State { get; }

    internal string? Sha256 { get; }

    internal MarkdownFingerprintRegion Region { get; }

    internal string? Cause { get; }

    internal bool IsSemantic => State == MarkdownFingerprintState.Semantic;
}
