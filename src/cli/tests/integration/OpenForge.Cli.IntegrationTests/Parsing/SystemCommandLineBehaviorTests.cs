using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.IntegrationTests.Parsing;

public sealed class SystemCommandLineBehaviorTests
{
    [Fact(DisplayName = "Pinned parser reports scalar repetition and Boolean occurrences")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public void PinnedParserReportsScalarRepetitionAndCountsBooleanOccurrences()
    {
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        var parser = new CliParser(tree);
        var scalar = parser.Parse(["--view=compact", "--view=expanded"]);
        var booleans = parser.Parse(["--json", "--json"]);

        Assert.NotEmpty(scalar.Result.Errors);
        var scalarResult = Assert.IsType<OptionResult>(scalar.Result.GetResult(tree.Options.View));
        Assert.Equal(2, scalarResult.IdentifierTokenCount);
        Assert.Equal(2, scalarResult.Tokens.Count(token => token.Type == TokenType.Argument));
        Assert.Empty(booleans.Result.Errors);
        var booleanResult = Assert.IsType<OptionResult>(booleans.Result.GetResult(tree.Options.Json));
        Assert.Equal(2, booleanResult.IdentifierTokenCount);
        Assert.Empty(booleanResult.Tokens);
    }

    [Fact(DisplayName = "Pinned parser normalizes values while guard owns delimiter shape")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public void ParserNormalizesAttachedAndSeparateValuesSoGuardOwnsShape()
    {
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        var attached = new CliParser(tree).Parse(["--workspace=example"]);
        var separate = new CliParser(tree).Parse(["--workspace", "example"]);

        Assert.Equal(
            attached.Result.Tokens.Select(token => (token.Type, token.Value)),
            separate.Result.Tokens.Select(token => (token.Type, token.Value)));
        Assert.Equal(["--workspace=example"], attached.OriginalArguments);
        Assert.Equal(["--workspace", "example"], separate.OriginalArguments);
    }
}
