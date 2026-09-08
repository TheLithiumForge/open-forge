using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Recovery.Models;

internal enum RecoveryEntryKind
{
    OrdinaryCreate,
    OrdinaryReplace,
    OrdinaryReplaceGeneratedRegion,
    OrdinaryDelete,
    RelativeFileLinkCreate,
    RelativeFileLinkDelete,
}

internal enum RecoveryEntryStateKind
{
    Missing,
    OrdinaryFile,
    RelativeFileLink,
}

internal sealed record RecoveryEntryState
{
    private RecoveryEntryState(
        RecoveryEntryStateKind kind,
        RecoveryContentIdentity? ordinaryFile,
        RelativeFileLinkIdentity? relativeFileLink)
    {
        Kind = kind;
        OrdinaryFile = ordinaryFile;
        RelativeFileLink = relativeFileLink;
    }

    internal RecoveryEntryStateKind Kind { get; }

    internal RecoveryContentIdentity? OrdinaryFile { get; }

    internal RelativeFileLinkIdentity? RelativeFileLink { get; }

    internal static RecoveryEntryState Missing { get; } = new(
        RecoveryEntryStateKind.Missing,
        ordinaryFile: null,
        relativeFileLink: null);

    internal static RecoveryEntryState Ordinary(RecoveryContentIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        return new RecoveryEntryState(
            RecoveryEntryStateKind.OrdinaryFile,
            identity,
            relativeFileLink: null);
    }

    internal static RecoveryEntryState RelativeLink(RelativeFileLinkIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        return new RecoveryEntryState(
            RecoveryEntryStateKind.RelativeFileLink,
            ordinaryFile: null,
            identity);
    }
}

internal sealed record RecoveryEntry
{
    private RecoveryEntry(
        int ordinal,
        CanonicalRelativePath logicalPath,
        RecoveryEntryKind kind,
        RecoveryEntryState prior,
        RecoveryEntryState intended,
        string? priorPayload)
    {
        Ordinal = ordinal;
        LogicalPath = logicalPath;
        Kind = kind;
        Prior = prior;
        Intended = intended;
        PriorPayload = priorPayload;
    }

    internal int Ordinal { get; }

    internal CanonicalRelativePath LogicalPath { get; }

    internal string TargetPath => LogicalPath.Value;

    internal RecoveryEntryKind Kind { get; }

    internal RecoveryEntryState Prior { get; }

    internal RecoveryEntryState Intended { get; }

    internal string? PriorPayload { get; }

    internal bool IntendedAbsent
        => Intended.Kind == RecoveryEntryStateKind.Missing;

    internal static RecoveryEntry Create(
        int ordinal,
        CanonicalRelativePath logicalPath,
        RecoveryEntryKind kind,
        RecoveryEntryState prior,
        RecoveryEntryState intended,
        string? priorPayload = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ordinal);

        ArgumentNullException.ThrowIfNull(logicalPath);
        ArgumentNullException.ThrowIfNull(prior);
        ArgumentNullException.ThrowIfNull(intended);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The recovery entry kind is not defined.");
        }

        var requiresOrdinaryPayload = prior.Kind == RecoveryEntryStateKind.OrdinaryFile;
        if (requiresOrdinaryPayload)
        {
            ValidatePayload(priorPayload, ordinal);
        }
        else if (priorPayload is not null)
        {
            throw new ArgumentException(
                "Only an ordinary-file prior state may carry a recovery payload name.",
                nameof(priorPayload));
        }

        if (!IsAdmissibleStatePair(kind, prior.Kind, intended.Kind))
        {
            throw new ArgumentException(
                "The recovery entry state pair does not match its typed entry kind.",
                nameof(intended));
        }

        return new RecoveryEntry(
            ordinal,
            logicalPath,
            kind,
            prior,
            intended,
            priorPayload);
    }

    internal static RecoveryEntry FromTarget(
        RecoveryBundleInput input,
        RecoveryBundleTarget target,
        int ordinal)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(target);
        if (!target.RequiresRecovery)
        {
            throw new ArgumentException(
                "A recovery entry requires a reversible target.",
                nameof(target));
        }

        if (target.LinkEffect is { } linkEffect)
        {
            if (target.LinkBefore is not { } linkBefore)
            {
                throw new ArgumentException(
                    "A relative file-link recovery target requires its prior observation.",
                    nameof(target));
            }

            var expectedState = FromLinkState(linkEffect.Expected);
            var observed = linkBefore.State switch
            {
                NoFollowLeafState.Missing => RecoveryEntryState.Missing,
                NoFollowLeafState.RelativeFileLink when linkBefore.RelativeFileLink is { } link =>
                    RecoveryEntryState.RelativeLink(link),
                _ => throw new ArgumentException(
                    "A relative file-link recovery target requires a Missing or RelativeFileLink observation.",
                    nameof(target)),
            };
            if (observed != expectedState)
            {
                throw new ArgumentException(
                    "A relative file-link recovery target requires the exact expected observation.",
                    nameof(target));
            }

            return Create(
                ordinal,
                CanonicalRelativePath.Create(
                    RecoveryBundleInput.GetRelativeTarget(linkEffect)),
                LinkKind(linkEffect.Kind),
                expectedState,
                FromLinkState(linkEffect.Intended),
                priorPayload: null);
        }

        var change = target.Change;
        var before = target.Before;

        var path = CanonicalRelativePath.Create(input.GetRelativeTarget(change));
        var kind = OrdinaryKind(change.Kind);
        var prior = change.Kind == PlannedFileChangeKind.Create
            ? RecoveryEntryState.Missing
            : RecoveryEntryState.Ordinary(RecoveryContentIdentity.Create(
                before.Bytes.Length,
                before.ContentHash
                    ?? throw new ArgumentException(
                        "An ordinary recovery target requires a prior content hash.",
                        nameof(target))));
        var intended = change.Kind == PlannedFileChangeKind.Delete
            ? RecoveryEntryState.Missing
            : RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes(
                change.IntendedBytes.AsSpan()));
        return Create(
            ordinal,
            path,
            kind,
            prior,
            intended,
            priorPayload: prior.Kind == RecoveryEntryStateKind.OrdinaryFile
                ? RecoveryBundleFormatV1.PayloadName(ordinal)
                : null);
    }

    internal static RecoveryEntry Create(
        int ordinal,
        string targetPath,
        PlannedFileChangeKind changeKind,
        RecoveryContentIdentity prior,
        RecoveryContentIdentity? intended)
    {
        ArgumentNullException.ThrowIfNull(prior);
        var kind = changeKind switch
        {
            PlannedFileChangeKind.Replace => RecoveryEntryKind.OrdinaryReplace,
            PlannedFileChangeKind.Delete => RecoveryEntryKind.OrdinaryDelete,
            PlannedFileChangeKind.ReplaceGeneratedRegion =>
                RecoveryEntryKind.OrdinaryReplaceGeneratedRegion,
            PlannedFileChangeKind.Create => throw new ArgumentOutOfRangeException(
                nameof(changeKind),
                changeKind,
                "An ordinary create recovery entry requires a missing prior state."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(changeKind),
                changeKind,
                "The planned file change kind is not defined."),
        };
        RecoveryEntryState intendedState;
        if (kind == RecoveryEntryKind.OrdinaryDelete)
        {
            if (intended is not null)
            {
                throw new ArgumentException(
                    "An ordinary deletion recovery entry cannot carry intended file identity.",
                    nameof(intended));
            }

            intendedState = RecoveryEntryState.Missing;
        }
        else
        {
            if (intended is null)
            {
                throw new ArgumentException(
                    "A non-deletion recovery entry requires intended file identity.",
                    nameof(intended));
            }

            intendedState = RecoveryEntryState.Ordinary(intended);
        }

        return Create(
            ordinal,
            CanonicalRelativePath.Create(ValidateRelativeTarget(targetPath)),
            kind,
            RecoveryEntryState.Ordinary(prior),
            intendedState,
            RecoveryBundleFormatV1.PayloadName(ordinal));
    }

    internal bool Matches(
        RecoveryBundleInput input,
        PlannedFileChange change)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(change);
        return Matches(input.GetRelativeTarget(change), change);
    }

    internal bool Matches(
        string targetPath,
        PlannedFileChange change)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        ArgumentNullException.ThrowIfNull(change);
        if (!string.Equals(TargetPath, targetPath, StringComparison.Ordinal)
            || !MatchesOrdinaryKind(Kind, change.Kind))
        {
            return false;
        }

        if (change.Kind == PlannedFileChangeKind.Create)
        {
            return Prior.Kind == RecoveryEntryStateKind.Missing
                && Intended.OrdinaryFile?.Matches(change.IntendedBytes.AsSpan()) == true;
        }

        return Prior.OrdinaryFile is { } prior
            && string.Equals(
                prior.Sha256,
                change.Expectation.ContentHash,
                StringComparison.Ordinal)
            && (change.Kind == PlannedFileChangeKind.Delete
                ? Intended.Kind == RecoveryEntryStateKind.Missing
                : Intended.OrdinaryFile?.Matches(change.IntendedBytes.AsSpan()) == true);
    }

    internal bool Matches(
        RecoveryBundleInput input,
        RelativeFileLinkEffect effect)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(effect);
        return Matches(RecoveryBundleInput.GetRelativeTarget(effect), effect);
    }

    internal bool Matches(
        string targetPath,
        RelativeFileLinkEffect effect)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        ArgumentNullException.ThrowIfNull(effect);
        return string.Equals(TargetPath, targetPath, StringComparison.Ordinal)
            && Kind == LinkKind(effect.Kind)
            && FromLinkState(effect.Expected) == Prior
            && FromLinkState(effect.Intended) == Intended;
    }

    private static bool IsAdmissibleStatePair(
        RecoveryEntryKind kind,
        RecoveryEntryStateKind prior,
        RecoveryEntryStateKind intended)
        => (kind, prior, intended) switch
        {
            (RecoveryEntryKind.OrdinaryCreate,
                RecoveryEntryStateKind.Missing,
                RecoveryEntryStateKind.OrdinaryFile) => true,
            (RecoveryEntryKind.OrdinaryReplace,
                RecoveryEntryStateKind.OrdinaryFile,
                RecoveryEntryStateKind.OrdinaryFile) => true,
            (RecoveryEntryKind.OrdinaryReplaceGeneratedRegion,
                RecoveryEntryStateKind.OrdinaryFile,
                RecoveryEntryStateKind.OrdinaryFile) => true,
            (RecoveryEntryKind.OrdinaryDelete,
                RecoveryEntryStateKind.OrdinaryFile,
                RecoveryEntryStateKind.Missing) => true,
            (RecoveryEntryKind.RelativeFileLinkCreate,
                RecoveryEntryStateKind.Missing,
                RecoveryEntryStateKind.RelativeFileLink) => true,
            (RecoveryEntryKind.RelativeFileLinkDelete,
                RecoveryEntryStateKind.RelativeFileLink,
                RecoveryEntryStateKind.Missing) => true,
            _ => false,
        };

    private static RecoveryEntryKind OrdinaryKind(PlannedFileChangeKind kind)
        => kind switch
        {
            PlannedFileChangeKind.Create => RecoveryEntryKind.OrdinaryCreate,
            PlannedFileChangeKind.Replace => RecoveryEntryKind.OrdinaryReplace,
            PlannedFileChangeKind.ReplaceGeneratedRegion =>
                RecoveryEntryKind.OrdinaryReplaceGeneratedRegion,
            PlannedFileChangeKind.Delete => RecoveryEntryKind.OrdinaryDelete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The planned file change kind is not defined."),
        };

    private static bool MatchesOrdinaryKind(
        RecoveryEntryKind kind,
        PlannedFileChangeKind changeKind)
        => (kind, changeKind) switch
        {
            (RecoveryEntryKind.OrdinaryCreate, PlannedFileChangeKind.Create) => true,
            (RecoveryEntryKind.OrdinaryReplace, PlannedFileChangeKind.Replace) => true,
            (RecoveryEntryKind.OrdinaryReplaceGeneratedRegion,
                PlannedFileChangeKind.ReplaceGeneratedRegion) => true,
            (RecoveryEntryKind.OrdinaryDelete, PlannedFileChangeKind.Delete) => true,
            _ => false,
        };

    private static RecoveryEntryKind LinkKind(RelativeFileLinkEffectKind kind)
        => kind switch
        {
            RelativeFileLinkEffectKind.Create => RecoveryEntryKind.RelativeFileLinkCreate,
            RelativeFileLinkEffectKind.Delete => RecoveryEntryKind.RelativeFileLinkDelete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The relative file-link effect kind is not defined."),
        };

    private static RecoveryEntryState FromLinkState(RelativeFileLinkState state)
        => state.State switch
        {
            NoFollowLeafState.Missing => RecoveryEntryState.Missing,
            NoFollowLeafState.RelativeFileLink when state.Link is { } link =>
                RecoveryEntryState.RelativeLink(link),
            _ => throw new ArgumentException(
                "A recovery entry accepts only Missing or RelativeFileLink states.",
                nameof(state)),
        };

    private static string ValidateRelativeTarget(string targetPath)
        => CanonicalRelativePath.Create(
            targetPath
                .Replace('\\', '/')
                .Replace(Path.DirectorySeparatorChar, '/')).Value;

    private static void ValidatePayload(string? priorPayload, int ordinal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(priorPayload);
        if (!priorPayload.StartsWith(
                $"{RecoveryBundleFormatV1.PayloadDirectoryName}/",
                StringComparison.Ordinal)
            || priorPayload.EndsWith('/')
            || !string.Equals(
                priorPayload,
                RecoveryBundleFormatV1.PayloadName(ordinal),
                StringComparison.Ordinal)
            || priorPayload.Contains('\\'))
        {
            throw new ArgumentException(
                "A recovery payload name must be a canonical payload-relative name.",
                nameof(priorPayload));
        }
    }
}
