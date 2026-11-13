using System.Text.Json;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Unit")]
public sealed class ToolComponentReceiptTests
{
    [Fact(DisplayName = "Decline receipts keep every absent entry without requiring a linked rewrite")]
    public void DeclineReceiptRoundTrips()
    {
        const string component = "fixture-component";
        const string file = "fixture.json";
        const string pointer = "/hooks/0";
        using var expected = JsonDocument.Parse("{\"fixture\":true}");
        var receipt = new ToolComponentDeclineReceipt(component,
            [new() { Host = ToolVocabulary.ClaudeCodeHost, File = file, Pointer = pointer, Expected = expected.RootElement.Clone() }]);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(receipt, WorkspaceOwnershipJsonContext.Default.ToolComponentDeclineReceipt);
        var decoded = Assert.IsType<ToolComponentDeclineReceipt>(JsonSerializer.Deserialize(bytes, WorkspaceOwnershipJsonContext.Default.ToolComponentDeclineReceipt));
        Assert.Equal(component, decoded.Id);
        var absent = Assert.Single(decoded.AbsentEntries);
        Assert.Equal(file, absent.File);
        Assert.Equal(pointer, absent.Pointer);
        Assert.Equal(ToolVocabulary.ClaudeCodeHost, absent.Host);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, absent.Expected));
    }
}
