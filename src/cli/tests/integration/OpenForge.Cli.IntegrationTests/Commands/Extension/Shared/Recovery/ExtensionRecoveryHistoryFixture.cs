using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport.Isolation;
using System.Runtime.CompilerServices;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;

internal static class ExtensionRecoveryHistoryFixture
{
    internal const string TargetPath = ".agents/recovery-history.txt";

    internal static async Task<string> CopyToLegacyAsync(RecoveryBundlePreparation preparation, CancellationToken cancellationToken)
    {
        var legacy = RecoveryStoreLocator.Legacy(preparation.Location.Workspace)
            ?? throw new InvalidOperationException("The recovery history fixture requires a legacy location.");
        Directory.CreateDirectory(legacy.WorkspaceDirectory);
        var path = Path.Combine(legacy.WorkspaceDirectory, Path.GetFileName(preparation.BundlePath));
        File.Copy(preparation.BundlePath, path);
        var read = await RecoveryBundleReader.ReadFinalAsync(legacy, path, cancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, read.State);
        return path;
    }

    internal static async Task MatchHistoryAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        string sourcePath,
        ReadOutputScenario scenario,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string testName = "")
    {
        var location = RecoveryStoreLocator.Local(workspace.Workspace);
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);
        await new ReadCommandOutputCapture(workspace.Path, sourcePath, location.WorkspaceDirectory)
            .MatchDetailsAsync(scenario, snapshotCollector: captures);
        CommandOutputSnapshot.MatchDetailSnapshot(captures.ToDictionary(pair => pair.Key,
            pair => pair.Value.Replace("<recovery-bundle>",
                "<workspace>/.open-forge-state/recovery/v1/<workspace-key>", StringComparison.Ordinal)),
            sourceFile, testName);
    }

    internal static async ValueTask<RecoveryBundlePreparation> CreateVerifiedFinalAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        string command,
        RecoveryBundleOperation operation,
        CancellationToken cancellationToken)
    {
        const string priorContents = "prior retained history bytes\n";
        workspace.CreateOccupant(TargetPath, priorContents);
        var targetPath = workspace.Combine(TargetPath);
        var prior = FileStateSnapshot.File(targetPath, targetPath, "prior retained history bytes\n"u8);
        var change = PlannedFileChange.Replace(
            prior.Expectation,
            "proposed recovery bytes\n"u8);
        var input = RecoveryBundleInput.Create(
            workspace.Workspace,
            command,
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Extension,
                operation,
                workspace.Workspace),
            Guid.NewGuid(),
            [RecoveryBundleTarget.Create(change, prior)]);
        var result = await RecoveryBundleStore.PrepareAsync(input, cancellationToken).ConfigureAwait(false);
        return result.Preparation
            ?? throw new InvalidOperationException(
                result.Cause ?? "The Extension recovery history fixture did not create a final bundle.");
    }

    internal static string CandidatePath(
        OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace workspace,
        Guid operationId,
        RecoveryBundleCandidateKind kind)
    {
        var storeRoot = TestDataHome.LocalRecoveryStoreRoot(workspace.PhysicalRoot);
        var directory = RecoveryBundlePathIdentity.WorkspaceDirectory(storeRoot, workspace.PhysicalRoot);
        Directory.CreateDirectory(directory);
        var fileName = kind switch
        {
            RecoveryBundleCandidateKind.Final => RecoveryBundleFormatV1.FinalFileName(operationId),
            RecoveryBundleCandidateKind.Draft => RecoveryBundleFormatV1.DraftFileName(operationId),
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The recovery candidate kind is not defined."),
        };
        return Path.Combine(directory, fileName);
    }
}
