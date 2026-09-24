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
        await workspace.SeedFrameworkAsync();
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
        Assert.Equal(Document("Toolkit"), workspace.ReadText(payload));
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal(priorBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Install blocks unverified recovery history at its exact path")]
    [InlineData("malformed")]
    [InlineData("draft")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task UnverifiedRecoveryHistoryBlocksWriteFree(string candidateKind)
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
            Guid.NewGuid(),
            kind);
        var candidateBytes = candidateKind == "draft"
            ? "incomplete draft"u8.ToArray()
            : [0x01, 0x02, 0x03, 0x04];
        await File.WriteAllBytesAsync(candidatePath, candidateBytes, TestContext.Current.CancellationToken);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("extension-install.recovery-conflict", finding.GetProperty("code").GetString());
        Assert.Equal(candidatePath, finding.GetProperty("subject").GetProperty("path").GetString());
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(candidateBytes, await File.ReadAllBytesAsync(candidatePath, TestContext.Current.CancellationToken));
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");
}
