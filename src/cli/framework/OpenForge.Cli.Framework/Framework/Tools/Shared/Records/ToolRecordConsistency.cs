using System.Text.Json;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Records;

internal static class ToolRecordConsistency
{
    internal static void Validate(WorkspaceSettingsDocument settings, WorkspaceOwnershipDocument ownership)
    {
        if (settings.Tools.Count != ownership.Tools.Count) throw new JsonException("Tool settings and detailed registrations disagree.");
        foreach (var (id, setting) in settings.Tools)
        {
            if (!ownership.Tools.TryGetValue(id, out var receipt) || receipt.Version != setting.Version
                || receipt.CodeRoot != (setting.CodeRoot ?? ".")
                || !(setting.Hosts ?? [.. ToolVocabulary.Hosts]).Order(StringComparer.Ordinal).SequenceEqual(receipt.Hosts.Order(StringComparer.Ordinal)))
                throw new JsonException("A tool pin, code root or host set differs between settings and lock.");
            var definition = new ToolCatalogueReader().Definition(receipt.ManifestBytes, "receipt." + receipt.ManifestFormat, false).Manifest;
            if (setting.Roles.Contains(ToolVocabulary.CodeNavigationRole, StringComparer.Ordinal)
                && !definition.Capabilities.Contains(ToolVocabulary.CodeGraphCapability, StringComparer.Ordinal))
                throw new JsonException("The tool doesn't provide its recorded role capability.");
        }
        if (settings.ToolRuns.Count != ownership.ToolRuns.Count) throw new JsonException("Tool execution summaries and detailed records disagree.");
        foreach (var (id, summary) in settings.ToolRuns)
        {
            if (!ownership.ToolRuns.TryGetValue(id, out var record) || summary.Action != record.Action || summary.State != record.State
                || !summary.Commands.SequenceEqual(record.ToSummary().Commands))
                throw new JsonException("A tool execution summary differs from its detailed record.");
        }
    }
}
