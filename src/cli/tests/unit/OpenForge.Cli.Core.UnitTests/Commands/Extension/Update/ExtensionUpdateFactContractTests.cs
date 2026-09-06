using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Update;

public sealed class ExtensionUpdateFactContractTests
{
    [Fact(DisplayName = "Extension Update package snapshots unique dependency identities"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void PackageSnapshotsUniqueDependencies()
    {
        var dependencies = new[] { "base" };
        var package = new ExtensionUpdatePackage("toolkit", selectedRoot: true, dependencies: dependencies);
        dependencies[0] = "changed-after-construction";

        Assert.Equal("toolkit", package.Id);
        Assert.True(package.SelectedRoot);
        Assert.Equal(["base"], package.Dependencies);
        Assert.Throws<ArgumentException>(() => new ExtensionUpdatePackage(
            "toolkit",
            selectedRoot: false,
            dependencies: ["base", "base"]));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdatePackage(
            "toolkit",
            selectedRoot: false,
            dependencies: [" "]));
    }

    [Fact(DisplayName = "Extension Update recovery facts require residual certainty only for retained bundles"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void RecoveryFactsRequireRetainedResidualCertainty()
    {
        var retained = new ExtensionUpdateRecovery(
            ExtensionUpdateRecoveryState.Retained,
            [".agents/toolkit.md"],
            "/recovery/bundle.zip");
        var removed = new ExtensionUpdateRecovery(
            ExtensionUpdateRecoveryState.Removed,
            [".agents/toolkit.md"],
            residualPath: null);

        Assert.Equal("/recovery/bundle.zip", retained.ResidualPath);
        Assert.Equal(ExtensionUpdateRecoveryState.Removed, removed.State);
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateRecovery(
            ExtensionUpdateRecoveryState.Retained,
            [],
            residualPath: null));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateRecovery(
            ExtensionUpdateRecoveryState.Removed,
            [],
            "/recovery/bundle.zip"));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateRecovery(
            ExtensionUpdateRecoveryState.Removed,
            [".agents/toolkit.md", ".agents/toolkit.md"],
            residualPath: null));
    }

    [Fact(DisplayName = "Extension Update finding derives the declared semantic status and rejects undefined values"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void FindingDerivesStatusAndRejectsUndefinedValues()
    {
        var divergence = new ExtensionUpdateFinding(
            ExtensionUpdateFindingCode.ManagedDivergence,
            "Current content differs.",
            ".agents/toolkit.md");
        var blocked = new ExtensionUpdateFinding(
            ExtensionUpdateFindingCode.TargetUnsafe,
            "Target is reserved.");

        Assert.Equal(CliSemanticStatus.Attention, divergence.Status);
        Assert.Equal(".agents/toolkit.md", divergence.Target);
        Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionUpdateFinding(
            (ExtensionUpdateFindingCode)int.MaxValue,
            "Invalid code."));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateFinding(
            ExtensionUpdateFindingCode.InvalidInput,
            " "));
    }
}
