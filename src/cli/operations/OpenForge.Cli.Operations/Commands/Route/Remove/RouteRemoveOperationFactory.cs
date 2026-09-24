using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Result;
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

namespace OpenForge.Cli.Core.Commands.Route.Remove;

internal static class RouteRemoveOperationFactory
{
    internal static RouteRemoveOperation Create(
        WorkspaceLockStoreRoot? lockStoreRoot = null,
        CliPrompt<RouteRemoveSourceSelectionQuestion, string>? sourceSelectionPrompt = null,
        CliPlanConfirmation<RouteRemoveResult, RouteRemoveConfirmationQuestion>? confirmation = null)
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var expectationValidator = new FileExpectationValidator(physicalPathResolver);
        var planBuilder = CreatePlanBuilder(physicalPathResolver, expectationValidator, sourceSelectionPrompt);
        var resultBuilder = new RouteRemoveResultBuilder();
        var applicationOperation = CreateApplicationOperation(
            planBuilder,
            expectationValidator,
            lockStoreRoot);
        return new RouteRemoveOperation(planBuilder, applicationOperation, resultBuilder, confirmation);
    }

    internal static RouteRemovePlanBuilder CreatePlanBuilder(
        CliPrompt<RouteRemoveSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
    {
        var physical = new PhysicalPathResolver();
        var expectation = new FileExpectationValidator(physical);
        var markdown = new MarkdownDocumentParser();
        var subject = new RouteRemoveSubjectResolver(
            new RouteRemoveSubjectSelector(
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
                sourceSelectionPrompt),
            expectation);
        var inventory = new RouteRemoveCategoryInventoryReader(
            physical,
            expectation);
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
            validator,
            resolver);
    }

    internal static RouteRemoveAppliedVerifier CreateAppliedVerifier()
    {
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var planBuilder = CreatePlanBuilder();
        return new RouteRemoveAppliedVerifier(
            new RouteRemovePostRemoveObserver(
                planBuilder,
                new RouteRemovePostRemoveVerifier()),
            validator,
            new PhysicalPathResolver());
    }

    internal static RouteRemoveApplicationCompletion CreateApplicationCompletion()
    {
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var planBuilder = CreatePlanBuilder();
        return new RouteRemoveApplicationCompletion(
            new RouteRemoveAppliedVerifier(
                new RouteRemovePostRemoveObserver(
                    planBuilder,
                    new RouteRemovePostRemoveVerifier()),
                validator,
                resolver),
            CreateOwnershipPublisher(validator));
    }

    private static RouteRemovePlanBuilder CreatePlanBuilder(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator,
        CliPrompt<RouteRemoveSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
    {
        var referencePlanner = CreateReferencePlanner(
            physicalPathResolver,
            expectationValidator);
        var navigationPlanner = CreateNavigationPlanner();
        return new RouteRemovePlanBuilder(
            CreateSubjectResolver(physicalPathResolver, expectationValidator, sourceSelectionPrompt),
            CreateInventoryReader(physicalPathResolver, expectationValidator),
            referencePlanner,
            navigationPlanner,
            new RouteRemoveCategoryAbsencePlanner(
                new SourceCatalogueReader(),
                physicalPathResolver,
                new RouteNavigationExposureReader(new MarkdownDocumentParser()),
                navigationPlanner,
                referencePlanner));
    }

    private static RouteRemoveSubjectResolver CreateSubjectResolver(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator,
        CliPrompt<RouteRemoveSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
        => new(
            new RouteRemoveSubjectSelector(
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

    private static RouteRemoveCategoryInventoryReader CreateInventoryReader(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator expectationValidator)
        => new(
            physicalPathResolver,
            expectationValidator);

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
            expectationValidator,
            new PhysicalPathResolver());
        return new RouteRemoveApplicationOperation(
            CreateLockManager(lockStoreRoot),
            new RouteRemovePlanRevalidator(planBuilder),
            CreateEffectApplication(expectationValidator),
            new RouteRemoveApplicationCompletion(
                appliedVerifier,
                CreateOwnershipPublisher(expectationValidator)));
    }

    private static RouteRemoveOwnershipPublisher CreateOwnershipPublisher(
        FileExpectationValidator expectationValidator)
        => new(
            new FileChangeApplier(
                new MutationRevalidator(expectationValidator),
                expectationValidator),
            expectationValidator);

    private static RouteRemoveEffectApplication CreateEffectApplication(
        FileExpectationValidator expectationValidator)
    {
        var revalidator = new MutationRevalidator(expectationValidator);
        return new RouteRemoveEffectApplication(
            new FileChangeApplier(revalidator, expectationValidator),
            new DirectoryDeletionApplier(revalidator, expectationValidator),
            expectationValidator,
            new PhysicalPathResolver());
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
