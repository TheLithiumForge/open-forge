using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallRecoveryHistoryIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install preserves an existing verified recovery final")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task VerifiedFinalHistoryDoesNotBlockInstall()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-verified-recovery-history");
        Assert.Equal(0, (await workspace.RunAsync(["install", "--automatic", "--frontmatter", "scoped"])).ExitCode);
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-verified-recovery-history-source");
        const string payload = ".agents/toolkit.md";
        source.AddPackage("toolkit", [], (payload, Document("Toolkit")));
        var preparation = await ExtensionRecoveryHistoryFixture.CreateVerifiedFinalAsync(
            workspace,
            ExtensionInstallDefinitions.CommandIdentity,
            RecoveryBundleOperation.Install,
            TestContext.Current.CancellationToken);
        var priorBytes = await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken);
        var catalogue = await RecoveryBundleCatalogue.ReadAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleCatalogueState.Available, catalogue.State);
        Assert.Contains(catalogue.Candidates, candidate => candidate.Path == preparation.BundlePath
            && candidate.Kind == RecoveryBundleCandidateKind.Final
            && candidate.Integrity == RecoveryBundleIntegrity.Verified
            && candidate.Verified is not null);
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Empty(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal(Document("Toolkit"), workspace.ReadText(payload));
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal(priorBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Install warns about unverified recovery history at its exact path")]
    [InlineData("malformed")]
    [InlineData("draft")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task UnverifiedRecoveryHistoryWarnsAndAllowsChanges(string candidateKind)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-install-unverified-recovery-{candidateKind}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-install-unverified-recovery-{candidateKind}-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit")));
        var kind = candidateKind == "draft"
            ? RecoveryBundleCandidateKind.Draft
            : RecoveryBundleCandidateKind.Final;
        var candidatePath = ExtensionRecoveryHistoryFixture.CandidatePath(
            workspace.Workspace,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            kind);
        var candidateBytes = candidateKind == "draft"
            ? "incomplete draft"u8.ToArray()
            : [0x01, 0x02, 0x03, 0x04];
        await File.WriteAllBytesAsync(candidatePath, candidateBytes, TestContext.Current.CancellationToken);
        var beforeSource = source.Snapshot();

        await ExtensionRecoveryHistoryFixture.MatchHistoryAsync(workspace, source.Path,
            new ReadOutputScenario
            {
                Situation = candidateKind,
                Arguments = ["extension", "install", "toolkit", "--source", source.Path, "--dry-run"],
                ExitCode = 2,
            }, testName: $"UnverifiedHistory.{candidateKind}");

        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("extension-install.recovery-history", finding.GetProperty("code").GetString());
        Assert.Equal(candidatePath, finding.GetProperty("subject").GetProperty("path").GetString());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(candidateBytes, await File.ReadAllBytesAsync(candidatePath, TestContext.Current.CancellationToken));
        Assert.True(File.Exists(workspace.Combine(".agents/toolkit.md")));
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");
}
