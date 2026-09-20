using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveEffectContractTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove effects preserve path, package provenance, action, outcome, and residual"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void EffectPreservesTypedFacts()
    {
        var effect = new ExtensionRemoveEffect(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionRemoveEffectKind.PackageFile,
            ExtensionRemoveEffectAction.Delete,
            ExtensionRemoveEffectOutcome.Planned,
            ExtensionRemoveEffectResidual.None);

        Assert.Equal(".agents/toolkit.md", effect.Path);
        Assert.Equal("toolkit", effect.PackageId);
        Assert.Equal(ExtensionRemoveEffectKind.PackageFile, effect.Kind);
        Assert.Equal(ExtensionRemoveEffectAction.Delete, effect.Action);
        Assert.Equal(ExtensionRemoveEffectOutcome.Planned, effect.Outcome);
        Assert.Equal(ExtensionRemoveEffectResidual.None, effect.Residual);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove effects permit lifecycle effects without package provenance"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void LifecycleEffectPermitsMissingPackageIdentity()
    {
        var effect = new ExtensionRemoveEffect(
            ".agents/open-forge.lifecycle.json",
            packageId: null,
            ExtensionRemoveEffectKind.Lifecycle,
            ExtensionRemoveEffectAction.ReleaseOwnership,
            ExtensionRemoveEffectOutcome.Verified,
            ExtensionRemoveEffectResidual.None);

        Assert.Null(effect.PackageId);
        Assert.Equal(ExtensionRemoveEffectKind.Lifecycle, effect.Kind);
        Assert.Equal(ExtensionRemoveEffectOutcome.Verified, effect.Outcome);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove effects reject absolute traversal separator control and blank paths"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void EffectRejectsUnsafePaths()
    {
        foreach (var path in new[]
        {
            "",
            " ",
            "/absolute.md",
            "C:/absolute.md",
            ".agents\\toolkit.md",
            ".agents/../toolkit.md",
            ".agents/./toolkit.md",
            ".agents//toolkit.md",
            ".agents/\u0001toolkit.md",
        })
        {
            Assert.Throws<ArgumentException>(() => new ExtensionRemoveEffect(
                path,
                "toolkit",
                ExtensionRemoveEffectKind.PackageFile,
                ExtensionRemoveEffectAction.Delete,
                ExtensionRemoveEffectOutcome.Planned,
                ExtensionRemoveEffectResidual.None));
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove effects reject blank package IDs and undefined enum values"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void EffectRejectsInvalidTypedFacts()
    {
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveEffect(
            ".agents/toolkit.md",
            " ",
            ExtensionRemoveEffectKind.PackageFile,
            ExtensionRemoveEffectAction.Delete,
            ExtensionRemoveEffectOutcome.Planned,
            ExtensionRemoveEffectResidual.None));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionRemoveEffect(
            ".agents/toolkit.md",
            "toolkit",
            (ExtensionRemoveEffectKind)int.MaxValue,
            ExtensionRemoveEffectAction.Delete,
            ExtensionRemoveEffectOutcome.Planned,
            ExtensionRemoveEffectResidual.None));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionRemoveEffect(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionRemoveEffectKind.PackageFile,
            (ExtensionRemoveEffectAction)int.MaxValue,
            ExtensionRemoveEffectOutcome.Planned,
            ExtensionRemoveEffectResidual.None));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionRemoveEffect(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionRemoveEffectKind.PackageFile,
            ExtensionRemoveEffectAction.Delete,
            (ExtensionRemoveEffectOutcome)int.MaxValue,
            ExtensionRemoveEffectResidual.None));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionRemoveEffect(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionRemoveEffectKind.PackageFile,
            ExtensionRemoveEffectAction.Delete,
            ExtensionRemoveEffectOutcome.Planned,
            (ExtensionRemoveEffectResidual)int.MaxValue));
    }
}
