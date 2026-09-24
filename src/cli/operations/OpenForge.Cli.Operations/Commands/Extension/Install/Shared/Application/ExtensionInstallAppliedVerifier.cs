using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;

internal sealed class ExtensionInstallAppliedVerifier(
    PhysicalPathResolver physicalPathResolver,
    FileExpectationValidator validator)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly FileExpectationValidator _validator = validator;
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

        var ownershipVerified = plan.OwnershipChange is not null || plan.Ownership.IsTrustworthy;
        if (ownershipVerified)
        {
            var actual = await WorkspaceOwnershipReader.ReadAsync(_physicalPathResolver,
                plan.Request.Workspace, cancellationToken).ConfigureAwait(false);
            var matches = plan.OwnershipChange is { } change
                ? actual.Snapshot is { HasBytes: true } snapshot
                    && snapshot.Bytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan())
                : actual.Snapshot?.Expectation == plan.Ownership.Snapshot?.Expectation;
            if (!matches)
            {
                return FinalFrameworkBoundary(false,
                    "The ownership receipt does not match the verified plan.");
            }
        }
        var ownershipState = ownershipVerified ? ExtensionInstallVerificationState.Verified
            : ExtensionInstallVerificationState.NotRequested;

        return new ExtensionInstallVerificationResult(
            new ExtensionInstallVerification(
                ExtensionInstallVerificationState.Verified,
                ExtensionInstallVerificationState.Verified,
                ownershipState,
                ownershipState),
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

        ExtensionInstallTopologyBuild currentBuild;
        try
        {
            currentBuild = await _topologyBuilder.BuildWithFindingsAsync(
                plan.Request,
                plan.ValidationPackages,
                plan.SettingsObservation.Document,
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

        var currentFindings = currentBuild.Findings.ToList();
        foreach (var plannedExclusion in plan.TopologyFindings.Where(
                     finding => finding.Code == ExtensionInstallFindingCode.PathExcluded))
        {
            if (!currentFindings.Contains(plannedExclusion))
            {
                currentFindings.Add(plannedExclusion);
            }
        }

        if (ExtensionInstallApplicationPreconditionValidator.AppliedTopologyEquals(
                plan.Topology,
                currentBuild.Topology)
            && FindingsEqual(plan.TopologyFindings, currentFindings))
        {
            return TargetTopologyVerification.Verified();
        }

        return TargetTopologyVerification.Failed(
                "The applied Extension source topology differs from the planned topology.",
                targetsVerified: true);
    }

    private static bool FindingsEqual(
        IReadOnlyList<ExtensionInstallFinding> expected,
        IReadOnlyList<ExtensionInstallFinding> actual)
        => expected
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.Target is null ? 0 : 1)
            .ThenBy(finding => finding.Target, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .SequenceEqual(actual
                .OrderBy(finding => finding.Code)
                .ThenBy(finding => finding.Target is null ? 0 : 1)
                .ThenBy(finding => finding.Target, StringComparer.Ordinal)
                .ThenBy(finding => finding.Cause, StringComparer.Ordinal));

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
                ExtensionInstallVerificationState.Failed,
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
