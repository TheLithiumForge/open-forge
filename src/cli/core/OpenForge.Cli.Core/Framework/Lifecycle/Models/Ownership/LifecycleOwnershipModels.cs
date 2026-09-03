using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;

internal enum LifecycleOwnershipReadState
{
    Trusted,
    Blocked,
    Interrupted,
}

internal enum LifecycleOwnershipSection
{
    Framework,
    Extensions,
}

internal enum LifecycleOwnershipManager
{
    Framework,
    Extension,
}

internal enum LifecycleOwnershipFindingCode
{
    LifecycleMissing,
    LifecycleUnavailable,
    LifecycleInvalid,
    FrameworkBlocked,
    ExtensionsBlocked,
    CrossSectionCollision,
    Interrupted,
}

internal sealed record LifecycleOwnershipSectionResult
{
    internal LifecycleOwnershipSectionResult(
        LifecycleOwnershipSection section,
        LifecycleOwnershipReadState state,
        string? cause)
    {
        if (!Enum.IsDefined(section))
        {
            throw new ArgumentOutOfRangeException(
                nameof(section),
                section,
                "The lifecycle ownership section is not defined.");
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The lifecycle ownership read state is not defined.");
        }

        if (state == LifecycleOwnershipReadState.Trusted && cause is not null
            || state != LifecycleOwnershipReadState.Trusted && string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "Lifecycle ownership section causes must match the read state.",
                nameof(cause));
        }

        Section = section;
        State = state;
        Cause = cause;
    }

    internal LifecycleOwnershipSection Section { get; }

    internal LifecycleOwnershipReadState State { get; }

    internal string? Cause { get; }
}

internal sealed record LifecycleOwnershipClaim
{
    internal LifecycleOwnershipClaim(
        string path,
        LifecycleOwnershipManager manager,
        string owner)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        if (!Enum.IsDefined(manager))
        {
            throw new ArgumentOutOfRangeException(
                nameof(manager),
                manager,
                "The lifecycle ownership manager is not defined.");
        }

        Path = path;
        Manager = manager;
        Owner = owner;
    }

    internal string Path { get; }

    internal LifecycleOwnershipManager Manager { get; }

    internal string Owner { get; }
}

internal sealed record LifecycleOwnershipFinding
{
    internal LifecycleOwnershipFinding(
        LifecycleOwnershipFindingCode code,
        LifecycleOwnershipSection? section,
        string? path,
        string cause)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The lifecycle ownership finding code is not defined.");
        }

        if (section is { } sectionValue && !Enum.IsDefined(sectionValue))
        {
            throw new ArgumentOutOfRangeException(
                nameof(section),
                section,
                "The lifecycle ownership finding section is not defined.");
        }

        if (path is not null && path.Length == 0)
        {
            throw new ArgumentException(
                "A lifecycle ownership finding path cannot be empty.",
                nameof(path));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Code = code;
        Section = section;
        Path = path;
        Cause = cause;
    }

    internal LifecycleOwnershipFindingCode Code { get; }

    internal LifecycleOwnershipSection? Section { get; }

    internal string? Path { get; }

    internal string Cause { get; }
}

internal sealed record LifecycleOwnershipReadResult
{
    internal LifecycleOwnershipReadResult(
        LifecycleOwnershipSectionResult framework,
        LifecycleOwnershipSectionResult extensions,
        IEnumerable<LifecycleOwnershipClaim> claims,
        FileExpectation? lifecycleFileExpectation,
        IEnumerable<LifecycleOwnershipFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(framework);
        ArgumentNullException.ThrowIfNull(extensions);
        ArgumentNullException.ThrowIfNull(claims);
        ArgumentNullException.ThrowIfNull(findings);
        if (framework.Section != LifecycleOwnershipSection.Framework
            || extensions.Section != LifecycleOwnershipSection.Extensions)
        {
            throw new ArgumentException(
                "Lifecycle ownership section results must retain Framework and Extensions identity.");
        }

        if (framework.State == LifecycleOwnershipReadState.Trusted
            && extensions.State == LifecycleOwnershipReadState.Trusted
            && lifecycleFileExpectation?.Kind != FileExpectationKind.File)
        {
            throw new ArgumentException(
                "Trusted lifecycle ownership requires one exact lifecycle-file expectation.",
                nameof(lifecycleFileExpectation));
        }

        Framework = framework;
        Extensions = extensions;
        Claims = claims
            .Select(claim => claim
                ?? throw new ArgumentException(
                    "Lifecycle ownership claims cannot contain null members.",
                    nameof(claims)))
            .Distinct()
            .OrderBy(claim => claim.Path, StringComparer.Ordinal)
            .ThenBy(claim => claim.Manager)
            .ThenBy(claim => claim.Owner, StringComparer.Ordinal)
            .ToImmutableArray();
        LifecycleFileExpectation = lifecycleFileExpectation;
        Findings = findings
            .Select(finding => finding
                ?? throw new ArgumentException(
                    "Lifecycle ownership findings cannot contain null members.",
                    nameof(findings)))
            .Distinct()
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.Section)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    internal LifecycleOwnershipSectionResult Framework { get; }

    internal LifecycleOwnershipSectionResult Extensions { get; }

    internal ImmutableArray<LifecycleOwnershipClaim> Claims { get; }

    internal FileExpectation? LifecycleFileExpectation { get; }

    internal ImmutableArray<LifecycleOwnershipFinding> Findings { get; }
}
