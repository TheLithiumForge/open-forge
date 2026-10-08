using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Operation;

internal sealed class InstallAppliedVerifier(
    PhysicalPathResolver physicalPathResolver)
{
    private readonly InstallTargetReader _targetReader = new(physicalPathResolver);
    private readonly InstallContentIdentity _contentIdentity = new();

    internal async ValueTask<InstallVerificationResult> VerifyAsync(
        InstallPlan plan,
        CancellationToken cancellationToken)
    {
        var result = await VerifyTargetsAsync(plan, cancellationToken).ConfigureAwait(false);
        if (result.State != InstallVerificationState.Verified) return result;
        foreach (var effect in plan.TargetEffects.Where(effect => effect.RelativePath == ".gitignore"))
        {
            var read = await _targetReader.ReadAsync(plan.Request.Workspace, effect.RelativePath, cancellationToken).ConfigureAwait(false);
            if (read.State != InstallTargetReadState.File || read.Snapshot is not { } snapshot
                || !snapshot.Bytes.AsSpan().SequenceEqual(effect.Change.IntendedBytes.AsSpan()))
                return Failed("The Git-ignore effect did not verify.");
        }
        return await VerifyRegistrationAsync(plan, cancellationToken).ConfigureAwait(false);
    }

    internal async ValueTask<InstallVerificationResult> VerifyRegistrationAsync(InstallPlan plan, CancellationToken cancellationToken)
    {
        if (plan.Request.Configuration is null || plan.IntendedState.Configuration is not { } configuration)
            return new(InstallVerificationState.Verified, Cause: null);
        var ownership = await WorkspaceOwnershipReader.ReadAsync(physicalPathResolver, plan.Request.Workspace, cancellationToken).ConfigureAwait(false);
        if (ownership.State != WorkspaceOwnershipReadState.Complete || ownership.Document.Framework is not { } framework
            || !framework.GitIgnoredRoutes.ToHashSet().SetEquals(configuration.GitIgnoredRoutes))
            return Failed("The configured route-sharing registration did not verify.");
        return new(InstallVerificationState.Verified, Cause: null);
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

        foreach (var effect in plan.TargetEffects.Where(effect => effect.RelativePath == ".agents/open-forge.json"))
        {
            var read = await _targetReader.ReadAsync(plan.Request.Workspace, effect.RelativePath, cancellationToken).ConfigureAwait(false);
            if (read.State != InstallTargetReadState.File || read.Snapshot is not { } snapshot
                || !snapshot.Bytes.AsSpan().SequenceEqual(effect.Change.IntendedBytes.AsSpan()))
                return Failed("A configuration settings or Git-ignore effect did not verify.");
        }

        foreach (var replacement in plan.IntendedState.Configuration?.FrontmatterPlan?.Replacements ?? [])
        {
            var read = await _targetReader.ReadAsync(plan.Request.Workspace, replacement.Path, cancellationToken).ConfigureAwait(false);
            if (read.State != InstallTargetReadState.File || read.Snapshot is not { } snapshot
                || !snapshot.Bytes.AsSpan().SequenceEqual(replacement.IntendedBytes))
                return Failed($"Converted file '{replacement.Path}' did not verify.");
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
