using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemovePriorRecoveryIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Remove preserves verified Core Update history while applying managed removal"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    [InlineData(
        RouteRemoveIntegrationWorkspace.LeafId,
        RouteRemoveIntegrationWorkspace.LeafPath,
        false)]
    [InlineData(
        RouteRemoveIntegrationWorkspace.CategoryId,
        RouteRemoveIntegrationWorkspace.CategoryChildPath,
        true)]
    public async Task VerifiedPriorUpdateFinalDoesNotBlockManagedRemoval(
        string sourceReference,
        string archivedPath,
        bool category)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create(
            $"route-remove-prior-verified-{(category ? "category" : "leaf")}");
        workspace.SeedExtensionClaim(archivedPath);
        var priorPreparation = await CreatePriorUpdateFinalAsync(workspace, archivedPath);
        var priorBytes = await File.ReadAllBytesAsync(
            priorPreparation.BundlePath,
            TestContext.Current.CancellationToken);
        var priorRead = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            priorPreparation.BundlePath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, priorRead.State);
        Assert.Equal(RecoveryBundleProducer.Framework, priorRead.Verified?.Attribution.Producer);
        Assert.Equal(RecoveryBundleOperation.Update, priorRead.Verified?.Attribution.Operation);

        try
        {
            var output = new StringWriter();
            var error = new StringWriter();
            var completion = await workspace.RunAsync(
                ["route", "remove", sourceReference, "--automatic", "--format", "json"],
                output,
                error);

            Assert.Equal(0, completion.ExitCode);
            Assert.Equal(CliSemanticStatus.Complete, completion.Status);
            Assert.Equal(string.Empty, error.ToString());
            using var result = JsonDocument.Parse(output.ToString());
            Assert.Equal(JsonValueKind.Null, result.RootElement.GetProperty("recovery").ValueKind);

            if (category)
            {
                Assert.Contains(
                    ".agents/guidance/topics",
                    workspace.ReadSettings().RemovedDirectories,
                    StringComparer.Ordinal);
                Assert.False(Directory.Exists(workspace.Combine(".agents/guidance/topics")));
            }
            else
            {
                Assert.Contains(archivedPath, workspace.ReadSettings().RemovedFiles, StringComparer.Ordinal);
                Assert.Contains(
                    RouteRemoveIntegrationWorkspace.LeafOverwritePath,
                    workspace.ReadSettings().RemovedFiles,
                    StringComparer.Ordinal);
                Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
                Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafOverwritePath)));
            }

            var extension = Assert.Single(workspace.ReadOwnership().Extensions);
            Assert.Equal("toolkit", extension.Id);
            Assert.DoesNotContain(archivedPath, extension.Paths, StringComparer.Ordinal);

            Assert.True(File.Exists(priorPreparation.BundlePath));
            Assert.Equal(
                priorBytes,
                await File.ReadAllBytesAsync(priorPreparation.BundlePath, TestContext.Current.CancellationToken));
            var retainedRead = await RecoveryBundleReader.ReadFinalAsync(
                workspace.Workspace,
                priorPreparation.BundlePath,
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleReadState.Valid, retainedRead.State);
            var catalogue = await RecoveryBundleCatalogue.ReadAsync(
                workspace.Workspace,
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleCatalogueState.Available, catalogue.State);
            var retained = Assert.Single(catalogue.Candidates);
            Assert.Equal(priorPreparation.BundlePath, retained.Path);
            Assert.Equal(RecoveryBundleCandidateKind.Final, retained.Kind);
            Assert.Equal(RecoveryBundleIntegrity.Verified, retained.Integrity);
            Assert.NotNull(retained.Verified);
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(priorPreparation.BundlePath);
        }
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Remove blocks unverified recovery candidates before workspace writes"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    [InlineData("malformed")]
    [InlineData("draft")]
    public async Task UnverifiedPriorRecoveryCandidateBlocksWithoutWorkspaceEffects(string candidateKind)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create(
            $"route-remove-prior-unverified-{candidateKind}");
        var directory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace.Workspace);
        Directory.CreateDirectory(directory);
        var operationId = Guid.NewGuid();
        var fileName = candidateKind == "draft"
            ? RecoveryBundleFormatV1.DraftFileName(operationId)
            : RecoveryBundleFormatV1.FinalFileName(operationId);
        var candidatePath = Path.Combine(directory, fileName);
        var candidateBytes = candidateKind == "draft"
            ? "incomplete recovery draft"u8.ToArray()
            : [0x01, 0x02, 0x03, 0x04];
        await File.WriteAllBytesAsync(candidatePath, candidateBytes, TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();
        var ownershipBefore = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath);

        try
        {
            var output = new StringWriter();
            var error = new StringWriter();
            var completion = await workspace.RunAsync(
                ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic", "--format", "json"],
                output,
                error);

            Assert.Equal(5, completion.ExitCode);
            Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
            Assert.Equal(string.Empty, error.ToString());
            using var result = JsonDocument.Parse(output.ToString());
            var finding = Assert.Single(result.RootElement.GetProperty("findings").EnumerateArray());
            Assert.Equal("route-remove.recovery-conflict", finding.GetProperty("code").GetString());
            Assert.Equal("file", finding.GetProperty("subject").GetProperty("kind").GetString());
            Assert.Equal(candidatePath, finding.GetProperty("subject").GetProperty("path").GetString());
            Assert.Equal(candidatePath, result.RootElement.GetProperty("recovery").GetProperty("path").GetString());
            var effects = result.RootElement.GetProperty("effects").EnumerateArray().ToArray();
            Assert.NotEmpty(effects);
            Assert.All(
                effects,
                effect => Assert.Equal("not-started", effect.GetProperty("outcome").GetString()));
            Assert.Equal(before, workspace.SnapshotHashes());
            Assert.Equal(ownershipBefore, workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath));
            Assert.False(File.Exists(workspace.Combine(".agents/open-forge.json")));
            Assert.True(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
            Assert.Equal(candidateBytes, await File.ReadAllBytesAsync(candidatePath, TestContext.Current.CancellationToken));

            var catalogue = await RecoveryBundleCatalogue.ReadAsync(
                workspace.Workspace,
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleCatalogueState.Available, catalogue.State);
            var blockedCandidate = Assert.Single(catalogue.Candidates);
            Assert.Equal(candidatePath, blockedCandidate.Path);
            Assert.Equal(
                candidateKind == "draft" ? RecoveryBundleCandidateKind.Draft : RecoveryBundleCandidateKind.Final,
                blockedCandidate.Kind);
            Assert.Equal(
                candidateKind == "draft" ? RecoveryBundleIntegrity.Incomplete : RecoveryBundleIntegrity.Malformed,
                blockedCandidate.Integrity);
        }
        finally
        {
            File.Delete(candidatePath);
        }
    }

    private static async Task<RecoveryBundlePreparation> CreatePriorUpdateFinalAsync(
        RouteRemoveIntegrationWorkspace workspace,
        string relativePath)
    {
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var logicalPath = workspace.Combine(relativePath);
        var before = FileStateSnapshot.File(
            logicalPath,
            logicalPath,
            await File.ReadAllBytesAsync(logicalPath, TestContext.Current.CancellationToken));
        var prepared = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                workspace.Workspace,
                command: "update",
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Framework,
                    RecoveryBundleOperation.Update,
                    workspace.Workspace),
                operationId,
                [RecoveryBundleTarget.Create(PlannedFileChange.Delete(before.Expectation), before)]),
            TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        return Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
    }
}
