using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Update;

public sealed class ExtensionUpdateRequestContractTests
{
    [Fact(DisplayName = "Extension Update request preserves its normalized selection and policies"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void RequestPreservesNormalizedSelectionAndPolicies()
    {
        var workspacePath = Path.Combine(Path.GetTempPath(), "extension-update-unit");
        var workspace = new CliWorkspace(
            workspacePath,
            workspacePath,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var requestedIds = new[] { "toolkit" };

        var request = new ExtensionUpdateRequest(
            workspace,
            ExtensionUpdateMode.DryRun,
            requestedIds,
            all: false,
            sourcePath: "catalogue",
            force: true,
            prune: true,
            automatic: true,
            allowInteraction: false);

        requestedIds[0] = "changed-after-construction";

        Assert.Same(workspace, request.Workspace);
        Assert.Equal(ExtensionUpdateMode.DryRun, request.Mode);
        Assert.Equal(["toolkit"], request.RequestedIds);
        Assert.NotSame(requestedIds, request.RequestedIds);
        Assert.False(request.All);
        Assert.Equal("catalogue", request.SourcePath);
        Assert.True(request.Force);
        Assert.True(request.Prune);
        Assert.True(request.Automatic);
        Assert.False(request.AllowInteraction);
    }

    [Fact(DisplayName = "Extension Update request rejects null inputs, null IDs, and undefined modes"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void RequestRejectsNullInputsAndUndefinedModes()
    {
        var workspace = Workspace();

        Assert.Throws<ArgumentNullException>(() => new ExtensionUpdateRequest(
            null!,
            ExtensionUpdateMode.Apply,
            [],
            false,
            null,
            false,
            false,
            false,
            false));
        Assert.Throws<ArgumentNullException>(() => new ExtensionUpdateRequest(
            workspace,
            ExtensionUpdateMode.Apply,
            null!,
            false,
            null,
            false,
            false,
            false,
            false));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateRequest(
            workspace,
            ExtensionUpdateMode.Apply,
            ["toolkit", null!],
            false,
            null,
            false,
            false,
            false,
            false));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionUpdateRequest(
            workspace,
            (ExtensionUpdateMode)int.MaxValue,
            [],
            false,
            null,
            false,
            false,
            false,
            false));
    }

    private static CliWorkspace Workspace()
        => new("extension-update-test-workspace", "extension-update-test-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
}
