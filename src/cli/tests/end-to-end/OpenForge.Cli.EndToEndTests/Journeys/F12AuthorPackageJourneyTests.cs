using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Filesystem;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F12AuthorPackageJourneyTests
{
    private const string ToolkitTarget = ".agents/guidance/toolkit.md";
    private const string OwnershipTarget = ".agents/open-forge.lock.json";
    private const string WorkspaceLockTarget = ".agents/open-forge.lock";
    private const string LifecycleTarget = ".agents/open-forge.lifecycle.json";
    private const string DistinctiveBody = "# Authored Toolkit\nF12 distinctive authored bytes.\n";

    [Fact(DisplayName = "F12 creates, authors, lists, inspects, and installs a custom package from its source"),
     Trait("Feature", "author and install custom package"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F12")]
    public async Task MainCreatesAuthorsAndInstallsFromExactSource()
    {
        using var catalogue = PublishedJourneyWorkspace.Create("journey-f12-main-catalogue");
        using var consumer = PublishedJourneyWorkspace.Create("journey-f12-main-consumer");
        catalogue.ExpectFiles("toolkit/extension.json", "toolkit/content/.agents/scaffold-reservation");
        var sourceBeforeCreate = catalogue.SnapshotState();
        var consumerBeforeCreate = consumer.SnapshotState();
        var dataHomeBeforeCreate = CaptureDataHomeState(catalogue.LockStore);

        var create = await catalogue.RunAsync(
            "extension", "create", "toolkit",
            "--path", catalogue.Path,
            "--name", "Toolkit",
            "--package-version", "1.0.0",
            "--automatic");
        AssertSuccessful(create);
        AssertCreateBoundary(
            catalogue,
            sourceBeforeCreate,
            dataHomeBeforeCreate,
            ["toolkit", "toolkit/extension.json", "toolkit/content", "toolkit/content/.agents"],
            ["."]);
        Assert.Equal(consumerBeforeCreate, consumer.SnapshotState());
        Assert.Contains("toolkit", create.StandardOutput, StringComparison.OrdinalIgnoreCase);
        var packagePath = catalogue.Combine("toolkit");
        var manifestPath = catalogue.Combine("toolkit/extension.json");
        Assert.True(Directory.Exists(packagePath));
        Assert.True(File.Exists(manifestPath));
        Assert.Contains(packagePath, create.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.True(Directory.Exists(catalogue.Combine("toolkit/content")));
        AssertManifest(manifestPath, "toolkit", "Toolkit", "1.0.0", dependencies: []);

        catalogue.WriteText(
            "toolkit/content/.agents/guidance/toolkit.md",
            OpenForgeDocumentSeed.Metadata("F12 authored toolkit", ["Extension"], DistinctiveBody));
        var authoredBytes = File.ReadAllBytes(catalogue.Combine("toolkit/content/.agents/guidance/toolkit.md"));
        var sourceBeforeConsumerCommands = catalogue.SnapshotState();

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ToolkitTarget);
        consumer.WriteText("unrelated-user.md", "consumer content remains independent\n");
        var consumerNote = File.ReadAllBytes(consumer.Combine("unrelated-user.md"));
        await InstallFrameworkAsync(consumer);

        var list = await consumer.RunAsync("extension", "list", "--source", catalogue.Path);
        AssertSuccessful(list);
        Assert.Contains("toolkit", list.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1.0.0", list.StandardOutput, StringComparison.OrdinalIgnoreCase);
        var inspect = await consumer.RunAsync(
            "extension", "inspect", "toolkit", "--source", catalogue.Path, "--detail", "standard");
        AssertSuccessful(inspect);
        Assert.Contains("toolkit", inspect.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("toolkit.md", inspect.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(sourceBeforeConsumerCommands, catalogue.SnapshotState());
        Assert.False(File.Exists(consumer.Combine(ToolkitTarget)));

        var install = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--automatic");
        AssertSuccessful(install);
        Assert.Contains("toolkit", install.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(authoredBytes, File.ReadAllBytes(consumer.Combine(ToolkitTarget)));
        AssertExtensionRecord(consumer, "toolkit", "1.0.0", catalogue.Path, [], ToolkitTarget);
        Assert.Equal(consumerNote, File.ReadAllBytes(consumer.Combine("unrelated-user.md")));
        Assert.Equal(sourceBeforeConsumerCommands, catalogue.SnapshotState());
        AssertPersistentRecoveryEvidence(consumer);
    }

    [Fact(DisplayName = "F12 empty scaffold reports no installable files without inventing targets"),
     Trait("Feature", "author and install custom package"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F12")]
    public async Task EmptyScaffoldReportsNoInstallableFiles()
    {
        using var catalogue = PublishedJourneyWorkspace.Create("journey-f12-empty-catalogue");
        using var consumer = PublishedJourneyWorkspace.Create("journey-f12-empty-consumer");
        catalogue.ExpectFiles("toolkit/extension.json", "toolkit/content/.agents/scaffold-reservation");
        var sourceBeforeCreate = catalogue.SnapshotState();
        var consumerBeforeCreate = consumer.SnapshotState();
        var dataHomeBeforeCreate = CaptureDataHomeState(catalogue.LockStore);

        var create = await catalogue.RunAsync(
            "extension", "create", "toolkit",
            "--path", catalogue.Path,
            "--name", "Toolkit",
            "--package-version", "1.0.0",
            "--automatic");
        AssertSuccessful(create);
        AssertCreateBoundary(
            catalogue,
            sourceBeforeCreate,
            dataHomeBeforeCreate,
            ["toolkit", "toolkit/extension.json", "toolkit/content", "toolkit/content/.agents"],
            ["."]);
        Assert.Equal(consumerBeforeCreate, consumer.SnapshotState());
        AssertManifest(catalogue.Combine("toolkit/extension.json"), "toolkit", "Toolkit", "1.0.0", dependencies: []);
        Assert.True(Directory.Exists(catalogue.Combine("toolkit/content")));
        var sourceBeforeConsumerCommands = catalogue.SnapshotState();

        consumer.ExpectCoreInstall();
        consumer.WriteText("neighbor.md", "empty scaffold neighbor\n");
        await InstallFrameworkAsync(consumer);

        var list = await consumer.RunAsync("extension", "list", "--source", catalogue.Path);
        AssertSuccessful(list);
        var inspect = await consumer.RunAsync("extension", "inspect", "toolkit", "--source", catalogue.Path);
        AssertSuccessful(inspect);
        Assert.Contains("toolkit", string.Join("\n", list.StandardOutput, inspect.StandardOutput), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("toolkit.md", string.Join("\n", list.StandardOutput, inspect.StandardOutput), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(sourceBeforeConsumerCommands, catalogue.SnapshotState());

        var install = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--automatic");
        Assert.Equal(2, install.ExitCode);
        Assert.NotEmpty(install.StandardOutput);
        Assert.Equal(string.Empty, install.StandardError);
        Assert.True(
            install.StandardOutput.Contains("no content", StringComparison.OrdinalIgnoreCase)
            || install.StandardOutput.Contains("nothing", StringComparison.OrdinalIgnoreCase));
        Assert.False(File.Exists(consumer.Combine(ToolkitTarget)));
        AssertNoExtensionRecord(consumer, "toolkit");
        Assert.Equal("empty scaffold neighbor\n", File.ReadAllText(consumer.Combine("neighbor.md")));
        Assert.Equal(sourceBeforeConsumerCommands, catalogue.SnapshotState());
        AssertPersistentRecoveryEvidence(consumer);
    }

    [Fact(DisplayName = "F12 public create reports a truthful Windows partial scaffold under child-directory denial"),
     Trait("Feature", "author and install custom package"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F12")]
    public async Task PartialScaffoldReportsManifestAndMissingContentOnWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("This public-create boundary requires an owned Windows directory ACL.");
            return;
        }

        using var catalogue = PublishedJourneyWorkspace.Create("journey-f12-partial-catalogue");
        catalogue.ExpectFiles("catalogue/toolkit/extension.json", "catalogue/toolkit/content/.agents/scaffold-reservation");
        var emptyCataloguePath = catalogue.Combine("catalogue");
        Directory.CreateDirectory(emptyCataloguePath);
        var sourceBeforeCreate = catalogue.SnapshotState();
        var dataHomeBeforeCreate = CaptureDataHomeState(catalogue.LockStore);
        ProcessRunResult result;
        using (var denied = WindowsChildDirectoryCreationDenial.Create(catalogue.Path, emptyCataloguePath))
        {
            result = await catalogue.RunAsync(
                "extension", "create", "toolkit",
                "--path", emptyCataloguePath,
                "--name", "Toolkit",
                "--package-version", "1.0.0",
                "--automatic");
        }

        AssertCreateBoundary(
            catalogue,
            sourceBeforeCreate,
            dataHomeBeforeCreate,
            ["catalogue/toolkit", "catalogue/toolkit/extension.json"],
            ["catalogue"]);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Contains("extension.json", result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("content", result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            result.StandardError.Contains("created", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("unfinished", StringComparison.OrdinalIgnoreCase)
            || result.StandardError.Contains("partial", StringComparison.OrdinalIgnoreCase));

        var packagePath = catalogue.Combine("catalogue/toolkit");
        var manifestPath = catalogue.Combine("catalogue/toolkit/extension.json");
        Assert.True(Directory.Exists(packagePath));
        Assert.True(File.Exists(manifestPath));
        AssertManifest(manifestPath, "toolkit", "Toolkit", "1.0.0", dependencies: []);
        Assert.False(Directory.Exists(catalogue.Combine("catalogue/toolkit/content")));
    }

    private static void AssertCreateBoundary(
        PublishedJourneyWorkspace source,
        IReadOnlyDictionary<string, string> before,
        DataHomeSnapshot dataHomeBefore,
        IReadOnlyCollection<string> allowedCreatedPaths,
        IReadOnlyCollection<string> allowedTimestampParents)
    {
        var after = source.SnapshotState();
        var created = allowedCreatedPaths.ToHashSet(StringComparer.Ordinal);
        var timestampParents = allowedTimestampParents.ToHashSet(StringComparer.Ordinal);

        foreach (var (path, beforeDescription) in before)
        {
            Assert.True(after.TryGetValue(path, out var afterDescription), $"Create removed '{path}'.");
            if (timestampParents.Contains(path))
            {
                Assert.StartsWith("type=directory;", beforeDescription, StringComparison.Ordinal);
                Assert.NotNull(afterDescription);
                Assert.StartsWith("type=directory;", afterDescription!, StringComparison.Ordinal);
                Assert.True(PublishedJourneyAssertions.DirectoryMetadataMatchesAfterChildMutation(beforeDescription, afterDescription!));
            }
            else
            {
                Assert.Equal(beforeDescription, afterDescription);
            }
        }

        foreach (var path in after.Keys.Except(before.Keys, StringComparer.Ordinal))
        {
            Assert.Contains(path, created);
        }

        foreach (var path in created)
        {
            Assert.Contains(path, after.Keys);
        }

        Assert.False(File.Exists(source.Combine(OwnershipTarget)));
        Assert.False(File.Exists(source.Combine(WorkspaceLockTarget)));
        Assert.False(File.Exists(source.Combine(LifecycleTarget)));
        AssertDataHomeUnchanged(source.LockStore, dataHomeBefore);
    }

    private static DataHomeSnapshot CaptureDataHomeState(PublishedWorkspaceLockStore lockStore)
        => new(
            CaptureOptionalDirectory(lockStore.LocalApplicationDataDirectory),
            CaptureOptionalDirectory(lockStore.RecoveryStoreRoot));

    private static IReadOnlyDictionary<string, string>? CaptureOptionalDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return PublishedWorkspaceTreeSnapshot.Capture(path);
        }

        Assert.False(File.Exists(path), $"Expected data-home path '{path}' to be a directory or absent.");
        return null;
    }

    private static void AssertDataHomeUnchanged(
        PublishedWorkspaceLockStore lockStore,
        DataHomeSnapshot expected)
    {
        Assert.Equal(expected.LocalApplicationData, CaptureOptionalDirectory(lockStore.LocalApplicationDataDirectory));
        Assert.Equal(expected.RecoveryStore, CaptureOptionalDirectory(lockStore.RecoveryStoreRoot));
        lockStore.AssertNoInfrastructure();
    }

    private sealed record DataHomeSnapshot(
        IReadOnlyDictionary<string, string>? LocalApplicationData,
        IReadOnlyDictionary<string, string>? RecoveryStore);

    private static async Task<ProcessRunResult> InstallFrameworkAsync(PublishedJourneyWorkspace consumer)
    {
        var result = await consumer.RunAsync("install", "--automatic");
        AssertSuccessful(result);
        Assert.True(File.Exists(consumer.Combine(OwnershipTarget)));
        return result;
    }

    private static void AssertManifest(
        string manifestPath,
        string id,
        string name,
        string version,
        string[] dependencies)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var manifest = document.RootElement;
        Assert.Equal(id, manifest.GetProperty("id").GetString());
        Assert.Equal(name, manifest.GetProperty("name").GetString());
        Assert.Equal(version, manifest.GetProperty("version").GetString());
        var description = manifest.GetProperty("description").GetString();
        Assert.NotNull(description);
        Assert.NotEmpty(description);
        Assert.Equal(dependencies, manifest.GetProperty("dependencies").EnumerateArray()
            .Select(dependency => Assert.IsType<string>(dependency.GetString())).ToArray());
    }

    private static void AssertExtensionRecord(
        PublishedJourneyWorkspace consumer,
        string id,
        string version,
        string sourcePath,
        IReadOnlyList<string> expectedDependencies,
        params string[] expectedPaths)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(consumer.Combine(OwnershipTarget)));
        var extension = Assert.Single(
            document.RootElement.GetProperty("extensions").EnumerateArray(),
            candidate => candidate.GetProperty("id").GetString() == id);
        Assert.Equal(id, extension.GetProperty("id").GetString());
        Assert.Equal(version, extension.GetProperty("version").GetString());
        Assert.Equal(sourcePath, extension.GetProperty("source").GetString());
        Assert.Equal(
            expectedPaths.Select(Normalize).OrderBy(path => path, StringComparer.Ordinal).ToArray(),
            extension.GetProperty("paths").EnumerateArray()
                .Select(path => Normalize(path.GetString()!))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray());
        Assert.Equal(
            expectedDependencies,
            extension.GetProperty("dependencies").EnumerateArray()
                .Select(dependency => Assert.IsType<string>(dependency.GetString()))
                .ToArray());
        Assert.Empty(extension.GetProperty("regions").EnumerateArray());
    }

    private static void AssertNoExtensionRecord(PublishedJourneyWorkspace consumer, string id)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(consumer.Combine(OwnershipTarget)));
        Assert.DoesNotContain(
            document.RootElement.GetProperty("extensions").EnumerateArray(),
            extension => extension.GetProperty("id").GetString() == id);
    }

    private static void AssertPersistentRecoveryEvidence(PublishedJourneyWorkspace workspace)
    {
        Assert.True(File.Exists(workspace.Combine(OwnershipTarget)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static void AssertSuccessful(ProcessRunResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static string Normalize(string path)
        => path.Replace('\\', '/');
}
