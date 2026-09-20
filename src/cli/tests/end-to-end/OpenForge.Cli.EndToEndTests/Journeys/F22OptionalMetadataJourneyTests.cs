using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F22OptionalMetadataJourneyTests
{
    private const string LoaderPath = ".agents/loader.md";
    private const string ParentId = "guidance";
    private const string ParentPath = ".agents/guidance/_guidance.md";
    private const string TargetId = "guidance/new-note";
    private const string TargetPath = ".agents/guidance/new-note.md";
    private const string Description = "Team operating notes";

    [Fact(DisplayName = "F22 creates an unannotated route, enriches it once and inspects the same source"), Trait("Feature", "route-correction"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F22")]
    public async Task CreatesEnrichesAndInspectsTheSameRouteSource()
    {
        using var workspace = await CreateInstalledWorkspaceAsync("f22-optional-metadata");
        workspace.WriteText(
            LoaderPath,
            GeneratedLoaderDocumentBuilder.Build(
                "- [Guidance](guidance/_guidance.md) - #Guidance"));
        workspace.WriteText(ParentPath, GuidanceParentDocument());
        workspace.ExpectFiles(TargetPath);
        Directory.CreateDirectory(workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path));

        var beforeCreate = SnapshotState(workspace);
        var create = await workspace.RunAsync("route", "create", TargetId);

        Assert.Equal(2, create.ExitCode);
        Assert.Equal(string.Empty, create.StandardError);
        AssertOptionalMetadataAction(create.StandardOutput);

        var targetAfterCreate = File.ReadAllText(workspace.Combine(TargetPath), Encoding.UTF8);
        var parentAfterCreate = File.ReadAllText(workspace.Combine(ParentPath), Encoding.UTF8);
        Assert.DoesNotContain("description:", targetAfterCreate, StringComparison.Ordinal);
        Assert.DoesNotContain("tags:", targetAfterCreate, StringComparison.Ordinal);
        Assert.Contains("new-note.md", parentAfterCreate, StringComparison.Ordinal);
        Assert.Contains("Authored guidance introduction.", parentAfterCreate, StringComparison.Ordinal);
        var afterCreate = SnapshotState(workspace);
        Assert.False(beforeCreate.ContainsKey("workspace/" + TargetPath));
        Assert.NotEqual(
            beforeCreate["workspace/" + ParentPath],
            afterCreate["workspace/" + ParentPath]);
        AssertAllowedRouteMutation(beforeCreate, afterCreate, workspace);

        var targetBodyBeforeEnrichment = BodyOf(targetAfterCreate);
        var beforeUpdate = SnapshotState(workspace);
        var update = await workspace.RunAsync(
            "route",
            "update",
            TargetId,
            "--description",
            Description,
            "--tag=Guidance");

        Assert.Equal(0, update.ExitCode);
        Assert.Equal(string.Empty, update.StandardError);
        Assert.Contains(TargetId, update.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("description", update.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(Description, update.StandardOutput, StringComparison.Ordinal);

        var targetAfterUpdate = File.ReadAllText(workspace.Combine(TargetPath), Encoding.UTF8);
        var parentAfterUpdate = File.ReadAllText(workspace.Combine(ParentPath), Encoding.UTF8);
        Assert.Equal(targetBodyBeforeEnrichment, BodyOf(targetAfterUpdate));
        Assert.Contains("description: Team operating notes", targetAfterUpdate, StringComparison.Ordinal);
        Assert.Contains("tags: [Guidance]", targetAfterUpdate, StringComparison.Ordinal);
        Assert.Contains(Description, parentAfterUpdate, StringComparison.Ordinal);
        Assert.Contains("new-note.md", parentAfterUpdate, StringComparison.Ordinal);
        Assert.Contains("Authored guidance introduction.", parentAfterUpdate, StringComparison.Ordinal);
        var afterUpdate = SnapshotState(workspace);
        Assert.NotEqual(
            beforeUpdate["workspace/" + ParentPath],
            afterUpdate["workspace/" + ParentPath]);
        Assert.NotEqual(
            beforeUpdate["workspace/" + TargetPath],
            afterUpdate["workspace/" + TargetPath]);
        AssertAllowedRouteMutation(beforeUpdate, afterUpdate, workspace);

        var beforeInspect = SnapshotState(workspace);
        var inspect = await RunReadOnlyAsync(
            workspace,
            beforeInspect,
            ["route", "inspect", TargetId]);
        Assert.Equal(0, inspect.ExitCode);
        Assert.Equal(string.Empty, inspect.StandardError);
        Assert.Contains(TargetId, inspect.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(TargetPath, inspect.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(ParentId, inspect.StandardOutput, StringComparison.Ordinal);

        var inspectJson = await RunReadOnlyAsync(
            workspace,
            beforeInspect,
            ["route", "inspect", TargetId, "--format=json", "--detail=full"]);
        Assert.Equal(0, inspectJson.ExitCode);
        Assert.Equal(string.Empty, inspectJson.StandardError);
        using var document = JsonDocument.Parse(inspectJson.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route inspect", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var data = root.GetProperty("data");
        Assert.Equal(TargetId, data.GetProperty("id").GetString());
        Assert.Equal(TargetPath, data.GetProperty("path").GetString());
        Assert.Equal(
            ["guidance", "new-note"],
            data.GetProperty("belongs").GetProperty("routeChain").EnumerateArray()
                .Select(value => value.GetString())
                .ToArray());
        Assert.Equal(ParentId, data.GetProperty("belongs").GetProperty("parent").GetString());
        Assert.Equal(beforeInspect, SnapshotState(workspace));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
    }

    [Fact(DisplayName = "F22 missing Route Create target is one invalid correction with no writes"), Trait("Feature", "route-correction"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F22")]
    public async Task MissingRouteCreateTargetIsOneInvalidCorrectionWithoutWrites()
    {
        using var workspace = await CreateInstalledWorkspaceAsync("f22-missing-target");
        var before = SnapshotState(workspace);
        var invalid = await RunReadOnlyAsync(
            workspace,
            before,
            ["route", "create"]);

        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.Contains("route create --help", invalid.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, SnapshotState(workspace));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
    }

    private static async Task<PublishedJourneyWorkspace> CreateInstalledWorkspaceAsync(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.WriteText("README.md", "# F22 W1 fixture\n\nPreserve this authored file.\n");
            var installed = await workspace.RunAsync("install", "--automatic");
            Assert.Equal(0, installed.ExitCode);
            Assert.Equal(string.Empty, installed.StandardError);
            Assert.True(File.Exists(workspace.Combine("AGENTS.md")));
            Assert.True(File.Exists(workspace.Combine(".agents/guidance/_guidance.md")));
            Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
            workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static async Task<ProcessRunResult> RunReadOnlyAsync(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, string> expectedState,
        IReadOnlyList<string> arguments)
    {
        var result = await PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => SnapshotState(workspace),
            arguments,
            workspace.ProcessEnvironment);
        Assert.Equal(expectedState, SnapshotState(workspace));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        return result;
    }

    private static string GuidanceParentDocument()
        => OpenForgeDocumentSeed.Metadata(
            description: "Guidance",
            tags: ["Guidance"],
            body: "\n" + OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = "- none - No entries - #Empty",
                Prefix = "# Guidance\n\nAuthored guidance introduction.",
            }));

    private static void AssertOptionalMetadataAction(string output)
    {
        var nextActions = output
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("Next:", StringComparison.Ordinal))
            .ToArray();
        Assert.True(
            nextActions.Any(line =>
                line.Contains(TargetId, StringComparison.Ordinal)
                && line.Contains("optional", StringComparison.OrdinalIgnoreCase)
                && line.Contains("description", StringComparison.OrdinalIgnoreCase)
                && line.Contains("tag", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("required", StringComparison.OrdinalIgnoreCase)),
            "The optional-metadata next action must retain guidance/new-note and describe description/tags as optional.");
    }

    private static void AssertAllowedRouteMutation(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        PublishedJourneyWorkspace workspace)
    {
        PublishedJourneyAssertions.AssertOnlyFileMutations(
            WorkspaceState(before),
            WorkspaceState(after),
            TargetPath,
            ParentPath);
        AssertExternalStateUnchanged(before, after, workspace);
    }

    private static void AssertExternalStateUnchanged(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        PublishedJourneyWorkspace workspace)
    {
        var beforeExternal = ExternalState(before);
        var afterExternal = ExternalState(after);
        Assert.Equal(
            beforeExternal.Keys.Order(StringComparer.Ordinal).ToArray(),
            afterExternal.Keys.Order(StringComparer.Ordinal).ToArray());

        foreach (var key in beforeExternal.Keys)
        {
            var beforeValue = beforeExternal[key];
            var afterValue = afterExternal[key];
            if (string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
            {
                continue;
            }

            Assert.True(
                IsAllowedEmptyDirectoryTimestampChange(
                    workspace,
                    key,
                    beforeValue,
                    afterValue),
                $"Unexpected external mutation at {key}: before={beforeValue}; after={afterValue}");
        }

        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static IReadOnlyDictionary<string, string> WorkspaceState(
        IReadOnlyDictionary<string, string> state)
        => SelectState(state, key => key.StartsWith("workspace/", StringComparison.Ordinal), "workspace/");

    private static IReadOnlyDictionary<string, string> ExternalState(
        IReadOnlyDictionary<string, string> state)
        => SelectState(
            state,
            key => key == "data-home"
                || key.StartsWith("data-home/", StringComparison.Ordinal)
                || key == "recovery-workspace"
                || key.StartsWith("recovery-workspace/", StringComparison.Ordinal),
            prefix: null);

    private static IReadOnlyDictionary<string, string> SelectState(
        IReadOnlyDictionary<string, string> state,
        Func<string, bool> predicate,
        string? prefix)
    {
        var selected = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in state.Where(pair => predicate(pair.Key)))
        {
            var key = prefix is null
                ? pair.Key
                : pair.Key[prefix.Length..];
            selected[key] = pair.Value;
        }

        return new ReadOnlyDictionary<string, string>(selected);
    }

    private static bool IsAllowedEmptyDirectoryTimestampChange(
        PublishedJourneyWorkspace workspace,
        string key,
        string before,
        string after)
    {
        if (!before.StartsWith("type=directory;", StringComparison.Ordinal)
            || !after.StartsWith("type=directory;", StringComparison.Ordinal)
            || !string.Equals(
                WithoutLastWrite(before),
                WithoutLastWrite(after),
                StringComparison.Ordinal))
        {
            return false;
        }

        var directoryPath = ExternalPath(workspace, key);
        return Directory.Exists(directoryPath)
            && !Directory.EnumerateFileSystemEntries(directoryPath).Any();
    }

    private static string ExternalPath(PublishedJourneyWorkspace workspace, string key)
    {
        string root;
        string relative;
        if (key == "data-home" || key.StartsWith("data-home/", StringComparison.Ordinal))
        {
            root = workspace.LockStore.LocalApplicationDataDirectory;
            relative = key == "data-home" ? "." : key["data-home/".Length..];
        }
        else if (key == "recovery-workspace"
            || key.StartsWith("recovery-workspace/", StringComparison.Ordinal))
        {
            root = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
            relative = key == "recovery-workspace" ? "." : key["recovery-workspace/".Length..];
        }
        else
        {
            throw new InvalidOperationException($"Not an external journey-state key: {key}");
        }

        return relative == "."
            ? root
            : Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string WithoutLastWrite(string description)
        => string.Join(';', description.Split(';').Where(
            field => !field.StartsWith("lastWriteUtcTicks=", StringComparison.Ordinal)));

    private static string BodyOf(string document)
    {
        var frontmatterEnd = document.IndexOf("\n---", 3, StringComparison.Ordinal);
        Assert.True(frontmatterEnd >= 0, "The created route did not retain a frontmatter boundary.");
        return document[(frontmatterEnd + "\n---".Length)..];
    }

    private static IReadOnlyDictionary<string, string> SnapshotState(PublishedJourneyWorkspace workspace)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in workspace.SnapshotState())
        {
            state[$"workspace/{entry.Key}"] = entry.Value;
        }

        AddDirectorySnapshot(
            state,
            "data-home",
            workspace.LockStore.LocalApplicationDataDirectory);
        AddDirectorySnapshot(
            state,
            "recovery-workspace",
            workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path));
        return new ReadOnlyDictionary<string, string>(state);
    }

    private static void AddDirectorySnapshot(
        IDictionary<string, string> state,
        string prefix,
        string path)
    {
        if (!Directory.Exists(path))
        {
            state[prefix] = File.Exists(path)
                ? $"file:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}"
                : "absent";
            return;
        }

        foreach (var entry in PublishedWorkspaceTreeSnapshot.Capture(path))
        {
            state[$"{prefix}/{entry.Key}"] = entry.Value;
        }
    }
}
