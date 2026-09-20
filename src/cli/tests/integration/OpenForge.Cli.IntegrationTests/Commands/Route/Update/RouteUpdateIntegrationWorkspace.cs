using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

internal sealed partial class RouteUpdateIntegrationWorkspace : IDisposable
{
    internal const string ParentId = "memory/project-alpha";
    internal const string ParentPath = ".agents/memory/project-alpha/_project-alpha.md";
    internal const string TargetId = "memory/project-alpha/overview";
    internal const string TargetPath = ".agents/memory/project-alpha/overview.md";
    internal const string OverwritePath = ".agents/memory/project-alpha/overview.overwrite.md";
    internal const string OverwriteText = "overwrite bytes must remain untouched\n";
    internal const string TemplateId = "templates/route";
    internal const string TemplatePath = ".agents/templates/route.md";
    internal const string TemplateBody = "# Exact Template\n\nKeep {tokens} exactly.\n";

    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;
    private readonly HashSet<string> _recoveryPaths = new(StringComparer.Ordinal);
    private bool _disposed;

    private RouteUpdateIntegrationWorkspace(
        TemporaryWorkspace temporary,
        WorkspaceLockTestStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _lockStore.Track(Workspace);
    }

    internal CliWorkspace Workspace { get; }

    internal WorkspaceLockStoreRoot LockStoreRoot => _lockStore.StoreRoot;

    internal FileStream HoldLock() => _lockStore.OpenExclusive(Workspace);

    internal void TrackRecoveryPath(string path) => _recoveryPaths.Add(path);

    internal static RouteUpdateIntegrationWorkspace Create(string purpose)
        => Create(purpose, seedRoute: true);

    internal static RouteUpdateIntegrationWorkspace CreateUnseeded(string purpose)
        => Create(purpose, seedRoute: false);

    private static RouteUpdateIntegrationWorkspace Create(
        string purpose,
        bool seedRoute)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            var workspace = new RouteUpdateIntegrationWorkspace(temporary, lockStore);
            if (seedRoute)
            {
                workspace.SeedOrdinaryRoute();
            }

            return workspace;
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal RouteUpdateRequest Request(
        string sourceReference = TargetId,
        RouteUpdatePatchRequest? patch = null,
        string? templateReference = null,
        RouteUpdateMode mode = RouteUpdateMode.Apply)
        => new(
            Workspace,
            sourceReference,
            patch ?? DescriptionPatch("After overview"),
            templateReference,
            mode);

    internal static RouteUpdatePatchRequest DescriptionPatch(string value)
        => Patch(description: new RouteUpdateDescriptionRequest
        {
            Requested = true,
            Value = value,
        });

    internal static RouteUpdatePatchRequest ResponsibilityPatch(string? value)
        => Patch(responsibility: new RouteUpdateResponsibilityRequest
        {
            Operation = value is null
                ? RouteUpdateResponsibilityOperation.Remove
                : RouteUpdateResponsibilityOperation.Set,
            Value = value,
        });

    internal static RouteUpdatePatchRequest TagsPatch(params string[] values)
        => Patch(tags: new RouteUpdateTagsRequest
        {
            Requested = true,
            Values = [.. values],
        });

    internal static RouteUpdatePatchRequest Patch(
        RouteUpdateDescriptionRequest? description = null,
        RouteUpdateResponsibilityRequest? responsibility = null,
        RouteUpdateTagsRequest? tags = null)
        => new()
        {
            Description = description ?? new RouteUpdateDescriptionRequest
            {
                Requested = false,
                Value = null,
            },
            Responsibility = responsibility ?? new RouteUpdateResponsibilityRequest
            {
                Operation = RouteUpdateResponsibilityOperation.NotRequested,
                Value = null,
            },
            Tags = tags ?? new RouteUpdateTagsRequest
            {
                Requested = false,
                Values = [],
            },
        };

    internal async ValueTask<SourceCatalogue> ReadCatalogueAsync()
        => await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(Workspace, [SourceLogicalPath.AgentsRoot]),
            TestContext.Current.CancellationToken);

    internal ValueTask<RouteUpdateObservationBuild> ObserveAsync(string reference)
        => CreateTargetObserver().ObserveAsync(
            Request(reference),
            TestContext.Current.CancellationToken);

    internal static ValueTask<RouteUpdatePlanBuild> BuildPlanAsync(RouteUpdateRequest request)
        => CreatePlanBuilder().BuildAsync(
            request,
            TestContext.Current.CancellationToken);

    internal ValueTask<RouteUpdateResult> ExecuteAsync(RouteUpdateRequest request)
        => ExecuteAsync(request, TestContext.Current.CancellationToken);

    internal ValueTask<RouteUpdateResult> ExecuteAsync(
        RouteUpdateRequest request,
        CancellationToken cancellationToken)
        => RouteUpdateOperationFactory.Create(LockStoreRoot).ExecuteAsync(
            request,
            cancellationToken);

    internal ValueTask<RouteUpdateResult> ExecutePlanAsync(RouteUpdatePlan plan)
    {
        var resultBuilder = new RouteUpdateResultBuilder();
        var expectationValidator = new FileExpectationValidator(
            new PhysicalPathResolver());
        var mutationRevalidator = new MutationRevalidator(expectationValidator);
        var planBuilder = CreatePlanBuilder();
        var preparer = new RouteUpdateApplicationPreparer(
            new RouteUpdatePlanRevalidator(
                planBuilder,
                new RouteUpdatePlanEquivalence()),
            mutationRevalidator);
        return new RouteUpdateApplicationOperation(
            new RouteUpdateApplicationPipeline(
                preparer,
                new RouteUpdateEffectApplication(
                    new FileChangeApplier(
                        mutationRevalidator,
                        expectationValidator).ApplyAsync),
                new RouteUpdateAppliedVerifier(
                    planBuilder,
                    expectationValidator)),
            resultBuilder,
            LockStoreRoot).ExecuteAsync(
                plan,
                TestContext.Current.CancellationToken);
    }

    internal async ValueTask<RouteTemplateResolution> ResolveTemplateAsync(string reference)
        => await new RouteTemplateResolver().ResolveAsync(
            new RouteTemplateResolutionRequest
            {
                Workspace = Workspace,
                Reference = reference,
                Catalogue = await ReadCatalogueAsync(),
            },
            TestContext.Current.CancellationToken);

    internal string ReadText(string path) => File.ReadAllText(Absolute(path));

    internal byte[] ReadBytes(string path) => File.ReadAllBytes(Absolute(path));

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => _temporary.SnapshotHashes();

    internal string Absolute(string path) => _temporary.Combine(path);

    internal void TrackRecovery(RecoveryBundlePreparation preparation)
        => _recoveryPaths.Add(preparation.BundlePath);

    internal void ReplaceRecoveryWithDirectory(RecoveryBundlePreparation preparation)
    {
        File.Delete(preparation.BundlePath);
        Directory.CreateDirectory(preparation.BundlePath);
        _recoveryPaths.Add(preparation.BundlePath);
    }

    internal async ValueTask<WorkspaceLockLease> AcquireLeaseAsync(Guid operationId)
    {
        var acquisition = await _lockStore.AcquireAsync(
            new WorkspaceLockRequest(
                Workspace,
                RouteUpdateDefinitions.CommandIdentity,
                operationId),
            TestContext.Current.CancellationToken);
        return acquisition.Lease
            ?? throw new InvalidOperationException(
                acquisition.Cause ?? "The Route Update test lock was not acquired.");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var path in _recoveryPaths)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }
        }

        _lockStore.Dispose();
        _temporary.Dispose();
        _disposed = true;
    }

    internal static RouteUpdatePlanBuilder CreatePlanBuilder()
    {
        var yamlParser = new YamlDocumentParser();
        return new RouteUpdatePlanBuilder(
            CreateTargetObserver(yamlParser),
            new RouteUpdateDestinationPlanner(
                new RouteTemplateResolver(),
                CreateMetadataPatcher(yamlParser),
                new RouteUpdateBodyPlanner()),
            new RouteUpdateNavigationPlanner(),
            new RouteUpdatePlanProjector());
    }

    private static RouteUpdateTargetObserver CreateTargetObserver(
        YamlDocumentParser? yamlParser = null)
    {
        var selectedYamlParser = yamlParser ?? new YamlDocumentParser();
        return new RouteUpdateTargetObserver(
            new RouteUpdateSourceSelector(
                new SourceCatalogueReader(),
                new SourceRouteFactsResolver(),
                new PhysicalPathResolver()),
            new RouteUpdateLayerObserver(new SourceDocumentSnapshotReader()),
            new MarkdownDocumentParser(),
            selectedYamlParser);
    }

    private static RouteUpdateMetadataPatcher CreateMetadataPatcher(
        YamlDocumentParser yamlParser)
        => new(
            new RouteUpdateMetadataLayoutReader(),
            new RouteUpdateMetadataEditPlanner(
                new FrameworkDocumentMetadataEmitter(),
                yamlParser),
            new RouteUpdateMetadataByteEditor());
}
