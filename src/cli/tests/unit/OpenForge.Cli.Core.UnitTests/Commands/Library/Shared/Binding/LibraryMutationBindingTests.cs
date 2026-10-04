using System.CommandLine;
using OpenForge.Cli.Core.Commands.Library;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Binding;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Binding;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Binding;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Binding;

[Trait("Feature", "library-interaction"), Trait("Evidence", "Unit")]
public sealed class LibraryMutationBindingTests
{
    [Theory(DisplayName = "Attach Git-ignore binds explicit typed yes and no"), Trait("Feature", "library-git-ignore"), Trait("Boundary", "Input")]
    [InlineData("--git-ignore", "true", true)]
    [InlineData("--git-ignore", "false", false)]
    [InlineData("--git-ignore=true", null, true)]
    [InlineData("--git-ignore:false", null, false)]
    public void AttachGitIgnoreBindsTypedInput(string option, string? value, bool expected)
    {
        var symbols = LibraryAttachBinding.CreateSymbols(LibraryBinding.CreateGroup());
        string[] arguments = value is null ? ["team", "shared/team", option] : ["team", "shared/team", option, value];
        var parsed = symbols.Command.Parse(arguments);
        Assert.Empty(parsed.Errors);
        var bound = LibraryAttachRequestBinder.Bind(new CliBindingParse(parsed, arguments), Invocation(CliFormat.Text), symbols);
        Assert.Equal(expected, Assert.IsType<LibraryAttachRequest>(bound.Request).GitIgnore);
    }

    [Theory(DisplayName = "Attach Git-ignore requires exactly one explicit singleton Boolean value"), Trait("Feature", "library-git-ignore"), Trait("Boundary", "Input")]
    [InlineData("--git-ignore")]
    [InlineData("--git-ignore", "yes")]
    [InlineData("--git-ignore", "true", "false")]
    [InlineData("--git-ignore", "true", "--git-ignore", "false")]
    public void AttachGitIgnoreRejectsInvalidInput(params string[] flags)
    {
        var symbols = LibraryAttachBinding.CreateSymbols(LibraryBinding.CreateGroup());
        string[] arguments = ["team", "shared/team", .. flags];
        var parsed = symbols.Command.Parse(arguments);
        var bound = LibraryAttachRequestBinder.Bind(new CliBindingParse(parsed, arguments), Invocation(CliFormat.Text), symbols);
        Assert.True(parsed.Errors.Count > 0 || bound.Request is null);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Attach automatic option binds apply without interactive permission")]
    public void AttachAutomaticOptionBindsWithoutInteractivePermission()
    {
        var symbols = LibraryAttachBinding.CreateSymbols(LibraryBinding.CreateGroup());
        var arguments = new[] { "team-knowledge", "shared/team-knowledge", "--automatic" };
        var parse = new CliBindingParse(symbols.Command.Parse(arguments), arguments);

        var bound = LibraryAttachRequestBinder.Bind(parse, Invocation(CliFormat.Text), symbols);

        var request = Assert.IsType<LibraryAttachRequest>(bound.Request);
        Assert.True(request.Automatic);
        Assert.False(request.AllowPrompt);
        Assert.Equal(LibraryMode.Apply, request.Mode);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Sync automatic option binds apply without interactive permission")]
    public void SyncAutomaticOptionBindsWithoutInteractivePermission()
    {
        var symbols = LibrarySyncBinding.CreateSymbols(LibraryBinding.CreateGroup());
        var arguments = new[] { "team-knowledge", "--automatic" };
        var parse = new CliBindingParse(symbols.Command.Parse(arguments), arguments);

        var bound = LibrarySyncRequestBinder.Bind(parse, Invocation(CliFormat.Text), symbols);

        var request = Assert.IsType<LibrarySyncRequest>(bound.Request);
        Assert.True(request.Automatic);
        Assert.False(request.AllowPrompt);
        Assert.Equal(LibraryMode.Apply, request.Mode);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Detach automatic option binds apply without interactive permission")]
    public void DetachAutomaticOptionBindsWithoutInteractivePermission()
    {
        var symbols = LibraryDetachBinding.CreateSymbols(LibraryBinding.CreateGroup());
        var arguments = new[] { "team-knowledge", "--automatic" };
        var parse = new CliBindingParse(symbols.Command.Parse(arguments), arguments);

        var bound = LibraryDetachRequestBinder.Bind(parse, Invocation(CliFormat.Text), symbols);

        var request = Assert.IsType<LibraryDetachRequest>(bound.Request);
        Assert.True(request.Automatic);
        Assert.False(request.AllowPrompt);
        Assert.Equal(LibraryMode.Apply, request.Mode);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "All library mutation commands expose automatic as a zero arity option")]
    public void AllMutationCommandsExposeAutomaticAsAZeroArityOption()
    {
        var attach = LibraryAttachBinding.CreateSymbols(LibraryBinding.CreateGroup());
        var sync = LibrarySyncBinding.CreateSymbols(LibraryBinding.CreateGroup());
        var detach = LibraryDetachBinding.CreateSymbols(LibraryBinding.CreateGroup());

        Assert.Equal("--automatic", attach.Automatic.Name);
        Assert.Equal(ArgumentArity.Zero, attach.Automatic.Arity);
        Assert.Equal("--automatic", sync.Automatic.Name);
        Assert.Equal(ArgumentArity.Zero, sync.Automatic.Arity);
        Assert.Equal("--automatic", detach.Automatic.Name);
        Assert.Equal(ArgumentArity.Zero, detach.Automatic.Arity);
    }

    private static CliInvocation Invocation(CliFormat format)
    {
        var workspace = new CliWorkspace(
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "library-binding-workspace")),
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "library-binding-workspace")),
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }
}
