using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

// Tool record behavior defines these additions. This planner has no filesystem effects.
internal static class ToolConfigEditor
{
    private static readonly byte[] EmptyObject = "{}"u8.ToArray();

    internal static ToolConfigEdit Json(ToolJsonValueChange change, byte[]? before, ToolAppliedChange? prior)
    {
        if (change.Value is not { } desired || desired.ValueKind == JsonValueKind.Undefined) throw new InvalidDataException("The JSON recipe has no value.");
        if (ToolJsonEntries.Parts(change.Pointer).Length == 0) throw new InvalidDataException("A JSON recipe must select a member.");
        using var entries = new ToolJsonEntries(before ?? EmptyObject);
        var current = entries.Value(change.Pointer);
        switch (change.Operation)
        {
            case ToolJsonValueOperation.SetIfAbsent:
                if (prior is null)
                {
                    var bytes = entries.Set(change.Pointer, desired, requireAbsent: true);
                    return new(bytes, new ToolJsonValueAddition { Id = change.Id, Path = change.PathTemplate, Pointer = change.Pointer, Value = desired.Clone() }, ToolChangeDisposition.Add);
                }
                if (prior is not ToolJsonValueAddition set || set.Path != change.PathTemplate || set.Pointer != change.Pointer
                    || current is not { } found || !JsonElement.DeepEquals(found, set.Value)) throw new InvalidDataException("The recorded JSON value has drifted.");
                if (JsonElement.DeepEquals(found, desired)) return new(before, prior, ToolChangeDisposition.NoChange);
                return new(entries.Set(change.Pointer, desired, requireAbsent: false), set with { Value = desired.Clone() }, ToolChangeDisposition.Migrate);
            case ToolJsonValueOperation.Union:
                if (desired.ValueKind != JsonValueKind.Array) throw new InvalidDataException("A union recipe requires an array value.");
                if (current is { } present && present.ValueKind != JsonValueKind.Array) throw new InvalidDataException("A JSON union requires an array or a missing member.");
                if (prior is not null && (prior is not ToolJsonUnionAddition union || union.Path != change.PathTemplate || union.Pointer != change.Pointer))
                    throw new InvalidDataException("The recorded JSON union has a different target or kind.");
                var items = desired.EnumerateArray().ToArray();
                if (items.Length == 0 && current is null && prior is null) return new(before, null, ToolChangeDisposition.NoChange);
                var oldItems = (prior as ToolJsonUnionAddition)?.Items ?? [];
                var stale = oldItems.Where(item => !items.Any(wanted => JsonElement.DeepEquals(item, wanted))).ToArray();
                var withoutStale = entries.RemoveItems(change.Pointer, stale);
                using (var clean = new ToolJsonEntries(withoutStale))
                {
                    var cleanValue = clean.Value(change.Pointer);
                    var existing = cleanValue?.EnumerateArray().ToArray() ?? [];
                    var added = oldItems.Where(item => items.Any(wanted => JsonElement.DeepEquals(item, wanted))).Select(item => item.Clone()).ToList();
                    foreach (var item in items)
                        if (!existing.Any(value => JsonElement.DeepEquals(value, item)) && !added.Any(value => JsonElement.DeepEquals(value, item))) added.Add(item.Clone());
                    var after = clean.Union(change.Pointer, items);
                    ToolAppliedChange? receipt = added.Count == 0 && prior is null ? null : new ToolJsonUnionAddition
                    { Id = change.Id, Path = change.PathTemplate, Pointer = change.Pointer, Items = added };
                    return new(after, receipt, Disposition(before, after, prior));
                }
            default: throw new ArgumentOutOfRangeException(nameof(change), change.Operation, "The JSON operation is not defined.");
        }
    }

    internal static ToolConfigEdit File(ToolManagedFileChange change, byte[]? before, byte[] desired, ToolAppliedChange? prior)
    {
        var digest = FileExpectation.Hash(desired);
        if (prior is null)
        {
            if (before is not null) throw new InvalidDataException("The managed file is already present.");
            return new(desired, new ToolManagedFileAddition { Id = change.Id, Path = change.PathTemplate, Sha256 = digest }, ToolChangeDisposition.Add);
        }
        if (prior is not ToolManagedFileAddition file || file.Path != change.PathTemplate || before is null || FileExpectation.Hash(before) != file.Sha256)
            throw new InvalidDataException("The managed file has drifted.");
        return new(desired, file with { Sha256 = digest }, Disposition(before, desired, prior));
    }

    internal static ToolConfigEdit Block(ToolMarkedBlockChange change, byte[]? before, ToolAppliedChange? prior)
    {
        var desired = ToolMarkedBlocks.Render(change.StartMarker, change.EndMarker, change.ContentTemplate);
        var source = before ?? [];
        var receipt = new ToolMarkedBlockAddition
        { Id = change.Id, Path = change.PathTemplate, StartMarker = change.StartMarker, EndMarker = change.EndMarker, ContentSha256 = FileExpectation.Hash(desired) };
        if (prior is null)
        {
            if (ToolMarkedBlocks.Find(source, change.StartMarker, change.EndMarker) is not null) throw new InvalidDataException("The marked block is already present without a receipt.");
            return new(ToolMarkedBlocks.Add(source, desired, preferEnd: change.WhenNotIgnored is not null), receipt, ToolChangeDisposition.Add);
        }
        if (prior is not ToolMarkedBlockAddition block || block.Path != change.PathTemplate) throw new InvalidDataException("The recorded block has a different path or kind.");
        var span = ToolMarkedBlocks.Find(source, block.StartMarker, block.EndMarker) ?? throw new InvalidDataException("The recorded block is missing.");
        if (FileExpectation.Hash(source.AsSpan(span.Start, span.End - span.Start)) != block.ContentSha256) throw new InvalidDataException("The marked block has drifted.");
        var without = ToolConfigBytes.Replace(source, [span]);
        if (ToolMarkedBlocks.Find(without, change.StartMarker, change.EndMarker) is not null) throw new InvalidDataException("The replacement block markers are already present.");
        var after = change.WhenNotIgnored is null ? ToolConfigBytes.Replace(source, [span with { Replacement = desired }])
            : ToolMarkedBlocks.Add(without, desired, preferEnd: true);
        return new(after, receipt, Disposition(before, after, prior));
    }

    internal static byte[]? Remove(ToolAppliedChange addition, byte[]? before)
    {
        if (before is null) return null;
        switch (addition)
        {
            case ToolJsonUnionAddition union:
                using (var entries = new ToolJsonEntries(before)) return entries.RemoveItems(union.Pointer, union.Items);
            case ToolJsonValueAddition value:
                using (var entries = new ToolJsonEntries(before))
                {
                    if (entries.Value(value.Pointer) is not { } current) return before;
                    if (!JsonElement.DeepEquals(current, value.Value)) throw new InvalidDataException("The recorded JSON value has drifted.");
                    return entries.Remove(value.Pointer);
                }
            case ToolManagedFileAddition file:
                if (FileExpectation.Hash(before) != file.Sha256) throw new InvalidDataException("The managed file has drifted.");
                return null;
            case ToolMarkedBlockAddition block:
                var span = ToolMarkedBlocks.Find(before, block.StartMarker, block.EndMarker);
                return span is null ? before : ToolConfigBytes.Replace(before, [span]);
            default: throw new ArgumentOutOfRangeException(nameof(addition), addition, "This addition has no file removal rule.");
        }
    }

    internal static string PathOf(ToolConfigChange change) => change switch
    {
        ToolJsonValueChange json => json.PathTemplate,
        ToolMarkedBlockChange block => block.PathTemplate,
        ToolManagedFileChange file => file.PathTemplate,
        _ => throw new ArgumentOutOfRangeException(nameof(change), change, "This change has no file target."),
    };

    internal static string PathOf(ToolAppliedChange addition) => addition switch
    {
        ToolJsonUnionAddition union => union.Path,
        ToolJsonValueAddition value => value.Path,
        ToolMarkedBlockAddition block => block.Path,
        ToolManagedFileAddition file => file.Path,
        _ => throw new ArgumentOutOfRangeException(nameof(addition), addition, "This addition has no file target."),
    };

    private static ToolChangeDisposition Disposition(byte[]? before, byte[] after, ToolAppliedChange? prior)
    {
        if (before is not null && before.AsSpan().SequenceEqual(after)) return ToolChangeDisposition.NoChange;
        return prior is null ? ToolChangeDisposition.Add : ToolChangeDisposition.Migrate;
    }
}
