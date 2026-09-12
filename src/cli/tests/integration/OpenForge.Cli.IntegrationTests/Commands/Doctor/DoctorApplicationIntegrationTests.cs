using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class DoctorApplicationIntegrationTests
{
    [Fact(DisplayName = "Doctor diagnoses a representative workspace with six retained domains and no writes"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    public async Task RepresentativeWorkspaceRetainsSixDomainsWithoutWrites()
    {
        using var workspace = CreateWorkspace("doctor-representative");
        var before = SnapshotState(workspace);

        var run = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--json"],
            workspace.Path);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(string.Empty, run.Error);
        using var document = JsonDocument.Parse(run.Output);
        AssertDoctorGraph(document.RootElement, "incomplete");
        Assert.Equal(before, SnapshotState(workspace));
    }

    [Fact(DisplayName = "Doctor retains every domain when the selected workspace is blocked"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    public async Task BlockedWorkspaceRetainsEveryDomain()
    {
        using var workspace = TemporaryWorkspace.Create("doctor-blocked-workspace");
        var file = workspace.CreateFile("workspace.txt", "not a directory");
        var before = SnapshotState(workspace);

        var run = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", file, "--json"],
            workspace.Path);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(string.Empty, run.Error);
        using var document = JsonDocument.Parse(run.Output);
        AssertDoctorGraph(document.RootElement, "blocked");
        Assert.Equal(before, SnapshotState(workspace));
    }

    [Fact(DisplayName = "Doctor repeats byte-for-byte and leaves workspace and recovery boundaries unchanged"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    public async Task RepeatedDiagnosisIsByteForByteAndReadOnly()
    {
        using var workspace = CreateWorkspace("doctor-repeatable");
        var before = SnapshotState(workspace);

        var first = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--json"],
            workspace.Path);
        var second = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--json"],
            workspace.Path);

        Assert.Equal(3, first.ExitCode);
        Assert.Equal(string.Empty, first.Error);
        using var firstDocument = JsonDocument.Parse(first.Output);
        AssertDoctorGraph(firstDocument.RootElement, "incomplete");
        Assert.Equal(3, second.ExitCode);
        Assert.Equal(string.Empty, second.Error);
        using var secondDocument = JsonDocument.Parse(second.Output);
        AssertDoctorGraph(secondDocument.RootElement, "incomplete");
        Assert.Equal(first.ExitCode, second.ExitCode);
        Assert.Equal(first.Output, second.Output);
        Assert.Equal(first.Error, second.Error);
        Assert.Equal(before, SnapshotState(workspace));
    }

    private static TemporaryWorkspace CreateWorkspace(string purpose)
    {
        var workspace = TemporaryWorkspace.Create(purpose);
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
        return workspace;
    }

    private static IReadOnlyDictionary<string, string> SnapshotState(
        TemporaryWorkspace workspace)
    {
        var cliWorkspace = new OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace(
            workspace.Path,
            workspace.Path,
            OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var recoveryDirectory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(cliWorkspace);
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["workspace-lock"] = PathState(workspace.Combine(".agents/open-forge.lock")),
            ["lifecycle"] = PathState(workspace.Combine(".agents/open-forge.lifecycle.json")),
            ["recovery"] = PathState(recoveryDirectory),
        };
        foreach (var (path, hash) in workspace.SnapshotHashes())
        {
            state[$"workspace/{path}"] = hash;
        }

        return state;
    }

    private static string PathState(string path)
    {
        if (File.Exists(path))
        {
            return $"file:{Convert.ToBase64String(File.ReadAllBytes(path))}";
        }

        return Directory.Exists(path) ? "directory" : "absent";
    }

    private static void AssertDoctorGraph(JsonElement root, string status)
    {
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("doctor", root.GetProperty("command").GetString());
        Assert.Equal(status, root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
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
        foreach (var finding in result.GetProperty("domains")
                     .EnumerateArray()
                     .SelectMany(domain => domain.GetProperty("findings").EnumerateArray()))
        {
            Assert.NotEmpty(finding.GetProperty("evidence").EnumerateArray());
        }
    }
}
