using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;

internal sealed class ExtensionInstallAppliedVerifier(
    PhysicalPathResolver physicalPathResolver,
    FileExpectationValidator validator,
    LifecycleStore lifecycleStore)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly FileExpectationValidator _validator = validator;
    private readonly LifecycleStore _lifecycleStore = lifecycleStore;
    private readonly FrameworkLifecycleCurrentnessReader _frameworkCurrentness = new(physicalPathResolver);
    private readonly ExtensionInstallTopologyBuilder _topologyBuilder = new();

    internal async ValueTask<ExtensionInstallVerificationResult> VerifyTopologyAsync(
        ExtensionInstallPlan plan,
        CancellationToken cancellationToken)
    {
        var topology = await ReadTargetTopologyAsync(plan, cancellationToken).ConfigureAwait(false);
        var verification = new ExtensionInstallVerification(
            ReadVerificationState(topology.TargetsVerified, topology.Cancelled),
            ReadVerificationState(topology.TopologyVerified, topology.Cancelled),
            ExtensionInstallVerificationState.Unknown,
            ExtensionInstallVerificationState.Unknown);
        return topology.IsVerified
            ? new ExtensionInstallVerificationResult(verification, Finding: null)
            : new ExtensionInstallVerificationResult(
                verification,
                new ExtensionInstallFinding(
                    topology.Cancelled
                        ? ExtensionInstallFindingCode.Interrupted
                        : ExtensionInstallFindingCode.TopologyVerificationFailed,
                    topology.Cause
                        ?? "The intended Extension target topology could not be verified."));
    }

    internal async ValueTask<ExtensionInstallVerificationResult> VerifyFinalAsync(
        ExtensionInstallPlan plan,
        CancellationToken cancellationToken)
    {
        var topology = await ReadTargetTopologyAsync(plan, cancellationToken).ConfigureAwait(false);
        if (!topology.IsVerified)
        {
            return FinalBoundary(
                topology,
                ExtensionInstallVerificationState.Unknown,
                ExtensionInstallVerificationState.Unknown);
        }

        var extensionsRead = await _lifecycleStore.ReadAsync(
            plan.Request.Workspace,
            LifecycleSection.Extensions,
            cancellationToken).ConfigureAwait(false);
        if (extensionsRead.State != LifecycleStoreReadState.Available
            || extensionsRead.Extensions is not { } extensions
            || extensionsRead.File is not { } extensionsFile
            || !ExtensionInstallApplicationPreconditionValidator.LifecycleEquals(
                plan.IntendedLifecycle,
                extensions))
        {
            var cancelled = extensionsRead.State == LifecycleStoreReadState.Cancelled;
            return new ExtensionInstallVerificationResult(
                new ExtensionInstallVerification(
                    ExtensionInstallVerificationState.Verified,
                    ExtensionInstallVerificationState.Verified,
                    cancelled
                        ? ExtensionInstallVerificationState.Unknown
                        : ExtensionInstallVerificationState.Failed,
                    ExtensionInstallVerificationState.Unknown),
                new ExtensionInstallFinding(
                    cancelled
                        ? ExtensionInstallFindingCode.Interrupted
                        : ExtensionInstallFindingCode.VerificationFailed,
                    extensionsRead.Cause
                        ?? "The final Extension lifecycle meaning does not match the intended state."));
        }

        var frameworkRead = await _lifecycleStore.ReadAsync(
            plan.Request.Workspace,
            LifecycleSection.Framework,
            cancellationToken).ConfigureAwait(false);
        if (frameworkRead.State != LifecycleStoreReadState.Available
            || frameworkRead.Framework is not { } framework
            || frameworkRead.File is not { } frameworkFile
            || frameworkFile.Expectation != extensionsFile.Expectation
            || !frameworkFile.Bytes.AsSpan().SequenceEqual(extensionsFile.Bytes.AsSpan())
            || !ExtensionInstallApplicationPreconditionValidator.LifecycleEquals(
                plan.FrameworkLifecycle,
                framework))
        {
            var cancelled = frameworkRead.State == LifecycleStoreReadState.Cancelled;
            return FinalFrameworkBoundary(
                cancelled,
                frameworkRead.Cause
                    ?? "The Framework lifecycle meaning changed during Extension Install.");
        }

        var currentness = await _frameworkCurrentness.ReadAsync(
            plan.Request.Workspace,
            ExtensionInstallFoundationReader.WithGeneratedTopology(framework, plan.Topology),
            plan.FrameworkPayload,
            cancellationToken).ConfigureAwait(false);
        if (currentness.State != FrameworkLifecycleCurrentnessState.Current)
        {
            return FinalFrameworkBoundary(
                currentness.State == FrameworkLifecycleCurrentnessState.Cancelled,
                currentness.Cause
                    ?? "The Framework targets changed during Extension Install.",
                currentness.Path);
        }

        return new ExtensionInstallVerificationResult(
            new ExtensionInstallVerification(
                ExtensionInstallVerificationState.Verified,
                ExtensionInstallVerificationState.Verified,
                ExtensionInstallVerificationState.Verified,
                ExtensionInstallVerificationState.Verified),
            Finding: null);
    }

    private async ValueTask<TargetTopologyVerification> ReadTargetTopologyAsync(
        ExtensionInstallPlan plan,
        CancellationToken cancellationToken)
    {
        foreach (var target in plan.Topology.IntendedTargetBytes.OrderBy(
                     pair => pair.Key,
                     StringComparer.Ordinal))
        {
            var logical = Path.GetFullPath(Path.Combine(
                plan.Request.Workspace.LexicalRoot,
                target.Key.Replace('/', Path.DirectorySeparatorChar)));
            var resolution = _physicalPathResolver.ResolveCandidate(
                plan.Request.Workspace.LexicalRoot,
                plan.Request.Workspace.PhysicalRoot,
                logical);
            if (resolution.State != PhysicalPathState.Contained)
            {
                return TargetTopologyVerification.Failed(
                    $"Extension target '{target.Key}' is not an ordinary contained file.");
            }

            var expectation = FileExpectation.File(
                logical,
                resolution.GetContainedPhysicalPath(),
                FileExpectation.Hash(target.Value.AsSpan()));
            var check = await _validator.ValidateAsync(
                plan.Request.Workspace,
                expectation,
                cancellationToken).ConfigureAwait(false);
            if (check.State == FileExpectationValidationState.Cancelled)
            {
                return TargetTopologyVerification.Interrupted();
            }

            if (check.State != FileExpectationValidationState.Matched)
            {
                return TargetTopologyVerification.Failed(
                    check.Cause ?? $"Extension target '{target.Key}' does not match its intended bytes.");
            }
        }

        ExtensionInstallTopology current;
        try
        {
            current = await _topologyBuilder.BuildAsync(
                plan.Request,
                plan.Packages,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return TargetTopologyVerification.Interrupted(targetsVerified: true);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or IOException)
        {
            return TargetTopologyVerification.Failed(exception.Message, targetsVerified: true);
        }

        if (ExtensionInstallApplicationPreconditionValidator.AppliedTopologyEquals(
                plan.Topology,
                current))
        {
            return TargetTopologyVerification.Verified();
        }

        return TargetTopologyVerification.Failed(
                "The applied Extension source topology differs from the planned topology.",
                targetsVerified: true);
    }

    private static ExtensionInstallVerificationResult FinalBoundary(
        TargetTopologyVerification topology,
        ExtensionInstallVerificationState extensions,
        ExtensionInstallVerificationState framework)
        => new(
            new ExtensionInstallVerification(
                ReadVerificationState(topology.TargetsVerified, topology.Cancelled),
                ReadVerificationState(topology.TopologyVerified, topology.Cancelled),
                extensions,
                framework),
            new ExtensionInstallFinding(
                topology.Cancelled
                    ? ExtensionInstallFindingCode.Interrupted
                    : ExtensionInstallFindingCode.VerificationFailed,
                topology.Cause ?? "Final Extension target verification failed."));

    private static ExtensionInstallVerificationState ReadVerificationState(
        bool verified,
        bool cancelled)
    {
        if (verified)
        {
            return ExtensionInstallVerificationState.Verified;
        }

        if (cancelled)
        {
            return ExtensionInstallVerificationState.Unknown;
        }

        return ExtensionInstallVerificationState.Failed;
    }

    private static ExtensionInstallVerificationResult FinalFrameworkBoundary(
        bool cancelled,
        string cause,
        string? target = null)
        => new(
            new ExtensionInstallVerification(
                ExtensionInstallVerificationState.Verified,
                ExtensionInstallVerificationState.Verified,
                ExtensionInstallVerificationState.Verified,
                cancelled
                    ? ExtensionInstallVerificationState.Unknown
                    : ExtensionInstallVerificationState.Failed),
            new ExtensionInstallFinding(
                cancelled
                    ? ExtensionInstallFindingCode.Interrupted
                    : ExtensionInstallFindingCode.VerificationFailed,
                cause,
                target));

    private sealed record TargetTopologyVerification(
        bool TargetsVerified,
        bool TopologyVerified,
        bool Cancelled,
        string? Cause)
    {
        internal bool IsVerified => TargetsVerified && TopologyVerified && !Cancelled;

        internal static TargetTopologyVerification Verified()
            => new(true, true, false, Cause: null);

        internal static TargetTopologyVerification Failed(
            string cause,
            bool targetsVerified = false)
            => new(targetsVerified, false, false, cause);

        internal static TargetTopologyVerification Interrupted(bool targetsVerified = false)
            => new(targetsVerified, false, true, "Extension verification was interrupted.");
    }
}
