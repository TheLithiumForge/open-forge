using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemovePriorRecoveryIntegrationTests
{
    private const string Target = ".agents/toolkit.md";

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove preserves a valid prior recovery final while retaining its own recovery")]
    public async Task ValidPriorRecoveryFinalDoesNotBlockIndependentRemoval()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-prior-recovery-valid");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-prior-recovery-valid-source");
        source.AddPackage("toolkit", [], (Target, Document("Toolkit v1")));
        await InstallAsync(workspace, source);

        source.ReplacePayload("toolkit", Target, Document("Toolkit v2"));
        var sourceBefore = source.Snapshot();
        var update = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, update.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, update.Status);
        Assert.Equal(string.Empty, update.StandardError);
        using var updateDocument = JsonDocument.Parse(update.StandardOutput);
        var priorPath = updateDocument.RootElement
            .GetProperty("recovery")
            .GetProperty("path")
            .GetString();
        Assert.NotNull(priorPath);
        var priorBytes = File.ReadAllBytes(priorPath!);
        var priorRead = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            priorPath!,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, priorRead.State);

        var remove = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, remove.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, remove.Status);
        Assert.Equal(string.Empty, remove.StandardError);
        using var removeDocument = JsonDocument.Parse(remove.StandardOutput);
        var removePath = removeDocument.RootElement
            .GetProperty("recovery")
            .GetProperty("path")
            .GetString();
        Assert.NotNull(removePath);
        Assert.NotEqual(priorPath, removePath);
        Assert.True(File.Exists(removePath));
        Assert.Equal(priorBytes, File.ReadAllBytes(priorPath!));
        var removeRead = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            removePath!,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, removeRead.State);
        Assert.False(File.Exists(workspace.Combine(Target)));
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(
        DisplayName = "Extension Remove keeps malformed or draft recovery candidates blocking and write-free")]
    [InlineData("malformed")]
    [InlineData("draft")]
    public async Task UnverifiedPriorRecoveryCandidateBlocksRemoval(string candidateKind)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-remove-prior-recovery-{candidateKind}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-remove-prior-recovery-{candidateKind}-source");
        source.AddPackage("toolkit", [], (Target, Document("Toolkit")));
        await InstallAsync(workspace, source);

        var directory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace.Workspace);
        Directory.CreateDirectory(directory);
        var operationId = Guid.NewGuid();
        var fileName = candidateKind == "draft"
            ? RecoveryBundleFormatV1.DraftFileName(operationId)
            : RecoveryBundleFormatV1.FinalFileName(operationId);
        var candidatePath = Path.Combine(directory, fileName);
        var candidateBytes = candidateKind == "draft"
            ? "incomplete draft"u8.ToArray()
            : [0x01, 0x02, 0x03, 0x04];
        File.WriteAllBytes(candidatePath, candidateBytes);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.recovery-conflict");
        Assert.Empty(document.RootElement.GetProperty("data").GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(candidateBytes, File.ReadAllBytes(candidatePath));
        Assert.True(File.Exists(workspace.Combine(Target)));
    }

    private static async Task InstallAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");
}
