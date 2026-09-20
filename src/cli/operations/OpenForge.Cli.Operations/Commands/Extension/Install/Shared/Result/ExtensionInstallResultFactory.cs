using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Result;

internal static class ExtensionInstallResultFactory
{
    internal static ExtensionInstallResult? SourceBoundary(
        ExtensionInstallRequest request,
        ExtensionSourceReadResult source)
        => source.State switch
        {
            ExtensionSourceReadState.Complete => null,
            ExtensionSourceReadState.Cancelled => Boundary(
                request,
                ExtensionInstallBoundaryEvidence.Empty,
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.Interrupted,
                    "Extension source reading was interrupted.")),
            ExtensionSourceReadState.Missing
                or ExtensionSourceReadState.Unavailable => Boundary(
                    request,
                    ExtensionInstallBoundaryEvidence.Empty,
                    new ExtensionInstallFinding(
                        ExtensionInstallFindingCode.SourceUnavailable,
                        source.Cause ?? "The Extension source is unavailable.",
                        source.Identity)),
            ExtensionSourceReadState.Invalid
                or ExtensionSourceReadState.Blocked => Boundary(
                    request,
                    ExtensionInstallBoundaryEvidence.Empty,
                    new ExtensionInstallFinding(
                        ExtensionInstallFindingCode.SourceInvalid,
                        source.Cause ?? "The Extension source is invalid.",
                        source.Identity)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(source),
                source.State,
                "The Extension source read state is not defined."),
        };

    internal static ExtensionInstallSource Source(ExtensionSourceReadResult source)
        => new(
            source.Kind switch
            {
                Framework.Extensions.Models.ExtensionSourceKind.EmbeddedCatalogue => ExtensionInstallSourceKind.Embedded,
                Framework.Extensions.Models.ExtensionSourceKind.Package => ExtensionInstallSourceKind.Package,
                Framework.Extensions.Models.ExtensionSourceKind.Catalogue => ExtensionInstallSourceKind.Catalogue,
                null => throw new InvalidOperationException(
                    "A complete Extension source requires its exact source kind."),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(source),
                    source.Kind,
                    "The Extension source kind is not defined."),
            },
            source.Kind == Framework.Extensions.Models.ExtensionSourceKind.EmbeddedCatalogue
                ? null
                : source.Identity,
            source.Identity,
            source.Packages.Count);

    internal static ExtensionInstallResult Boundary(
        ExtensionInstallRequest request,
        ExtensionInstallBoundaryEvidence evidence,
        ExtensionInstallFinding finding)
        => Result(
            request,
            new ExtensionInstallResultFacts
            {
                Selection = evidence.Selection,
                Source = evidence.Source,
                Packages = evidence.Packages,
                Framework = null,
                Footprint = null,
                Effects = [],
                GeneratedNavigation = null,
                Lifecycle = new ExtensionInstallLifecycle(
                    ExtensionInstallLifecycleAction.None,
                    ExtensionInstallLifecycleOutcome.NotRequested),
                Recovery = new ExtensionInstallRecovery(
                    ExtensionInstallRecoveryState.NotRequired,
                    [],
                    residualPath: null),
                Verification = new ExtensionInstallVerification(
                    ExtensionInstallVerificationState.NotRequested,
                    ExtensionInstallVerificationState.NotRequested,
                    ExtensionInstallVerificationState.NotRequested,
                    ExtensionInstallVerificationState.NotRequested),
            },
            [finding]);

    internal static ExtensionInstallResultFacts PlanFacts(
        ExtensionInstallPlannedFactsInput input)
    {
        var effects = input.EffectPlan.Effects.Select(effect => effect.Result).ToArray();
        var directories = input.EffectPlan.Effects
            .Select(effect => effect.DirectoryCreation)
            .OfType<Framework.Mutation.Models.Filesystem.Directories.PlannedDirectoryCreation>()
            .Select(creation => Relative(input.Request, creation.LogicalPath))
            .Order(StringComparer.Ordinal)
            .ToArray();
        return new ExtensionInstallResultFacts
        {
            Selection = input.Selection,
            Source = input.Source,
            Packages = input.Packages,
            Framework = new ExtensionInstallFramework(
                input.FrameworkPayload.InventoryFingerprint,
                input.FrameworkOwnership?.Paths.Length ?? 0,
                input.FrameworkOwnership?.Regions.Length ?? 0),
            Footprint = new ExtensionInstallFootprint(
                input.Packages.Count,
                input.Topology.IntendedTargetBytes.Keys.Order(StringComparer.Ordinal),
                input.Topology.Regions.Select(region => region.Path).Order(StringComparer.Ordinal),
                directories),
            Effects = effects,
            GeneratedNavigation = new ExtensionInstallGeneratedNavigation(input.Topology.Regions),
            Lifecycle = new ExtensionInstallLifecycle(
                input.EffectPlan.LifecycleAction,
                input.EffectPlan.LifecycleAction switch
                {
                    ExtensionInstallLifecycleAction.None => ExtensionInstallLifecycleOutcome.NotRequested,
                    ExtensionInstallLifecycleAction.Preserve => ExtensionInstallLifecycleOutcome.AlreadyCurrent,
                    ExtensionInstallLifecycleAction.Publish => ExtensionInstallLifecycleOutcome.Planned,
                    _ => throw new InvalidOperationException("The ownership publication action is not defined."),
                }),
            Recovery = new ExtensionInstallRecovery(
                effects.Length == 0 && input.EffectPlan.OwnershipChange is null
                    ? ExtensionInstallRecoveryState.NotRequired
                    : ExtensionInstallRecoveryState.NotCreated,
                [],
                residualPath: null),
            Verification = new ExtensionInstallVerification(
                ExtensionInstallVerificationState.Planned,
                ExtensionInstallVerificationState.Planned,
                ExtensionInstallVerificationState.Planned,
                ExtensionInstallVerificationState.Planned),
        };
    }

    internal static ExtensionInstallResult Result(
        ExtensionInstallRequest request,
        ExtensionInstallResultFacts facts,
        IEnumerable<ExtensionInstallFinding> findings)
        => new(request, facts, findings);

    internal static ExtensionInstallResult NoOp(
        ExtensionInstallPlan plan,
        WorkspacePermissionResult permissions,
        IEnumerable<ExtensionInstallFinding>? findings = null)
        => Result(
            plan.Request,
            plan.Facts with
            {
                Permissions = permissions,
                Effects = [],
                Lifecycle = plan.Facts.Lifecycle with
                {
                    Outcome = plan.Facts.Lifecycle.Action == ExtensionInstallLifecycleAction.None
                        ? ExtensionInstallLifecycleOutcome.NotRequested : ExtensionInstallLifecycleOutcome.AlreadyCurrent,
                },
                Recovery = new ExtensionInstallRecovery(
                    ExtensionInstallRecoveryState.NotRequired,
                    [],
                    residualPath: null),
                Verification = new ExtensionInstallVerification(
                    ExtensionInstallVerificationState.Verified,
                    ExtensionInstallVerificationState.Verified,
                    ExtensionInstallVerificationState.Verified,
                    ExtensionInstallVerificationState.Verified),
            },
            plan.TopologyFindings.Concat(findings ?? []));

    internal static ExtensionInstallResult PlanBoundary(
        ExtensionInstallPlan plan,
        ExtensionInstallFinding finding,
        WorkspacePermissionResult? permissions = null,
        IEnumerable<ExtensionInstallFinding>? additionalFindings = null)
        => Result(
            plan.Request,
            plan.Facts with
            {
                Permissions = permissions ?? plan.Facts.Permissions,
                Effects = [.. plan.Facts.Effects.Select(effect => effect with
                {
                    Outcome = ExtensionInstallEffectOutcome.NotStarted,
                })],
                Lifecycle = plan.Facts.Lifecycle with
                {
                    Outcome = plan.Facts.Lifecycle.Action == ExtensionInstallLifecycleAction.Publish
                        ? ExtensionInstallLifecycleOutcome.NotStarted
                        : plan.Facts.Lifecycle.Outcome,
                },
                Verification = new ExtensionInstallVerification(
                    ExtensionInstallVerificationState.Unknown,
                    ExtensionInstallVerificationState.Unknown,
                    ExtensionInstallVerificationState.Unknown,
                    ExtensionInstallVerificationState.Unknown),
            },
            plan.TopologyFindings
                .Concat(additionalFindings ?? [])
                .Append(finding));

    internal static ExtensionInstallResult Application(
        ExtensionInstallPlan plan,
        ExtensionInstallApplicationProgress progress,
        ExtensionInstallFinding? finding,
        IEnumerable<ExtensionInstallFinding>? findings = null)
        => Result(
            plan.Request,
            plan.Facts with
            {
                Effects = progress.Effects,
                Permissions = progress.Permissions,
                Lifecycle = plan.Facts.Lifecycle with
                {
                    Outcome = progress.LifecycleOutcome,
                },
                Recovery = progress.Recovery,
                Verification = progress.Verification,
            },
            finding is null
                ? plan.TopologyFindings.Concat(findings ?? [])
                : plan.TopologyFindings.Concat(findings ?? []).Append(finding));

    internal static ExtensionInstallBoundaryEvidence Evidence(
        ExtensionInstallSource source,
        ExtensionInstallSelection? selection = null,
        IReadOnlyList<ExtensionInstallPackage>? packages = null)
        => new(source, selection, packages ?? []);

    private static string Relative(ExtensionInstallRequest request, string logical)
        => Path.GetRelativePath(request.Workspace.LexicalRoot, logical)
            .Replace(Path.DirectorySeparatorChar, '/');
}
