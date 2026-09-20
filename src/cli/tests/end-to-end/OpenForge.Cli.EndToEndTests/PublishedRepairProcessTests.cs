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
            ["repair", "--automatic", "--dry-run", "--format=json"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("repair", root.GetProperty("command").GetString());
        Assert.Equal("completed-with-warnings", root.GetProperty("status").GetString());
        Assert.Equal(
            "Would repair 1 link. 1 problem still needs a choice.",
            root.GetProperty("summary").GetProperty("headline").GetString());
        // The native report publishes the command's own facts: the safe-exact rewrite is a
        // planned effect, the guided link is a remaining problem, and `data` carries only the
        // Repair-owned projection rather than the whole internal result.
        var repair = root.GetProperty("data");
        Assert.Equal("dry-run", repair.GetProperty("mode").GetString());
        Assert.Equal("automatic", repair.GetProperty("selection").GetString());
        var repaired = Assert.Single(repair.GetProperty("repairs").EnumerateArray());
        Assert.Equal(RepairWorkspace.SourcePath, repaired.GetProperty("path").GetString());
        Assert.Equal("./guide.md", repaired.GetProperty("from").GetString());
        Assert.Equal("guide.md", repaired.GetProperty("to").GetString());
        var remaining = Assert.Single(repair.GetProperty("remaining").EnumerateArray());
        Assert.Equal(RepairWorkspace.SourcePath, remaining.GetProperty("path").GetString());
        Assert.Equal("guided", remaining.GetProperty("kind").GetString());
        Assert.Equal(4, remaining.GetProperty("candidates").GetInt32());
        // A dry run plans the effect without applying it.
        var effect = Assert.Single(root.GetProperty("effects").EnumerateArray());
        Assert.Equal("link", effect.GetProperty("kind").GetString());
        Assert.Equal("rewritten", effect.GetProperty("action").GetString());
        Assert.Equal("planned", effect.GetProperty("outcome").GetString());
        var counts = root.GetProperty("counts");
        Assert.Equal(1, counts.GetProperty("linksRepaired").GetInt32());
        Assert.Equal(1, counts.GetProperty("problemsRemaining").GetInt32());
        Assert.Equal(1, counts.GetProperty("problemsNeedingChoice").GetInt32());
        Assert.Equal(0, counts.GetProperty("problemsNeedingHand").GetInt32());
        var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());
        Assert.Equal("repair.guided-finding-remaining", finding.GetProperty("code").GetString());
        // Nothing was written, so no recovery bundle was required.
        Assert.Equal("not-required", root.GetProperty("recovery").GetProperty("disposition").GetString());
        working.AssertNoWriteInfrastructure();

        var textResult = await RunWithoutWritesAsync(
            target,
            working,
            ["repair", "--automatic", "--dry-run"]);
        Assert.Equal(2, textResult.ExitCode);
        Assert.Equal(string.Empty, textResult.StandardError);
        Assert.Equal(
            "Would repair 1 link. 1 problem still needs a choice.",
            textResult.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].TrimEnd('\r'));
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
                "--format=json",
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
        Assert.Equal("completed", appliedRoot.GetProperty("status").GetString());
        var appliedResult = appliedRoot.GetProperty("data");
        Assert.Equal("apply", appliedResult.GetProperty("mode").GetString());
        Assert.Equal("relink", appliedResult.GetProperty("selection").GetString());
        var appliedRepair = Assert.Single(appliedResult.GetProperty("repairs").EnumerateArray());
        Assert.Equal(RepairWorkspace.GuidedExpectedDestination, appliedRepair.GetProperty("from").GetString());
        Assert.Equal("replacement.md", appliedRepair.GetProperty("to").GetString());
        Assert.Empty(appliedResult.GetProperty("remaining").EnumerateArray());
        var appliedEffect = Assert.Single(appliedRoot.GetProperty("effects").EnumerateArray());
        Assert.Equal("rewritten", appliedEffect.GetProperty("action").GetString());
        Assert.Equal("done", appliedEffect.GetProperty("outcome").GetString());
        Assert.Equal(1, appliedRoot.GetProperty("counts").GetProperty("linksRepaired").GetInt32());
        Assert.Equal(0, appliedRoot.GetProperty("counts").GetProperty("problemsRemaining").GetInt32());
        Assert.Empty(appliedRoot.GetProperty("findings").EnumerateArray());
        // The recovery bundle is removed once the repair verifies.
        Assert.Equal("removed", appliedRoot.GetProperty("recovery").GetProperty("disposition").GetString());
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
            ["repair", "--automatic", "--format=json"]);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        using var noOpDocument = JsonDocument.Parse(noOp.StandardOutput);
        var noOpRoot = noOpDocument.RootElement;
        Assert.Equal("completed", noOpRoot.GetProperty("status").GetString());
        // Converged: the second automatic pass finds nothing left to repair, so it plans no
        // effect, reports no remaining problem, and needs no recovery bundle.
        Assert.Equal("Nothing to repair.", noOpRoot.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Equal("nothing-to-do", noOpRoot.GetProperty("summary").GetProperty("kind").GetString());
        Assert.Empty(noOpRoot.GetProperty("effects").EnumerateArray());
        Assert.Empty(noOpRoot.GetProperty("findings").EnumerateArray());
        var noOpCounts = noOpRoot.GetProperty("counts");
        Assert.Equal(0, noOpCounts.GetProperty("linksRepaired").GetInt32());
        Assert.Equal(0, noOpCounts.GetProperty("problemsRemaining").GetInt32());
        Assert.Equal("not-required", noOpRoot.GetProperty("recovery").GetProperty("disposition").GetString());
        var noOpResult = noOpRoot.GetProperty("data");
        Assert.Equal("apply", noOpResult.GetProperty("mode").GetString());
        Assert.Empty(noOpResult.GetProperty("repairs").EnumerateArray());
        Assert.Empty(noOpResult.GetProperty("remaining").EnumerateArray());
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

        private string RecoveryDirectory() => _lockStore.RecoveryWorkspaceDirectory(Path);

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
