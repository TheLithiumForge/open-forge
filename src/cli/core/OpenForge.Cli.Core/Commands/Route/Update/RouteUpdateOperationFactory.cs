using OpenForge.Cli.Core.Commands.Route.Shared.Templates;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Update;

internal static class RouteUpdateOperationFactory
{
    internal static RouteUpdateOperation Create(
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        var yamlParser = new YamlDocumentParser();
        var metadataPatcher = new RouteUpdateMetadataPatcher(
            new RouteUpdateMetadataLayoutReader(),
            new RouteUpdateMetadataEditPlanner(
                new FrameworkDocumentMetadataEmitter(),
                yamlParser),
            new RouteUpdateMetadataByteEditor());
        var destinationPlanner = new RouteUpdateDestinationPlanner(
            new RouteTemplateResolver(),
            metadataPatcher,
            new RouteUpdateBodyPlanner());
        var physicalPathResolver = new PhysicalPathResolver();
        var targetObserver = new RouteUpdateTargetObserver(
            new RouteUpdateSourceSelector(
                new SourceCatalogueReader(),
                new SourceRouteFactsResolver(),
                physicalPathResolver),
            new RouteUpdateLayerObserver(new SourceDocumentSnapshotReader()),
            new MarkdownDocumentParser(),
            yamlParser);
        var planBuilder = new RouteUpdatePlanBuilder(
            targetObserver,
            destinationPlanner,
            new RouteUpdateNavigationPlanner(),
            new RouteUpdatePlanProjector());
        var expectationValidator = new FileExpectationValidator(
            physicalPathResolver);
        var mutationRevalidator = new MutationRevalidator(expectationValidator);
        var resultBuilder = new RouteUpdateResultBuilder();
        var recoveryReader = new RecoveryBundleReader();
        var recoveryCatalogue = new RecoveryBundleCatalogue(recoveryReader);
        var recoveryPreparer = new RouteUpdateRecoveryPreparer(
            recoveryCatalogue,
            new RecoveryBundleStore(recoveryReader));
        var recoveryCompleter = new RouteUpdateRecoveryCompleter(
            recoveryCatalogue,
            new RecoveryBundleDeletionGuard(recoveryCatalogue, recoveryReader));
        var preparer = new RouteUpdateApplicationPreparer(
            new RouteUpdatePlanRevalidator(
                planBuilder,
                new RouteUpdatePlanEquivalence()),
            recoveryPreparer,
            mutationRevalidator);
        var applicationPipeline = new RouteUpdateApplicationPipeline(
            preparer,
            new RouteUpdateEffectApplication(
                new FileChangeApplier(
                    mutationRevalidator,
                    expectationValidator)),
            new RouteUpdateAppliedVerifier(
                planBuilder,
                expectationValidator),
            recoveryCompleter);
        return new RouteUpdateOperation(
            planBuilder,
            new RouteUpdateApplicationOperation(
                applicationPipeline,
                resultBuilder,
                lockStoreRoot),
            resultBuilder);
    }
}
