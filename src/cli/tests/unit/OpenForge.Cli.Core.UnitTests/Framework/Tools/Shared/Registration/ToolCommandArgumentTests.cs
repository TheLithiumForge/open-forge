using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Unit")]
public sealed class ToolCommandArgumentTests
{
    [Theory(DisplayName = "Command rewrites keep empty, repeated and escaped arguments as separate array elements")]
    [InlineData(0), InlineData(1)]
    public void KeepsArgumentArrays(int form)
    {
        string[] args = ["", "--", "space and 🛠", "quote\"", "back\\slash", "--"];
        var declaration = ToolEntryCase.Definition().Rewrites[form];
        var rewrite = declaration switch
        {
            ToolJsonCommandRewrite json => json with { Replacement = json.Replacement with { Args = args } },
            ToolTomlCommandRewrite toml => (ToolEntryRewrite)(toml with { Replacement = toml.Replacement with { Args = args } }),
            _ => throw new ArgumentOutOfRangeException(nameof(form)),
        };
        var request = ToolEntryCase.Request(rewrite);
        var files = ToolEntryCase.Files(rewrite, ToolEntryCase.Original(rewrite));
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, files).Edits);
        files[rewrite.File] = edit.After;
        var receipt = Assert.Single(new ToolEntryVerifier().Verify(request, files).Rewrites);
        var replacement = receipt.Rewrite switch
        {
            ToolJsonCommandRewrite json => json.Replacement,
            ToolTomlCommandRewrite toml => toml.Replacement,
            _ => throw new ArgumentOutOfRangeException(nameof(form)),
        };
        Assert.Equal(args, replacement.Args);
        Assert.Equal(ToolEntryState.Matching, Assert.Single(new ToolEntryPlanner().Observe(request, files)).State);
    }
}
