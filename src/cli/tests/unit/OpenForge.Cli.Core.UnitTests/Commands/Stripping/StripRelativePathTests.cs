using OpenForge.Cli.Core.Commands.Stripping.Shared.Documents;

namespace OpenForge.Cli.Core.UnitTests.Commands.Stripping;

public sealed class StripRelativePathTests
{
    [Theory(DisplayName = "Flatten relinks use the plain relative path between canonical workspace paths"), Trait("Feature", "strip"), Trait("Evidence", "Unit")]
    [InlineData("docs", "docs/rules.md", "rules.md")]
    [InlineData("docs/game", "docs/rules.md", "../rules.md")]
    [InlineData("docs", "docs/game/board.png", "game/board.png")]
    [InlineData("", "docs/rules.md", "docs/rules.md")]
    [InlineData("docs/ui", "src/app.ts", "../../src/app.ts")]
    public void RelativePathsMatchPosixRelative(string fromDirectory, string target, string expected)
        => Assert.Equal(expected, StripLinkEdits.RelativePath(fromDirectory, target));
}
