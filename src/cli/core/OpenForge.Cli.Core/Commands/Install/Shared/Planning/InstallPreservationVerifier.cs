using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallPreservationVerifier(
    PhysicalPathResolver physicalPathResolver)
{
    private readonly InstallTargetReader _targetReader = new(physicalPathResolver);
    private readonly InstallContentIdentity _contentIdentity = new();

    internal async ValueTask<InstallPreservationVerification> VerifyAsync(
        FrameworkLifecycleState current,
        InstallIntendedState intended,
        OpenForge.Cli.Core.Framework.Workspace.CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var baseKeys = intended.TargetBytes.Keys
            .Select(path => (Path: path, Region: (string?)null))
            .Concat(intended.GeneratedRegionPaths.Select(path => (
                Path: path,
                Region: (string?)InstallContentIdentity.GeneratedRegionIdentity)))
            .Concat(intended.ManagedBlockBytes.Keys.Select(path => (
                Path: path,
                Region: (string?)null)))
            .ToHashSet();
        var preserved = current.Targets
            .Where(target => !baseKeys.Contains((target.Path, target.Region)))
            .OrderBy(target => target.Path, StringComparer.Ordinal)
            .ThenBy(target => target.Region, StringComparer.Ordinal)
            .ToArray();
        if (preserved.Length == 0)
        {
            return Verified();
        }

        var generatedKeys = current.GeneratedRegions
            .Select(region => (region.Path, Region: (string?)region.Region))
            .ToHashSet();
        foreach (var pathGroup in preserved.GroupBy(
                     target => target.Path,
                     StringComparer.Ordinal))
        {
            var read = await _targetReader.ReadAsync(
                    workspace,
                    pathGroup.Key,
                    cancellationToken)
                .ConfigureAwait(false);
            var boundary = ReadBoundary(read);
            if (boundary is not null)
            {
                return boundary;
            }

            var snapshot = read.Snapshot
                ?? throw new InvalidOperationException(
                    "A verified scoped Framework target requires exact current bytes.");
            foreach (var target in pathGroup)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Cancelled(target.Path);
                }

                string fingerprint;
                try
                {
                    fingerprint = ReadFingerprint(
                        target,
                        generatedKeys,
                        snapshot.Bytes.AsSpan());
                }
                catch (Exception exception) when (exception is ArgumentException
                    or InvalidDataException
                    or InvalidOperationException)
                {
                    return Blocked(
                        target.Path,
                        $"A preserved scoped Framework target is malformed or unsafe: {exception.Message}");
                }

                if (!string.Equals(
                        fingerprint,
                        target.BaselineFingerprint,
                        StringComparison.Ordinal))
                {
                    return Changed(
                        target.Path,
                        "A preserved scoped Framework target differs from its persisted baseline.");
                }
            }
        }

        return Verified();
    }

    private string ReadFingerprint(
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generatedKeys,
        ReadOnlySpan<byte> bytes)
    {
        var generated = generatedKeys.Contains((target.Path, target.Region));
        if (generated)
        {
            if (target.SourceAssetPath is not null
                || !string.Equals(
                    target.Region,
                    InstallContentIdentity.GeneratedRegionIdentity,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Only a provenance-free entries relationship is a supported derived region.");
            }

            return _contentIdentity.ReadPersistedGeneratedFingerprint(
                bytes,
                target.FingerprintKind);
        }

        if (target.SourceAssetPath is null)
        {
            throw new InvalidDataException(
                "A preserved source-backed target requires persisted source provenance.");
        }

        if (target.Region is null)
        {
            return _contentIdentity.ReadPersistedSourceFingerprint(
                bytes,
                target.FingerprintKind);
        }

        if (string.Equals(
                target.Region,
                InstallContentIdentity.GeneratedRegionIdentity,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "The entries identity requires one matching generated-region relationship.");
        }

        return _contentIdentity.ReadPersistedManagedBlockFingerprint(
            bytes,
            target.FingerprintKind);
    }

    private static InstallPreservationVerification? ReadBoundary(
        InstallTargetRead read)
    {
        return read.State switch
        {
            InstallTargetReadState.File => null,
            InstallTargetReadState.Missing => Changed(
                read.RelativePath,
                "A preserved scoped Framework target is missing."),
            InstallTargetReadState.Unavailable => Incomplete(
                read.RelativePath,
                read.Cause
                    ?? "A preserved scoped Framework target is unavailable."),
            InstallTargetReadState.Blocked => Blocked(
                read.RelativePath,
                read.Cause
                    ?? "A preserved scoped Framework target is unsafe."),
            InstallTargetReadState.Cancelled => Cancelled(read.RelativePath),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "The scoped Framework target read state is not defined."),
        };
    }

    private static InstallPreservationVerification Verified()
        => new()
        {
            State = InstallPreservationVerificationState.Verified,
            Subject = null,
            Cause = null,
        };

    private static InstallPreservationVerification Changed(
        string subject,
        string cause)
        => new()
        {
            State = InstallPreservationVerificationState.Changed,
            Subject = subject,
            Cause = cause,
        };

    private static InstallPreservationVerification Incomplete(
        string subject,
        string cause)
        => new()
        {
            State = InstallPreservationVerificationState.Incomplete,
            Subject = subject,
            Cause = cause,
        };

    private static InstallPreservationVerification Blocked(
        string subject,
        string cause)
        => new()
        {
            State = InstallPreservationVerificationState.Blocked,
            Subject = subject,
            Cause = cause,
        };

    private static InstallPreservationVerification Cancelled(string subject)
        => new()
        {
            State = InstallPreservationVerificationState.Cancelled,
            Subject = subject,
            Cause = "Scoped Framework preservation verification was interrupted.",
        };
}
