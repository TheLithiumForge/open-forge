using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Remove;

internal static class RouteRemoveOperationFactory
{
    internal static RouteRemoveOperation Create(
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var expectationValidator = new FileExpectationValidator(physicalPathResolver);
        var planBuilder = CreatePlanBuilder(physicalPathResolver, expectationValidator);
        var resultBuilder = new RouteRemoveResultBuilder();
        var applicationOperation = CreateApplicationOperation(
            planBuilder,
            expectationValidator,
            lockStoreRoot);
        return new RouteRemoveOperation(planBuilder, applicationOperation, resultBuilder);
    }

    private static RouteRemovePlanBuilder CreatePlanBuilder(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator)
    {
        var referencePlanner = CreateReferencePlanner(
            physicalPathResolver,
            expectationValidator);
        var navigationPlanner = CreateNavigationPlanner();
        return new RouteRemovePlanBuilder(
            CreateSubjectResolver(physicalPathResolver, expectationValidator),
            CreateInventoryReader(physicalPathResolver, expectationValidator),
            referencePlanner,
            navigationPlanner,
            new RouteRemoveCategoryAbsencePlanner(
                new SourceCatalogueReader(),
                physicalPathResolver,
                new LifecycleOwnershipReader(physicalPathResolver),
                new RouteRemoveNavigationExposureReader(new MarkdownDocumentParser()),
                navigationPlanner,
                referencePlanner));
    }

    private static RouteRemoveSubjectResolver CreateSubjectResolver(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator)
        => new(
            new SourceCatalogueReader(),
            new SourceReferenceResolver((workspace, canonicalPath) =>
                physicalPathResolver.ResolveCandidate(
                    workspace.LexicalRoot,
                    workspace.PhysicalRoot,
                    Path.Combine(
                        workspace.LexicalRoot,
                        canonicalPath.Replace('/', Path.DirectorySeparatorChar)))),
            new SourceRouteFactsResolver(),
            new RouteRemoveNavigationExposureReader(new MarkdownDocumentParser()),
            expectationValidator);

    private static RouteRemoveCategoryInventoryReader CreateInventoryReader(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator)
        => new(
            physicalPathResolver,
            expectationValidator,
            new LifecycleOwnershipReader(physicalPathResolver));

    private static RouteRemoveReferencePlanner CreateReferencePlanner(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator)
    {
        var markdownParser = new MarkdownDocumentParser();
        var destinationResolver = new SourceLinkDestinationResolver(
            (workspace, lexicalPath) => physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalPath),
            StrictUtf8FileReader.ReadAsync,
            markdownParser.Parse);
        return new RouteRemoveReferencePlanner(
            new RouteMarkdownCatalogueReader(physicalPathResolver),
            markdownParser,
            destinationResolver,
            expectationValidator);
    }

    private static RouteRemoveNavigationPlanner CreateNavigationPlanner()
        => new(
            new GeneratedNavigationFormationBuilder(),
            new GeneratedNavigationRegionPlanner(),
            new RouteRemoveNavigationSourceProjector());

    private static RouteRemoveApplicationOperation CreateApplicationOperation(
        RouteRemovePlanBuilder planBuilder,
        FileExpectationValidator expectationValidator,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        var appliedVerifier = new RouteRemoveAppliedVerifier(
            new RouteRemovePostRemoveObserver(
                planBuilder,
                new RouteRemovePostRemoveVerifier()),
            expectationValidator);
        return new RouteRemoveApplicationOperation(
            CreateLockManager(lockStoreRoot),
            new RouteRemovePlanRevalidator(planBuilder),
            CreateEffectApplication(expectationValidator),
            new RouteRemoveApplicationCompletion(appliedVerifier));
    }

    private static RouteRemoveEffectApplication CreateEffectApplication(
        FileExpectationValidator expectationValidator)
    {
        var revalidator = new MutationRevalidator(expectationValidator);
        return new RouteRemoveEffectApplication(
            new FileChangeApplier(revalidator, expectationValidator),
            new DirectoryDeletionApplier(revalidator, expectationValidator),
            expectationValidator);
    }

    private static WorkspaceLockManager CreateLockManager(
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        if (lockStoreRoot is not null)
        {
            return new WorkspaceLockManager(lockStoreRoot);
        }

        return WorkspaceLockManager.CreateForCurrentUser();
    }
}
