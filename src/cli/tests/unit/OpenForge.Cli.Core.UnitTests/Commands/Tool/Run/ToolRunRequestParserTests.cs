using OpenForge.Cli.Core.Commands.Tool.Run;
using OpenForge.Cli.Core.Commands.Tool.Run.Models.Parsing;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Run;

[Trait("Feature", "tool-run"), Trait("Evidence", "Unit")]
public sealed class ToolRunRequestParserTests
{
    private static readonly ToolRunEnvironment EnvironmentFacts = new(Path.GetFullPath("."), null, false);
    private static readonly string[] Payload = [CliSyntaxDefinitions.Help.Name, "space value", "", "quote\"value", "--", "@child-input", "line\nvalue"];

    [Theory(DisplayName = "Tool Run forwards every token after its ID and removes at most one immediate delimiter")]
    [InlineData(false)]
    [InlineData(true)]
    public void KeepsChildArguments(bool delimiter)
    {
        var arguments = new List<string> { ToolFixtureSource.Id };
        if (delimiter) arguments.Add("--");
        arguments.AddRange(Payload);
        var input = ToolRunRequestParser.Parse([.. arguments], EnvironmentFacts);
        Assert.Empty(input.Diagnostics);
        Assert.NotNull(input.Request);
        Assert.Equal(ToolFixtureSource.Id, input.Request.Id);
        Assert.Equal(Payload, input.Request.Arguments);
        Assert.False(input.Global?.Help);
    }

    [Theory(DisplayName = "Tool Run uses native workspace option forms only before the ID")]
    [InlineData(" ")]
    [InlineData("=")]
    [InlineData(":")]
    public void ReadsWorkspaceBeforeId(string separator)
    {
        var workspace = Path.Combine(EnvironmentFacts.CurrentDirectory, "workspace with spaces");
        string[] prefix = separator == " " ? [CliSyntaxDefinitions.Workspace.Name, workspace] : [$"{CliSyntaxDefinitions.Workspace.Name}{separator}{workspace}"];
        var child = new[] { CliSyntaxDefinitions.Workspace.Name, "child-workspace", CliSyntaxDefinitions.Format.Name, "child-format" };
        var input = ToolRunRequestParser.Parse([.. prefix, ToolFixtureSource.Id, .. child], EnvironmentFacts);
        Assert.Empty(input.Diagnostics);
        Assert.Equal(workspace, input.Request?.WorkspacePath);
        Assert.Equal(child, input.Request?.Arguments);
    }

    [Theory(DisplayName = "Tool Run rejects missing input, repeated singleton options and writer flags before its ID")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void RejectsInvalidPrelude(int scenario)
    {
        string[] arguments = scenario switch
        {
            0 => [],
            1 => [CliSyntaxDefinitions.Workspace.Name],
            2 => [CliSyntaxDefinitions.Workspace.Name, "", ToolFixtureSource.Id],
            3 => [CliSyntaxDefinitions.Workspace.Name, EnvironmentFacts.CurrentDirectory, CliSyntaxDefinitions.Workspace.Name, EnvironmentFacts.CurrentDirectory, ToolFixtureSource.Id],
            4 => ["--automatic", ToolFixtureSource.Id],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var input = ToolRunRequestParser.Parse(arguments, EnvironmentFacts);
        Assert.Null(input.Request);
        Assert.NotEmpty(input.Diagnostics);
    }

    [Fact(DisplayName = "Tool Run keeps a second delimiter as child input")]
    public void KeepsSecondDelimiter()
    {
        var input = ToolRunRequestParser.Parse([ToolFixtureSource.Id, "--", "--", .. Payload], EnvironmentFacts);
        Assert.NotNull(input.Request);
        Assert.Equal(["--", .. Payload], input.Request.Arguments);
    }

    [Fact(DisplayName = "Tool Run reads presentation options before the ID without changing child tokens")]
    public void AcceptsPresentationPrelude()
    {
        var input = ToolRunRequestParser.Parse([CliSyntaxDefinitions.Format.Name, "json", CliSyntaxDefinitions.Detail.Name, "debug", ToolFixtureSource.Id, .. Payload], EnvironmentFacts with { Interactive = true });
        Assert.Empty(input.Diagnostics);
        Assert.Equal(CliFormat.Json, input.Global?.OutputFormat);
        Assert.Equal(CliDetail.Debug, input.Global?.Detail);
        Assert.Equal(Payload, input.Request?.Arguments);
        Assert.NotNull(input.Request);
        Assert.False(input.Request.Interactive);
    }

    [Fact(DisplayName = "Tool Run help before the ID stays an Open Forge terminal request")]
    public void ReadsPreludeHelp()
    {
        var input = ToolRunRequestParser.Parse([CliSyntaxDefinitions.Help.Name], EnvironmentFacts);
        Assert.Empty(input.Diagnostics);
        Assert.Null(input.Request);
        Assert.True(input.Global?.Help);
    }

    [Fact(DisplayName = "Tool Run preserves root options before the command path without parsing child help")]
    public void ReadsComposedRootPrelude()
    {
        var group = OpenForge.Cli.Core.Commands.Tool.ToolBinding.CreateGroup();
        var symbols = ToolRunBinding.CreateSymbols(group);
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [new CliRootBranch(group, CliHelpContent.Empty)], []);
        var input = ToolRunRequestParser.Parse(tree, symbols.Id,
            [CliSyntaxDefinitions.Workspace.Name, EnvironmentFacts.CurrentDirectory, group.Name, symbols.Command.Name, ToolFixtureSource.Id, .. Payload], EnvironmentFacts);
        Assert.Empty(input.Diagnostics);
        Assert.Equal(EnvironmentFacts.CurrentDirectory, input.Request?.WorkspacePath);
        Assert.Equal(Payload, input.Request?.Arguments);
        Assert.False(input.Global?.Help);
    }
}
