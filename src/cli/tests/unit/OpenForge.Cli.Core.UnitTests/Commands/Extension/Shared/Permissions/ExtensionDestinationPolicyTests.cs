using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Shared.Permissions;

[Trait("Feature", "extension-destination"), Trait("Evidence", "Unit")]
public sealed class ExtensionDestinationPolicyTests
{
    private const string ArchiveName = "archive.zip";
    private static readonly string Root = Path.GetFullPath("extension-policy-unit");
    private static readonly CliWorkspace Workspace = new(Root, Root, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    public static TheoryData<string> RuntimeDestinations
    {
        get
        {
            return
            [
                Path.GetRelativePath(Root, WorkspaceRuntimePaths.DirectoryPath(Workspace)).Replace('\\', '/'),
                Path.GetRelativePath(Root, Path.Combine(WorkspaceRuntimePaths.RecoveryRoot(Workspace), ArchiveName)).Replace('\\', '/'),
            ];
        }
    }

    [Theory(DisplayName = "Extension destinations reserve the runtime directory without ignore rules")]
    [MemberData(nameof(RuntimeDestinations))]
    public void RuntimeDestinationsAreReserved(string path)
    {
        Assert.False(ExtensionDestinationPolicy.IsAllowed(Workspace, path));
    }

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
