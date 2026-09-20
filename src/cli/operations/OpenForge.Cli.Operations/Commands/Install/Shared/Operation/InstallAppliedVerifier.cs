using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Operation;

internal sealed class InstallAppliedVerifier(
    PhysicalPathResolver physicalPathResolver)
{
    private readonly InstallTargetReader _targetReader = new(physicalPathResolver);
    private readonly InstallContentIdentity _contentIdentity = new();

    internal ValueTask<InstallVerificationResult> VerifyAsync(
        InstallPlan plan,
        CancellationToken cancellationToken)
        => VerifyTargetsAsync(plan, cancellationToken);

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
                targetRead.Reads,
                plan.IntendedState)
                ? new InstallVerificationResult(
                    InstallVerificationState.Verified,
                    Cause: null)
                : Failed(
                    "The applied Framework targets do not match the intended payload and generated projection.");
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
