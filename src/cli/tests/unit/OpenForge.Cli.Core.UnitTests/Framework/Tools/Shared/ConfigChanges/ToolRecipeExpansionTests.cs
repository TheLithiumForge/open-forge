using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-recipes"), Trait("Evidence", "Unit")]
public sealed class ToolRecipeExpansionTests
{
    [Theory(DisplayName = "Recipe expansion handles dollar escaping and single nonrecursive substitution")]
    [InlineData("${mainRoot}", "context"), InlineData("$$${mainRoot}", "$context"), InlineData("$cash", "$cash"), InlineData("$${unknown}", "${unknown}"), InlineData("$$", "$")]
    public void DollarRules(string source, string expected) => Assert.Equal(expected, ToolRecipeExpansion.Expand(source, Context()));

    [Theory(DisplayName = "Unknown malformed and unavailable recipe tokens fail before effects")]
    [InlineData("${bad}"), InlineData("${}"), InlineData("${mainRoot"), InlineData("${client:absent}"), InlineData("${previous}")]
    public void InvalidTokens(string source) => Assert.Throws<InvalidDataException>(() => ToolRecipeExpansion.Expand(source, Context()));

    [Fact(DisplayName = "Recipe roots client roots and previous native values use their declared bindings")]
    public void Bindings()
    {
        var context = Context();
        ToolRecipeExpansion.Validate(context);
        Assert.Equal(context.WorkspacePath, ToolRecipeExpansion.Expand(ToolRecipeDefinitions.WorkspaceToken, context));
        Assert.Equal(context.HomePath, ToolRecipeExpansion.Expand(ToolRecipeDefinitions.HomeToken, context));
        Assert.Equal(context.BinPath, ToolRecipeExpansion.Expand(ToolRecipeDefinitions.BinToken, context));
        Assert.Equal(context.NativeHomePath, ToolRecipeExpansion.Expand(ToolRecipeDefinitions.NativeHomeToken, context));
        Assert.Equal(context.NativeInstallDirectory, ToolRecipeExpansion.Expand(ToolRecipeDefinitions.NativeInstallDirectoryToken, context));
        Assert.Equal(context.ClientRoots["alpha"], ToolRecipeExpansion.Expand(ToolRecipeDefinitions.ClientTokenPrefix + "alpha}", context));
        Assert.Equal("off", ToolRecipeExpansion.Expand(ToolRecipeDefinitions.PreviousValueToken, context, "off"));
    }

    [Fact(DisplayName = "Inserted recipe values containing tokens stay literal")]
    public void Nonrecursive()
        => Assert.Equal("${unknown}", ToolRecipeExpansion.Expand(ToolRecipeDefinitions.MainRootToken, Context() with { MainRoot = "${unknown}" }));

    [Fact(DisplayName = "JSON expansion changes nested string values and keeps property names literal")]
    public void JsonValues()
    {
        using var source = JsonDocument.Parse("{\"${unknown}\":[\"${mainRoot}\",{\"value\":\"$$${mainRoot}\"}],\"number\":42}");
        var result = ToolRecipeExpansion.Json(source.RootElement, Context());
        Assert.Equal("context", result.GetProperty("${unknown}")[0].GetString());
        Assert.Equal("$context", result.GetProperty("${unknown}")[1].GetProperty("value").GetString());
        Assert.Equal(42, result.GetProperty("number").GetInt32());
    }

    [Theory(DisplayName = "Invalid main-root paths and client keys fail recipe context validation")]
    [InlineData("../escape", "alpha"), InlineData("context/", "alpha"), InlineData("context", "BadKey")]
    public void InvalidContext(string mainRoot, string key)
        => Assert.Throws<InvalidDataException>(() => ToolRecipeExpansion.Validate(Context() with { MainRoot = mainRoot, ClientRoots = new Dictionary<string, string> { [key] = Path.GetTempPath() } }));

    [Theory(DisplayName = "Native output patterns accept exact paths and single-level wildcard components")]
    [InlineData((int)ToolNativeOutputMatch.Exact, "file.json"), InlineData((int)ToolNativeOutputMatch.ExistingPattern, "profiles/*/file?.json")]
    public void OutputPatterns(int match, string path) => ToolNativeOutputs.ValidatePattern(path, (ToolNativeOutputMatch)match);

    [Fact(DisplayName = "Recursive wildcard and unknown output match kinds fail declaration validation")]
    public void InvalidOutputPatterns()
    {
        Assert.Throws<InvalidDataException>(() => ToolNativeOutputs.ValidatePattern("*.json", ToolNativeOutputMatch.Exact));
        Assert.Throws<InvalidDataException>(() => ToolNativeOutputs.ValidatePattern("**/file", ToolNativeOutputMatch.ExistingPattern));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolNativeOutputs.ValidatePattern("file", (ToolNativeOutputMatch)999));
        Assert.Equal(new[] { ToolNativeOutputMatch.Exact, ToolNativeOutputMatch.ExistingPattern }, Enum.GetValues<ToolNativeOutputMatch>());
    }

    private static ToolRecipeContext Context() => new()
    {
        WorkspacePath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "synthetic-workspace")),
        HomePath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "synthetic-home")),
        BinPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "synthetic-bin")),
        NativeHomePath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "synthetic-native-home")),
        NativeInstallDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "synthetic-install")),
        MainRoot = "context",
        ClientRoots = new Dictionary<string, string> { ["alpha"] = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "synthetic-client")) },
    };
}
