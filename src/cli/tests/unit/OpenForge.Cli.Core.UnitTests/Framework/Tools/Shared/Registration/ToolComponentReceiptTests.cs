using System.Text.Json;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Unit")]
public sealed class ToolComponentReceiptTests
{
    [Fact(DisplayName = "The lock graph keeps decline receipts and older tool receipts omit them")]
    public void ToolReceiptCarriesDeclines()
    {
        using var expected = JsonDocument.Parse("{\"fixture\":true}");
        var decline = new ToolComponentDeclineReceipt("fixture-component", [new()
        {
            Host = ToolVocabulary.ClaudeCodeHost, File = ToolFixtureSource.ManifestName, Pointer = "/fixture", Expected = expected.RootElement.Clone(),
        }]);
        var receipt = new ToolReceipt
        {
            Id = ToolFixtureSource.Id,
            Version = ToolFixtureSource.Version,
            Target = ToolVocabulary.Targets.Order(StringComparer.Ordinal).First(),
            ManifestBytes = ToolFixtureSource.ManifestBytes(),
            ManifestSha256 = new string('1', 64),
            ManifestFormat = "json",
            AdapterRevision = 1,
            CodeRoot = ".",
            Hosts = [ToolVocabulary.ClaudeCodeHost],
            Bindings = [],
            Files = [],
            RuntimeDirectories = [],
            NativeProjects = [],
            DeclinedComponents = [decline],
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(receipt, WorkspaceOwnershipJsonContext.Default.ToolReceipt);
        var decoded = Assert.IsType<ToolReceipt>(JsonSerializer.Deserialize(bytes, WorkspaceOwnershipJsonContext.Default.ToolReceipt));
        Assert.Equal(decline.Id, Assert.Single(Assert.IsType<ToolComponentDeclineReceipt[]>(decoded.DeclinedComponents)).Id);
        var olderBytes = JsonSerializer.SerializeToUtf8Bytes(receipt with { DeclinedComponents = null }, WorkspaceOwnershipJsonContext.Default.ToolReceipt);
        Assert.Null(JsonSerializer.Deserialize(olderBytes, WorkspaceOwnershipJsonContext.Default.ToolReceipt)?.DeclinedComponents);
    }

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
