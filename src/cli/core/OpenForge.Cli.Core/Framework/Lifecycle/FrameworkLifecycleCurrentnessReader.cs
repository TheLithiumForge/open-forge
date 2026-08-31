using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed class FrameworkLifecycleCurrentnessReader
{
    private const string GeneratedRegionIdentity = "entries";
    private readonly FrameworkContentIdentity _contentIdentity = new();
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal FrameworkLifecycleCurrentnessReader(
        PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal ValueTask<FrameworkLifecycleCurrentness> ReadAsync(
        CliWorkspace workspace,
        FrameworkLifecycleState lifecycle,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(lifecycle);
        ArgumentNullException.ThrowIfNull(payload);
        return ReadCoreAsync(workspace, lifecycle, payload, cancellationToken);
    }

    private async ValueTask<FrameworkLifecycleCurrentness> ReadCoreAsync(
        CliWorkspace workspace,
        FrameworkLifecycleState lifecycle,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }

        if (!string.Equals(
                lifecycle.Source.InventoryFingerprint,
                payload.InventoryFingerprint,
                StringComparison.Ordinal))
        {
            return SourceMismatch(
                "The lifecycle Framework inventory differs from the running embedded inventory.");
        }

        var generatedTargets = lifecycle.GeneratedRegions
            .Select(region => (region.Path, Region: (string?)region.Region))
            .ToHashSet();
        foreach (var target in lifecycle.Targets)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Cancelled();
            }

            var ownershipBoundary = ValidateOwnership(target, generatedTargets, payload);
            if (ownershipBoundary is not null)
            {
                return ownershipBoundary;
            }

            var lexicalPath = Path.Combine(
                workspace.LexicalRoot,
                target.Path.Replace('/', Path.DirectorySeparatorChar));
            var resolution = _physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalPath);
            var resolutionBoundary = ReadResolutionBoundary(target.Path, resolution);
            if (resolutionBoundary is not null)
            {
                return resolutionBoundary;
            }

            byte[] bytes;
            try
            {
                bytes = await File.ReadAllBytesAsync(
                        resolution.GetContainedPhysicalPath(),
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Cancelled();
            }
            catch (Exception exception) when (exception is FileNotFoundException
                or DirectoryNotFoundException)
            {
                return Missing(target.Path, "The lifecycle Framework target is missing.");
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException
                or IOException)
            {
                return Unavailable(
                    target.Path,
                    $"The lifecycle Framework target cannot be read: {exception.Message}");
            }

            string fingerprint;
            try
            {
                fingerprint = ReadFingerprint(target, generatedTargets, bytes);
            }
            catch (Exception exception) when (exception is ArgumentException
                or DecoderFallbackException
                or InvalidDataException
                or InvalidOperationException)
            {
                return Blocked(
                    target.Path,
                    $"The lifecycle Framework target is malformed or unsafe: {exception.Message}");
            }

            if (!string.Equals(
                    fingerprint,
                    target.BaselineFingerprint,
                    StringComparison.Ordinal))
            {
                return Changed(
                    target.Path,
                    "The lifecycle Framework target differs from its persisted baseline.");
            }
        }

        return Current();
    }

    private FrameworkLifecycleCurrentness? ValidateOwnership(
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generatedTargets,
        FrameworkPayload payload)
    {
        var generated = generatedTargets.Contains((target.Path, target.Region));
        if (generated)
        {
            return target.SourceAssetPath is null
                    && string.Equals(target.Region, GeneratedRegionIdentity, StringComparison.Ordinal)
                    && string.Equals(
                        target.FingerprintKind,
                        LifecycleSchema.ExactBytesFingerprintKind,
                        StringComparison.Ordinal)
                ? null
                : Blocked(
                    target.Path,
                    "A generated lifecycle Framework target has unsupported provenance or fingerprint identity.");
        }

        if (target.SourceAssetPath is not { } sourceAssetPath
            || payload.Find(sourceAssetPath) is not { } asset)
        {
            return SourceMismatch(
                "A lifecycle Framework target does not belong to the running embedded inventory.");
        }

        string sourceFingerprint;
        try
        {
            sourceFingerprint = _contentIdentity.ReadSourceFingerprint(
                asset.Bytes.AsSpan(),
                target.FingerprintKind);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            return SourceMismatch(
                $"A lifecycle Framework source asset cannot establish its running identity: {exception.Message}");
        }

        return string.Equals(
                sourceFingerprint,
                target.BaselineFingerprint,
                StringComparison.Ordinal)
            ? null
            : SourceMismatch(
                "A lifecycle Framework target baseline does not match its running embedded source asset.");
    }

    private string ReadFingerprint(
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generatedTargets,
        ReadOnlySpan<byte> bytes)
    {
        if (generatedTargets.Contains((target.Path, target.Region)))
        {
            return _contentIdentity.ReadGeneratedEntriesFingerprint(
                bytes,
                target.FingerprintKind);
        }

        if (target.Path is FrameworkPayloadAsset.RootAgentPath
            or FrameworkPayloadAsset.RootClaudePath)
        {
            var recognition = _contentIdentity.ReadManagedBlock(bytes);
            if (recognition.State != FrameworkManagedBlockState.Present
                || recognition.ExistingBlockBytes is null)
            {
                throw new InvalidDataException(
                    recognition.Cause
                        ?? "The managed host requires one complete ordered Open Forge marker boundary.");
            }

            return _contentIdentity.ReadSourceFingerprint(
                recognition.ExistingBlockBytes,
                target.FingerprintKind);
        }

        if (target.Region is not null)
        {
            throw new InvalidDataException(
                "Only entries regions and root managed blocks are supported Framework target regions.");
        }

        return _contentIdentity.ReadSourceFingerprint(bytes, target.FingerprintKind);
    }

    private static FrameworkLifecycleCurrentness? ReadResolutionBoundary(
        string path,
        PhysicalPathResolution resolution)
    {
        return resolution.State switch
        {
            PhysicalPathState.Contained => null,
            PhysicalPathState.Missing => Missing(
                path,
                "The lifecycle Framework target is missing."),
            PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure => Unavailable(
                path,
                resolution.Failure?.DirectCause
                    ?? "The lifecycle Framework target is unavailable."),
            PhysicalPathState.Dangling
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported => Blocked(
                    path,
                    resolution.Failure?.DirectCause
                        ?? "The lifecycle Framework target is not a safe contained path."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The physical path state is not defined."),
        };
    }

    private static FrameworkLifecycleCurrentness Current()
        => new(FrameworkLifecycleCurrentnessState.Current, path: null, cause: null);

    private static FrameworkLifecycleCurrentness SourceMismatch(string cause)
        => new(FrameworkLifecycleCurrentnessState.SourceMismatch, path: null, cause);

    private static FrameworkLifecycleCurrentness Changed(string path, string cause)
        => new(FrameworkLifecycleCurrentnessState.Changed, path, cause);

    private static FrameworkLifecycleCurrentness Missing(string path, string cause)
        => new(FrameworkLifecycleCurrentnessState.Missing, path, cause);

    private static FrameworkLifecycleCurrentness Unavailable(string path, string cause)
        => new(FrameworkLifecycleCurrentnessState.Unavailable, path, cause);

    private static FrameworkLifecycleCurrentness Blocked(string path, string cause)
        => new(FrameworkLifecycleCurrentnessState.Blocked, path, cause);

    private static FrameworkLifecycleCurrentness Cancelled()
        => new(FrameworkLifecycleCurrentnessState.Cancelled, path: null, cause: null);
}
