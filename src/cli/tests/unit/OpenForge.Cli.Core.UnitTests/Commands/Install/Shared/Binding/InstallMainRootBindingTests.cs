using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Binding;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Binding;

[Trait("Feature", "install-main-root"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class InstallMainRootBindingTests
{
    private const string CustomRoot = "docs/context";
    private const string OtherRoot = "handbook/context";
    private const string RetrySnapshot = "custom-root-retry";

    [Theory(DisplayName = "Install retains the requested context folder through native option delimiters")]
    [InlineData(" "), InlineData("="), InlineData(":")]
    public void NativeDelimiters(string delimiter)
    {
        var input = Read(delimiter == " " ? [InstallDefinitions.MainRoot.Name, CustomRoot] : [InstallDefinitions.MainRoot.Name + delimiter + CustomRoot]);
        Assert.Null(input.SetupError);
        Assert.Equal(CustomRoot, input.RequestedMainRoot);
        Assert.Null(input.Setup);
    }

    [Fact(DisplayName = "Omitted and explicit default roots remain distinct inputs")]
    public void DefaultInput()
    {
        Assert.Null(Read([]).RequestedMainRoot);
        Assert.Equal(WorkspaceLayout.DefaultMainRoot, Read([InstallDefinitions.MainRoot.Name, WorkspaceLayout.DefaultMainRoot]).RequestedMainRoot);
    }

    [Fact(DisplayName = "Repeated context folder values must agree and missing values fail")]
    public void Occurrences()
    {
        Assert.Null(Read([InstallDefinitions.MainRoot.Name, CustomRoot, InstallDefinitions.MainRoot.Name, CustomRoot]).SetupError);
        Assert.NotNull(Read([InstallDefinitions.MainRoot.Name, CustomRoot, InstallDefinitions.MainRoot.Name, OtherRoot]).SetupError);
        Assert.NotNull(Read([InstallDefinitions.MainRoot.Name]).SetupError);
    }

    [Theory(DisplayName = "Install rejects noncanonical and reserved context folders")]
    [InlineData(""), InlineData("."), InlineData("../outside"), InlineData("docs/../context")]
    [InlineData(WorkspaceLayout.DefaultMainRoot + "/context"), InlineData(WorkspaceRuntimePaths.DirectoryName)]
    public void InvalidFolder(string root)
        => Assert.NotNull(Read([InstallDefinitions.MainRoot.Name, root]).SetupError);

    [Fact(DisplayName = "An automatic retry preserves the selected context folder")]
    public void RetryKeepsRoot()
    {
        var next = InstallDefinitions.ReadNextAction(CliSemanticStatus.Invalid,
            [new InstallFinding(InstallFindingCode.ConfirmationRequired, nameof(RetryKeepsRoot))],
            Read([InstallDefinitions.MainRoot.Name, CustomRoot]));
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(next?.Command), RetrySnapshot);
    }

    private static InstallBindingInput Read(string[] arguments)
    {
        var symbols = InstallBinding.CreateSymbols();
        return InstallBindingInputReader.Read(symbols.InstallCommand.Parse(arguments), symbols);
    }
}
