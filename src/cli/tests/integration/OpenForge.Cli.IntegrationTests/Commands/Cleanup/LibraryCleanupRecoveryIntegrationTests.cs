using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class LibraryCleanupRecoveryIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("record-create", "attach"), InlineData("record-replace", "sync"), InlineData("record-delete", "detach")]
    [InlineData("link-create", "attach"), InlineData("link-delete", "detach")]
    public static async Task DeletesOnlyVerifiedLibraryBundleAndNeverAppliesItsEntries(string kind, string operation)
    {
        using var workspace = new LibraryResidualWorkspace(dangling: true);
        await workspace.PrepareAsync(kind, operation);
        var before = workspace.Files.Snapshot();
        var run = await CliHostCapture.RunAsync(["cleanup", "--format", "json"], workspace.Files.Path);
        Assert.True(run.ExitCode == 0, $"Expected bundle-only cleanup: {run.ExitCode}; {run.Error}; {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        var items = document.RootElement.GetProperty("data").GetProperty("items");
        Assert.Single(items.EnumerateArray());
        Assert.False(File.Exists(workspace.Preparation.BundlePath));
        Assert.Equal(before, workspace.Files.Snapshot());
    }

    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("record-create"), InlineData("link-create"), InlineData("link-delete")]
    public static async Task DryRunValidatesTypedFinalWithoutChangingBundleOrDestination(string kind)
    {
        using var workspace = new LibraryResidualWorkspace(dangling: true);
        await workspace.PrepareAsync(kind);
        var before = workspace.Files.Snapshot();
        var bundle = File.ReadAllBytes(workspace.Preparation.BundlePath);
        var run = await CliHostCapture.RunAsync(["cleanup", "--dry-run", "--format", "json"], workspace.Files.Path);
        Assert.True(run.ExitCode == 0, $"Expected Cleanup preview: {run.ExitCode}; {run.Error}; {run.Output}");
        Assert.Equal(before, workspace.Files.Snapshot());
        Assert.Equal(bundle, File.ReadAllBytes(workspace.Preparation.BundlePath));
        Assert.Contains("library", run.Output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MalformedSelectedFinalIsPreservedAndNeverUsedAsRecoveryAuthority()
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("link-create");
        File.WriteAllText(workspace.Preparation.BundlePath, "Malformed Library residual, preserve exact bytes.");
        var before = workspace.Files.Snapshot();
        var bundle = File.ReadAllBytes(workspace.Preparation.BundlePath);
        var run = await CliHostCapture.RunAsync(["cleanup", "--format", "json"], workspace.Files.Path);
        Assert.True(run.ExitCode == 2, $"Expected malformed-final warning: {run.ExitCode}; {run.Error}; {run.Output}");
        Assert.Equal(before, workspace.Files.Snapshot());
        Assert.Equal(bundle, File.ReadAllBytes(workspace.Preparation.BundlePath));
    }
    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task LibraryCandidateDeletionRequiresTheRealConsumerLease()
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("link-create");
        var before = workspace.Files.Snapshot();
        var bundle = File.ReadAllBytes(workspace.Preparation.BundlePath);
        CliHostCaptureResult run;
        using (workspace.HoldLock())
        {
            run = await CliHostCapture.RunAsync(["cleanup", "--format", "json"], workspace.Files.Path);
        }

        Assert.True(run.ExitCode == 5, $"Expected Cleanup lease refusal: {run.ExitCode}; {run.Error}; {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "cleanup.workspace-lock-unavailable");
        Assert.Equal(before, workspace.Files.Snapshot());
        Assert.Equal(bundle, File.ReadAllBytes(workspace.Preparation.BundlePath));
    }
}
