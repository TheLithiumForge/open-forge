using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Update;

public sealed class ExtensionUpdateEffectContractTests
{
    [Fact(DisplayName = "Extension Update logical changes preserve package provenance and generated-region identity"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void LogicalChangesPreservePackageAndGeneratedIdentity()
    {
        var package = new ExtensionUpdateLogicalChange(
            ExtensionUpdateComparisonTargetKind.PackageFile,
            ExtensionUpdateChangeAction.Replace,
            region: null,
            sourceAssetPath: "toolkit/payload/.agents/toolkit.md");
        var generated = new ExtensionUpdateLogicalChange(
            ExtensionUpdateComparisonTargetKind.GeneratedRegion,
            ExtensionUpdateChangeAction.Replace,
            region: "entries",
            sourceAssetPath: null);

        Assert.Equal(ExtensionUpdateComparisonTargetKind.PackageFile, package.Kind);
        Assert.Equal("toolkit/payload/.agents/toolkit.md", package.SourceAssetPath);
        Assert.Null(package.Region);
        Assert.Equal(ExtensionUpdateComparisonTargetKind.GeneratedRegion, generated.Kind);
        Assert.Equal("entries", generated.Region);
        Assert.Null(generated.SourceAssetPath);
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateLogicalChange(
            ExtensionUpdateComparisonTargetKind.PackageFile,
            ExtensionUpdateChangeAction.Replace,
            "entries",
            "toolkit/payload/.agents/toolkit.md"));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateLogicalChange(
            ExtensionUpdateComparisonTargetKind.GeneratedRegion,
            ExtensionUpdateChangeAction.Replace,
            "entries",
            "toolkit/payload/.agents/toolkit.md"));
    }

    [Fact(DisplayName = "Extension Update effects snapshot ordered logical changes and reject unsafe shape"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void EffectsSnapshotChangesAndRejectUnsafeShape()
    {
        var changes = new[]
        {
            new ExtensionUpdateLogicalChange(
                ExtensionUpdateComparisonTargetKind.PackageFile,
                ExtensionUpdateChangeAction.Replace,
                region: null,
                sourceAssetPath: "toolkit/payload/.agents/toolkit.md"),
            new ExtensionUpdateLogicalChange(
                ExtensionUpdateComparisonTargetKind.GeneratedRegion,
                ExtensionUpdateChangeAction.Replace,
                region: "entries",
                sourceAssetPath: null),
        };
        var effect = new ExtensionUpdateEffect(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionUpdateEffectKind.PackageFile,
            ExtensionUpdateEffectAction.Replace,
            changes,
            ExtensionUpdateEffectOutcome.Planned,
            ExtensionUpdateEffectResidual.None);

        changes[0] = new ExtensionUpdateLogicalChange(
            ExtensionUpdateComparisonTargetKind.PackageFile,
            ExtensionUpdateChangeAction.Preserve,
            region: null,
            sourceAssetPath: "toolkit/payload/.agents/other.md");

        Assert.Equal(".agents/toolkit.md", effect.Path);
        Assert.Equal("toolkit", effect.PackageId);
        Assert.Equal(ExtensionUpdateEffectKind.PackageFile, effect.Kind);
        Assert.Equal(ExtensionUpdateEffectAction.Replace, effect.Action);
        Assert.Equal(ExtensionUpdateChangeAction.Replace, effect.Changes[0].Action);
        Assert.Equal(ExtensionUpdateChangeAction.Replace, effect.Changes[1].Action);
        Assert.Equal(ExtensionUpdateEffectOutcome.Planned, effect.Outcome);
        Assert.Equal(ExtensionUpdateEffectResidual.None, effect.Residual);
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateEffect(
            "../toolkit.md",
            "toolkit",
            ExtensionUpdateEffectKind.PackageFile,
            ExtensionUpdateEffectAction.Replace,
            [changes[0]],
            ExtensionUpdateEffectOutcome.Planned,
            ExtensionUpdateEffectResidual.None));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateEffect(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionUpdateEffectKind.PackageFile,
            ExtensionUpdateEffectAction.Replace,
            [],
            ExtensionUpdateEffectOutcome.Planned,
            ExtensionUpdateEffectResidual.None));
    }
}
