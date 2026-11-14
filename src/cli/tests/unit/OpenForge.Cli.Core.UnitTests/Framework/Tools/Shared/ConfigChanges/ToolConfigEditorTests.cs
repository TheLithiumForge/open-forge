using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-config"), Trait("Evidence", "Unit")]
public sealed class ToolConfigEditorTests
{
    private const string Target = "synthetic.json";
    private const string Pointer = "/items";
    private const string Start = "# synthetic start";
    private const string End = "# synthetic end";

    [Fact(DisplayName = "Union appends only missing items and records no pre-existing item")]
    public void UnionAccounting()
    {
        var before = Bytes(" { \"keep\" : [ 1, 2 ], \"items\" : [\"present\"] }\r\n");
        var edit = ToolConfigEditor.Json(Json("[\"present\",\"new\",\"new\"]"), before, null);
        var addition = Assert.IsType<ToolJsonUnionAddition>(edit.Addition);
        Assert.Equal("new", Assert.Single(addition.Items).GetString());
        Assert.Equal(Bytes(" { \"keep\" : [ 1, 2 ], \"items\" : [\"present\",\"new\"] }\r\n"), edit.Bytes);
        Assert.Equal(ToolChangeDisposition.Add, edit.Disposition);
        Assert.Equal(before, ToolConfigEditor.Remove(addition, edit.Bytes));
    }

    [Theory(DisplayName = "Initial JSON sets conflict with every present member including null and an equal value")]
    [InlineData("null"), InlineData("true"), InlineData("false"), InlineData("[]"), InlineData("{}")]
    public void PresentSetConflicts(string current)
        => Assert.Throws<InvalidDataException>(() => ToolConfigEditor.Json(Json("true", ToolJsonValueOperation.SetIfAbsent), Bytes("{\"items\":" + current + "}"), null));

    [Fact(DisplayName = "An absent set creates its receipt and only its JSON member")]
    public void SetAbsent()
    {
        var before = Bytes("{\"keep\": [ 1 ]}\n");
        var edit = ToolConfigEditor.Json(Json("null", ToolJsonValueOperation.SetIfAbsent), before, null);
        var receipt = Assert.IsType<ToolJsonValueAddition>(edit.Addition);
        Assert.Equal(JsonValueKind.Null, receipt.Value.ValueKind);
        Assert.Equal(before, ToolConfigEditor.Remove(receipt, edit.Bytes));
    }

    [Fact(DisplayName = "Set reapply is unchanged and migration requires the recorded value")]
    public void SetMigration()
    {
        var change = Json("true", ToolJsonValueOperation.SetIfAbsent);
        var first = ToolConfigEditor.Json(change, null, null);
        var same = ToolConfigEditor.Json(change, first.Bytes, first.Addition);
        Assert.Equal(ToolChangeDisposition.NoChange, same.Disposition);
        Assert.Equal(first.Bytes, same.Bytes);
        var migrated = ToolConfigEditor.Json(change with { Value = Element("false") }, first.Bytes, first.Addition);
        Assert.Equal(ToolChangeDisposition.Migrate, migrated.Disposition);
        Assert.Throws<InvalidDataException>(() => ToolConfigEditor.Json(change, Bytes("{\"items\":42}"), first.Addition));
        Assert.Throws<InvalidDataException>(() => ToolConfigEditor.Remove(Assert.IsType<ToolJsonValueAddition>(first.Addition), Bytes("{\"items\":42}")));
    }

    [Theory(DisplayName = "Non-array union destinations conflict without modifying their input")]
    [InlineData("null"), InlineData("false"), InlineData("1"), InlineData("{}")]
    public void UnionShape(string current)
        => Assert.Throws<InvalidDataException>(() => ToolConfigEditor.Json(Json("[1]"), Bytes("{\"items\":" + current + "}"), null));

    [Fact(DisplayName = "Union migration extends receipts without claiming newly pre-existing values")]
    public void UnionMigration()
    {
        var first = ToolConfigEditor.Json(Json("[1,2]"), Bytes("{\"items\":[1]}"), null);
        var same = ToolConfigEditor.Json(Json("[1,2]"), first.Bytes, first.Addition);
        Assert.Equal(ToolChangeDisposition.NoChange, same.Disposition);
        var migrated = ToolConfigEditor.Json(Json("[1,2,3,4]"), Bytes("{\"items\":[1,2,3]}"), first.Addition);
        var receipt = Assert.IsType<ToolJsonUnionAddition>(migrated.Addition);
        Assert.Equal(new[] { 2, 4 }, receipt.Items.Select(item => item.GetInt32()));
        var reduced = ToolConfigEditor.Json(Json("[1,3,4]"), migrated.Bytes, receipt);
        Assert.Equal(new[] { 4 }, Assert.IsType<ToolJsonUnionAddition>(reduced.Addition).Items.Select(item => item.GetInt32()));
        using var parsed = new ToolJsonEntries(Assert.IsType<byte[]>(reduced.Bytes));
        Assert.Equal(new[] { 1, 3, 4 }, parsed.Value(Pointer)?.EnumerateArray().Select(item => item.GetInt32()));
    }

    [Fact(DisplayName = "Union removal leaves unrelated objects and missing recorded items alone")]
    public void UnionRemoval()
    {
        var receipt = new ToolJsonUnionAddition { Id = "union", Path = Target, Pointer = Pointer, Items = [Element("2"), Element("9")] };
        var after = ToolConfigEditor.Remove(receipt, Bytes("{\"items\":[1,2,3],\"keep\": 4}"));
        Assert.Equal(Bytes("{\"items\":[1,3],\"keep\": 4}"), after);
    }

    [Fact(DisplayName = "Union removal consumes only the recorded number of duplicate values")]
    public void UnionDuplicateRemoval()
    {
        var receipt = new ToolJsonUnionAddition { Id = "union", Path = Target, Pointer = Pointer, Items = [Element("2")] };
        Assert.Equal(Bytes("{\"items\":[1,2,3]}"), ToolConfigEditor.Remove(receipt, Bytes("{\"items\":[1,2,2,3]}")));
    }

    [Fact(DisplayName = "An empty union on an absent target is a no-op without an unrecorded file")]
    public void EmptyUnion()
    {
        var edit = ToolConfigEditor.Json(Json("[]"), null, null);
        Assert.Null(edit.Bytes);
        Assert.Null(edit.Addition);
        Assert.Equal(ToolChangeDisposition.NoChange, edit.Disposition);
    }

    [Theory(DisplayName = "JSON Pointer escapes select exact object keys and missing object parents")]
    [InlineData("/a~1b/~0name", "a/b", "~name"), InlineData("/nested/item", "nested", "item")]
    public void PointerParents(string pointer, string parent, string member)
    {
        using var entries = new ToolJsonEntries(Bytes("{}"));
        var after = entries.Set(pointer, Element("true"), requireAbsent: true);
        using var document = JsonDocument.Parse(after);
        Assert.True(document.RootElement.GetProperty(parent).GetProperty(member).GetBoolean());
    }

    [Theory(DisplayName = "Invalid JSON pointers and duplicate JSON properties are rejected")]
    [InlineData("bad"), InlineData("/~"), InlineData("/~2"), InlineData("/x~3y")]
    public void InvalidPointer(string pointer) => Assert.Throws<InvalidDataException>(() => ToolJsonEntries.Parts(pointer));

    [Fact(DisplayName = "Duplicate JSON member names cannot create an ambiguous edit")]
    public void DuplicateMembers() => Assert.Throws<JsonException>(() => new ToolJsonEntries(Bytes("{\"items\":[],\"items\":[]}")));

    [Fact(DisplayName = "JSON editing keeps a UTF-8 BOM and unrelated Unicode bytes")]
    public void BytePreservation()
    {
        byte[] before = [0xef, 0xbb, 0xbf, .. Bytes("{\"keep\":\"λ\",\"items\":[] }\r\n")];
        var edit = ToolConfigEditor.Json(Json("[1]"), before, null);
        byte[] expected = [0xef, 0xbb, 0xbf, .. Bytes("{\"keep\":\"λ\",\"items\":[1] }\r\n")];
        Assert.Equal(expected, edit.Bytes);
    }

    [Fact(DisplayName = "Marked block add reapply migration and removal keep surrounding bytes")]
    public void BlockLifecycle()
    {
        var before = Bytes("unrelated without newline");
        var change = Block("first");
        var first = ToolConfigEditor.Block(change, before, null);
        Assert.Equal(ToolChangeDisposition.NoChange, ToolConfigEditor.Block(change, first.Bytes, first.Addition).Disposition);
        var migrated = ToolConfigEditor.Block(change with { ContentTemplate = "second" }, first.Bytes, first.Addition);
        Assert.Equal(ToolChangeDisposition.Migrate, migrated.Disposition);
        Assert.Equal(before, ToolConfigEditor.Remove(Assert.IsType<ToolMarkedBlockAddition>(migrated.Addition), migrated.Bytes));
    }

    [Theory(DisplayName = "Malformed duplicated reversed and partial block markers conflict")]
    [InlineData("# synthetic start\nx\n"), InlineData("# synthetic end\n# synthetic start\n"), InlineData("prefix # synthetic start\nx\n# synthetic end\n")]
    [InlineData("# synthetic start\n# synthetic start\nx\n# synthetic end\n"), InlineData("# synthetic start\nx\n# synthetic end\n# synthetic end\n")]
    public void AmbiguousBlocks(string source) => Assert.Throws<InvalidDataException>(() => ToolConfigEditor.Block(Block("value"), Bytes(source), null));

    [Fact(DisplayName = "Block drift prevents migration but removal deletes the uniquely marked span")]
    public void BlockDrift()
    {
        var first = ToolConfigEditor.Block(Block("first"), Bytes("keep\n"), null);
        var changed = Bytes(Start + "\nchanged\n" + End + "\nkeep\n");
        Assert.Throws<InvalidDataException>(() => ToolConfigEditor.Block(Block("second"), changed, first.Addition));
        Assert.Equal(Bytes("keep\n"), ToolConfigEditor.Remove(Assert.IsType<ToolMarkedBlockAddition>(first.Addition), changed));
    }

    [Fact(DisplayName = "A block without a receipt conflicts even when its content matches")]
    public void UnrecordedBlock()
    {
        var first = ToolConfigEditor.Block(Block("first"), null, null);
        Assert.Throws<InvalidDataException>(() => ToolConfigEditor.Block(Block("first"), first.Bytes, null));
    }

    [Fact(DisplayName = "Managed files require absence and migrate only matching recorded hashes")]
    public void FileLifecycle()
    {
        var change = new ToolManagedFileChange
        {
            Id = "file",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.AbsentFile,
            Description = "synthetic file",
            PathTemplate = Target,
            TemplateResource = "synthetic-resource"
        };
        var first = ToolConfigEditor.File(change, null, Bytes("first"), null);
        var receipt = Assert.IsType<ToolManagedFileAddition>(first.Addition);
        Assert.Equal(FileExpectation.Hash(Bytes("first")), receipt.Sha256);
        Assert.Equal(ToolChangeDisposition.NoChange, ToolConfigEditor.File(change, first.Bytes, Bytes("first"), receipt).Disposition);
        Assert.Equal(ToolChangeDisposition.Migrate, ToolConfigEditor.File(change, first.Bytes, Bytes("second"), receipt).Disposition);
        Assert.Throws<InvalidDataException>(() => ToolConfigEditor.File(change, first.Bytes, Bytes("first"), null));
        Assert.Throws<InvalidDataException>(() => ToolConfigEditor.File(change, Bytes("drift"), Bytes("second"), receipt));
        Assert.Throws<InvalidDataException>(() => ToolConfigEditor.Remove(receipt, Bytes("drift")));
        Assert.Null(ToolConfigEditor.Remove(receipt, first.Bytes));
    }

    [Fact(DisplayName = "Undefined JSON recipe operations fail before any edit")]
    public void UndefinedOperation() => Assert.Throws<ArgumentOutOfRangeException>(() => ToolConfigEditor.Json(Json("true", (ToolJsonValueOperation)999), null, null));

    private static ToolJsonValueChange Json(string value, ToolJsonValueOperation operation = ToolJsonValueOperation.Union) => new()
    {
        Id = "json",
        Scope = ToolEffectScope.Workspace,
        InitialState = ToolInitialConfigState.JsonArrayOrMissing,
        Description = "synthetic JSON",
        PathTemplate = Target,
        Pointer = Pointer,
        Operation = operation,
        Value = Element(value)
    };
    private static ToolMarkedBlockChange Block(string content) => new()
    {
        Id = "block",
        Scope = ToolEffectScope.Workspace,
        InitialState = ToolInitialConfigState.UniqueMarkedBlock,
        Description = "synthetic block",
        PathTemplate = Target,
        StartMarker = Start,
        EndMarker = End,
        ContentTemplate = content
    };
    private static byte[] Bytes(string value) => Encoding.UTF8.GetBytes(value);
    private static JsonElement Element(string value)
    {
        using var document = JsonDocument.Parse(value);
        return document.RootElement.Clone();
    }
}
