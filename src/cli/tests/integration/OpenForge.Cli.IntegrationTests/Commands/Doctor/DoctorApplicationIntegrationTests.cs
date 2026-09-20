using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class DoctorApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Doctor diagnoses a representative workspace with six retained domains and no writes"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    public async Task RepresentativeWorkspaceRetainsSixDomainsWithoutWrites()
    {
        using var workspace = CreateWorkspace("doctor-representative");
        var before = SnapshotState(workspace);

        var run = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--format", "json", "--detail", "full"],
            workspace.Path);

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(string.Empty, run.Error);
        using var document = JsonDocument.Parse(run.Output);
        AssertDoctorGraph(document.RootElement, "completed-with-warnings");
        Assert.Equal(before, SnapshotState(workspace));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Doctor retains every domain when the selected workspace is blocked"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    public async Task BlockedWorkspaceRetainsEveryDomain()
    {
        using var workspace = TemporaryWorkspace.Create("doctor-blocked-workspace");
        var file = workspace.CreateFile("workspace.txt", "not a directory");
        var before = SnapshotState(workspace);

        var run = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", file, "--format", "json", "--detail", "full"],
            workspace.Path);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(string.Empty, run.Error);
        using var document = JsonDocument.Parse(run.Output);
        AssertDoctorGraph(document.RootElement, "blocked");
        Assert.Equal(before, SnapshotState(workspace));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Doctor repeats byte-for-byte and leaves workspace and recovery boundaries unchanged"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    public async Task RepeatedDiagnosisIsByteForByteAndReadOnly()
    {
        using var workspace = CreateWorkspace("doctor-repeatable");
        var before = SnapshotState(workspace);

        var first = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--format", "json", "--detail", "full"],
            workspace.Path);
        var second = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--format", "json", "--detail", "full"],
            workspace.Path);

        Assert.Equal(2, first.ExitCode);
        Assert.Equal(string.Empty, first.Error);
        using var firstDocument = JsonDocument.Parse(first.Output);
        AssertDoctorGraph(firstDocument.RootElement, "completed-with-warnings");
        Assert.Equal(2, second.ExitCode);
        Assert.Equal(string.Empty, second.Error);
        using var secondDocument = JsonDocument.Parse(second.Output);
        AssertDoctorGraph(secondDocument.RootElement, "completed-with-warnings");
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
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("doctor", root.GetProperty("command").GetString());
        Assert.Equal(status, root.GetProperty("status").GetString());
        var next = root.GetProperty("next");
        if (status == "blocked")
        {
            Assert.Equal(JsonValueKind.Null, next.ValueKind);
        }
        else
        {
            Assert.Equal(JsonValueKind.Object, next.ValueKind);
            Assert.Equal("command", next.GetProperty("kind").GetString());
            Assert.False(string.IsNullOrWhiteSpace(next.GetProperty("command").GetString()));
        }
        var result = root.GetProperty("data");
        var categories = result.GetProperty("categories").EnumerateArray().ToArray();
        Assert.Equal(
            [
                "Workspace",
                "Recovery data",
                "Routes and Entries",
                "Links",
                "Framework files",
                "Extensions",
            ],
            categories.Select(category => category.GetProperty("name").GetString()));
        Assert.Equal(status == "completed-with-warnings" ? "complete" : status,
            categories[0].GetProperty("coverage").GetString());
        foreach (var finding in root.GetProperty("findings").EnumerateArray())
        {
            Assert.NotEmpty(finding.GetProperty("evidence").EnumerateArray());
        }
    }
}
