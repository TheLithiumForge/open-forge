using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRepairProcessTests
{
    [Fact(DisplayName = "Published Repair help succeeds without inspecting or writing a workspace"),
     Trait("Feature", "repair"), Trait("Evidence", "EndToEnd")]
    public async Task HelpSucceedsWithoutWorkspaceInspectionOrWrites()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = RepairWorkspace.Create(
            "repair-e2e-help",
            includeSafeExact: false,
            includeGuided: false);
        var missingWorkspace = working.Combine("missing-help-workspace");

        var result = await RunWithoutWritesAsync(
            target,
            working,
            ["repair", "--help", "--workspace", missingWorkspace]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("open-forge repair", result.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
        working.AssertNoWriteInfrastructure();
    }

    [Fact(DisplayName = "Published Repair automatic JSON dry-run previews safe-exact work while leaving guided "
        + "candidates and infrastructure unchanged"),
     Trait("Feature", "repair"), Trait("Evidence", "EndToEnd")]
    public async Task AutomaticJsonDryRunPreviewsSafeExactWithoutEffects()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = RepairWorkspace.Create(
            "repair-e2e-dry-run",
            includeSafeExact: true,
            includeGuided: true);

        var result = await RunWithoutWritesAsync(
            target,
            working,
            ["repair", "--automatic", "--dry-run", "--json"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("repair", root.GetProperty("command").GetString());
        Assert.Equal("attention", root.GetProperty("status").GetString());
        var repair = root.GetProperty("result");
        Assert.Equal("dry-run", repair.GetProperty("mode").GetString());
        Assert.True(repair.GetProperty("automatic").GetBoolean());
        var selection = repair.GetProperty("selection");
        Assert.Single(selection.GetProperty("selected").EnumerateArray());
        var unselected = Assert.Single(selection.GetProperty("unselected").EnumerateArray());
        Assert.Equal("missing-target-relink", unselected.GetProperty("member").GetString());
        Assert.Equal("missing.md", unselected.GetProperty("expectedDestination").GetString());
        var candidates = unselected.GetProperty("candidates").GetProperty("items").EnumerateArray();
        Assert.Contains(
            candidates,
            candidate => candidate.GetProperty("target").GetProperty("path").GetString()
                == RepairWorkspace.GuidedTargetPath);
        var plan = repair.GetProperty("plan");
        Assert.False(plan.GetProperty("blocked").GetBoolean());
        Assert.Single(plan.GetProperty("effects").EnumerateArray());
        Assert.Empty(plan.GetProperty("noOps").EnumerateArray());
        var counts = repair.GetProperty("counts");
        Assert.Equal(1, counts.GetProperty("selectedEffects").GetInt32());
        Assert.Equal(0, counts.GetProperty("appliedEffects").GetInt32());
        Assert.Equal(1, counts.GetProperty("guided").GetInt32());
        Assert.Equal("ready", repair.GetProperty("preflight").GetProperty("state").GetString());
        Assert.Equal("not-requested", repair.GetProperty("application").GetProperty("state").GetString());
        Assert.Equal("not-created", repair.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("not-requested", repair.GetProperty("verification").GetProperty("targets").GetString());
        Assert.Equal("not-requested", repair.GetProperty("verification").GetProperty("resultingBytes").GetString());
        Assert.Equal("not-requested", repair.GetProperty("verification").GetProperty("postConditions").GetString());
        working.AssertNoWriteInfrastructure();
    }

    [Fact(DisplayName = "Published Repair explicit relink preserves content, removes recovery, and converges "
        + "to a verified automatic no-op"),
     Trait("Feature", "repair"), Trait("Evidence", "EndToEnd")]
    public async Task ExplicitRelinkPreservesContentAndConvergesToNoOp()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = RepairWorkspace.Create(
            "repair-e2e-explicit-relink",
            includeSafeExact: false,
            includeGuided: true);
        var unrelatedBefore = working.ReadBytes(RepairWorkspace.UnrelatedPath);

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            working.Path,
            [
                "repair",
                "--json",
                "--relink",
                working.GuidedSourceLocation,
                RepairWorkspace.GuidedExpectedDestination,
                RepairWorkspace.GuidedTargetPath,
            ],
            working.EnvironmentVariables);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        var appliedRoot = appliedDocument.RootElement;
        Assert.Equal("repair", appliedRoot.GetProperty("command").GetString());
        Assert.Equal("complete", appliedRoot.GetProperty("status").GetString());
        var appliedResult = appliedRoot.GetProperty("result");
        Assert.Equal("apply", appliedResult.GetProperty("mode").GetString());
        Assert.Equal("explicit-relinks", appliedResult.GetProperty("selectionMode").GetString());
        Assert.Equal(1, appliedResult.GetProperty("counts").GetProperty("selectedEffects").GetInt32());
        Assert.Equal(1, appliedResult.GetProperty("counts").GetProperty("appliedEffects").GetInt32());
        Assert.Equal(1, appliedResult.GetProperty("counts").GetProperty("verifiedEffects").GetInt32());
        Assert.Equal("removed", appliedResult.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("complete", appliedResult.GetProperty("postDiagnosis").GetProperty("state").GetString());
        Assert.Single(appliedResult.GetProperty("plan").GetProperty("effects").EnumerateArray());
        Assert.Empty(appliedResult.GetProperty("plan").GetProperty("conflicts").EnumerateArray());
        Assert.Equal(
            RepairWorkspace.ExpectedSourceAfterGuided,
            working.ReadText(RepairWorkspace.SourcePath));
        Assert.Contains(
            "[Guided](replacement.md)",
            working.ReadText(RepairWorkspace.SourcePath),
            StringComparison.Ordinal);
        Assert.Equal(unrelatedBefore, working.ReadBytes(RepairWorkspace.UnrelatedPath));
        working.AssertPersistentExternalLock();
        working.AssertNoRecoveryArtifacts();

        var beforeNoOp = working.SnapshotState();
        var noOp = await RunWithoutWorkspaceWritesAsync(
            target,
            working,
            ["repair", "--automatic", "--json"]);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        using var noOpDocument = JsonDocument.Parse(noOp.StandardOutput);
        var noOpRoot = noOpDocument.RootElement;
        Assert.Equal("complete", noOpRoot.GetProperty("status").GetString());
        var noOpResult = noOpRoot.GetProperty("result");
        Assert.Equal(0, noOpResult.GetProperty("counts").GetProperty("noOps").GetInt32());
        Assert.Equal("not-requested", noOpResult.GetProperty("application").GetProperty("state").GetString());
        Assert.Equal("not-required", noOpResult.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("verified", noOpResult.GetProperty("verification").GetProperty("targets").GetString());
        Assert.Equal("verified", noOpResult.GetProperty("verification").GetProperty("resultingBytes").GetString());
        Assert.Equal("verified", noOpResult.GetProperty("verification").GetProperty("postConditions").GetString());
        var noOpPlan = noOpResult.GetProperty("plan");
        Assert.True(noOpPlan.GetProperty("noOp").GetBoolean());
        Assert.Empty(noOpPlan.GetProperty("effects").EnumerateArray());
        Assert.Empty(noOpPlan.GetProperty("noOps").EnumerateArray());
        Assert.Equal(beforeNoOp, working.SnapshotState());
        working.AssertPersistentExternalLock();
        working.AssertNoRecoveryArtifacts();
    }

    private static async Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        RepairWorkspace working,
        IReadOnlyList<string> arguments)
    {
        var result = await RunWithoutWorkspaceWritesAsync(target, working, arguments);
        working.AssertNoWriteInfrastructure();
        return result;
    }

    private static Task<ProcessRunResult> RunWithoutWorkspaceWritesAsync(
        PublishedExecutableTarget target,
        RepairWorkspace working,
        IReadOnlyList<string> arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            arguments,
            working.EnvironmentVariables);

    private sealed class RepairWorkspace : IDisposable
    {
        internal const string SourcePath = ".agents/docs/source.md";
        internal const string SafeTargetPath = ".agents/docs/guide.md";
        internal const string GuidedTargetPath = ".agents/docs/replacement.md";
        internal const string UnrelatedPath = ".agents/docs/unrelated.bin";
        internal const string GuidedExpectedDestination = "missing.md";

        private const string SafeLink = "[Safe](./guide.md)";
        private const string GuidedLink = "[Guided](missing.md)";
        private const string SourceBodyPrefix = "\n# Source\n\n";
        private const string UnrelatedLine = "Unrelated bytes stay exactly.\n";
        private static readonly byte[] UnrelatedBytes = [0, 255, 1, 128, 13, 10, 0];
        private readonly TemporaryWorkspace _workspace;
        private readonly PublishedWorkspaceLockStore _lockStore;
        private readonly string _lockPath;
        private bool _disposed;

        private RepairWorkspace(
            TemporaryWorkspace workspace,
            PublishedWorkspaceLockStore lockStore)
        {
            _workspace = workspace;
            _lockStore = lockStore;
            _lockPath = lockStore.Track(workspace.Path);
        }

        internal string Path => _workspace.Path;

        internal IReadOnlyDictionary<string, string> EnvironmentVariables
            => _lockStore.EnvironmentVariables;

        internal string GuidedSourceLocation
        {
            get
            {
                var source = ReadText(SourcePath);
                var destination = source.IndexOf(GuidedExpectedDestination, StringComparison.Ordinal);
                if (destination < 0)
                {
                    throw new InvalidOperationException("The published Repair fixture has no guided destination.");
                }

                var line = 1 + source[..destination].Count(character => character == '\n');
                var lineStart = source.LastIndexOf('\n', destination == 0 ? 0 : destination - 1);
                lineStart = lineStart < 0 ? 0 : lineStart + 1;
                return $"{SourcePath}@{line}:{destination - lineStart + 1}";
            }
        }

        internal static string ExpectedSourceAfterGuided
            => OpenForgeDocumentSeed.Metadata(
                description: "Source",
                tags: ["Docs"],
                body: SourceBodyPrefix + "[Guided](replacement.md)\n" + UnrelatedLine);

        internal static RepairWorkspace Create(
            string purpose,
            bool includeSafeExact,
            bool includeGuided)
        {
            var workspace = TemporaryWorkspace.Create(purpose);
            var lockStore = PublishedWorkspaceLockStore.Create($"{purpose}-lock-store");
            try
            {
                workspace.WriteText(
                    "AGENTS.md",
                    "# Workspace\n\nRead `.agents/loader.md`.\n");
                workspace.WriteText(
                    ".agents/loader.md",
                    GeneratedLoaderDocumentBuilder.Build(
                        "- [Docs](docs/index.md) - #Docs"));
                workspace.WriteText(
                    ".agents/docs/index.md",
                    OpenForgeDocumentSeed.Metadata(
                        description: "Docs",
                        tags: ["Docs"],
                        body: "\n# Docs\n\nRepair published fixture.\n"));
                workspace.WriteText(
                    SourcePath,
                    OpenForgeDocumentSeed.Metadata(
                        description: "Source",
                        tags: ["Docs"],
                        body: BuildSource(includeSafeExact, includeGuided)));
                workspace.WriteText(
                    SafeTargetPath,
                    OpenForgeDocumentSeed.Metadata(
                        description: "Guide",
                        tags: ["Docs"],
                        body: "\n# Guide\n\nSafe exact target.\n"));
                workspace.WriteText(
                    GuidedTargetPath,
                    OpenForgeDocumentSeed.Metadata(
                        description: "Guided",
                        tags: ["Docs"],
                        body: "\n# Guided\n\nGuided target.\n"));
                workspace.WriteBytes(UnrelatedPath, UnrelatedBytes);
                return new RepairWorkspace(workspace, lockStore);
            }
            catch
            {
                lockStore.Dispose();
                workspace.Dispose();
                throw;
            }
        }

        internal string Combine(string relativePath)
            => _workspace.Combine(relativePath);

        internal string ReadText(string relativePath)
            => File.ReadAllText(_workspace.Combine(relativePath));

        internal byte[] ReadBytes(string relativePath)
            => File.ReadAllBytes(_workspace.Combine(relativePath));

        internal SortedDictionary<string, string> SnapshotState()
        {
            var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var (path, hash) in _workspace.SnapshotHashes())
            {
                state[$"workspace/{path}"] = hash;
            }

            state["lock"] = PathState(_lockPath);
            state["recovery"] = PathState(RecoveryDirectory());
            return state;
        }

        internal void AssertNoWriteInfrastructure()
        {
            _lockStore.AssertNoInfrastructure();
            _lockStore.AssertNoRecoveryArtifacts(Path);
            Assert.False(File.Exists(_workspace.Combine(".agents/open-forge.lock")));
        }

        internal void AssertPersistentExternalLock()
            => _lockStore.AssertPersistentZeroByteLock(Path);

        internal void AssertNoRecoveryArtifacts()
            => _lockStore.AssertNoRecoveryArtifacts(Path);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _lockStore.RemoveRecoveryArtifacts(Path);
            }
            finally
            {
                _lockStore.Dispose();
                _workspace.Dispose();
                _disposed = true;
            }
        }

        private static string BuildSource(bool includeSafeExact, bool includeGuided)
        {
            var links = new List<string>();
            if (includeSafeExact)
            {
                links.Add(SafeLink);
            }

            if (includeGuided)
            {
                links.Add(GuidedLink);
            }

            return SourceBodyPrefix
                + string.Join('\n', links)
                + (links.Count == 0 ? string.Empty : "\n")
                + UnrelatedLine;
        }

        private string RecoveryDirectory()
        {
            var localApplicationData = EnvironmentVariables.TryGetValue(
                "XDG_DATA_HOME",
                out var configured)
                ? configured
                : Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.DoNotVerify);
            var normalized = System.IO.Path.TrimEndingDirectorySeparator(
                System.IO.Path.GetFullPath(Path));
            var identity = OperatingSystem.IsWindows() ? normalized.ToUpperInvariant() : normalized;
            var key = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
            return System.IO.Path.Combine(
                localApplicationData,
                "OpenForge",
                "recovery",
                "v1",
                key);
        }

        private static string PathState(string path)
        {
            if (File.Exists(path))
            {
                return $"file:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}";
            }

            if (!Directory.Exists(path))
            {
                return "absent";
            }

            var entries = Directory
                .EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories)
                .Order(StringComparer.Ordinal)
                .Select(entry =>
                {
                    var relative = System.IO.Path.GetRelativePath(path, entry).Replace('\\', '/');
                    return File.Exists(entry)
                        ? $"file:{relative}:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(entry)))}"
                        : $"directory:{relative}";
                });
            return $"directory:{string.Join('|', entries)}";
        }
    }
}
