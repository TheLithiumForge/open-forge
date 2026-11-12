using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Shared.Configuration;
using OpenForge.Cli.Core.Framework.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools;

[Trait("Feature", "tool-host-members"), Trait("Evidence", "Unit")]
public sealed class ToolHostConfigReaderTests
{
    [Theory(DisplayName = "Host member reads compare parsed values while keeping unrelated entries")]
    [InlineData(ToolVocabulary.JsonMembers)]
    [InlineData(ToolVocabulary.TomlTable)]
    public void ReadsExactMember(string format)
    {
        using var expected = JsonDocument.Parse("{\"command\":\"fixture\",\"args\":[\"a\"],\"env\":{\"FIXTURE\":\"value\"}}");
        var selector = format == ToolVocabulary.JsonMembers ? "/mcpServers/open-forge-fixture" : "mcp_servers.open-forge-fixture";
        var source = format == ToolVocabulary.JsonMembers
            ? "{\"unrelated\":true,\"mcpServers\":{\"open-forge-fixture\":{\"args\":[\"a\"],\"env\":{\"FIXTURE\":\"value\"},\"command\":\"fixture\"}}}"
            : "# untouched\nunrelated = true\n[mcp_servers.\"open-forge-fixture\"]\nargs = ['a'] # keep\ncommand = 'fixture'\n[mcp_servers.open-forge-fixture.env]\nFIXTURE = 'value'\n";
        var bytes = Encoding.UTF8.GetBytes(source);
        var before = bytes.ToArray();
        var read = ToolHostConfigReader.ReadMember(new("fixture", format, selector, expected.RootElement, false), bytes);
        Assert.Null(read.Cause);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, Assert.IsType<JsonElement>(read.Value)));
        Assert.Equal(before, bytes);
    }

    [Theory(DisplayName = "Invalid or duplicate host fields cannot count as a matching member")]
    [InlineData(ToolVocabulary.JsonMembers, "{\"value\":1,\"value\":2}")]
    [InlineData(ToolVocabulary.TomlTable, "value=1\nvalue=2\n")]
    [InlineData(ToolVocabulary.TomlTable, "[broken\n")]
    public void RejectsInvalidDocuments(string format, string source)
    {
        using var expected = JsonDocument.Parse("{}");
        var selector = format == ToolVocabulary.JsonMembers ? "/value" : "value";
        var read = ToolHostConfigReader.ReadMember(new("fixture", format, selector, expected.RootElement, false), Encoding.UTF8.GetBytes(source));
        Assert.Null(read.Value);
        Assert.NotNull(read.Cause);
    }
}
