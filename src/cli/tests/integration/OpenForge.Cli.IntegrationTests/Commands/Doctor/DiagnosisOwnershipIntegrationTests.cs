using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class DiagnosisOwnershipIntegrationTests
{
    private const string RootPath = ".agents/docs/_docs.md";
    private const string SourcePath = ".agents/docs/source.md";
    private const string LibraryRecordPath = ".agents/open-forge.lock.json";
    private const string LibrarySourceRootPath = "shared/library";

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Doctor, Index, and Repair hand off one workspace diagnosis to its owning fix"),
     Trait("Feature", "diagnosis-ownership"), Trait("Evidence", "Integration")]
    public async Task SharedWorkspaceHandoffUsesReportAndFixOwners()
    {
        using var workspace = CreateWorkspace("diagnosis-ownership-handoff");

        var beforeDiagnosis = workspace.SnapshotHashes();
        var initialDiagnosis = await RunDoctorAsync(workspace);
        Assert.Equal(beforeDiagnosis, workspace.SnapshotHashes());
        Assert.Contains("route.generated-region-stale", FindingCodes(initialDiagnosis));
        Assert.Contains("reference.same-target-path", FindingCodes(initialDiagnosis));

        var rootBeforeIndex = File.ReadAllText(workspace.Combine(RootPath));
        var sourceBeforeIndex = File.ReadAllText(workspace.Combine(SourcePath));
        var index = await CliHostCapture.RunAsync(
            ["index", RootPath],
            workspace.Path);
        Assert.Equal(0, index.ExitCode);
        Assert.Equal(string.Empty, index.Error);
        Assert.NotEqual(rootBeforeIndex, File.ReadAllText(workspace.Combine(RootPath)));
        Assert.Equal(sourceBeforeIndex, File.ReadAllText(workspace.Combine(SourcePath)));

        var beforeSecondDiagnosis = workspace.SnapshotHashes();
        var afterIndexDiagnosis = await RunDoctorAsync(workspace);
        Assert.Equal(beforeSecondDiagnosis, workspace.SnapshotHashes());
        Assert.DoesNotContain(
            FindingCodes(afterIndexDiagnosis),
            code => code.StartsWith("route.generated-entry-", StringComparison.Ordinal)
                || code is "route.generated-region-stale" or "route.generated-region-missing");
        Assert.Contains("reference.same-target-path", FindingCodes(afterIndexDiagnosis));

        var rootBeforeRepair = File.ReadAllText(workspace.Combine(RootPath));
        var repair = await CliHostCapture.RunAsync(
            ["repair", "--automatic", "--format", "json"],
            workspace.Path);
        Assert.Equal(0, repair.ExitCode);
        Assert.Equal(string.Empty, repair.Error);
        using (var repairDocument = JsonDocument.Parse(repair.Output))
        {
            var repairData = repairDocument.RootElement.GetProperty("data");
            var repaired = Assert.Single(repairData.GetProperty("repairs").EnumerateArray());
            Assert.Equal(SourcePath, repaired.GetProperty("path").GetString());
            Assert.Equal("./guide.md", repaired.GetProperty("from").GetString());
            Assert.Equal("guide.md", repaired.GetProperty("to").GetString());
            Assert.Empty(repairData.GetProperty("remaining").EnumerateArray());
        }

        Assert.Equal(rootBeforeRepair, File.ReadAllText(workspace.Combine(RootPath)));
        Assert.DoesNotContain("[Guide](./guide.md)", File.ReadAllText(workspace.Combine(SourcePath)), StringComparison.Ordinal);
        Assert.Contains("[Guide](guide.md)", File.ReadAllText(workspace.Combine(SourcePath)), StringComparison.Ordinal);

        var beforeFinalDiagnosis = workspace.SnapshotHashes();
        var finalDiagnosis = await RunDoctorAsync(workspace);
        Assert.Equal(beforeFinalDiagnosis, workspace.SnapshotHashes());
        Assert.DoesNotContain(
            FindingCodes(finalDiagnosis),
            code => code.StartsWith("route.generated-", StringComparison.Ordinal));
        Assert.DoesNotContain("reference.same-target-path", FindingCodes(finalDiagnosis));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Repair blocks required and completes with unrelated Doctor blocking findings"),
     Trait("Feature", "diagnosis-ownership"), Trait("Evidence", "Integration")]
    public async Task RepairDoesNotReportCompletionOverBlockingDoctorFinding()
    {
        using var workspace = CreateWorkspace("diagnosis-ownership-blocked-repair");
        var rootPath = workspace.Combine(RootPath);
        var validRoot = File.ReadAllText(rootPath);
        File.AppendAllText(rootPath, "\n## Entries\n- duplicate\n");

        var beforeRepair = await RunDoctorAsync(workspace);
        AssertBlockingFinding(beforeRepair, "route.generated-region-duplicate");

        var repair = await CliHostCapture.RunAsync(
            ["repair", "--automatic", "--format", "json"],
            workspace.Path);
        using (var repairDocument = JsonDocument.Parse(repair.Output))
        {
            Assert.Equal("blocked", repairDocument.RootElement.GetProperty("status").GetString());
        }
        Assert.Contains("[Guide](./guide.md)", File.ReadAllText(workspace.Combine(SourcePath)), StringComparison.Ordinal);

        var afterRepair = await RunDoctorAsync(workspace);
        AssertBlockingFinding(afterRepair, "route.generated-region-duplicate");

        File.WriteAllText(rootPath, validRoot);
        workspace.CreateDirectory(LibrarySourceRootPath);
        workspace.WriteText(
            LibraryRecordPath,
            """{"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/library","destinationRoot":".","paths":[".agents/docs/source.md"]}]}""");

        var unrelatedBeforeRepair = await RunDoctorAsync(workspace);
        AssertBlockingFinding(unrelatedBeforeRepair, "library.projection-retargeted");

        var unrelatedRepair = await CliHostCapture.RunAsync(
            ["repair", "--automatic", "--format", "json"],
            workspace.Path);
        Assert.Equal(2, unrelatedRepair.ExitCode);
        Assert.Equal(string.Empty, unrelatedRepair.Error);
        Assert.Equal("completed-with-warnings", Status(unrelatedRepair));
        Assert.DoesNotContain("[Guide](./guide.md)", File.ReadAllText(workspace.Combine(SourcePath)), StringComparison.Ordinal);
        Assert.Contains("[Guide](guide.md)", File.ReadAllText(workspace.Combine(SourcePath)), StringComparison.Ordinal);

        var unrelatedAfterRepair = await RunDoctorAsync(workspace);
        AssertBlockingFinding(unrelatedAfterRepair, "library.projection-retargeted");
        Assert.DoesNotContain("reference.same-target-path", FindingCodes(unrelatedAfterRepair));
    }

    private static async Task<CliHostCaptureResult> RunDoctorAsync(TemporaryWorkspace workspace)
        => await CliHostCapture.RunAsync(
            ["doctor", "--format", "json", "--detail", "full"],
            workspace.Path);

    private static IReadOnlyList<string> FindingCodes(CliHostCaptureResult result)
    {
        using var document = JsonDocument.Parse(result.Output);
        return document.RootElement
            .GetProperty("findings")
            .EnumerateArray()
            .Select(finding => finding.GetProperty("code").GetString()
                ?? throw new InvalidOperationException("A diagnosis finding had no code."))
            .ToArray();
    }

    private static void AssertBlockingFinding(CliHostCaptureResult result, string code)
    {
        using var document = JsonDocument.Parse(result.Output);
        var finding = Assert.Single(document.RootElement
            .GetProperty("findings")
            .EnumerateArray(),
            candidate => candidate.GetProperty("code").GetString() == code);
        Assert.Equal("blocked-repair", finding.GetProperty("resolution").GetString());
    }

    private static string Status(CliHostCaptureResult result)
    {
        using var document = JsonDocument.Parse(result.Output);
        return document.RootElement.GetProperty("status").GetString()
            ?? throw new InvalidOperationException("A command result had no status.");
    }

    private static TemporaryWorkspace CreateWorkspace(string purpose)
    {
        var workspace = TemporaryWorkspace.Create(purpose);
        workspace.WriteText("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.WriteText(
            ".agents/loader.md",
            GeneratedLoaderDocumentBuilder.Build("- [Docs](docs/_docs.md) - #Docs"));
        workspace.WriteText(
            RootPath,
            OpenForgeDocumentSeed.Metadata(
                description: "Docs",
                tags: ["Docs"],
                body: $"\n{OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                {
                    Entries = "- stale",
                    Prefix = "# Docs",
                })}"));
        workspace.WriteText(
            SourcePath,
            OpenForgeDocumentSeed.Metadata(
                description: "Source",
                tags: ["Docs"],
                body: "\n# Source\n\n[Guide](./guide.md)\n"));
        workspace.WriteText(
            ".agents/docs/guide.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Guide",
                tags: ["Docs"],
                body: "\n# Guide\n"));
        return workspace;
    }
}
