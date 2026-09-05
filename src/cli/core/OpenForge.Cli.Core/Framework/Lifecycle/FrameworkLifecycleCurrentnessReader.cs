using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed class FrameworkLifecycleCurrentnessReader
{
    private readonly LifecycleManagedTargetReader _managedTargetReader;
    private readonly FrameworkLifecycleTargetIdentity _targetIdentity = new();

    internal FrameworkLifecycleCurrentnessReader(
        PhysicalPathResolver physicalPathResolver)
    {
        _managedTargetReader = new LifecycleManagedTargetReader(physicalPathResolver);
    }

    internal async ValueTask<FrameworkLifecycleCurrentness> ReadAsync(
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

            var ownershipBoundary = ReadSourceBoundary(target, generatedTargets, payload);
            if (ownershipBoundary is not null)
            {
                return ownershipBoundary;
            }

            var read = await _managedTargetReader
                .ReadAsync(workspace, target.Path, cancellationToken)
                .ConfigureAwait(false);
            var readBoundary = ReadTargetBoundary(target.Path, read);
            if (readBoundary is not null)
            {
                return readBoundary;
            }

            string fingerprint;
            try
            {
                fingerprint = _targetIdentity.ReadFingerprint(
                    target,
                    generatedTargets,
                    read.Bytes.Span);
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

    private FrameworkLifecycleCurrentness? ReadSourceBoundary(
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generatedTargets,
        FrameworkPayload payload)
    {
        var validation = _targetIdentity.ValidateSource(target, generatedTargets, payload);
        return validation.State switch
        {
            FrameworkLifecycleTargetSourceState.Valid => null,
            FrameworkLifecycleTargetSourceState.SourceMismatch => SourceMismatch(
                validation.Cause
                    ?? "The lifecycle Framework source identity is unavailable."),
            FrameworkLifecycleTargetSourceState.Blocked => Blocked(
                target.Path,
                validation.Cause
                    ?? "The lifecycle Framework target identity is blocked."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(validation),
                validation.State,
                "The Framework lifecycle target source state is not defined."),
        };
    }

    private static FrameworkLifecycleCurrentness? ReadTargetBoundary(
        string path,
        LifecycleManagedTargetReadResult read)
        => read.State switch
        {
            LifecycleManagedTargetReadState.Available => null,
            LifecycleManagedTargetReadState.Missing => Missing(
                path,
                "The lifecycle Framework target is missing."),
            LifecycleManagedTargetReadState.Unavailable => Unavailable(
                path,
                read.Failure?.DirectCause
                    ?? "The lifecycle Framework target is unavailable."),
            LifecycleManagedTargetReadState.Blocked => Blocked(
                path,
                read.Failure?.DirectCause
                        ?? "The lifecycle Framework target is not a safe contained path."),
            LifecycleManagedTargetReadState.Cancelled => Cancelled(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "The lifecycle target read state is not defined."),
        };

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
