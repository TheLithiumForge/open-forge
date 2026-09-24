using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;
using OpenForge.Cli.Core.Commands.Route.Shared.Navigation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed class RouteRemoveCategoryAbsencePlanner(
    SourceCatalogueReader catalogueReader,
    PhysicalPathResolver physicalPathResolver,
    RouteNavigationExposureReader exposureReader,
    RouteRemoveNavigationPlanner navigationPlanner,
    RouteRemoveReferencePlanner referencePlanner)
{
    private readonly SourceCatalogueReader _catalogueReader = catalogueReader;
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly RouteNavigationExposureReader _exposureReader = exposureReader;
    private readonly RouteRemoveNavigationPlanner _navigationPlanner = navigationPlanner;
    private readonly RouteRemoveReferencePlanner _referencePlanner = referencePlanner;

    internal async ValueTask<RouteRemovePlanBuild> BuildAsync(
        RouteRemoveAbsenceScope scope,
        CancellationToken cancellationToken)
        => await BuildAsync(scope, postRemovePlan: null, allowPlannedClaims: false, cancellationToken)
            .ConfigureAwait(false);

    internal async ValueTask<RouteRemovePlanBuild> BuildPostRemoveAsync(
        RouteRemoveAbsenceScope scope,
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
        => await BuildAsync(scope, plan, allowPlannedClaims: false, cancellationToken)
            .ConfigureAwait(false);

    internal async ValueTask<RouteRemovePlanBuild> BuildPostContentAsync(
        RouteRemoveAbsenceScope scope,
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
        => await BuildAsync(scope, plan, allowPlannedClaims: true, cancellationToken)
            .ConfigureAwait(false);

    private async ValueTask<RouteRemovePlanBuild> BuildAsync(
        RouteRemoveAbsenceScope scope,
        RouteRemovePlan? postRemovePlan,
        bool allowPlannedClaims,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        var formation = RouteRemoveBoundary.Start(scope.Request) with
        {
            Source = scope.Source,
            Subject = scope.Subject,
        };
        var catalogue = await _catalogueReader.ReadAsync(
            new SourceCatalogueRequest(scope.Request.Workspace, [SourceLogicalPath.AgentsRoot]),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                scope.IntendedPath,
                "Route Remove absence proof was interrupted during source discovery.");
        }

        if (catalogue.Issues.FirstOrDefault(issue =>
                issue.Stage == SourceCatalogueIssueStage.Root) is { } rootIssue)
        {
            var unsafeRoot = rootIssue.Code == SourceCatalogueIssueCode.RootUnsafe;
            return Stop(
                formation,
                unsafeRoot ? RouteRemoveFindingCode.WorkspaceUnsafe : RouteRemoveFindingCode.WorkspaceUnavailable,
                unsafeRoot ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
                rootIssue.AttemptedCanonicalPath,
                rootIssue.Failure?.DirectCause
                    ?? "The workspace source root is unsafe or unavailable.");
        }

        var retainedCandidate = scope.Mode == RouteRemoveAbsenceScopeMode.PlannedSubject
            ? catalogue.FindAllCandidatesById(scope.Id)
                .Any(candidate => scope.Contains(candidate.CanonicalPath))
            : catalogue.FindAllCandidatesById(scope.Id).Count != 0;
        if (retainedCandidate)
        {
            var retainedOverwrite = scope.Mode == RouteRemoveAbsenceScopeMode.LogicalRequest
                && catalogue.FindAllCandidatesById(scope.Id).Any(candidate =>
                    string.Equals(candidate.CanonicalPath, scope.LeafOverwritePath, StringComparison.Ordinal));
            if (retainedOverwrite)
            {
                return Stop(
                    formation,
                    RouteRemoveFindingCode.InvalidSubject,
                    CliSemanticStatus.Invalid,
                    scope.LeafOverwritePath,
                    "The selected source is an overwrite file.");
            }

            return Stop(
                formation,
                RouteRemoveFindingCode.TargetChanged,
                CliSemanticStatus.Blocked,
                scope.IntendedPath,
                "The requested category identity is present and must be planned from its current source facts.");
        }

        foreach (var path in scope.ProbePaths)
        {
            if (Resolve(scope, path) is { } boundary)
            {
                return Stop(formation, boundary.Code, boundary.Status, path, boundary.Cause);
            }
        }

        var settings = await WorkspaceSettingsReader.ReadAsync(
            _physicalPathResolver,
            scope.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || settings.Snapshot is null)
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.SettingsUnavailable,
                CliSemanticStatus.Blocked,
                WorkspaceSettingsDefinitions.RelativePath,
                settings.Cause ?? "The required workspace settings are unavailable or invalid.");
        }

        var ownership = await WorkspaceOwnershipReader.ReadAsync(
            _physicalPathResolver, scope.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        var projectedOwnership = RouteRemoveOwnershipProjector.Project(ownership, scope);
        formation = formation with { Ownership = projectedOwnership };
        if (!RouteRemovePersistencePlanner.IsOwnershipTrusted(ownership))
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.OwnershipUnavailable,
                CliSemanticStatus.Blocked,
                scope.IntendedPath,
                ownership.Cause ?? "The required workspace ownership record is unavailable or is not canonical.");
        }

        if (allowPlannedClaims
            && postRemovePlan is not null
            && !MatchesOwnershipObservation(postRemovePlan.Projection.Ownership, ownership))
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.TargetChanged,
                CliSemanticStatus.Blocked,
                ownership.LogicalPath,
                "The ownership lock changed before selected content claims could be released.");
        }

        if (projectedOwnership.State == RouteRemoveOwnershipState.Claimed)
        {
            var expectedClaims = postRemovePlan?.Preview.Persistence.Ownership.Claims ?? [];
            if (!allowPlannedClaims || postRemovePlan is null
                || !projectedOwnership.Claims.SequenceEqual(expectedClaims))
            {
                return Stop(
                    formation,
                    RouteRemoveFindingCode.OwnershipClaimed,
                    CliSemanticStatus.Blocked,
                    scope.IntendedPath,
                    "Lifecycle ownership claims the requested category boundary beyond the accepted removal plan.");
            }
        }

        var hasRemovalIntent = postRemovePlan is not null
            ? postRemovePlan.Projection.RemovalSelection.Files
                .Concat(postRemovePlan.Projection.RemovalSelection.Directories)
                .DefaultIfEmpty(scope.IntendedPath)
                .All(path => WorkspaceRemovals.IsPathRemoved(path, settings.Document))
            : new[]
                {
                    scope.IntendedPath,
                    scope.CategoryRoot,
                    scope.LeafPath,
                    scope.LeafOverwritePath,
                }
                .Distinct(StringComparer.Ordinal)
                .Any(path => WorkspaceRemovals.IsPathRemoved(path, settings.Document));
        if (hasRemovalIntent)
        {
            formation = formation with
            {
                Persistence = PersistedRemoval(scope, settings, projectedOwnership, postRemovePlan, allowPlannedClaims),
            };
        }

        var exposure = await _exposureReader.ReadAsync(
            scope.Request.Workspace,
            catalogue,
            cancellationToken).ConfigureAwait(false);
        if (exposure.IsCancelled)
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                scope.IntendedPath,
                "Generated-navigation absence inspection was interrupted.");
        }

        if (!exposure.UnavailableParents.IsEmpty)
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.ProjectionIncomplete,
                CliSemanticStatus.Incomplete,
                exposure.UnavailableParents[0],
                "Generated-navigation absence could not be established from every parent source.");
        }

        if (exposure.ExposedPaths.FirstOrDefault(scope.Contains) is { } exposedPath)
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.GeneratedRegionUnsafe,
                CliSemanticStatus.Blocked,
                exposedPath,
                "Generated navigation still exposes the requested category boundary.");
        }

        if (postRemovePlan is not null)
        {
            var navigation = _navigationPlanner.Observe(
                postRemovePlan,
                catalogue,
                cancellationToken);
            if (navigation.State == RouteRemoveNavigationPostRemoveState.Interrupted)
            {
                return Stop(
                    formation,
                    RouteRemoveFindingCode.Interrupted,
                    CliSemanticStatus.Interrupted,
                    scope.IntendedPath,
                    navigation.Cause
                        ?? "Final Route Remove navigation observation was interrupted.");
            }

            if (navigation.State == RouteRemoveNavigationPostRemoveState.Failed)
            {
                return Stop(
                    formation,
                    RouteRemoveFindingCode.ProjectionIncomplete,
                    CliSemanticStatus.Incomplete,
                    scope.IntendedPath,
                    navigation.Cause
                        ?? "Final Route Remove navigation projection could not be verified.");
            }
        }

        formation = formation with
        {
            GeneratedNavigation = new RouteRemoveGeneratedNavigation
            {
                Coverage = RouteRemoveCoverage.Complete,
            },
        };
        var references = await _referencePlanner.ProveAbsenceAsync(
            scope,
            catalogue,
            cancellationToken).ConfigureAwait(false);
        formation = formation with { References = references.References };
        if (references.Finding is { } finding)
        {
            return Stop(formation, finding.Code, finding.Status, finding.Target, finding.Cause);
        }

        formation = formation with
        {
            Plan = new RouteRemovePlanFacts
            {
                Completeness = RouteRemovePlanCompleteness.Complete,
                Safety = RouteRemovePlanSafety.Safe,
            },
            Verification = RouteRemoveVerificationState.Verified,
        };
        return new RouteRemovePlanBuild(plan: null, formation);
    }

    private RouteRemoveFinding? Resolve(RouteRemoveAbsenceScope scope, string canonicalPath)
    {
        var lexicalPath = SourceLogicalPath.ToLexicalPath(
            scope.Request.Workspace.LexicalRoot,
            canonicalPath);
        var resolution = _physicalPathResolver.ResolveCandidate(
            scope.Request.Workspace.LexicalRoot,
            scope.Request.Workspace.PhysicalRoot,
            lexicalPath);
        if (resolution.State == PhysicalPathState.Contained
            && scope.Mode == RouteRemoveAbsenceScopeMode.LogicalRequest
            && string.Equals(canonicalPath, scope.LeafOverwritePath, StringComparison.Ordinal))
        {
            return new RouteRemoveFinding(
                RouteRemoveFindingCode.InvalidSubject,
                CliSemanticStatus.Invalid,
                canonicalPath,
                "The selected source is an overwrite file.");
        }

        return resolution.State switch
        {
            PhysicalPathState.Missing => null,
            PhysicalPathState.Contained => new RouteRemoveFinding(
                RouteRemoveFindingCode.TargetChanged,
                CliSemanticStatus.Blocked,
                canonicalPath,
                "The requested category boundary is present and requires a fresh removal plan."),
            PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure => new RouteRemoveFinding(
                RouteRemoveFindingCode.WorkspaceUnavailable,
                CliSemanticStatus.Incomplete,
                canonicalPath,
                resolution.Failure?.DirectCause
                    ?? "The requested category absence could not be inspected completely."),
            PhysicalPathState.Dangling
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported => new RouteRemoveFinding(
                    RouteRemoveFindingCode.SourceUnsafe,
                    CliSemanticStatus.Blocked,
                    canonicalPath,
                    resolution.Failure?.DirectCause
                        ?? "The requested category absence crosses an unsafe filesystem boundary."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(canonicalPath),
                resolution.State,
                "The physical path state is not defined."),
        };
    }

    private static bool MatchesOwnershipObservation(
        WorkspaceOwnershipRead expected,
        WorkspaceOwnershipRead actual)
    {
        if (expected.State != actual.State)
        {
            return false;
        }

        if (expected.Snapshot is not { } before)
        {
            return actual.Snapshot is null;
        }

        return actual.Snapshot is { } after
            && before.Expectation == after.Expectation
            && before.Bytes.AsSpan().SequenceEqual(after.Bytes.AsSpan());
    }

    private static RouteRemovePersistence PersistedRemoval(
        RouteRemoveAbsenceScope scope,
        WorkspaceSettingsRead settings,
        RouteRemoveOwnership ownership,
        RouteRemovePlan? postRemovePlan,
        bool allowPlannedClaims)
    {
        if (postRemovePlan is { } plan)
        {
            var planned = plan.Preview.Persistence;
            return planned with
            {
                Settings = planned.Settings with
                {
                    Outcome = plan.Projection.SettingsChange is null
                        ? RouteRemovePersistenceOutcome.Unchanged
                        : RouteRemovePersistenceOutcome.Applied,
                },
                Ownership = planned.Ownership with
                {
                    Outcome = plan.Projection.OwnershipChange is null
                        ? RouteRemovePersistenceOutcome.Unchanged
                        : allowPlannedClaims
                            ? RouteRemovePersistenceOutcome.Planned
                            : RouteRemovePersistenceOutcome.Applied,
                },
            };
        }

        var exactPaths = new HashSet<string>(StringComparer.Ordinal)
        {
            scope.IntendedPath,
            scope.CategoryRoot,
            scope.LeafPath,
            scope.LeafOverwritePath,
        };
        var categories = settings.Document.RemovedCategories
            .Where(category => string.Equals(
                scope.CategoryRoot,
                $"{SourceLogicalPath.AgentsRoot}/{category}",
                StringComparison.Ordinal))
            .ToImmutableArray();
        var files = settings.Document.RemovedFiles
            .Where(exactPaths.Contains)
            .ToImmutableArray();
        var directories = settings.Document.RemovedDirectories
            .Where(exactPaths.Contains)
            .ToImmutableArray();

        return new RouteRemovePersistence
        {
            Settings = new RouteRemoveSettingsRemoval
            {
                Outcome = RouteRemovePersistenceOutcome.Unchanged,
                Path = WorkspaceSettingsDefinitions.RelativePath,
                Categories = categories,
                Files = files,
                Directories = directories,
            },
            Ownership = new RouteRemoveOwnershipRelease
            {
                Outcome = RouteRemovePersistenceOutcome.Unchanged,
                Claims = ownership.Claims,
            },
        };
    }

    private static RouteRemovePlanBuild Stop(
        RouteRemoveResultFormation formation,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => new(
            plan: null,
            RouteRemoveBoundary.Stop(formation, code, status, target, cause));
}
