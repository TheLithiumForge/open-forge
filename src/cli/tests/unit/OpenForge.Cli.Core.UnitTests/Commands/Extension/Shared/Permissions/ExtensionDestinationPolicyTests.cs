using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Shared.Permissions;

[Trait("Feature", "extension-destination"), Trait("Evidence", "Unit")]
public sealed class ExtensionDestinationPolicyTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Extension destinations exclude authored settings and generated ownership state")]
    [InlineData(".agents/open-forge.json"), InlineData(".agents/open-forge.lock.json")]
    [InlineData(".agents/open-forge.json/child.md"), InlineData(".agents/open-forge.lock.json/child.md")]
    [InlineData(".agents/OPEN-FORGE.JSON"), InlineData(".agents/OPEN-FORGE.LOCK.JSON")]
    public void WorkspaceStateIsReserved(string path)
        => Assert.False(ExtensionDestinationPolicy.IsAllowed(path));

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Extension destinations preserve ordinary names beside reserved state files")]
    [InlineData(".agents/open-forge.json.md"), InlineData(".agents/open-forge.lock.json.md")]
    public void OrdinarySiblingNamesRemainAllowed(string path)
        => Assert.True(ExtensionDestinationPolicy.IsAllowed(path));
}
