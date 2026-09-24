using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;

internal static class ExtensionRecoveryHistoryFixture
{
    internal const string TargetPath = ".agents/recovery-history.txt";

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
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.None)
            ?? throw new InvalidOperationException("LocalApplicationData must be observable for Integration evidence.");
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
