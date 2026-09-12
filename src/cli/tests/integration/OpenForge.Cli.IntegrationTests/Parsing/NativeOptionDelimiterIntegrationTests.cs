using System.CommandLine;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;

namespace OpenForge.Cli.IntegrationTests.Parsing;

public sealed class NativeOptionDelimiterIntegrationTests
{
    [Theory(DisplayName = "Composed CLI accepts native tag delimiters and preserves repeated value order")]
    [Trait("Feature", "native-option-delimiters"), Trait("Evidence", "Integration")]
    [InlineData("find", " ")]
    [InlineData("find", "=")]
    [InlineData("find", ":")]
    [InlineData("route create", " ")]
    [InlineData("route create", "=")]
    [InlineData("route create", ":")]
    [InlineData("route init", " ")]
    [InlineData("route init", "=")]
    [InlineData("route init", ":")]
    [InlineData("route update", " ")]
    [InlineData("route update", "=")]
    [InlineData("route update", ":")]
    public void TagsUseNativeDelimiters(string command, string delimiter)
    {
        var application = CliCompositionRoot.Create(new CliProcessIdentity("open-forge", "test"));
        var arguments = command.Split(' ').ToList();
        if (command != "find")
        {
            arguments.Add("memory/working/example");
        }

        arguments.AddRange(Value("--tag", "Second", delimiter));
        arguments.AddRange(Value("--tag", "First", delimiter));
        var parse = CliCoreApplicationAccess.Tree(application).Parse([.. arguments]);
        Assert.Empty(parse.Result.Errors);
        Assert.Null(CliTerminalValidator.Validate(parse).InvalidInput);
        var tag = Assert.IsType<Option<string[]>>(Assert.Single(
            parse.Result.CommandResult.Command.Options, option => option.Name == "--tag"));
        var values = parse.Result.GetValue(tag);
        Assert.NotNull(values);
        Assert.Equal(["Second", "First"], values);
    }

    [Theory(DisplayName = "Composed Route List accepts native depth delimiters without changing the value")]
    [Trait("Feature", "native-option-delimiters"), Trait("Evidence", "Integration")]
    [InlineData("0", " ")]
    [InlineData("0", "=")]
    [InlineData("0", ":")]
    [InlineData("2", " ")]
    [InlineData("2", "=")]
    [InlineData("2", ":")]
    [InlineData("all", " ")]
    [InlineData("all", "=")]
    [InlineData("all", ":")]
    public void DepthUsesNativeDelimiters(string value, string delimiter)
    {
        var application = CliCompositionRoot.Create(new CliProcessIdentity("open-forge", "test"));
        var parse = CliCoreApplicationAccess.Tree(application).Parse(
            ["route", "list", .. Value("--depth", value, delimiter)]);
        Assert.Empty(parse.Result.Errors);
        Assert.Null(CliTerminalValidator.Validate(parse).InvalidInput);
        var depth = Assert.IsType<Option<string>>(Assert.Single(
            parse.Result.CommandResult.Command.Options, option => option.Name == "--depth"));
        Assert.Equal(value, parse.Result.GetValue(depth));
    }

    private static string[] Value(string option, string value, string delimiter)
        => delimiter == " " ? [option, value] : [$"{option}{delimiter}{value}"];
}
