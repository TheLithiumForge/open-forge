using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Update;

public sealed class ExtensionUpdateBindingTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Extension Update binding forms a complete dry-run request with embedded-source default"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void BindingFormsDryRunRequestWithEmbeddedSourceDefault()
    {
        var symbols = ExtensionUpdateBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments =
        [
            "toolkit",
            "base",
            "--force",
            "--prune",
            "--automatic",
            "--dry-run",
            "--dry-run",
            "--allow-path", "docs",
            "--allow-path", "tools",
        ];
        var parse = symbols.Command.Parse(arguments);
        var request = ExtensionUpdateBinding.BindRequest(
            symbols,
            new CliBindingParse(parse, arguments),
            Invocation(CliFormat.Text));

        Assert.Equal(["docs", "tools"], request.AllowPath);
        Assert.Equal(ExtensionUpdateMode.DryRun, request.Mode);
        Assert.Equal(["toolkit", "base"], request.RequestedIds);
        Assert.Null(request.SourcePath);
        Assert.True(request.Force);
        Assert.True(request.Prune);
        Assert.True(request.Automatic);
        Assert.False(request.AllowInteraction);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Extension Update binding preserves explicit source and human interaction policy"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void BindingPreservesExplicitSourceAndHumanInteractionPolicy()
    {
        var symbols = ExtensionUpdateBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments = ["toolkit", "--source", "/catalogue", "--force"];
        var parse = symbols.Command.Parse(arguments);
        var request = ExtensionUpdateBinding.BindRequest(
            symbols,
            new CliBindingParse(parse, arguments),
            Invocation(CliFormat.Text));

        Assert.Equal(ExtensionUpdateMode.Apply, request.Mode);
        Assert.Equal(["toolkit"], request.RequestedIds);
        Assert.Equal("/catalogue", request.SourcePath);
        Assert.True(request.Force);
        Assert.False(request.Prune);
        Assert.False(request.Automatic);
        Assert.True(request.AllowInteraction);
    }

    private static CliInvocation Invocation(CliFormat format)
    {
        var path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "extension-update-binding"));
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "0.0.0-dev"),
            new CliPresentation(format, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, path),
            new CliWorkspace(path, path, CliWorkspaceSelectionMethod.CurrentDirectory));
    }
}
