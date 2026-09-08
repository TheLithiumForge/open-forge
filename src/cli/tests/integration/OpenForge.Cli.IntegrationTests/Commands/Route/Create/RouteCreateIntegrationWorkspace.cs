using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

internal sealed class RouteCreateIntegrationWorkspace : IDisposable
{
    internal const string ParentId = "memory/project-alpha";
    internal const string ParentPath = ".agents/memory/project-alpha/_project-alpha.md";
    internal const string TargetId = "memory/project-alpha/overview";
    internal const string TargetPath = ".agents/memory/project-alpha/overview.md";
    internal const string TemplateId = "templates/route";
    internal const string TemplatePath = ".agents/templates/route.md";
    internal const string TemplateBody = "# Starting point\n\nDescribe {topic}.\n";
    private const string LoaderEntries = """
        - [Project Alpha](memory/project-alpha/_project-alpha.md) - #Project
        - [Templates](templates/_templates.md) - #Template
        """;
    private const string MalformedTemplateDocument = """
        ---
        open-forge:
          description: [
        ---
        # Invalid Template
        """;

    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;
    private readonly HashSet<string> _recoveryPaths = new(StringComparer.Ordinal);
    private string? _applicationCreatedTargetPath;
    private bool _disposed;

    private RouteCreateIntegrationWorkspace(
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

    internal static RouteCreateIntegrationWorkspace Create(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            return new RouteCreateIntegrationWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal RouteCreateRequest Request(
        RouteCreateMode mode = RouteCreateMode.Apply,
        string? templateReference = null)
        => new(
            workspace: Workspace,
            fileTarget: TargetId,
            metadata: new RouteCreateMetadataInput(
                description: "Project overview",
                tags: ["Docs", "Overview"],
                responsibility: "Explains the project"),
            templateReference: templateReference,
            mode: mode);

    internal void SeedBase()
    {
        WriteText(
            ".agents/loader.md",
            GeneratedLoaderDocumentBuilder.Build(LoaderEntries));
        WriteText(ParentPath, ParentDocument(entries: "- none - No entries - #Empty"));
        WriteText(
            ".agents/templates/_templates.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Templates",
                tags: ["Template"],
                body: $"\n{OpenForgeDocumentSeed.GeneratedEntries("- [Route Template](route.md) - #Template")}"));
    }

    internal void SeedTemplate()
        => WriteText(
            TemplatePath,
            OpenForgeDocumentSeed.Metadata(
                description: "Route Template",
                tags: ["Template"],
                body: TemplateBody));

    internal void SeedMalformedTemplate()
        => WriteText(TemplatePath, MalformedTemplateDocument);

    internal void SeedCompleteTarget()
    {
        WriteBytes(TargetPath, [.. TargetBytes()]);
        _temporary.ReplaceBytes(ParentPath, [.. ParentIntendedBytes()]);
    }

    internal void SeedDifferingTarget()
        => WriteText(
            TargetPath,
            OpenForgeDocumentSeed.Metadata(
                description: "Different overview",
                tags: ["Different"],
                body: "# Different\n"));

    internal void SeedAmbiguousParent()
        => WriteText(
            ".agents/memory/project-alpha/index.md",
            ParentDocument(entries: "- none - No entries - #Empty"));

    internal void RemoveParent()
        => File.Delete(Absolute(ParentPath));

    internal void SeedUnsafeTargetAlias()
        => _temporary.CreateFileSymbolicLink(TargetPath, Absolute(ParentPath));

    internal void ChangeParentAfterPlanning()
        => _temporary.ReplaceText(
            ParentPath,
            ParentDocument(entries: "- changed - Concurrent change - #Changed"));

    internal void ChangeTargetAfterApplication()
        => _temporary.ReplaceText(
            TargetPath,
            OpenForgeDocumentSeed.Metadata(
                description: "Changed after application",
                tags: ["Changed"],
                body: "# Changed\n"));

    internal void OwnApplicationCreatedTarget()
    {
        var path = Absolute(TargetPath);
        if (EntryExists(path))
        {
            throw new InvalidOperationException(
                "The Route Create application target must be absent before production runs.");
        }

        _applicationCreatedTargetPath = path;
    }

    internal async ValueTask<SourceCatalogue> ReadCatalogueAsync()
        => await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(Workspace, [SourceLogicalPath.AgentsRoot]),
            TestContext.Current.CancellationToken);

    internal async ValueTask<RouteCreatePlan> BuildPlanAsync()
    {
        var catalogue = await ReadCatalogueAsync();
        var parent = catalogue.FindByPath(ParentPath)
            ?? throw new InvalidOperationException("The Route Create parent seed was not discovered.");
        var target = IntendedTarget();
        var targetBytes = TargetBytes();
        var targetAbsolutePath = Absolute(TargetPath);
        var parentAbsolutePath = Absolute(ParentPath);
        var parentBeforeBytes = await File.ReadAllBytesAsync(
            parentAbsolutePath,
            TestContext.Current.CancellationToken);
        var parentIntendedBytes = ParentIntendedBytes();
        var targetSnapshot = FileStateSnapshot.Missing(targetAbsolutePath);
        var parentSnapshot = FileStateSnapshot.File(
            parentAbsolutePath,
            parentAbsolutePath,
            parentBeforeBytes);
        var targetChange = PlannedFileChange.Create(
            targetSnapshot.Expectation,
            targetBytes.AsSpan());
        var parentChange = PlannedFileChange.ReplaceGeneratedRegion(
            parentSnapshot.Expectation,
            parentIntendedBytes.AsSpan());
        var formation = new GeneratedNavigationFormationBuilder().Build(
            catalogue,
            [.. catalogue.Sources, target]);
        return new RouteCreatePlan
        {
            Request = Request(),
            Preview = Preview(
                targetBytes,
                parentSnapshot,
                parentIntendedBytes),
            NavigationFormation = formation,
            TargetSnapshot = targetSnapshot,
            TargetSource = target,
            ParentSource = parent,
            TemplateSource = null,
            IntendedTargetBytes = targetBytes,
            FileChanges = [targetChange, parentChange],
            RecoveryTargets = [RecoveryBundleTarget.Create(parentChange, parentSnapshot)],
        };
    }

    internal ImmutableArray<FileChangeReceipt> ApplyPlan(RouteCreatePlan plan)
    {
        var targetChange = plan.FileChanges[0];
        var parentChange = plan.FileChanges[1];
        _temporary.WriteBytes(TargetPath, [.. targetChange.IntendedBytes]);
        _temporary.ReplaceBytes(ParentPath, [.. parentChange.IntendedBytes]);

        var parentBefore = plan.RecoveryTargets[0].Before;
        return
        [
            FileChangeReceipt.Verified(
                targetChange,
                plan.TargetSnapshot,
                FileStateSnapshot.File(
                    targetChange.LogicalPath,
                    targetChange.LogicalPath,
                    targetChange.IntendedBytes.AsSpan())),
            FileChangeReceipt.Verified(
                parentChange,
                parentBefore,
                FileStateSnapshot.File(
                    parentChange.LogicalPath,
                    parentChange.LogicalPath,
                    parentChange.IntendedBytes.AsSpan())),
        ];
    }

    internal async ValueTask<WorkspaceLockLease> AcquireLeaseAsync(Guid operationId)
    {
        var result = await _lockStore.AcquireAsync(
            new WorkspaceLockRequest(
                Workspace,
                RouteCreateDefinitions.CommandIdentity,
                operationId),
            TestContext.Current.CancellationToken);
        return result.Lease
            ?? throw new InvalidOperationException(
                result.Cause ?? "The Route Create integration lock was not acquired.");
    }

    internal async ValueTask<RecoveryBundlePreparation> PrepareRecoveryBundleAsync(
        RouteCreatePlan plan,
        Guid operationId)
    {
        var result = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                Workspace,
                RouteCreateDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Route,
                    RecoveryBundleOperation.Create,
                    Workspace),
                operationId,
                plan.RecoveryTargets),
            TestContext.Current.CancellationToken);
        if (result.State != RecoveryBundlePreparationState.Prepared
            || result.Preparation is not { } preparation)
        {
            throw new InvalidOperationException(
                result.Cause ?? "The Route Create recovery fixture could not prepare its bundle.");
        }

        TrackRecovery(preparation);
        return preparation;
    }

    internal void TrackRecovery(RecoveryBundlePreparation preparation)
        => _recoveryPaths.Add(preparation.BundlePath);

    internal string Absolute(string relativePath)
        => _temporary.Combine(relativePath);

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => _temporary.SnapshotHashes();

    internal string ReadText(string relativePath)
        => File.ReadAllText(Absolute(relativePath));

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var path in _recoveryPaths)
        {
            DeleteRecovery(path);
        }

        if (_applicationCreatedTargetPath is { } targetPath)
        {
            DeleteApplicationCreatedTarget(targetPath);
        }

        _lockStore.Dispose();
        _temporary.Dispose();
        _disposed = true;
    }

    private RouteCreateResultFormation Preview(
        ImmutableArray<byte> targetBytes,
        FileStateSnapshot parentBefore,
        ImmutableArray<byte> parentBytes)
        => new()
        {
            Workspace = Workspace,
            Mode = RouteCreateMode.Apply,
            Target = new RouteCreateTarget
            {
                Requested = TargetId,
                Id = TargetId,
                Path = TargetPath,
            },
            Parent = new RouteCreateParent
            {
                Id = ParentId,
                Path = ParentPath,
                Form = RouteCreateParentForm.Canonical,
            },
            Metadata = new RouteCreateMetadata
            {
                Description = "Project overview",
                Responsibility = "Explains the project",
                Tags = ["Docs", "Overview"],
            },
            Template = null,
            Plan = new RouteCreatePlanFacts
            {
                Completeness = RouteCreatePlanCompleteness.Complete,
                Safety = RouteCreatePlanSafety.Safe,
            },
            Effects =
            [
                new RouteCreateEffect
                {
                    Path = TargetPath,
                    Kind = RouteCreateEffectKind.RoutedFile,
                    Action = RouteCreateEffectAction.Create,
                    Change = new RouteCreateEffectChange
                    {
                        Before = null,
                        Expected = FileExpectation.Hash(targetBytes.AsSpan()),
                    },
                    Outcome = RouteCreateEffectOutcome.Planned,
                    Residual = RouteCreateEffectResidual.None,
                },
                new RouteCreateEffect
                {
                    Path = ParentPath,
                    Kind = RouteCreateEffectKind.GeneratedRegion,
                    Action = RouteCreateEffectAction.Replace,
                    Change = new RouteCreateEffectChange
                    {
                        Before = parentBefore.ContentHash,
                        Expected = FileExpectation.Hash(parentBytes.AsSpan()),
                    },
                    Outcome = RouteCreateEffectOutcome.Planned,
                    Residual = RouteCreateEffectResidual.None,
                },
            ],
            UnchangedPaths =
            [
                ".agents/loader.md",
                ".agents/templates/_templates.md",
            ],
            Recovery = new RouteCreateRecovery
            {
                State = RouteCreateRecoveryState.NotCreated,
                ResidualPath = null,
            },
            Verification = RouteCreateVerificationState.NotRequested,
            Findings = [],
        };

    private SourceLogicalSource IntendedTarget()
        => new(
            identity: new SourceLogicalIdentity(TargetId, TargetPath),
            @base: new SourceLayer(
                canonicalPath: TargetPath,
                physicalPath: Absolute(TargetPath),
                form: SourceDocumentForm.Markdown,
                kind: SourceLayerKind.Base));

    private static ImmutableArray<byte> TargetBytes()
        => new FrameworkMarkdownDocumentWriter().Write(
            new FrameworkDocumentMetadata(
                description: "Project overview",
                tags: ["Docs", "Overview"],
                responsibility: "Explains the project"),
            body: string.Empty);

    private static ImmutableArray<byte> ParentIntendedBytes()
        => [.. Encoding.UTF8.GetBytes(ParentDocument(
            entries: "- [Project overview](overview.md) - #Docs #Overview"))];

    private static string ParentDocument(string entries)
        => OpenForgeDocumentSeed.Metadata(
            description: "Project Alpha",
            tags: ["Project"],
            body: $"\n{OpenForgeDocumentSeed.GeneratedEntries(entries)}");

    private void WriteText(string relativePath, string contents)
        => _temporary.WriteText(relativePath, contents);

    private void WriteBytes(string relativePath, byte[] contents)
        => _temporary.WriteBytes(relativePath, contents);

    private static void DeleteRecovery(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var directory = Path.GetDirectoryName(path);
        if (directory is not null
            && Directory.Exists(directory)
            && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }

    private static void DeleteApplicationCreatedTarget(string path)
    {
        if (!EntryExists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            return;
        }

        File.Delete(path);
    }

    private static bool EntryExists(string path)
    {
        try
        {
            _ = File.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }
}
