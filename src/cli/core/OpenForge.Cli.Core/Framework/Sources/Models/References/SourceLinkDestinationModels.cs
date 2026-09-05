using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Sources.Models.References;

internal enum SourceLinkTargetKind
{
    Local,
    External,
    Unsupported,
}

internal enum SourceLinkTargetResolution
{
    Complete,
    Missing,
    FragmentMissing,
    Malformed,
    Absolute,
    Query,
    EncodingUnsupported,
    OutsideWorkspace,
    PhysicalEscape,
    Ambiguous,
    Unreadable,
    Unsupported,
    ExternalUnchecked,
}

internal enum SourceLinkNetworkState
{
    NetworkNotAttempted,
}

internal enum SourceLinkDestinationFindingCode
{
    DestinationMalformed,
    DestinationUnsupported,
    TargetUnsafe,
    InvalidEncoding,
    TargetMissing,
    TargetUnreadable,
    TargetAmbiguous,
    IdentityCollision,
    FragmentMissing,
}

internal sealed record SourceLinkIdentity
{
    public required string? Id { get; init; }

    public required string Path { get; init; }
}

internal sealed record SourceLinkDestinationInput
{
    private string _sourceCanonicalPath = string.Empty;

    public required CliWorkspace Workspace { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required string SourceCanonicalPath
    {
        get => _sourceCanonicalPath;
        init
        {
            _sourceCanonicalPath = SourceWorkspaceRelativePath.ValidateMarkdown(
                value,
                nameof(value));
        }
    }

    public required string RawDestination { get; init; }
}

internal sealed record SourceLinkTarget
{
    public required SourceLinkTargetKind Kind { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public required string? PhysicalPath { get; init; }

    public required SourceLayerKind? Layer { get; init; }

    public required SourceLinkTargetResolution Resolution { get; init; }

    public required SourceLinkNetworkState? Network { get; init; }
}

internal sealed record SourceLinkDestinationFinding
{
    public required SourceLinkDestinationFindingCode Code { get; init; }

    public required string Cause { get; init; }

    public required IReadOnlyList<SourceLinkIdentity> Candidates { get; init; }
}

internal sealed record SourceLinkDestinationFacts
{
    public required string? Fragment { get; init; }

    public required SourceLinkTarget Target { get; init; }

    public required SourceLinkDestinationFinding? Finding { get; init; }

    public SourceLinkCanonicalFragment? CanonicalFragment { get; private init; }

    internal SourceLinkDestinationFacts WithCanonicalFragment(
        SourceLinkCanonicalFragment canonicalFragment)
    {
        ArgumentNullException.ThrowIfNull(canonicalFragment);
        if (Target.Resolution != SourceLinkTargetResolution.FragmentMissing
            || Fragment is null
            || Finding?.Code != SourceLinkDestinationFindingCode.FragmentMissing
            || !string.Equals(Fragment, canonicalFragment.Authored, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "A canonical fragment requires one exact fragment-missing destination fact.");
        }

        return this with { CanonicalFragment = canonicalFragment };
    }
}

internal sealed record SourceLinkCanonicalFragment
{
    internal SourceLinkCanonicalFragment(string authored, string canonical)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authored);
        ArgumentException.ThrowIfNullOrWhiteSpace(canonical);
        if (string.Equals(authored, canonical, StringComparison.Ordinal))
        {
            throw new ArgumentException("A canonical fragment correction must change the authored spelling.", nameof(canonical));
        }

        Authored = authored;
        Canonical = canonical;
    }

    internal string Authored { get; }

    internal string Canonical { get; }
}
