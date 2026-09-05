using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedDoctorProcessTests
{
    [Fact(DisplayName = "Published Doctor help is reachable without inspecting a workspace"), Trait("Feature", "doctor-command"), Trait("Evidence", "EndToEnd")]
    public async Task HelpIsReachableWithoutWorkspaceInspection()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = DoctorWorkspace.Create();
        var missingWorkspace = working.Combine("missing-help-workspace");

        var result = await RunWithoutWritesAsync(
            target,
            working,
            ["doctor", "--help", "--workspace", missingWorkspace]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("open-forge doctor", result.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Fact(DisplayName = "Published Doctor JSON retains six domains and reports a read-only result"), Trait("Feature", "doctor-command"), Trait("Evidence", "EndToEnd")]
    public async Task JsonJourneyRetainsSixDomainsAndReadOnlyResult()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = DoctorWorkspace.Create();

        var result = await RunWithoutWritesAsync(
            target,
            working,
            ["doctor", "--json"]);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        AssertDoctorGraph(document.RootElement, "incomplete");
        AssertExtensionObservationHorizons(document.RootElement);
    }

    [Fact(DisplayName = "Published Doctor keeps invalid and blocked journeys on the contracted error stream"), Trait("Feature", "doctor-command"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidGrammarAndBlockedWorkspaceUseErrorStream()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = DoctorWorkspace.Create();
        var invalid = await RunWithoutWritesAsync(
            target,
            working,
            ["doctor", "unexpected"]);

        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.NotEqual(string.Empty, invalid.StandardError);

        var selectedFile = working.CreateFile("selected-workspace", "not a directory");
        var blocked = await RunWithoutWritesAsync(
            target,
            working,
            ["doctor", "--workspace", selectedFile]);

        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(string.Empty, blocked.StandardOutput);
        Assert.NotEqual(string.Empty, blocked.StandardError);
    }

    private static async Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        DoctorWorkspace working,
        IReadOnlyList<string> arguments)
    {
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            arguments,
            working.EnvironmentVariables);
        working.AssertNoInfrastructure();
        return result;
    }

    private static void AssertDoctorGraph(JsonElement root, string status)
    {
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("doctor", root.GetProperty("command").GetString());
        Assert.Equal(status, root.GetProperty("status").GetString());
        var result = root.GetProperty("result");
        Assert.True(result.GetProperty("readOnly").GetBoolean());
        Assert.False(result.GetProperty("changesMade").GetBoolean());
        Assert.Equal(status, result.GetProperty("coverage").GetString());
        Assert.Equal(
            [
                "workspace-entry",
                "recovery-residuals",
                "routes-metadata-overwrites-generated-navigation",
                "local-references",
                "framework-lifecycle",
                "extension-lifecycle",
            ],
            result.GetProperty("domains")
                .EnumerateArray()
                .Select(domain => domain.GetProperty("domain").GetString()));
    }

    private static void AssertExtensionObservationHorizons(JsonElement root)
    {
        const string roleHorizon =
            "Typed Extension bridge-registration role and observed-state authority is unavailable; no target role was inferred.";
        const string manifestHorizon =
            "A bounded contained Extension manifest candidate universe is unavailable; no installed manifest scan was inferred.";
        var extension = Assert.Single(
            root.GetProperty("result")
                .GetProperty("domains")
                .EnumerateArray(),
            domain => string.Equals(
                domain.GetProperty("domain").GetString(),
                "extension-lifecycle",
                StringComparison.Ordinal));
        var limitations = extension.GetProperty("limitations")
            .EnumerateArray()
            .Select(limitation => limitation.GetProperty("message").GetString())
            .ToArray();
        Assert.Equal(
            [roleHorizon, manifestHorizon],
            limitations.Where(message => message is roleHorizon or manifestHorizon));
    }

    private sealed class DoctorWorkspace : IDisposable
    {
        private readonly TemporaryWorkspace _workspace;
        private readonly PublishedWorkspaceLockStore _lockStore;
        private bool _disposed;

        private DoctorWorkspace(
            TemporaryWorkspace workspace,
            PublishedWorkspaceLockStore lockStore)
        {
            _workspace = workspace;
            _lockStore = lockStore;
            _ = lockStore.Track(workspace.Path);
        }

        internal string Path => _workspace.Path;

        internal IReadOnlyDictionary<string, string> EnvironmentVariables
            => _lockStore.EnvironmentVariables;

        internal static DoctorWorkspace Create()
        {
            var workspace = TemporaryWorkspace.Create("doctor-e2e");
            var lockStore = PublishedWorkspaceLockStore.Create("doctor-e2e-lock-store");
            try
            {
                workspace.WriteText(
                    "AGENTS.md",
                    "# Workspace\n\nRead `.agents/loader.md`.\n");
                workspace.WriteText(
                    ".agents/loader.md",
                    GeneratedLoaderDocumentBuilder.Build(
                        "- [Status](status/_status.md) - #Status"));
                workspace.WriteText(
                    ".agents/status/_status.md",
                    OpenForgeDocumentSeed.Metadata(
                        description: "Status",
                        tags: ["Status"],
                        body: "\n# Status\n\nDoctor evidence.\n"));
                return new DoctorWorkspace(workspace, lockStore);
            }
            catch
            {
                lockStore.Dispose();
                workspace.Dispose();
                throw;
            }
        }

        internal string Combine(string relativePath) => _workspace.Combine(relativePath);

        internal string CreateFile(string relativePath, string contents)
            => _workspace.CreateFile(relativePath, contents);

        internal IReadOnlyDictionary<string, string> SnapshotState()
        {
            var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var (path, hash) in _workspace.SnapshotHashes())
            {
                state[$"workspace/{path}"] = hash;
            }

            return state;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _workspace.Dispose();
            }
            finally
            {
                _lockStore.Dispose();
                _disposed = true;
            }
        }

        internal void AssertNoInfrastructure() => _lockStore.AssertNoInfrastructure();
    }
}
