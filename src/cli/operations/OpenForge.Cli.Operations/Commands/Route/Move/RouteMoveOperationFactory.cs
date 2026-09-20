using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.References;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Interaction;
using OpenForge.Cli.Core.Commands.Route.Shared.Navigation;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move;

internal static class RouteMoveOperationFactory
{
    internal static RouteMoveOperation Create(
        WorkspaceLockStoreRoot? lockStoreRoot = null,
        CliPrompt<RouteMoveSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var expectationValidator = new FileExpectationValidator(physicalPathResolver);
        var planBuilder = CreatePlanBuilder(physicalPathResolver, expectationValidator, sourceSelectionPrompt);
        var resultBuilder = new RouteMoveResultBuilder();
        var applicationOperation = CreateApplicationOperation(
            planBuilder,
            physicalPathResolver,
            expectationValidator,
            lockStoreRoot);
        return new RouteMoveOperation(planBuilder, applicationOperation, resultBuilder);
    }

    private static RouteMovePlanBuilder CreatePlanBuilder(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator,
        CliPrompt<RouteMoveSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
        => new(
            CreateSubjectResolver(physicalPathResolver, expectationValidator, sourceSelectionPrompt),
            CreateInventoryReader(physicalPathResolver, expectationValidator),
            new RouteMoveDestinationResolver(expectationValidator),
            CreateReferencePlanner(physicalPathResolver, expectationValidator),
            CreateNavigationPlanner());

    private static RouteMoveSubjectResolver CreateSubjectResolver(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator,
        CliPrompt<RouteMoveSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
        => new(
            new RouteMoveSubjectSelector(
                new SourceCatalogueReader(),
                new SourceReferenceResolver((workspace, canonicalPath) =>
                    physicalPathResolver.ResolveCandidate(
                        workspace.LexicalRoot,
                        workspace.PhysicalRoot,
                        Path.Combine(
                            workspace.LexicalRoot,
                            canonicalPath.Replace('/', Path.DirectorySeparatorChar)))),
                new SourceRouteFactsResolver(),
                new RouteNavigationExposureReader(new MarkdownDocumentParser()),
                sourceSelectionPrompt),
            expectationValidator);

    private static RouteMoveCategoryInventoryReader CreateInventoryReader(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator)
        => new(
            physicalPathResolver,
            expectationValidator);

    private static RouteMoveReferencePlanner CreateReferencePlanner(
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
        return new RouteMoveReferencePlanner(
            new RouteMarkdownCatalogueReader(physicalPathResolver),
            markdownParser,
            destinationResolver,
            expectationValidator);
    }

    private static RouteMoveNavigationPlanner CreateNavigationPlanner()
        => new(
            new GeneratedNavigationFormationBuilder(),
            new GeneratedNavigationRegionPlanner(),
            new RouteMoveNavigationSourceProjector());

    private static RouteMoveApplicationOperation CreateApplicationOperation(
        RouteMovePlanBuilder planBuilder,
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        var appliedVerifier = new RouteMoveAppliedVerifier(
            CreatePostMoveObserver(physicalPathResolver, expectationValidator),
            expectationValidator);
        return new RouteMoveApplicationOperation(
            CreateLockManager(lockStoreRoot),
            new RouteMovePlanRevalidator(planBuilder),
            CreateEffectApplication(expectationValidator),
            new RouteMoveApplicationCompletion(appliedVerifier));
    }

    private static RouteMovePostMoveObserver CreatePostMoveObserver(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator)
        => new(
            CreateSubjectResolver(physicalPathResolver, expectationValidator),
            CreateInventoryReader(physicalPathResolver, expectationValidator),
            CreateReferencePlanner(physicalPathResolver, expectationValidator),
            CreateNavigationPlanner(),
            expectationValidator);

    private static RouteMoveEffectApplication CreateEffectApplication(
        FileExpectationValidator expectationValidator)
    {
        var revalidator = new MutationRevalidator(expectationValidator);
        return new RouteMoveEffectApplication(
            new DirectoryCreationApplier(revalidator, expectationValidator),
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
