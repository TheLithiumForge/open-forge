using OpenForge.Cli.Core.Commands.Route.Shared.Templates;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Interaction;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update;

internal static class RouteUpdateOperationFactory
{
    internal static RouteUpdateOperation Create(
        WorkspaceLockStoreRoot? lockStoreRoot = null,
        CliPrompt<RouteUpdateSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
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
                physicalPathResolver,
                sourceSelectionPrompt),
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
        var preparer = new RouteUpdateApplicationPreparer(
            new RouteUpdatePlanRevalidator(
                planBuilder,
                new RouteUpdatePlanEquivalence()),
            mutationRevalidator);
        var applicationPipeline = new RouteUpdateApplicationPipeline(
            preparer,
            new RouteUpdateEffectApplication(
                new FileChangeApplier(
                    mutationRevalidator,
                    expectationValidator).ApplyAsync),
            new RouteUpdateAppliedVerifier(
                planBuilder,
                expectationValidator));
        return new RouteUpdateOperation(
            planBuilder,
            new RouteUpdateApplicationOperation(
                applicationPipeline,
                resultBuilder,
                lockStoreRoot),
            resultBuilder);
    }
}
