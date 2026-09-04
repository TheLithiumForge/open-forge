using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install;

public sealed class ExtensionInstallApplicationMappingTests
{
    [Fact(DisplayName = "Extension Install leaves a target-changed directory effect unstarted"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    public void TargetChangedDirectoryReceiptRemainsNotStarted()
    {
        var logical = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "extension-install-mapping"));
        var before = FileStateSnapshot.Missing(logical);
        var creation = PlannedDirectoryCreation.Create(before.Expectation);
        var receipt = DirectoryCreationReceipt.NotStarted(
            creation,
            before,
            logical,
            FilesystemNotStartedReason.TargetChanged,
            "The directory target changed.");

        var mapping = ExtensionInstallEffectApplication.MapDirectoryReceipt(
            receipt,
            ".agents/toolkit");

        Assert.Null(mapping.RecordedOutcome);
        var finding = Assert.IsType<ExtensionInstallFinding>(mapping.Finding);
        Assert.Equal(ExtensionInstallFindingCode.TargetChanged, finding.Code);
        Assert.Equal(".agents/toolkit", finding.Target);
    }
}
