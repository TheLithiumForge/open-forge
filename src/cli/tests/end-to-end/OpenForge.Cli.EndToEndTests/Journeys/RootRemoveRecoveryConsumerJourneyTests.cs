using System.IO.Compression;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class RootRemoveRecoveryConsumerJourneyTests
{
    private const string Feature = "root-remove-recovery-consumers";

    [Fact(DisplayName = "Root remove recovery is read by status and doctor, then removed by cleanup"), Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task RetainedWorkspaceBundleRemainsConsumable()
    {
        using var workspace = PublishedJourneyWorkspace.Create("root-remove-recovery-consumer");
        const string targetPath = "retained-note.md";
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(".agents/open-forge.json", targetPath);
        workspace.WriteText(targetPath, "This file is captured before removal.\n");

        var install = await workspace.RunAsync("install", "--automatic");
        Assert.Equal(0, install.ExitCode);
        Assert.Empty(install.StandardError);

        var removed = await workspace.RunAsync(
            "remove",
            targetPath,
            "--automatic",
            "--format=json",
            "--detail=full");

        Assert.Equal(0, removed.ExitCode);
        Assert.Empty(removed.StandardError);
        Assert.False(File.Exists(workspace.Combine(targetPath)));

        var recoveryDirectory = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
        var bundlePath = Assert.Single(Directory.EnumerateFiles(
            recoveryDirectory,
            "*.zip",
            SearchOption.TopDirectoryOnly));
        using (var archive = ZipFile.OpenRead(bundlePath))
        {
            var manifestEntry = archive.GetEntry("manifest.json");
            Assert.NotNull(manifestEntry);
            using var manifestStream = manifestEntry!.Open();
            using var manifest = JsonDocument.Parse(manifestStream);
            var attribution = manifest.RootElement.GetProperty("attribution");
            Assert.Equal("workspace", attribution.GetProperty("producer").GetString());
            Assert.Equal("remove", attribution.GetProperty("operation").GetString());
        }

        var status = await workspace.RunAsync("status", "--format=json", "--detail=full");
        using (var statusDocument = JsonDocument.Parse(status.StandardOutput))
        {
            var candidates = statusDocument.RootElement
                .GetProperty("data")
                .GetProperty("recovery")
                .GetProperty("candidates")
                .EnumerateArray()
                .ToArray();
            Assert.Contains(
                candidates,
                candidate => candidate.GetProperty("path").GetString() == bundlePath
                    && candidate.GetProperty("kind").GetString() == "final"
                    && candidate.GetProperty("integrity").GetString() == "verified");
        }

        var doctor = await workspace.RunAsync("doctor", "--format=json", "--detail=full");
        using (var doctorDocument = JsonDocument.Parse(doctor.StandardOutput))
        {
            var recoveryFindings = doctorDocument.RootElement
                .GetProperty("findings")
                .EnumerateArray()
                .Where(finding => finding.GetProperty("category").GetString() == "Recovery data")
                .ToArray();
            Assert.Contains(
                recoveryFindings,
                finding => finding.GetProperty("code").GetString() == "recovery.bundle-recognized");
        }

        var preview = await workspace.RunAsync("cleanup", "--dry-run", "--format=json", "--detail=full");
        using (var previewDocument = JsonDocument.Parse(preview.StandardOutput))
        {
            var items = previewDocument.RootElement
                .GetProperty("data")
                .GetProperty("items")
                .EnumerateArray()
                .ToArray();
            Assert.Contains(
                items,
                item => item.GetProperty("path").GetString() == bundlePath
                    && item.GetProperty("kind").GetString() == "bundle"
                    && item.GetProperty("outcome").GetString() == "would-be-removed");
        }
        Assert.True(File.Exists(bundlePath));

        var cleanup = await workspace.RunAsync("cleanup", "--format=json", "--detail=full");
        Assert.Equal(0, cleanup.ExitCode);
        Assert.Empty(cleanup.StandardError);
        Assert.False(File.Exists(bundlePath));

        var statusAfterCleanup = await workspace.RunAsync("status", "--format=json", "--detail=full");
        using (var statusDocument = JsonDocument.Parse(statusAfterCleanup.StandardOutput))
        {
            var candidates = statusDocument.RootElement
                .GetProperty("data")
                .GetProperty("recovery")
                .GetProperty("candidates")
                .EnumerateArray();
            Assert.DoesNotContain(candidates, candidate => candidate.GetProperty("path").GetString() == bundlePath);
        }

        var doctorAfterCleanup = await workspace.RunAsync("doctor", "--format=json", "--detail=full");
        using (var doctorDocument = JsonDocument.Parse(doctorAfterCleanup.StandardOutput))
        {
            var recoveryFindings = doctorDocument.RootElement
                .GetProperty("findings")
                .EnumerateArray()
                .Where(finding => finding.GetProperty("category").GetString() == "Recovery data");
            Assert.DoesNotContain(
                recoveryFindings,
                finding => finding.GetProperty("code").GetString() == "recovery.bundle-recognized");
        }
    }
}
