using System.Text;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.Framework.Lifecycle;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

internal sealed class RouteRemoveIntegrationWorkspace : IDisposable
{
    internal const string LeafId = "guidance/old guide";
    internal const string LeafPath = ".agents/guidance/old guide.md";
    internal const string LeafOverwritePath = ".agents/guidance/old guide.overwrite.md";
    internal const string CategoryId = "guidance/topics";
    internal const string CategoryPath = ".agents/guidance/topics/_topics.md";
    internal const string CategoryChildPath = ".agents/guidance/topics/child.md";
    internal const string CategoryNotesPath = ".agents/guidance/topics/notes.md";
    internal const string CategoryResourcePath = ".agents/guidance/topics/assets/settings.json";
    internal const string LoaderCategoryId = "loader-topics";
    internal const string LoaderCategoryPath = ".agents/loader-topics/_loader-topics.md";
    internal const string LoaderCategoryChildPath = ".agents/loader-topics/child.md";
    internal const string ParentPath = ".agents/guidance/_guidance.md";
    internal const string LoaderPath = ".agents/loader.md";
    internal const string LifecyclePath = ".agents/open-forge.lifecycle.json";

    private readonly TemporaryWorkspace temporary;
    private readonly WorkspaceLockTestStore lockStore;
    private bool disposed;

    private RouteRemoveIntegrationWorkspace(
        TemporaryWorkspace temporary,
        WorkspaceLockTestStore lockStore)
    {
        this.temporary = temporary;
        this.lockStore = lockStore;
        Workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = lockStore.Track(Workspace);
    }

    internal CliWorkspace Workspace { get; }

    internal WorkspaceLockStoreRoot LockStoreRoot => lockStore.StoreRoot;

    internal string Path => temporary.Path;

    internal static RouteRemoveIntegrationWorkspace Create(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            var workspace = new RouteRemoveIntegrationWorkspace(temporary, lockStore);
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

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => temporary.SnapshotHashes();

    internal string Combine(string relativePath)
        => temporary.Combine(relativePath);

    internal string ReadText(string relativePath)
        => File.ReadAllText(Combine(relativePath), Encoding.UTF8);

    internal byte[] ReadBytes(string relativePath)
        => File.ReadAllBytes(Combine(relativePath));

    internal void WriteText(string relativePath, string contents)
    {
        if (File.Exists(Combine(relativePath)))
        {
            temporary.ReplaceText(relativePath, contents);
        }
        else
        {
            temporary.CreateFile(relativePath, contents);
        }
    }

    internal void WriteBytes(string relativePath, byte[] contents)
    {
        if (File.Exists(Combine(relativePath)))
        {
            temporary.ReplaceBytes(relativePath, contents);
        }
        else
        {
            temporary.CreateFile(relativePath, contents);
        }
    }

    internal void DeleteFile(string relativePath)
    {
        var path = Combine(relativePath);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    internal void RemoveLifecycle()
        => DeleteFile(LifecyclePath);

    internal void SeedInvalidUtf8()
        => WriteBytes("unreadable.md", [0xff, 0xfe, 0xfd]);

    internal void SeedFrameworkClaim(string path)
    {
        var envelope = LifecycleStoreIntegrationDocuments.Envelope(
            temporary,
            LifecycleStoreIntegrationDocuments.Framework(path),
            LifecycleStoreIntegrationDocuments.EmptyExtensions());
        WriteBytes(
            LifecyclePath,
            LifecycleStoreIntegrationDocuments.Serialize(envelope));
    }

    internal void SeedExtensionClaim(string path)
    {
        var envelope = LifecycleStoreIntegrationDocuments.Envelope(
            temporary,
            LifecycleStoreIntegrationDocuments.Framework(),
            LifecycleStoreIntegrationDocuments.Extensions(path));
        WriteBytes(
            LifecyclePath,
            LifecycleStoreIntegrationDocuments.Serialize(envelope));
    }

    internal void AssertNoLockInfrastructure()
        => Assert.False(lockStore.InfrastructureExists);

    internal async Task<WorkspaceLockLease> AcquireLeaseAsync(Guid operationId)
    {
        var result = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(
                Workspace,
                "route remove",
                operationId),
            TestContext.Current.CancellationToken);
        return result.Lease
            ?? throw new InvalidOperationException(
                result.Cause ?? "The Route Remove integration lock was not acquired.");
    }

    internal async ValueTask<RouteRemovePlan> BuildApplicationPlanAsync(
        string sourceReference = LeafId)
    {
        var build = await RouteRemovePlanBuilder.Create().BuildAsync(
            new RouteRemoveRequest(Workspace, sourceReference, RouteRemoveMode.Apply),
            TestContext.Current.CancellationToken);
        return Assert.IsType<RouteRemovePlan>(build.Plan);
    }

    internal async Task<CliProcessCompletion> RunAsync(
        IReadOnlyList<string> arguments,
        StringWriter standardOutput,
        StringWriter standardError)
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"),
            new CliCompositionInputs
            {
                StandardInput = TextReader.Null,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
                LockStoreRoot = LockStoreRoot,
            });

        return await application.RunAsync(
            arguments.ToArray(),
            new CliProcessEnvironment(Path),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        try
        {
            DeleteRecoveryArtifacts();
        }
        finally
        {
            lockStore.Dispose();
            temporary.Dispose();
            disposed = true;
        }
    }

    private void SeedOrdinaryWorkspace()
    {
        WriteText(
            LoaderPath,
            Entrypoint(
                "Loader",
                "- [Guidance](guidance/_guidance.md) - #Route\n- [Loader topics](loader-topics/_loader-topics.md) - #Route"));
        WriteText(
            ParentPath,
            Entrypoint(
                "Guidance",
                "- [Old guide](old%20guide.md) - #Guide\n- [Topics](topics/_topics.md) - #Topic"));
        WriteText(
            LeafPath,
            Markdown(
                "Old guide",
                "# Old guide\n\nA leaf body.\n"));
        WriteText(
            LeafOverwritePath,
            "Overwrite body with [topics](topics/_topics.md).\n");
        WriteText(
            CategoryPath,
            Entrypoint(
                "Topics",
                "- [Child](child.md) - #Child"));
        WriteText(
            CategoryChildPath,
            Markdown(
                "Child",
                "# Child\n\nChild body.\n"));
        WriteText(CategoryNotesPath, "# Notes\n\nUnrouted category content.\n");
        WriteBytes(CategoryResourcePath, [0x00, 0x01, 0x02, 0xff]);
        WriteText(
            LoaderCategoryPath,
            Entrypoint(
                "Loader topics",
                "- [Loader child](child.md) - #Route"));
        WriteText(
            LoaderCategoryChildPath,
            Markdown(
                "Loader child",
                "# Loader child\n\nLoader-rooted category body.\n"));
        WriteText(
            "README.md",
            "Prefix [Old guide](.agents/guidance/old%20guide.md#part) suffix.\n");
        WriteText(
            "notes.md",
            "[Topics](.agents/guidance/topics/_topics.md) remains.\n");
        var envelope = LifecycleStoreIntegrationDocuments.Envelope(
            temporary,
            LifecycleStoreIntegrationDocuments.Framework(),
            LifecycleStoreIntegrationDocuments.EmptyExtensions());
        WriteBytes(
            LifecyclePath,
            LifecycleStoreIntegrationDocuments.Serialize(envelope));
    }

    private void DeleteRecoveryArtifacts()
    {
        string directory;
        try
        {
            directory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(Workspace);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var path in Directory.EnumerateFileSystemEntries(directory).ToArray())
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: false);
            }
        }

        if (!Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }

    private static string Markdown(string description, string body)
        => OpenForgeDocumentSeed.Metadata(description, ["Route"], body);

    private static string Entrypoint(string description, string entries)
        => OpenForgeDocumentSeed.Metadata(
            description,
            ["Route"],
            $"# {description}\n\n{OpenForgeDocumentSeed.GeneratedEntries(entries)}");
}
