using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install;

internal static class ExtensionInstallOperationFactory
{
    internal static ExtensionInstallOperation Create(
        ExtensionInstallInteraction interaction,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator);
        var foundationReader = new ExtensionInstallFoundationReader(physicalPathResolver);
        var verifier = new ExtensionInstallAppliedVerifier(physicalPathResolver, validator);
        var permissions = new ExtensionPermissionOperation(interaction.Permission);
        return new ExtensionInstallOperation(
            new ExtensionInstallPlanner(
                interaction.SelectPackages,
                interaction.SelectionQuestion,
                interaction.Force,
                interaction.ForceQuestion,
                physicalPathResolver),
            new MutationPreflight(validator),
            permissions,
            new ExtensionInstallApplicationOperation(
                lockStoreRoot is null
                    ? WorkspaceLockManager.CreateForCurrentUser()
                    : new WorkspaceLockManager(lockStoreRoot),
                new ExtensionInstallApplicationPreconditionValidator(
                    new ExtensionInstallSourceResolver(physicalPathResolver),
                    foundationReader,
                    revalidator,
                    validator),
                permissions,
                new ExtensionInstallEffectApplication(
                    new DirectoryCreationApplier(revalidator, validator),
                    new FileChangeApplier(revalidator, validator),
                    verifier)),
            interaction);
    }
}

internal sealed class ExtensionInstallOperation(
    ExtensionInstallPlanner planner,
    MutationPreflight preflight,
    ExtensionPermissionOperation permissions,
    ExtensionInstallApplicationOperation application,
    ExtensionInstallInteraction interaction)
{
    private readonly ExtensionInstallPlanner _planner = planner;
    private readonly MutationPreflight _preflight = preflight;
    private readonly ExtensionPermissionOperation _permissions = permissions;
    private readonly ExtensionInstallApplicationOperation _application = application;
    private readonly ExtensionInstallInteraction _interaction = interaction
        ?? throw new ArgumentNullException(nameof(interaction));

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
                    "Extension install was cancelled. Nothing was changed."));
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
                    "Extension install was cancelled. Nothing was changed."));
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

        ExtensionPermissionStage permission;
        try
        {
            var required = plan.Packages.SelectMany(package => package.Payload
                .Where(file => file.TargetPath is { } path && !ExtensionDestinationPolicy.IsImplicit(path))
                .Select(file => file.TargetPath ?? throw new InvalidOperationException("A planned Extension target requires its path.")));
            var changedPaths = plan.Effects.Where(effect => effect.FileChange is not null)
                .Select(effect => effect.Result.Path).ToHashSet(StringComparer.Ordinal);
            var targets = required.Select(requirement => new ExtensionPermissionTarget(requirement,
                changedPaths.Contains(requirement) ? ExtensionPermissionEffect.Copy : ExtensionPermissionEffect.Preserve));
            permission = await _permissions.DetermineAsync(new(request.Workspace, [.. targets], plan.SourceRead.Identity,
                request.Mode == ExtensionInstallMode.Apply && !request.Automatic && request.AllowInteraction,
                request.AllowPath), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionInstallResultFactory.PlanBoundary(plan,
                new(ExtensionInstallFindingCode.Interrupted, "Extension install was cancelled. Nothing was changed."));
        }
        catch (Exception)
        {
            return ExtensionInstallResultFactory.PlanBoundary(plan,
                new(ExtensionInstallFindingCode.OperationFailed, "Extension permission determination failed unexpectedly."));
        }
        if (permission.Failure is { } failure)
        {
            var cause = failure == ExtensionPermissionFailure.Interrupted
                ? "Extension install was cancelled. Nothing was changed."
                : "The requested Extension destinations require shared permission in .agents/open-forge.json; use --allow-path <path> to record a grant.";
            return ExtensionInstallResultFactory.PlanBoundary(plan,
                new(ExtensionInstallDefinitions.ReadPermissionFinding(failure), cause), permission.Result);
        }
        var contentFindings = ReadContentFindings(plan);
        var forcePreview = new ExtensionInstallResult(
            PreviewRequest(request),
            plan.Facts with { Permissions = permission.Result },
            [.. build.Result.Findings, .. contentFindings]);
        ExtensionInstallFinding? forceBoundary;
        try
        {
            forceBoundary = await _planner.AuthorizeForceAsync(
                request,
                plan,
                forcePreview,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            forceBoundary = new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension install was cancelled. Nothing was changed.");
        }
        if (forceBoundary is not null)
        {
            return ExtensionInstallResultFactory.PlanBoundary(plan, forceBoundary, permission.Result);
        }
        var execution = new ExtensionInstallExecutionPlan(plan, permission);
        if (execution.IsNoOp)
        {
            return ExtensionInstallResultFactory.NoOp(plan, permission.Result, contentFindings);
        }
        if (request.Mode == ExtensionInstallMode.DryRun)
        {
            return new(request, plan.Facts with { Permissions = permission.Result }, [.. build.Result.Findings, .. contentFindings]);
        }

        if (!request.Automatic)
        {
            if (!request.AllowInteraction)
            {
                return ExtensionInstallResultFactory.PlanBoundary(plan,
                    new(ExtensionInstallFindingCode.ConfirmationRequired,
                        "Extension install needs confirmation, and this session cannot ask."),
                    permission.Result);
            }

            CliPromptReply<bool> approval;
            try
            {
                approval = await _interaction.Apply(
                    forcePreview,
                    _interaction.ApplyQuestion,
                    new CliPromptPolicy(!request.Automatic && request.AllowInteraction),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return ExtensionInstallResultFactory.PlanBoundary(plan,
                    new(ExtensionInstallFindingCode.Interrupted, "Extension install was cancelled. Nothing was changed."),
                    permission.Result);
            }
            if (approval.State == CliPromptState.Unavailable)
            {
                return ExtensionInstallResultFactory.PlanBoundary(plan,
                    new(ExtensionInstallFindingCode.ConfirmationRequired,
                        "Extension install needs confirmation, and this session cannot ask."),
                    permission.Result);
            }
            if (approval.State != CliPromptState.Answered || !approval.Value)
            {
                return ExtensionInstallResultFactory.PlanBoundary(plan,
                    new(ExtensionInstallFindingCode.Interrupted, "Extension install was cancelled. Nothing was changed."),
                    permission.Result);
            }
        }

        var outcome = await _application.ExecuteAsync(execution, cancellationToken).ConfigureAwait(false);
        return ExtensionInstallResultFactory.Application(plan, outcome.Progress, outcome.Finding, contentFindings);
    }

    private static ExtensionInstallRequest PreviewRequest(ExtensionInstallRequest request)
        => new(
            request.Workspace,
            ExtensionInstallMode.DryRun,
            request.RequestedIds,
            request.All,
            request.SourcePath,
            request.Force,
            request.Automatic,
            request.AllowInteraction,
            request.AllowPath);

    /// <summary>
    /// A leaf package that carries no payload installs nothing, which otherwise
    /// completes with no effects and no explanation. Dependency-only packages are
    /// valid aggregate packages when their dependency closure carries the payload.
    /// </summary>
    private static IReadOnlyList<ExtensionInstallFinding> ReadContentFindings(ExtensionInstallPlan plan)
        => [.. plan.Packages
            .Where(package => package.Payload.Count == 0 && package.Dependencies.Count == 0)
            .Where(package => !WasEntirePayloadExcluded(plan, package.Id))
            .Select(package => new ExtensionInstallFinding(
                ExtensionInstallFindingCode.PackageContentMissing,
                $"Extension package '{package.Id}' delivered no files. Package payload belongs under "
                    + $"'{ExtensionPackageLayout.ContentDirectoryName}/' beside the manifest.",
                package.Id))];

    private static bool WasEntirePayloadExcluded(
        ExtensionInstallPlan plan,
        string packageId)
    {
        var sourcePackage = plan.SourceRead.Packages.FirstOrDefault(package =>
            string.Equals(package.Id, packageId, StringComparison.Ordinal));
        if (sourcePackage is null || sourcePackage.Payload.Count == 0)
        {
            return false;
        }

        return sourcePackage.Payload.All(file => WorkspaceRemovals.IsPathRemoved(
            file.TargetPath ?? file.Path,
            plan.SettingsObservation.Document));
    }
}
