using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Storage;

[Trait("Feature", "tool-store"), Trait("Evidence", "Unit")]
public sealed class ToolStoreLocatorTests
{
    [Fact(DisplayName = "The shared test oracle redirects tool storage through the production override")]
    public void UsesOwnedOverride()
    {
        Assert.Equal(TestDataHome.ToolVariable, ToolStoreLocator.OverrideVariable);
        var configured = Environment.GetEnvironmentVariable(TestDataHome.ToolVariable) ?? throw new InvalidOperationException("The test tool-store override is missing.");
        var locations = ToolStoreLocator.ForCurrentUser();
        Assert.Equal(Path.Combine(configured, ToolStoreLocator.InstallsDirectory), locations.Installs);
        Assert.Equal(Path.Combine(configured, ToolStoreLocator.ArchivesDirectory), locations.Archives);
        Assert.Equal(Path.Combine(configured, ToolStoreLocator.StateDirectory), locations.State);
    }

    [Fact(DisplayName = "Relative tool-store overrides are rejected without creating a directory")]
    public void RejectsRelativeRoot() => Assert.Throws<ArgumentException>(() => ToolStoreLocator.FromRoot("relative-fixture-store"));
}
