using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Recovery.Models;

internal sealed record RecoveryBundleEntry
{
    private RecoveryBundleEntry(
        int ordinal,
        string targetPath,
        PlannedFileChangeKind changeKind,
        RecoveryContentIdentity prior,
        RecoveryContentIdentity? intended)
    {
        Ordinal = ordinal;
        TargetPath = targetPath;
        ChangeKind = changeKind;
        Prior = prior;
        PayloadName = RecoveryBundleFormatV1.PayloadName(ordinal);
        Intended = intended;
    }

    internal int Ordinal { get; }

    internal string TargetPath { get; }

    internal PlannedFileChangeKind ChangeKind { get; }

    internal RecoveryContentIdentity Prior { get; }

    internal string PayloadName { get; }

    internal bool IntendedAbsent => ChangeKind == PlannedFileChangeKind.Delete;

    internal RecoveryContentIdentity? Intended { get; }

    internal static RecoveryBundleEntry FromTarget(
        RecoveryBundleInput input,
        RecoveryBundleTarget target,
        int ordinal)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(target);
        if (!target.RequiresRecovery || target.Before.ContentHash is not { } priorHash)
        {
            throw new ArgumentException(
                "A recovery entry requires an existing ordinary-file target.",
                nameof(target));
        }

        var intended = target.Change.Kind == PlannedFileChangeKind.Delete
            ? null
            : RecoveryContentIdentity.FromBytes(target.Change.IntendedBytes.AsSpan());
        return Create(
            ordinal: ordinal,
            targetPath: input.GetRelativeTarget(target.Change),
            changeKind: target.Change.Kind,
            prior: RecoveryContentIdentity.Create(target.Before.Bytes.Length, priorHash),
            intended: intended);
    }

    internal static RecoveryBundleEntry Create(
        int ordinal,
        string targetPath,
        PlannedFileChangeKind changeKind,
        RecoveryContentIdentity prior,
        RecoveryContentIdentity? intended)
    {
        if (ordinal < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal));
        }

        ArgumentNullException.ThrowIfNull(prior);
        var normalizedTarget = ValidateRelativeTarget(targetPath);
        if (changeKind is not (PlannedFileChangeKind.Replace
            or PlannedFileChangeKind.Delete
            or PlannedFileChangeKind.ReplaceGeneratedRegion))
        {
            throw new ArgumentOutOfRangeException(
                nameof(changeKind),
                changeKind,
                "A recovery entry requires an existing-target change kind.");
        }

        if ((changeKind == PlannedFileChangeKind.Delete) != (intended is null))
        {
            throw new ArgumentException(
                "Only a deletion recovery entry omits intended file identity.",
                nameof(intended));
        }

        return new RecoveryBundleEntry(
            ordinal: ordinal,
            targetPath: normalizedTarget,
            changeKind: changeKind,
            prior: prior,
            intended: intended);
    }

    internal bool Matches(
        RecoveryBundleInput input,
        PlannedFileChange change)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(change);
        if (!string.Equals(TargetPath, input.GetRelativeTarget(change), StringComparison.Ordinal)
            || ChangeKind != change.Kind
            || !string.Equals(Prior.Sha256, change.Expectation.ContentHash, StringComparison.Ordinal))
        {
            return false;
        }

        if (change.Kind == PlannedFileChangeKind.Delete)
        {
            return Intended is null;
        }

        return Intended?.Matches(change.IntendedBytes.AsSpan()) == true;
    }

    internal static string ValidateRelativeTarget(string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        if (Path.IsPathFullyQualified(targetPath)
            || targetPath.Contains('\\')
            || targetPath.StartsWith("/", StringComparison.Ordinal)
            || targetPath.EndsWith("/", StringComparison.Ordinal)
            || targetPath.Split('/').Any(component => component is "" or "." or ".."))
        {
            throw new ArgumentException(
                "A recovery target path must be a canonical contained relative path.",
                nameof(targetPath));
        }

        return targetPath;
    }
}
