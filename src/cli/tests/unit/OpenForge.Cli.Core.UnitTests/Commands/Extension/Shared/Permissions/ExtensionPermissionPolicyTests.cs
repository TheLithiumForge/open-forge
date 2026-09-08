using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Models.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Update;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Shared.Permissions;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Unit")]
public sealed class ExtensionPermissionPolicyTests
{
    [Theory]
    [InlineData(".agents/guide.md", true)]
    [InlineData(".apm/agents/guide.md", true)]
    [InlineData("README.md", true)]
    [InlineData(".agents", false)]
    [InlineData(".AGENTS/guide.md", false)]
    [InlineData(".agents/open-forge.PERMISSIONS.json", false)]
    [InlineData(".agents/open-forge.LIFECYCLE.json", false)]
    [InlineData(".agents/open-forge.LIBRARIES.json", false)]
    [InlineData(".agents/open-forge.LOCK", false)]
    [InlineData("nested/.GIT/config", false)]
    [InlineData(".agents/open-forge.PERMISSIONS.json/note.txt", false)]
    [InlineData(".agents/open-forge.LIFECYCLE.json/note.txt", false)]
    [InlineData(".agents/open-forge.LIBRARIES.json/note.txt", false)]
    [InlineData(".agents/open-forge.LOCK/note.txt", false)]
    [InlineData(".apm/guide.overwrite.md", false)]
    [InlineData("../outside", false)]
    [InlineData("", false)]
    public static void AdmissionKeepsProtectedPortablePathsClosed(string path, bool allowed)
        => Assert.Equal(allowed, ExtensionDestinationPolicy.IsAllowed(path));

    [Theory]
    [InlineData((int)ExtensionPermissionFailure.Required, "permission-required")]
    [InlineData((int)ExtensionPermissionFailure.Declined, "permission-declined")]
    [InlineData((int)ExtensionPermissionFailure.Invalid, "permissions-invalid")]
    [InlineData((int)ExtensionPermissionFailure.Unavailable, "permissions-unavailable")]
    [InlineData((int)ExtensionPermissionFailure.Changed, "permissions-changed")]
    [InlineData((int)ExtensionPermissionFailure.WriteFailed, "permission-write-failed")]
    [InlineData((int)ExtensionPermissionFailure.Interrupted, "interrupted")]
    public static void EveryMutationMapsEachPermissionFailure(int value, string suffix)
    {
        var failure = (ExtensionPermissionFailure)value;
        Assert.Equal($"extension-install.{suffix}", ExtensionInstallDefinitions.ReadMachineName(ExtensionInstallDefinitions.ReadPermissionFinding(failure)));
        Assert.Equal($"extension-update.{suffix}", ExtensionUpdateDefinitions.ReadMachineName(ExtensionUpdateDefinitions.ReadPermissionFinding(failure)));
        Assert.Equal($"extension-remove.{suffix}", ExtensionRemoveDefinitions.ReadMachineName(ExtensionRemoveDefinitions.ReadPermissionFinding(failure)));
    }

    [Fact]
    public static void UndefinedPermissionFailuresAreRejected()
    {
        var undefined = (ExtensionPermissionFailure)int.MaxValue;
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionInstallDefinitions.ReadPermissionFinding(undefined));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionUpdateDefinitions.ReadPermissionFinding(undefined));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadPermissionFinding(undefined));
    }

    [Theory]
    [InlineData((int)ExtensionPermissionEffect.Copy, "copy")]
    [InlineData((int)ExtensionPermissionEffect.Delete, "delete")]
    [InlineData((int)ExtensionPermissionEffect.ReleaseOwnership, "release ownership")]
    [InlineData((int)ExtensionPermissionEffect.Preserve, "preserve content")]
    public static void PromptDescribesTheActualPlannedEffect(int value, string expected)
        => Assert.Equal(expected, ExtensionPermissionOperation.ReadEffectName((ExtensionPermissionEffect)value));

    [Fact]
    public static void UndefinedPromptEffectsAreRejected()
        => Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionPermissionOperation.ReadEffectName((ExtensionPermissionEffect)int.MaxValue));
}
