using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

internal static class ToolHookEntries
{
    internal const string Hooks = "hooks";
    internal const string Type = "type";
    internal const string CommandType = "command";

    internal static ToolHookMatch? Find(ToolJsonEntries document, string eventPointer, string command)
    {
        var groups = Groups(document, eventPointer);
        ToolHookMatch? found = null;
        for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            var group = groups[groupIndex];
            var hooks = group.GetProperty(Hooks);
            for (var hookIndex = 0; hookIndex < hooks.GetArrayLength(); hookIndex++)
            {
                var hook = hooks[hookIndex];
                if (hook.ValueKind != JsonValueKind.Object) throw new InvalidDataException("A hook entry must be an object.");
                if (!hook.TryGetProperty(ToolJsonEntries.Command, out var current)) continue;
                if (current.ValueKind != JsonValueKind.String) throw new InvalidDataException("A hook command must be a string.");
                if (current.GetString() != command) continue;
                if (!hook.TryGetProperty(Type, out var type) || type.ValueKind != JsonValueKind.String || type.GetString() != CommandType)
                    throw new InvalidDataException("The selected hook is not a command entry.");
                if (found is not null) throw new InvalidDataException("The exact hook command appears more than once in its event.");
                var groupPointer = eventPointer + "/" + groupIndex.ToString(CultureInfo.InvariantCulture);
                found = new(groupPointer, groupPointer + "/" + Hooks + "/" + hookIndex.ToString(CultureInfo.InvariantCulture), group, hook);
            }
        }
        return found;
    }

    internal static JsonElement[] Groups(ToolJsonEntries document, string eventPointer)
    {
        var parts = ToolJsonEntries.Parts(eventPointer);
        if (parts.Length != 2 || parts[0] != Hooks || parts[1].Length == 0)
            throw new InvalidDataException("An exact hook event Pointer is required.");
        if (document.Value("") is not { ValueKind: JsonValueKind.Object }
            || document.Value("/" + Hooks) is { ValueKind: not JsonValueKind.Object })
            throw new InvalidDataException("Hook settings and events must be objects.");
        if (document.Value(eventPointer) is not { } value) return [];
        if (value.ValueKind != JsonValueKind.Array) throw new InvalidDataException("A hook event must be an array of matcher groups.");
        var groups = value.EnumerateArray().ToArray();
        if (groups.Any(group => group.ValueKind != JsonValueKind.Object || !group.TryGetProperty(Hooks, out var hooks) || hooks.ValueKind != JsonValueKind.Array))
            throw new InvalidDataException("A matcher group must contain a hooks array.");
        return groups;
    }

    internal static string ExpectedCommand(ToolJsonEntryRemoval removal)
    {
        if (removal.Expected.ValueKind != JsonValueKind.Object || !removal.Expected.TryGetProperty(ToolJsonEntries.Command, out var command)
            || command.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(command.GetString()))
            throw new InvalidDataException("A hook removal requires its complete expected command entry.");
        return command.GetString() ?? throw new InvalidDataException();
    }

    internal static JsonElement EmptyGroup(JsonElement group)
    {
        var node = JsonNode.Parse(group.GetRawText())?.AsObject() ?? throw new InvalidDataException();
        node[Hooks] = new JsonArray();
        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }

    internal static ToolHookGroupReceipt? CreatedGroup(
        ToolEntryRewriteRequest request, string file, string eventPointer, ToolHookMatch selected, ToolJsonEntries current)
    {
        var empty = EmptyGroup(selected.Group);
        foreach (var receipt in request.CreatedHookGroups.Where(receipt => receipt.File == file && receipt.EventPointer == eventPointer))
        {
            if (receipt.EmptyGroup.ValueKind != JsonValueKind.Object || !receipt.EmptyGroup.TryGetProperty(Hooks, out var hooks)
                || hooks.ValueKind != JsonValueKind.Array || hooks.GetArrayLength() != 0)
                throw new InvalidDataException("The created hook-group receipt is invalid.");
            if (!JsonElement.DeepEquals(empty, receipt.EmptyGroup)) continue;
            if (Groups(current, eventPointer).Count(group => JsonElement.DeepEquals(EmptyGroup(group), empty)) != 1)
                throw new InvalidDataException("The created hook-group receipt is ambiguous.");
            return receipt;
        }
        if (request.BeforeInstallerFiles is null || !request.BeforeInstallerFiles.TryGetValue(file, out var priorBytes)) return null;
        using var prior = priorBytes is null ? null : new ToolJsonEntries(priorBytes);
        var previous = prior is null ? [] : Groups(prior, eventPointer);
        // Every prior group must survive unchanged outside the selected group before we call that group new.
        var others = Groups(current, eventPointer).ToList();
        var selectedIndex = int.Parse(ToolJsonEntries.Parts(selected.GroupPointer)[^1], CultureInfo.InvariantCulture);
        others.RemoveAt(selectedIndex);
        foreach (var group in previous)
        {
            var index = others.FindIndex(candidate => JsonElement.DeepEquals(candidate, group));
            if (index < 0) return null;
            others.RemoveAt(index);
        }
        return new(file, eventPointer, empty);
    }

    internal static ToolJsonEntryRemoval[] ResolveRemovals(ToolJsonEntries document, ToolJsonEntryRemoval[] removals, ToolEntryRewriteRequest? request)
    {
        var resolved = new List<ToolJsonEntryRemoval>();
        var selected = new List<(ToolJsonEntryRemoval Removal, ToolHookMatch Match)>();
        foreach (var removal in removals)
        {
            if (!removal.MatchCommand) { resolved.Add(removal); continue; }
            var match = Find(document, removal.Pointer, ToolHookEntries.ExpectedCommand(removal)) ?? throw new InvalidDataException();
            selected.Add((removal, match));
            resolved.Add(removal with { Pointer = match.EntryPointer, MatchCommand = false });
        }
        var pointers = resolved.Select(removal => removal.Pointer).ToHashSet(StringComparer.Ordinal);
        var groups = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (removal, match) in selected)
        {
            if (!removal.RemoveCreatedEmptyGroup || request is null || groups.Contains(match.GroupPointer)) continue;
            var hooks = match.Group.GetProperty(ToolHookEntries.Hooks);
            if (!Enumerable.Range(0, hooks.GetArrayLength()).All(index => pointers.Contains(match.GroupPointer + "/" + ToolHookEntries.Hooks + "/" + index.ToString(CultureInfo.InvariantCulture)))) continue;
            if (ToolHookEntries.CreatedGroup(request, file: removal.File, eventPointer: removal.Pointer, selected: match, current: document) is null) continue;
            groups.Add(match.GroupPointer);
            resolved.RemoveAll(entry => entry.Pointer.StartsWith(match.GroupPointer + "/", StringComparison.Ordinal));
            resolved.Add(removal with { Pointer = match.GroupPointer, Expected = match.Group.Clone(), MatchCommand = false });
        }
        return [.. resolved];
    }

}
