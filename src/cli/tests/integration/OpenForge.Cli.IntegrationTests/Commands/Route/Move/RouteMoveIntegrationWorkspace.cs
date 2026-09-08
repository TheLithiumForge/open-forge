using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.References;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.Framework.Lifecycle;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

internal sealed class RouteMoveIntegrationWorkspace : IDisposable
{
    internal const string LeafId = "guidance/old guide";
    internal const string LeafPath = ".agents/guidance/old guide.md";
    internal const string OverwritePath = ".agents/guidance/old guide.overwrite.md";
    internal const string LeafDestination = ".agents/guidance/new guide.md";
    internal const string CrossRouteDestination = ".agents/archive/new guide.md";
    internal const string CategoryId = "guidance/topics";
    internal const string CategoryPath = ".agents/guidance/topics/_topics.md";
    internal const string CategoryDestination = ".agents/archive/topics/_topics.md";
    internal const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    internal const string ApplicationCategoryPath = ".agents/guidance/application/_application.md";
    internal const string ApplicationCategoryDestination = ".agents/archive/application/_application.md";

    private readonly TemporaryWorkspace temporary;
    private readonly WorkspaceLockTestStore lockStore;
    private readonly HashSet<string> recoveryPaths = new(StringComparer.Ordinal);
    private readonly HashSet<string> symbolicLinkPaths = new(StringComparer.Ordinal);

    private RouteMoveIntegrationWorkspace(
        TemporaryWorkspace temporary,
        WorkspaceLockTestStore lockStore)
    {
        this.temporary = temporary;
        this.lockStore = lockStore;
        Workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal CliWorkspace Workspace { get; }

    internal static RouteMoveIntegrationWorkspace Create(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            var workspace = new RouteMoveIntegrationWorkspace(temporary, lockStore);
            workspace.SeedOrdinaryWorkspace();
            return workspace;
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal RouteMoveRequest Request(
        string sourceReference = LeafId,
        string destinationTarget = LeafDestination,
        RouteMoveMode mode = RouteMoveMode.DryRun)
        => new(Workspace, sourceReference, destinationTarget, mode);

    internal static RouteMovePlanBuilder CreatePlanBuilder()
        => CreatePlanBuilderStatic();

    private static RouteMovePlanBuilder CreatePlanBuilderStatic()
    {
        var physical = new PhysicalPathResolver();
        var expectation = new FileExpectationValidator(physical);
        var catalogue = new RouteMarkdownCatalogueReader(physical);
        var markdown = new MarkdownDocumentParser();
        var sourceDestination = new SourceLinkDestinationResolver(
            (workspace, lexicalPath) => physical.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalPath),
            StrictUtf8FileReader.ReadAsync,
            markdown.Parse);
        var ownership = new LifecycleOwnershipReader(physical);
        return new RouteMovePlanBuilder(
            new RouteMoveSubjectResolver(
                new OpenForge.Cli.Core.Framework.Sources.Inventory.SourceCatalogueReader(),
                new SourceReferenceResolver((workspace, canonicalPath) =>
                    physical.ResolveCandidate(
                        workspace.LexicalRoot,
                        workspace.PhysicalRoot,
                        Path.Combine(
                            workspace.LexicalRoot,
                            canonicalPath.Replace('/', Path.DirectorySeparatorChar)))),
                new SourceRouteFactsResolver(),
                new RouteMoveNavigationExposureReader(new MarkdownDocumentParser()),
                expectation),
            new RouteMoveCategoryInventoryReader(physical, expectation, ownership),
            new RouteMoveDestinationResolver(expectation),
            new RouteMoveReferencePlanner(catalogue, markdown, sourceDestination, expectation),
            new RouteMoveNavigationPlanner(
                new GeneratedNavigationFormationBuilder(),
                new GeneratedNavigationRegionPlanner(),
                new RouteMoveNavigationSourceProjector()));
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => temporary.SnapshotHashes();

    internal string Absolute(string relativePath) => temporary.Combine(relativePath);

    internal string ReadText(string relativePath)
        => File.ReadAllText(Absolute(relativePath), Encoding.UTF8);

    internal void WriteText(string relativePath, string contents)
    {
        if (File.Exists(Absolute(relativePath)))
        {
            temporary.ReplaceText(relativePath, contents);
        }
        else
        {
            temporary.CreateFile(relativePath, contents);
        }
    }

    internal void DeleteFile(string relativePath)
    {
        var path = Absolute(relativePath);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    internal void SeedScenario(string scenario)
    {
        switch (scenario)
        {
            case "leaf-id":
            case "leaf-base-path":
            case "leaf-overwrite-path":
            case "same-parent":
            case "cross-route":
            case "dry-run":
            case "apply":
            case "incoming-reference":
            case "outgoing-reference":
            case "unicode-definition-fragment":
            case "external-uri":
                return;
            case "category":
                return;
            case "compatibility-category":
                MoveCategoryEntrypointToCompatibilityName();
                return;
            case "loader":
            case "workspace-root":
            case "native-source":
            case "resource-source":
            case "unsupported-source":
                return;
            case "orphan-overwrite":
                DeleteFile(LeafPath);
                return;
            case "ambiguous-route":
                WriteText(".agents/guidance/index.md", Entrypoint("Guidance compatibility", "- none - No entries - #Empty"));
                return;
            case "identity-collision":
                WriteText(".agents/guidance/old guide/_old guide.md", Entrypoint("Collision", "- none - No entries - #Empty"));
                return;
            case "missing-parent":
            case "id-like-destination":
            case "invalid-leaf-extension":
            case "invalid-category-form":
                return;
            case "occupied-destination":
                WriteText(LeafDestination, Markdown("Occupied", "# Occupied\n"));
                return;
            case "self-move":
            case "inside-source":
                return;
            case "overwrite-collision":
                WriteText(".agents/guidance/new guide.overwrite.md", "collision overwrite\n");
                return;
            case "ownership-claim":
                SeedLifecycleClaim(LeafPath);
                return;
            case "unrelated-ownership-claim":
                SeedLifecycleClaim(".agents/archive/_archive.md");
                return;
            case "ownership-malformed":
                WriteText(LifecyclePath, "{ malformed lifecycle");
                return;
            case "ownership-stale":
                WriteText(
                    LifecyclePath,
                    ReadText(LifecyclePath).Replace(
                        temporary.Path,
                        $"{temporary.Path}-stale",
                        StringComparison.Ordinal));
                return;
            case "ownership-incomplete":
                WriteText(
                    LifecyclePath,
                    ReadText(LifecyclePath).Replace(
                        "\"coverage\":\"complete\"",
                        "\"coverage\":\"partial\"",
                        StringComparison.Ordinal));
                return;
            case "ownership-conflicting":
                SeedLifecycleConflict(LeafPath);
                return;
            case "unsupported-reference":
                WriteText("README.md", "[old guide][target]\n\n[target]: .agents/guidance/old guide.md\n");
                return;
            case "unsafe-reference":
                CreateSymbolicLink("alias.md", "README.md");
                return;
            case "unsafe-generated-region":
                WriteText(".agents/guidance/_guidance.md", Markdown("Guidance", "## Entries\n\n<!-- open-forge:generated-index:start -->\n"));
                return;
            case "invalid-utf8":
                temporary.CreateFile("invalid.md", [0xff, 0xfe, 0xfd]);
                return;
            case "aliased-destination":
                WriteText("alias-target.md", Markdown("Alias target", "# Alias target\n"));
                CreateSymbolicLink(LeafDestination, "alias-target.md");
                return;
            case "category-unsafe-alias":
                CreateSymbolicLink(
                    ".agents/guidance/topics/unsafe-link.md",
                    "README.md");
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown Route Move test scenario.");
        }
    }

    internal async ValueTask<RouteMovePlan> BuildApplicationPlanAsync()
    {
        SeedApplicationCategory();
        var request = new RouteMoveRequest(
            Workspace,
            "guidance/application",
            ApplicationCategoryDestination,
            RouteMoveMode.Apply);
        var build = await CreatePlanBuilder().BuildAsync(
            request,
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteMovePlan>(build.Plan);
        AssertApplicationEffects(plan);
        return plan;
    }

    internal async ValueTask<WorkspaceLockLease> AcquireLeaseAsync(Guid operationId)
    {
        var result = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(
                Workspace,
                RouteMoveDefinitions.CommandIdentity,
                operationId),
            TestContext.Current.CancellationToken);
        return result.Lease
            ?? throw new InvalidOperationException(
                result.Cause ?? "The Route Move test lock was not acquired.");
    }

    internal void TrackRecovery(RecoveryBundlePreparation preparation)
        => recoveryPaths.Add(preparation.BundlePath);

    internal void ReplaceRecoveryWithDirectory(RecoveryBundlePreparation preparation)
    {
        File.Delete(preparation.BundlePath);
        Directory.CreateDirectory(preparation.BundlePath);
        recoveryPaths.Add(preparation.BundlePath);
    }

    internal string BlockRecoveryWorkspaceDirectory()
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.Create)
            ?? throw new InvalidOperationException(
                "The Route Move recovery-unavailability test requires LocalApplicationData.");
        Directory.CreateDirectory(storeRoot);
        var workspaceDirectory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            storeRoot,
            Workspace.PhysicalRoot);
        File.WriteAllText(workspaceDirectory, "block recovery directory creation\n", Encoding.UTF8);
        recoveryPaths.Add(workspaceDirectory);
        return workspaceDirectory;
    }

    internal static RouteMoveEffectApplication CreateEffectApplication()
    {
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var revalidator = new MutationRevalidator(validator);
        return new RouteMoveEffectApplication(
            new DirectoryCreationApplier(revalidator, validator),
            new FileChangeApplier(revalidator, validator),
            new DirectoryDeletionApplier(revalidator, validator),
            validator);
    }

    internal static RouteMoveAppliedVerifier CreateAppliedVerifier()
    {
        var physical = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physical);
        var markdown = new MarkdownDocumentParser();
        var destination = new SourceLinkDestinationResolver(
            (workspace, lexicalPath) => physical.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalPath),
            StrictUtf8FileReader.ReadAsync,
            markdown.Parse);
        return new RouteMoveAppliedVerifier(
            new RouteMovePostMoveObserver(
                new RouteMoveSubjectResolver(
                    new SourceCatalogueReader(),
                    new SourceReferenceResolver((workspace, canonicalPath) =>
                        physical.ResolveCandidate(
                            workspace.LexicalRoot,
                            workspace.PhysicalRoot,
                            Path.Combine(
                                workspace.LexicalRoot,
                                canonicalPath.Replace('/', Path.DirectorySeparatorChar)))),
                    new SourceRouteFactsResolver(),
                    new RouteMoveNavigationExposureReader(markdown),
                    validator),
                new RouteMoveCategoryInventoryReader(
                    physical,
                    validator,
                    new LifecycleOwnershipReader(physical)),
                new RouteMoveReferencePlanner(
                    new RouteMarkdownCatalogueReader(physical),
                    markdown,
                    destination,
                    validator),
                new RouteMoveNavigationPlanner(
                    new GeneratedNavigationFormationBuilder(),
                    new GeneratedNavigationRegionPlanner(),
                    new RouteMoveNavigationSourceProjector()),
                validator),
            validator);
    }

    public void Dispose()
    {
        foreach (var path in symbolicLinkPaths)
        {
            File.Delete(path);
        }

        foreach (var path in recoveryPaths)
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

        DeleteApplicationDestination();
        lockStore.Dispose();
        temporary.Dispose();
    }

    private void DeleteApplicationDestination()
    {
        var destination = Absolute(ApplicationCategoryDestination);
        if (File.Exists(destination))
        {
            var attributes = File.GetAttributes(destination);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException(
                    "The Route Move application destination is not an ordinary file.");
            }

            File.Delete(destination);
        }

        var parent = Path.GetDirectoryName(destination)
            ?? throw new InvalidOperationException(
                "The Route Move application destination requires its parent directory.");
        if (Directory.Exists(parent))
        {
            var attributes = File.GetAttributes(parent);
            if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException(
                    "The Route Move application destination parent is not an ordinary directory.");
            }

            Directory.Delete(parent, recursive: false);
        }
    }

    private void CreateSymbolicLink(
        string relativePath,
        string targetRelativePath)
    {
        var path = Absolute(relativePath);
        File.CreateSymbolicLink(path, Absolute(targetRelativePath));
        symbolicLinkPaths.Add(path);
    }

    private void SeedApplicationCategory()
    {
        if (!File.Exists(Absolute(ApplicationCategoryPath)))
        {
            WriteText(
                ApplicationCategoryPath,
                Entrypoint("Application", "- none - No entries - #Empty"));
        }

        const string applicationEntry =
            "- [Application](application/_application.md) - #Route\n";
        var guidance = ReadText(".agents/guidance/_guidance.md");
        if (!guidance.Contains(applicationEntry, StringComparison.Ordinal))
        {
            WriteText(
                ".agents/guidance/_guidance.md",
                guidance.Replace(
                    "- [Old guide](old%20guide.md) - #Guide\n",
                    applicationEntry + "- [Old guide](old%20guide.md) - #Guide\n",
                    StringComparison.Ordinal));
        }
    }

    private static void AssertApplicationEffects(RouteMovePlan plan)
        => Assert.Collection(
            plan.Preview.Effects,
            effect => AssertApplicationEffect(
                effect,
                ".agents/archive/application",
                RouteMoveEffectKind.Directory,
                RouteMoveEffectAction.Create),
            effect => AssertApplicationEffect(
                effect,
                ApplicationCategoryDestination,
                RouteMoveEffectKind.MovedFile,
                RouteMoveEffectAction.Create),
            effect => AssertApplicationEffect(
                effect,
                ".agents/archive/_archive.md",
                RouteMoveEffectKind.GeneratedRegion,
                RouteMoveEffectAction.Replace),
            effect => AssertApplicationEffect(
                effect,
                ".agents/guidance/_guidance.md",
                RouteMoveEffectKind.GeneratedRegion,
                RouteMoveEffectAction.Replace),
            effect => AssertApplicationEffect(
                effect,
                ApplicationCategoryPath,
                RouteMoveEffectKind.MovedFile,
                RouteMoveEffectAction.Delete),
            effect => AssertApplicationEffect(
                effect,
                ".agents/guidance/application",
                RouteMoveEffectKind.Directory,
                RouteMoveEffectAction.Delete));

    private static void AssertApplicationEffect(
        RouteMoveEffect effect,
        string path,
        RouteMoveEffectKind kind,
        RouteMoveEffectAction action)
    {
        Assert.Equal(path, effect.Path);
        Assert.Equal(kind, effect.Kind);
        Assert.Equal(action, effect.Action);
    }

    private void SeedOrdinaryWorkspace()
    {
        WriteText(
            ".agents/loader.md",
            Entrypoint("Loader", "- [Archive](archive/_archive.md) - #Route\n- [Guidance](guidance/_guidance.md) - #Route"));
        WriteText(
            ".agents/guidance/_guidance.md",
            Entrypoint(
                "Guidance",
                "- [Old guide](old%20guide.md) - #Guide\n- [Topics](topics/_topics.md) - #Topic"));
        WriteText(".agents/archive/_archive.md", Entrypoint("Archive", "- none - No entries - #Empty"));
        WriteText(
            LeafPath,
            Markdown(
                "Old guide",
                "# Old guide\n\n[Archive](../archive/_archive.md#entries)\n\n[External](https://example.com/a?b=c#d)\n"));
        WriteText(OverwritePath, "Local overwrite with [archive](../archive/_archive.md).\n");
        WriteText(
            CategoryPath,
            Entrypoint("Topics", "- [Child](child.md) - #Child"));
        WriteText(
            ".agents/guidance/topics/child.md",
            Markdown("Child", "# Child\n\n[Old guide](../old%20guide.md#section).\n"));
        WriteText(
            ".agents/guidance/topics/child.overwrite.md",
            "Child overwrite keeps [old guide](../old%20guide.md#section).\n");
        WriteText(
            ".agents/guidance/topics/notes.md",
            Markdown("Unrouted notes", "# Notes\n\nNot exposed by generated navigation.\n"));
        WriteText(
            ".agents/guidance/topics/native/SKILL.md",
            Markdown("Native skill", "# Native skill\n"));
        temporary.CreateFile(".agents/guidance/topics/image.bin", [0x00, 0x01, 0x02, 0xff]);
        WriteText(".agents/guidance/topics/assets/settings.json", "{\"enabled\":true}\n");
        WriteText(
            "README.md",
            "# Outside catalogue\n\n[Old guide](.agents/guidance/old%20guide.md#section)\n\n[Unicode destination](<.agents/guidance/old%20guide.md#caf%C3%A9>)\n");
        WriteText(
            "notes.md",
            "[old guide][target] and [again][target]\n\n[target]: .agents/guidance/old%20guide.md#section\n");
        SeedLifecycleClaim(path: null);
    }

    private void SeedLifecycleClaim(string? path)
    {
        var framework = LifecycleStoreIntegrationDocuments.Framework();
        var extensions = path is null
            ? LifecycleStoreIntegrationDocuments.EmptyExtensions()
            : LifecycleStoreIntegrationDocuments.Extensions(path);
        var envelope = LifecycleStoreIntegrationDocuments.Envelope(
            temporary,
            framework,
            extensions);
        var bytes = LifecycleStoreIntegrationDocuments.Serialize(envelope);
        if (File.Exists(Absolute(LifecyclePath)))
        {
            temporary.ReplaceBytes(LifecyclePath, bytes);
        }
        else
        {
            temporary.CreateFile(LifecyclePath, bytes);
        }
    }

    private void SeedLifecycleConflict(string path)
    {
        var envelope = LifecycleStoreIntegrationDocuments.Envelope(
            temporary,
            LifecycleStoreIntegrationDocuments.Framework(path),
            LifecycleStoreIntegrationDocuments.Extensions(path));
        temporary.ReplaceBytes(
            LifecyclePath,
            LifecycleStoreIntegrationDocuments.Serialize(envelope));
    }

    private void MoveCategoryEntrypointToCompatibilityName()
    {
        temporary.MoveFile(CategoryPath, ".agents/guidance/topics/index.md");
    }

    private static string Markdown(string description, string body)
        => OpenForgeDocumentSeed.Metadata(description, ["Route"], body);

    private static string Entrypoint(string description, string entries)
        => OpenForgeDocumentSeed.Metadata(
            description,
            ["Route"],
            $"# {description}\n\n{OpenForgeDocumentSeed.GeneratedEntries(entries)}");
}
