using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Operation;

internal sealed class InstallAppliedVerifier(
    PhysicalPathResolver physicalPathResolver,
    LifecycleStore lifecycleStore)
{
    private readonly InstallTargetReader _targetReader = new(physicalPathResolver);
    private readonly LifecycleStore _lifecycleStore = lifecycleStore;
    private readonly InstallContentIdentity _contentIdentity = new();

    internal async ValueTask<InstallVerificationResult> VerifyAsync(
        InstallPlan plan,
        CancellationToken cancellationToken)
    {
        var targetRead = await ReadTargetsAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        if (targetRead.Result is { } targetBoundary)
        {
            return targetBoundary;
        }

        var lifecycle = await _lifecycleStore.ReadAsync(
                plan.Request.Workspace,
                LifecycleSection.Framework,
                cancellationToken)
            .ConfigureAwait(false);
        if (lifecycle.State == LifecycleStoreReadState.Cancelled)
        {
            return Cancelled();
        }

        if (lifecycle.State != LifecycleStoreReadState.Available
            || lifecycle.Framework is not { } framework)
        {
            return Failed(
                lifecycle.Cause
                    ?? "The published Framework lifecycle state is unavailable or invalid.");
        }

        try
        {
            return _contentIdentity.IsCurrentBaseExact(
                framework,
                plan.IntendedLifecycle,
                targetRead.Reads,
                plan.IntendedState)
                ? new InstallVerificationResult(
                    InstallVerificationState.Verified,
                    Cause: null)
                : Failed(
                    "The applied Framework targets do not match the published lifecycle baseline.");
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            return Failed(
                $"The applied Framework result could not be verified: {exception.Message}");
        }
    }

    internal async ValueTask<InstallVerificationResult> VerifyTargetsAsync(
        InstallPlan plan,
        CancellationToken cancellationToken)
    {
        var targetRead = await ReadTargetsAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        if (targetRead.Result is { } targetBoundary)
        {
            return targetBoundary;
        }

        try
        {
            return _contentIdentity.IsCurrentBaseExact(
                plan.IntendedLifecycle,
                plan.IntendedLifecycle,
                targetRead.Reads,
                plan.IntendedState)
                ? new InstallVerificationResult(
                    InstallVerificationState.Verified,
                    Cause: null)
                : Failed(
                    "The applied Framework targets do not match their intended baseline.");
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            return Failed(
                $"The applied Framework targets could not be verified: {exception.Message}");
        }
    }

    private async ValueTask<InstallTargetVerificationRead> ReadTargetsAsync(
        InstallPlan plan,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new InstallTargetVerificationRead(
                new Dictionary<string, InstallTargetRead>(StringComparer.Ordinal),
                Cancelled());
        }

        var reads = new Dictionary<string, InstallTargetRead>(StringComparer.Ordinal);
        foreach (var path in plan.IntendedState.TargetBytes.Keys
                     .Concat(plan.IntendedState.ManagedBlockBytes.Keys)
                     .Distinct(StringComparer.Ordinal)
                     .Order(StringComparer.Ordinal))
        {
            var read = await _targetReader.ReadAsync(
                    plan.Request.Workspace,
                    path,
                    cancellationToken)
                .ConfigureAwait(false);
            if (read.State == InstallTargetReadState.Cancelled)
            {
                return new InstallTargetVerificationRead(reads, Cancelled());
            }

            if (read.State != InstallTargetReadState.File)
            {
                return new InstallTargetVerificationRead(
                    reads,
                    Failed(
                        read.Cause
                            ?? $"The verified Install target '{path}' is not an ordinary readable file."));
            }

            reads.Add(path, read);
        }

        return new InstallTargetVerificationRead(reads, Result: null);
    }

    private static InstallVerificationResult Failed(string cause)
        => new(InstallVerificationState.Failed, cause);

    private static InstallVerificationResult Cancelled()
        => new(InstallVerificationState.Cancelled, Cause: null);
}

internal sealed record InstallTargetVerificationRead(
    IReadOnlyDictionary<string, InstallTargetRead> Reads,
    InstallVerificationResult? Result);
