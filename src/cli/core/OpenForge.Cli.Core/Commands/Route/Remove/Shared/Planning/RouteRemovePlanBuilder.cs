using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed class RouteRemovePlanBuilder(
    RouteRemoveSubjectResolver subjectResolver,
    RouteRemoveCategoryInventoryReader inventoryReader,
    RouteRemoveReferencePlanner referencePlanner,
    RouteRemoveNavigationPlanner navigationPlanner,
    RouteRemoveCategoryAbsencePlanner absencePlanner)
{
    private readonly RouteRemoveSubjectResolver _subjectResolver = subjectResolver;
    private readonly RouteRemoveCategoryInventoryReader _inventoryReader = inventoryReader;
    private readonly RouteRemoveReferencePlanner _referencePlanner = referencePlanner;
    private readonly RouteRemoveNavigationPlanner _navigationPlanner = navigationPlanner;
    private readonly RouteRemoveCategoryAbsencePlanner _absencePlanner = absencePlanner;

    internal static RouteRemovePlanBuilder Create()
    {
        var physical = new PhysicalPathResolver();
        var expectation = new FileExpectationValidator(physical);
        var markdown = new MarkdownDocumentParser();
        var subject = new RouteRemoveSubjectResolver(
            new SourceCatalogueReader(),
            new SourceReferenceResolver((workspace, canonicalPath) =>
                physical.ResolveCandidate(
                    workspace.LexicalRoot,
                    workspace.PhysicalRoot,
                    Path.Combine(
                        workspace.LexicalRoot,
                        canonicalPath.Replace('/', Path.DirectorySeparatorChar)))),
            new SourceRouteFactsResolver(),
            new RouteRemoveNavigationExposureReader(markdown),
            expectation);
        var inventory = new RouteRemoveCategoryInventoryReader(
            physical,
            expectation,
            new LifecycleOwnershipReader(physical));
        var destinationResolver = new SourceLinkDestinationResolver(
            (workspace, lexicalPath) => physical.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalPath),
            StrictUtf8FileReader.ReadAsync,
            markdown.Parse);
        var references = new RouteRemoveReferencePlanner(
            new RouteMarkdownCatalogueReader(physical),
            markdown,
            destinationResolver,
            expectation);
        var navigation = new RouteRemoveNavigationPlanner(
            new GeneratedNavigationFormationBuilder(),
            new GeneratedNavigationRegionPlanner(),
            new RouteRemoveNavigationSourceProjector());
        var absence = new RouteRemoveCategoryAbsencePlanner(
            new SourceCatalogueReader(),
            physical,
            new LifecycleOwnershipReader(physical),
            new RouteRemoveNavigationExposureReader(markdown),
            navigation,
            references);
        return new RouteRemovePlanBuilder(subject, inventory, references, navigation, absence);
    }

    internal async ValueTask<RouteRemovePlanBuild> BuildAsync(
        RouteRemoveRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var subject = await _subjectResolver.ResolveAsync(
            new RouteRemoveSubjectResolutionRequest { Request = request },
            cancellationToken).ConfigureAwait(false);
        if (subject.Boundary is { } subjectBoundary)
        {
            if (subjectBoundary.Findings.Any(finding =>
                    finding.Code == RouteRemoveFindingCode.SourceNotFound)
                && RouteRemoveAbsenceScope.TryCreate(request) is { } scope)
            {
                return await _absencePlanner.BuildAsync(scope, cancellationToken)
                    .ConfigureAwait(false);
            }

            return new RouteRemovePlanBuild(plan: null, subjectBoundary);
        }

        var resolved = subject.Subject
            ?? throw new InvalidOperationException("Successful Route Remove resolution requires its subject.");
        var inventory = await _inventoryReader.ReadAsync(
            new RouteRemoveCategoryInventoryRequest { Subject = resolved },
            cancellationToken).ConfigureAwait(false);
        if (inventory.Boundary is { } inventoryBoundary)
        {
            return new RouteRemovePlanBuild(plan: null, inventoryBoundary);
        }

        var facts = inventory.Inventory
            ?? throw new InvalidOperationException("Successful Route Remove inventory requires its facts.");
        var references = await _referencePlanner.BuildAsync(
            new RouteRemoveReferencePlanningRequest
            {
                Subject = resolved,
                CatalogueRequest = new RouteMarkdownCatalogueRequest(
                    request.Workspace,
                    ["."],
                    [],
                    new RouteMarkdownCatalogueFilters([".md"])),
            },
            facts,
            cancellationToken).ConfigureAwait(false);
        if (references.Boundary is { } referenceBoundary)
        {
            return new RouteRemovePlanBuild(plan: null, referenceBoundary with
            {
                Ownership = RouteRemoveOwnershipProjector.Project(facts.Ownership, resolved),
            });
        }

        var referencePlan = references.Plan
            ?? throw new InvalidOperationException("Successful Route Remove reference planning requires its plan.");
        var navigation = _navigationPlanner.Build(resolved);
        if (navigation.Boundary is { } navigationBoundary)
        {
            return new RouteRemovePlanBuild(plan: null, navigationBoundary with
            {
                Ownership = RouteRemoveOwnershipProjector.Project(facts.Ownership, resolved),
                References = referencePlan.References,
            });
        }

        var projection = RouteRemoveEffectPlanner.Build(
            facts,
            referencePlan,
            navigation.Plan
                ?? throw new InvalidOperationException("Successful Route Remove navigation planning requires its plan."));
        var plan = RouteRemovePlanProjector.Build(projection, facts);
        return new RouteRemovePlanBuild(plan, plan.Preview);
    }

    internal ValueTask<RouteRemovePlanBuild> BuildAbsenceAsync(
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return _absencePlanner.BuildPostRemoveAsync(
            RouteRemoveAbsenceScope.FromPlan(plan),
            plan,
            cancellationToken);
    }
}

internal sealed record RouteRemoveAbsenceScope
{
    public required RouteRemoveRequest Request { get; init; }

    public required string Id { get; init; }

    public required string CategoryRoot { get; init; }

    public required string LeafPath { get; init; }

    public required string LeafOverwritePath { get; init; }

    public required string IntendedPath { get; init; }

    public required RouteRemoveSource Source { get; init; }

    public required RouteRemoveSubject Subject { get; init; }

    internal bool Contains(string canonicalPath)
        => string.Equals(canonicalPath, LeafPath, StringComparison.Ordinal)
            || string.Equals(canonicalPath, LeafOverwritePath, StringComparison.Ordinal)
            || string.Equals(canonicalPath, CategoryRoot, StringComparison.Ordinal)
            || canonicalPath.StartsWith($"{CategoryRoot}/", StringComparison.Ordinal);

    internal static RouteRemoveAbsenceScope? TryCreate(RouteRemoveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var parsed = SourceReferenceParser.Parse(request.SourceReference);
        if (parsed.State != SourceReferenceParseState.Valid)
        {
            return null;
        }

        return parsed.Kind switch
        {
            SourceReferenceKind.SourceId => FromId(
                request,
                parsed.AttemptedId
                    ?? throw new InvalidOperationException(
                        "A valid Route Remove source ID requires its attempted identity.")),
            SourceReferenceKind.SourcePath => FromPath(
                request,
                parsed.AttemptedPath
                    ?? throw new InvalidOperationException(
                        "A valid Route Remove source path requires its attempted identity.")),
            _ => throw new ArgumentOutOfRangeException(
                nameof(request),
                parsed.Kind,
                "The Route Remove source-reference kind is not defined."),
        };
    }

    internal static RouteRemoveAbsenceScope FromPlan(RouteRemovePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var source = plan.Preview.Source;
        var id = source.Id
            ?? throw new InvalidOperationException(
                "An accepted Route Remove plan requires its exact source identity.");
        var intendedPath = source.Path
            ?? throw new InvalidOperationException(
                "An accepted Route Remove plan requires its exact source path.");
        var subjectKind = plan.Preview.Subject.Kind
            ?? throw new InvalidOperationException(
                "An accepted Route Remove plan requires its exact subject kind.");
        var categoryRoot = $"{SourceLogicalPath.AgentsRoot}/{id}";
        return new RouteRemoveAbsenceScope
        {
            Request = plan.Request,
            Id = id,
            CategoryRoot = categoryRoot,
            LeafPath = $"{SourceLogicalPath.AgentsRoot}/{id}.md",
            LeafOverwritePath = $"{SourceLogicalPath.AgentsRoot}/{id}.overwrite.md",
            IntendedPath = intendedPath,
            Source = source,
            Subject = new RouteRemoveSubject { Kind = subjectKind },
        };
    }

    private static RouteRemoveAbsenceScope FromId(RouteRemoveRequest request, string id)
    {
        var categoryRoot = $"{SourceLogicalPath.AgentsRoot}/{id}";
        var name = id[(id.LastIndexOf('/') + 1)..];
        var intendedPath = $"{categoryRoot}/_{name}.md";
        return new RouteRemoveAbsenceScope
        {
            Request = request,
            Id = id,
            CategoryRoot = categoryRoot,
            LeafPath = $"{SourceLogicalPath.AgentsRoot}/{id}.md",
            LeafOverwritePath = $"{SourceLogicalPath.AgentsRoot}/{id}.overwrite.md",
            IntendedPath = intendedPath,
            Source = new RouteRemoveSource
            {
                Requested = request.SourceReference,
                SelectedBy = RouteRemoveSourceSelection.SourceId,
                Id = id,
                Path = intendedPath,
                Form = RouteRemoveSourceForm.CanonicalEntrypoint,
            },
            Subject = new RouteRemoveSubject { Kind = RouteRemoveSubjectKind.Category },
        };
    }

    private static RouteRemoveAbsenceScope? FromPath(RouteRemoveRequest request, string path)
    {
        if (!SourceFormClassifier.TryClassify(path, out var form)
            || !SourceFormClassifier.IsEntrypoint(form))
        {
            return null;
        }

        var id = SourceIdentity.DeriveId(path);
        if (id is null)
        {
            return null;
        }

        var categoryRoot = SourceLogicalPath.ReadParent(path);
        return new RouteRemoveAbsenceScope
        {
            Request = request,
            Id = id,
            CategoryRoot = categoryRoot,
            LeafPath = $"{SourceLogicalPath.AgentsRoot}/{id}.md",
            LeafOverwritePath = $"{SourceLogicalPath.AgentsRoot}/{id}.overwrite.md",
            IntendedPath = path,
            Source = new RouteRemoveSource
            {
                Requested = request.SourceReference,
                SelectedBy = RouteRemoveSourceSelection.BasePath,
                Id = id,
                Path = path,
                Form = form == SourceDocumentForm.CanonicalEntrypoint
                    ? RouteRemoveSourceForm.CanonicalEntrypoint
                    : RouteRemoveSourceForm.CompatibilityEntrypoint,
            },
            Subject = new RouteRemoveSubject { Kind = RouteRemoveSubjectKind.Category },
        };
    }
}

internal sealed class RouteRemoveCategoryAbsencePlanner(
    SourceCatalogueReader catalogueReader,
    PhysicalPathResolver physicalPathResolver,
    LifecycleOwnershipReader ownershipReader,
    RouteRemoveNavigationExposureReader exposureReader,
    RouteRemoveNavigationPlanner navigationPlanner,
    RouteRemoveReferencePlanner referencePlanner)
{
    private readonly SourceCatalogueReader _catalogueReader = catalogueReader;
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly LifecycleOwnershipReader _ownershipReader = ownershipReader;
    private readonly RouteRemoveNavigationExposureReader _exposureReader = exposureReader;
    private readonly RouteRemoveNavigationPlanner _navigationPlanner = navigationPlanner;
    private readonly RouteRemoveReferencePlanner _referencePlanner = referencePlanner;

    internal async ValueTask<RouteRemovePlanBuild> BuildAsync(
        RouteRemoveAbsenceScope scope,
        CancellationToken cancellationToken)
        => await BuildAsync(scope, postRemovePlan: null, cancellationToken)
            .ConfigureAwait(false);

    internal async ValueTask<RouteRemovePlanBuild> BuildPostRemoveAsync(
        RouteRemoveAbsenceScope scope,
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
        => await BuildAsync(scope, plan, cancellationToken)
            .ConfigureAwait(false);

    private async ValueTask<RouteRemovePlanBuild> BuildAsync(
        RouteRemoveAbsenceScope scope,
        RouteRemovePlan? postRemovePlan,
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

        if (catalogue.FindAllCandidatesById(scope.Id).Count != 0)
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.TargetChanged,
                CliSemanticStatus.Blocked,
                scope.IntendedPath,
                "The requested category identity is present and must be planned from its current source facts.");
        }

        foreach (var path in new[] { scope.LeafPath, scope.LeafOverwritePath, scope.CategoryRoot })
        {
            if (Resolve(scope, path) is { } boundary)
            {
                return Stop(formation, boundary.Code, boundary.Status, path, boundary.Cause);
            }
        }

        var ownership = await _ownershipReader.ReadAsync(
            scope.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        var projectedOwnership = RouteRemoveOwnershipProjector.Project(ownership, scope);
        formation = formation with { Ownership = projectedOwnership };
        if (projectedOwnership.State == RouteRemoveOwnershipState.Interrupted)
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                scope.IntendedPath,
                ownership.Findings.FirstOrDefault()?.Cause
                    ?? "Lifecycle ownership inspection was interrupted.");
        }

        if (projectedOwnership.State == RouteRemoveOwnershipState.Blocked)
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.OwnershipUnavailable,
                CliSemanticStatus.Blocked,
                scope.IntendedPath,
                ownership.Findings.FirstOrDefault()?.Cause
                    ?? "Lifecycle ownership could not be established from one trusted snapshot.");
        }

        if (projectedOwnership.State == RouteRemoveOwnershipState.Claimed)
        {
            return Stop(
                formation,
                RouteRemoveFindingCode.OwnershipClaimed,
                CliSemanticStatus.Blocked,
                scope.IntendedPath,
                "Lifecycle ownership still claims the requested category boundary.");
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
