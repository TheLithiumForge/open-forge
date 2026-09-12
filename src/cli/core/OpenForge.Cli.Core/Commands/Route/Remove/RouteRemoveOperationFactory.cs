using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Navigation;
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

    internal static RouteRemovePlanBuilder CreatePlanBuilder()
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
            new RouteNavigationExposureReader(markdown),
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
            new RouteNavigationExposureReader(markdown),
            navigation,
            references);
        return new RouteRemovePlanBuilder(subject, inventory, references, navigation, absence);
    }

    internal static RouteRemovePlanRevalidator CreatePlanRevalidator()
        => new(CreatePlanBuilder());

    internal static RouteRemoveEffectApplication CreateEffectApplication()
    {
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        return new RouteRemoveEffectApplication(
            new FileChangeApplier(revalidator, validator),
            new DirectoryDeletionApplier(revalidator, validator),
            validator);
    }

    internal static RouteRemoveAppliedVerifier CreateAppliedVerifier()
    {
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var planBuilder = CreatePlanBuilder();
        return new RouteRemoveAppliedVerifier(
            new RouteRemovePostRemoveObserver(
                planBuilder,
                new RouteRemovePostRemoveVerifier()),
            validator);
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
                new RouteNavigationExposureReader(new MarkdownDocumentParser()),
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
            new RouteNavigationExposureReader(new MarkdownDocumentParser()),
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
