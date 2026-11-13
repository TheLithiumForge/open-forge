using System.Text;
using OpenForge.Cli.Core.Framework.Libraries.Shared.GitIgnore;
using OpenForge.Cli.Core.Framework.Tools.Shared.GitIgnore;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.GitIgnore;

[Trait("Feature", "tool-ignore"), Trait("Evidence", "Unit")]
public sealed class ToolGitIgnoreSectionTests
{
    [Fact(DisplayName = "Tool ignore edits keep the BOM, line endings and Library section byte for byte")]
    public void KeepsOtherSections()
    {
        var prefix = $"\uFEFF# authored\r\n{LibraryGitIgnoreSection.Begin}\r\n/fixture-library\r\n{LibraryGitIgnoreSection.End}\r\n";
        var bytes = Encoding.UTF8.GetBytes(prefix);
        var written = ToolGitIgnoreSection.Rewrite(bytes, [], ["fixture-tool/"]);
        var expected = Encoding.UTF8.GetBytes($"{prefix}{ToolGitIgnoreSection.Begin}\r\n/fixture-tool/\r\n{ToolGitIgnoreSection.End}\r\n");
        Assert.Equal(expected, written);
        Assert.Equal(written, ToolGitIgnoreSection.Rewrite(written, ["fixture-tool/"], ["fixture-tool/"]));
        Assert.Equal(bytes, ToolGitIgnoreSection.Rewrite(written, ["fixture-tool/"], []));
    }

    [Fact(DisplayName = "Releasing one tool keeps another tool's exact ignore rule")]
    public void KeepsOtherTools()
    {
        var current = ToolGitIgnoreSection.Rewrite([], [], ["fixture-a/", "fixture-b/"]);
        var released = ToolGitIgnoreSection.Rewrite(current, ["fixture-a/", "fixture-b/"], ["fixture-b/"]);
        var expected = Encoding.UTF8.GetBytes($"{ToolGitIgnoreSection.Begin}\n/fixture-b/\n{ToolGitIgnoreSection.End}\n");
        Assert.Equal(expected, released);
    }

    [Theory(DisplayName = "Malformed or edited Tool sections block changes")]
    [InlineData("duplicate-markers")]
    [InlineData("missing-end")]
    [InlineData("unknown-rule")]
    public void RejectsUnrecognizedSections(string situation)
    {
        var text = $"{ToolGitIgnoreSection.Begin}\n/fixture/\n{ToolGitIgnoreSection.End}\n";
        text = situation switch
        {
            "duplicate-markers" => text + text,
            "missing-end" => $"{ToolGitIgnoreSection.Begin}\n/fixture/\n",
            "unknown-rule" => $"{ToolGitIgnoreSection.Begin}\n/authored-rule\n{ToolGitIgnoreSection.End}\n",
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        Assert.Throws<InvalidDataException>(() => ToolGitIgnoreSection.Rewrite(Encoding.UTF8.GetBytes(text), ["fixture/"], []));
    }
}
