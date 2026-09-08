namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

// The final component is classified without resolving a link target.  The
// value is intentionally neutral so ordinary mutation and Library link
// mutation can share the same safety boundary.
internal enum NoFollowLeafState
{
    Missing,
    OrdinaryFile,
    Directory,
    RelativeFileLink,
    Link,
    ReparsePoint,
    Special,
    Inaccessible,
    Unknown,
}

internal enum NoFollowLinkKind
{
    SymbolicLink,
    Junction,
    Other,
}

internal enum NoFollowLinkTargetForm
{
    Relative,
    Absolute,
    Unsupported,
    Unavailable,
}

internal sealed record RelativeFileLinkIdentity
{
    private RelativeFileLinkIdentity(
        NoFollowLinkKind linkKind,
        string rawRelativeTarget)
    {
        LinkKind = linkKind;
        RawRelativeTarget = rawRelativeTarget;
    }

    internal NoFollowLinkKind LinkKind { get; }

    internal string RawRelativeTarget { get; }

    internal static RelativeFileLinkIdentity Create(
        NoFollowLinkKind linkKind,
        string rawRelativeTarget)
    {
        if (linkKind != NoFollowLinkKind.SymbolicLink)
        {
            throw new ArgumentException(
                "A relative file-link identity requires a symbolic-link kind.",
                nameof(linkKind));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(rawRelativeTarget);
        if (Path.IsPathFullyQualified(rawRelativeTarget)
            || rawRelativeTarget.Contains('\\')
            || rawRelativeTarget.StartsWith('/'))
        {
            throw new ArgumentException(
                "A relative file-link identity requires the exact slash-separated relative target.",
                nameof(rawRelativeTarget));
        }

        return new RelativeFileLinkIdentity(linkKind, rawRelativeTarget);
    }
}

internal sealed record NoFollowLinkIdentity
{
    private NoFollowLinkIdentity(
        NoFollowLinkKind linkKind,
        string? rawTarget,
        NoFollowLinkTargetForm targetForm)
    {
        LinkKind = linkKind;
        RawTarget = rawTarget;
        TargetForm = targetForm;
    }

    internal NoFollowLinkKind LinkKind { get; }

    internal string? RawTarget { get; }

    internal NoFollowLinkTargetForm TargetForm { get; }

    internal static NoFollowLinkIdentity Create(
        NoFollowLinkKind linkKind,
        string? rawTarget,
        NoFollowLinkTargetForm targetForm)
    {
        if (!Enum.IsDefined(linkKind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(linkKind),
                linkKind,
                "The link kind is not defined.");
        }

        if (!Enum.IsDefined(targetForm))
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetForm),
                targetForm,
                "The link target form is not defined.");
        }

        if (targetForm == NoFollowLinkTargetForm.Unavailable)
        {
            if (rawTarget is not null)
            {
                throw new ArgumentException(
                    "An unavailable link target cannot carry a raw target.",
                    nameof(rawTarget));
            }
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(rawTarget);
        }

        return new NoFollowLinkIdentity(linkKind, rawTarget, targetForm);
    }
}

internal sealed record NoFollowLeafObservation
{
    private NoFollowLeafObservation(
        string logicalPath,
        NoFollowLeafState state,
        RelativeFileLinkIdentity? relativeFileLink,
        NoFollowLinkIdentity? link,
        FilesystemFailure? failure)
    {
        LogicalPath = NormalizeLogicalPath(logicalPath);
        State = state;
        RelativeFileLink = relativeFileLink;
        Link = link;
        Failure = failure;
    }

    internal string LogicalPath { get; }

    internal NoFollowLeafState State { get; }

    internal RelativeFileLinkIdentity? RelativeFileLink { get; }

    internal NoFollowLinkIdentity? Link { get; }

    internal FilesystemFailure? Failure { get; }

    internal static NoFollowLeafObservation Missing(string logicalPath)
        => Create(logicalPath, NoFollowLeafState.Missing);

    internal static NoFollowLeafObservation OrdinaryFile(string logicalPath)
        => Create(logicalPath, NoFollowLeafState.OrdinaryFile);

    internal static NoFollowLeafObservation Directory(string logicalPath)
        => Create(logicalPath, NoFollowLeafState.Directory);

    internal static NoFollowLeafObservation CreateRelativeFileLink(
        string logicalPath,
        RelativeFileLinkIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        return new NoFollowLeafObservation(
            logicalPath,
            NoFollowLeafState.RelativeFileLink,
            identity,
            link: null,
            failure: null);
    }

    internal static NoFollowLeafObservation CreateLink(
        string logicalPath,
        NoFollowLinkIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        return new NoFollowLeafObservation(
            logicalPath,
            NoFollowLeafState.Link,
            relativeFileLink: null,
            identity,
            failure: null);
    }

    internal static NoFollowLeafObservation Classified(
        string logicalPath,
        NoFollowLeafState state,
        FilesystemFailure? failure = null)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A no-follow leaf state must be a defined value.");
        }

        if (state is NoFollowLeafState.Missing
            or NoFollowLeafState.OrdinaryFile
            or NoFollowLeafState.Directory
            or NoFollowLeafState.RelativeFileLink
            or NoFollowLeafState.Link)
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A classified no-follow observation requires an exceptional leaf state.");
        }

        if (state is (NoFollowLeafState.Inaccessible or NoFollowLeafState.Unknown)
            && failure is null)
        {
            throw new ArgumentException(
                "An inaccessible or unknown leaf observation requires a failure fact.",
                nameof(failure));
        }

        return new NoFollowLeafObservation(
            logicalPath,
            state,
            relativeFileLink: null,
            link: null,
            failure);
    }

    private static NoFollowLeafObservation Create(
        string logicalPath,
        NoFollowLeafState state)
        => new(
            logicalPath,
            state,
            relativeFileLink: null,
            link: null,
            failure: null);

    private static string NormalizeLogicalPath(string logicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
        return Path.GetFullPath(logicalPath);
    }
}
