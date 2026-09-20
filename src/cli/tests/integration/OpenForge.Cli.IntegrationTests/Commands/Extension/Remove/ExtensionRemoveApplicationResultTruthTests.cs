using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveApplicationResultTruthTests
{
    [Fact(DisplayName = "Extension Remove recovery truth retains a prepared final only after a handled failure"), Trait("Feature", "extension-remove"), Trait("Evidence", "IntegrationBehavior"), Trait("Boundary", "OS")]
    public async Task RecoveryTruthRetainsPreparedFinalAfterFailureAndForgetsItAfterCleanupEscape()
    {
        using var temporary = TemporaryWorkspace.Create("extension-remove-result-truth");
        var preparation = await PrepareRecoveryAsync(temporary);
        try
        {
            var retained = ExtensionRemoveApplicationResultFactory.RecoveryAfterFailure(preparation);
            var unknown = ExtensionRemoveApplicationResultFactory.RecoveryAfterCleanupEscape(preparation);

            Assert.Equal(ExtensionRemoveRecoveryState.Retained, retained.State);
            Assert.Equal(preparation.BundlePath, retained.ResidualPath);
            Assert.Equal(["target.md"], retained.ProtectedPaths);
            Assert.Equal(ExtensionRemoveRecoveryState.Unknown, unknown.State);
            Assert.Null(unknown.ResidualPath);
            Assert.Equal(["target.md"], unknown.ProtectedPaths);
        }
        finally
        {
            DeleteBundle(preparation.BundlePath);
        }
    }

    private static async Task<RecoveryBundlePreparation> PrepareRecoveryAsync(
        TemporaryWorkspace temporary)
    {
        var workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var path = temporary.CreateFile("target.md", "prior");
        var before = FileStateSnapshot.File(path, path, "prior"u8);
        var change = PlannedFileChange.Delete(before.Expectation);
        var input = RecoveryBundleInput.Create(
            workspace,
            "extension remove",
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Extension,
                RecoveryBundleOperation.Remove,
                workspace),
            Guid.NewGuid(),
            [RecoveryBundleTarget.Create(change, before)]);

        var result = await RecoveryBundleStore.PrepareAsync(
            input,
            CancellationToken.None);
        return Assert.IsType<RecoveryBundlePreparation>(result.Preparation);
    }

    private static void DeleteBundle(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var directory = Path.GetDirectoryName(path);
        if (directory is not null
            && Directory.Exists(directory)
            && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }
}
