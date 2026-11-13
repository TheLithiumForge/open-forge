using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools;

[Trait("Feature", "tool-records"), Trait("Evidence", "Unit")]
public sealed class ToolReceiptTests
{
    [Fact(DisplayName = "Workspace tool receipts hold store references and reject physical binding coordinates")]
    public void KeepsPhysicalBindingsOutsideWorkspaceLock()
    {
        var receipt = Receipt() with { Bindings = [new("fixture-store", "install", new string('1', 64))] };
        var document = WorkspaceOwnershipDocument.Empty with
        { Tools = ImmutableDictionary<string, ToolReceipt>.Empty.Add(ToolFixtureSource.Id, receipt) };
        var bytes = WorkspaceOwnershipCodec.Write(document);
        var root = Assert.IsType<JsonObject>(JsonNode.Parse(bytes));
        var tools = Assert.IsType<JsonObject>(root[JsonNamingPolicy.CamelCase.ConvertName(nameof(WorkspaceOwnershipDocument.Tools))]);
        var tool = Assert.IsType<JsonObject>(tools[ToolFixtureSource.Id]);
        var references = Assert.IsType<JsonArray>(tool[JsonNamingPolicy.CamelCase.ConvertName(nameof(ToolReceipt.Bindings))]);
        var reference = Assert.IsType<JsonObject>(Assert.Single(references));
        var physicalRoot = JsonNamingPolicy.CamelCase.ConvertName(nameof(ToolStoreBinding.PhysicalRoot));
        Assert.False(reference.ContainsKey(physicalRoot));
        Assert.NotNull(WorkspaceOwnershipCodec.Read(bytes).Document);
        reference[physicalRoot] = Path.GetFullPath("fixture-store");
        Assert.Null(WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(root.ToJsonString())).Document);
    }

    [Fact(DisplayName = "Lock v2 round trips exact tool snapshots and keeps the existing Framework receipt")]
    public void RoundTripsPairedRecords()
    {
        var receipt = Receipt();
        var document = new WorkspaceOwnershipDocument(WorkspaceOwnershipDefinitions.SchemaVersion,
            new(new("fixture-framework", "1.0.0"), [], []), [], [])
        {
            Tools = ImmutableDictionary<string, ToolReceipt>.Empty.Add(ToolFixtureSource.Id, receipt),
            Recovery = [new("fixture-recovery", ".open-forge-state/recovery/v2/fixture/operation.zip", new string('1', 64), "prepared")],
        };
        var bytes = WorkspaceOwnershipCodec.Write(document);
        var read = Assert.IsType<WorkspaceOwnershipDocument>(WorkspaceOwnershipCodec.Read(bytes).Document);
        var framework = Assert.IsType<FrameworkOwnership>(read.Framework);
        Assert.Equal(Assert.IsType<FrameworkOwnership>(document.Framework).Source, framework.Source);
        Assert.Empty(framework.Paths);
        Assert.Empty(framework.Regions);
        Assert.Equal(receipt.ManifestBytes, read.Tools[ToolFixtureSource.Id].ManifestBytes);
        Assert.Equal(receipt.ManifestSha256, read.Tools[ToolFixtureSource.Id].ManifestSha256);
        Assert.Equal(document.Recovery.ToArray(), read.Recovery.ToArray());
        using var json = JsonDocument.Parse(bytes);
        Assert.Equal(WorkspaceOwnershipDefinitions.SchemaVersion, json.RootElement.GetProperty("schemaVersion").GetInt32());
    }

    [Theory(DisplayName = "Malformed tool lock claims cannot become verified receipts")]
    [InlineData("snapshot")]
    [InlineData("pin")]
    [InlineData("path")]
    [InlineData("null")]
    [InlineData("duplicate")]
    public void RejectsMalformedClaims(string kind)
    {
        var document = WorkspaceOwnershipDocument.Empty with { Tools = ImmutableDictionary<string, ToolReceipt>.Empty.Add(ToolFixtureSource.Id, Receipt()) };
        var root = JsonNode.Parse(WorkspaceOwnershipCodec.Write(document))?.AsObject() ?? throw new InvalidDataException();
        var tools = Assert.IsType<JsonObject>(root["tools"]);
        var tool = Assert.IsType<JsonObject>(tools[ToolFixtureSource.Id]);
        switch (kind)
        {
            case "snapshot": tool["manifestSha256"] = new string('0', 64); break;
            case "pin": tool["version"] = "2.0.0"; break;
            case "path": tool["files"] = new JsonArray(new JsonObject { ["storeId"] = null, ["path"] = "../escape", ["sha256"] = new string('1', 64), ["executable"] = false }); break;
            case "null": tool["bindings"] = null; break;
        }
        var text = root.ToJsonString();
        if (kind == "duplicate") text = text.Replace("\"tools\":", "\"tools\":{},\"tools\":", StringComparison.Ordinal);
        Assert.Null(WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(text)).Document);
    }

    private static ToolReceipt Receipt() => new()
    {
        Id = ToolFixtureSource.Id,
        Version = ToolFixtureSource.Version,
        Target = "win-x64",
        ManifestSha256 = ToolCatalogueReader.Hash(ToolFixtureSource.ManifestBytes()),
        ManifestBytes = ToolFixtureSource.ManifestBytes(),
        ManifestFormat = "json",
        AdapterRevision = 1,
        CodeRoot = ".",
        Hosts = [],
        Bindings = [],
        Files = [],
        Members = [],
        RuntimeDirectories = [],
        NativeProjects = [],
    };
}
