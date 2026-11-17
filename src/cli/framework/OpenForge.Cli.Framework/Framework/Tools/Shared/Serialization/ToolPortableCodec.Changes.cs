using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

internal sealed partial class ToolPortableCodec
{
    private static ToolAppliedChange ReadChange(JsonObject entry)
    {
        var id = String(entry, ToolRecordDefinitions.IdProperty);
        var kind = String(entry, ToolRecordDefinitions.KindProperty);
        if (kind == ToolRecordDefinitions.NativeCommandKind)
            return new ToolNativeCommandAddition { Id = id, AllowListId = String(entry, ToolRecordDefinitions.AllowListIdProperty) };
        var path = String(entry, ToolRecordDefinitions.PathProperty);
        if (Path.IsPathRooted(path) || path.Contains('\\') || path.Split('/').Any(part => part is "" or "." or "..") || path.Contains(':'))
            throw Invalid(ToolRecordDefinitions.PathProperty, "a workspace-relative file path");
        switch (kind)
        {
            case ToolRecordDefinitions.JsonUnionKind:
                if (entry[ToolRecordDefinitions.ItemsProperty] is not JsonArray items) throw Invalid(ToolRecordDefinitions.ItemsProperty, "an array");
                return new ToolJsonUnionAddition { Id = id, Path = path, Pointer = Pointer(entry), Items = items.Select(Element).ToArray() };
            case ToolRecordDefinitions.JsonValueKind:
                if (!entry.ContainsKey(ToolRecordDefinitions.ValueProperty)) throw Invalid(ToolRecordDefinitions.ValueProperty, "a JSON value");
                return new ToolJsonValueAddition { Id = id, Path = path, Pointer = Pointer(entry), Value = Element(entry[ToolRecordDefinitions.ValueProperty]) };
            case ToolRecordDefinitions.MarkedBlockKind:
                var digest = String(entry, ToolRecordDefinitions.ContentSha256Property);
                ValidateDigest(digest);
                return new ToolMarkedBlockAddition
                {
                    Id = id,
                    Path = path,
                    ContentSha256 = digest,
                    StartMarker = String(entry, ToolRecordDefinitions.StartMarkerProperty),
                    EndMarker = String(entry, ToolRecordDefinitions.EndMarkerProperty),
                };
            case ToolRecordDefinitions.ManagedFileKind:
                var hash = String(entry, ToolRecordDefinitions.Sha256Property);
                ValidateDigest(hash);
                return new ToolManagedFileAddition { Id = id, Path = path, Sha256 = hash };
            default:
                throw Invalid(ToolRecordDefinitions.KindProperty, "a recognized additive receipt kind");
        }
    }

    private static JsonObject WriteChange(ToolAppliedChange change)
    {
        var entry = new JsonObject { [ToolRecordDefinitions.IdProperty] = change.Id };
        switch (change)
        {
            case ToolJsonUnionAddition union:
                entry[ToolRecordDefinitions.KindProperty] = ToolRecordDefinitions.JsonUnionKind;
                entry[ToolRecordDefinitions.PathProperty] = union.Path;
                entry[ToolRecordDefinitions.PointerProperty] = union.Pointer;
                entry[ToolRecordDefinitions.ItemsProperty] = new JsonArray(union.Items.Select(item => JsonNode.Parse(item.GetRawText())).ToArray());
                break;
            case ToolJsonValueAddition value:
                entry[ToolRecordDefinitions.KindProperty] = ToolRecordDefinitions.JsonValueKind;
                entry[ToolRecordDefinitions.PathProperty] = value.Path;
                entry[ToolRecordDefinitions.PointerProperty] = value.Pointer;
                entry[ToolRecordDefinitions.ValueProperty] = JsonNode.Parse(value.Value.GetRawText());
                break;
            case ToolMarkedBlockAddition block:
                entry[ToolRecordDefinitions.KindProperty] = ToolRecordDefinitions.MarkedBlockKind;
                entry[ToolRecordDefinitions.PathProperty] = block.Path;
                entry[ToolRecordDefinitions.StartMarkerProperty] = block.StartMarker;
                entry[ToolRecordDefinitions.EndMarkerProperty] = block.EndMarker;
                entry[ToolRecordDefinitions.ContentSha256Property] = block.ContentSha256;
                break;
            case ToolManagedFileAddition file:
                entry[ToolRecordDefinitions.KindProperty] = ToolRecordDefinitions.ManagedFileKind;
                entry[ToolRecordDefinitions.PathProperty] = file.Path;
                entry[ToolRecordDefinitions.Sha256Property] = file.Sha256;
                break;
            case ToolNativeCommandAddition native:
                entry[ToolRecordDefinitions.KindProperty] = ToolRecordDefinitions.NativeCommandKind;
                entry[ToolRecordDefinitions.AllowListIdProperty] = native.AllowListId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change));
        }
        _ = ReadChange(entry);
        return entry;
    }

}
