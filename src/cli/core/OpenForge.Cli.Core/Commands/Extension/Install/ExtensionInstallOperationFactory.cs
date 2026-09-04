using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Install;

internal static class ExtensionInstallOperationFactory
{
    internal static ExtensionInstallOperation Create(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        ArgumentNullException.ThrowIfNull(interactiveSession);
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var lifecycleStore = new LifecycleStore(physicalPathResolver);
        var recoveryReader = new RecoveryBundleReader();
        var recoveryCatalogue = new RecoveryBundleCatalogue(recoveryReader);
        var revalidator = new MutationRevalidator(validator);
        var foundationReader = new ExtensionInstallFoundationReader(
            lifecycleStore,
            new FrameworkLifecycleCurrentnessReader(physicalPathResolver),
            recoveryCatalogue);
        var verifier = new ExtensionInstallAppliedVerifier(
            physicalPathResolver,
            validator,
            lifecycleStore);
        return new ExtensionInstallOperation(
            new ExtensionInstallPlanner(
                interactiveSession,
                physicalPathResolver,
                lifecycleStore,
                recoveryCatalogue),
            new MutationPreflight(validator),
            new ExtensionInstallApplicationOperation(
                lockStoreRoot is null
                    ? WorkspaceLockManager.CreateForCurrentUser()
                    : new WorkspaceLockManager(lockStoreRoot),
                new ExtensionInstallApplicationPreconditionValidator(
                    new ExtensionInstallSourceResolver(physicalPathResolver),
                    foundationReader,
                    revalidator),
                new ExtensionInstallRecoveryOperation(
                    new RecoveryBundleStore(recoveryReader),
                    recoveryCatalogue,
                    new RecoveryBundleDeletionGuard(recoveryCatalogue, recoveryReader)),
                new ExtensionInstallEffectApplication(
                    new DirectoryCreationApplier(revalidator, validator),
                    new FileChangeApplier(revalidator, validator),
                    verifier)));
    }
}

internal sealed class ExtensionInstallOperation(
    ExtensionInstallPlanner planner,
    MutationPreflight preflight,
    ExtensionInstallApplicationOperation application)
{
    private readonly ExtensionInstallPlanner _planner = planner;
    private readonly MutationPreflight _preflight = preflight;
    private readonly ExtensionInstallApplicationOperation _application = application;

    internal async ValueTask<ExtensionInstallResult> ExecuteAsync(
        ExtensionInstallRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.All && request.RequestedIds.Count > 0)
        {
            return ExtensionInstallResult.Empty(
                request.Workspace,
                request.Mode,
                request.Force,
                request.Automatic,
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.InvalidInput,
                    "Explicit Extension IDs and --all cannot be combined."));
        }

        ExtensionInstallPlanBuild build;
        try
        {
            build = await _planner.BuildAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionInstallResult.Empty(
                request.Workspace,
                request.Mode,
                request.Force,
                request.Automatic,
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.Interrupted,
                    "Extension Install planning was interrupted."));
        }
        catch (Exception)
        {
            return ExtensionInstallResult.Empty(
                request.Workspace,
                request.Mode,
                request.Force,
                request.Automatic,
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.OperationFailed,
                    "Extension Install planning failed unexpectedly."));
        }

        if (build.Plan is not { } plan)
        {
            return build.Result;
        }

        if (plan.IsNoOp)
        {
            return ExtensionInstallResultFactory.NoOp(plan);
        }

        MutationValidationResult preflight;
        try
        {
            preflight = await _preflight.ValidateAsync(
                request.Workspace,
                plan.DirectoryCreations,
                plan.AllFileChanges,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionInstallResultFactory.PlanBoundary(
                plan,
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.Interrupted,
                    "Extension Install preflight was interrupted."));
        }
        catch (Exception)
        {
            return ExtensionInstallResultFactory.PlanBoundary(
                plan,
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.OperationFailed,
                    "Extension Install preflight failed unexpectedly."));
        }

        if (preflight.State != MutationValidationState.Valid)
        {
            return ExtensionInstallResultFactory.PlanBoundary(
                plan,
                new ExtensionInstallFinding(
                    preflight.State == MutationValidationState.Cancelled
                        ? ExtensionInstallFindingCode.Interrupted
                        : ExtensionInstallFindingCode.TargetUnsafe,
                    preflight.Cause
                        ?? "The complete Extension Install plan is stale, unavailable, or unsafe."));
        }

        if (request.Mode == ExtensionInstallMode.DryRun)
        {
            return build.Result;
        }

        var outcome = await _application.ExecuteAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        return ExtensionInstallResultFactory.Application(
            plan,
            outcome.Progress,
            outcome.Finding);
    }
}
