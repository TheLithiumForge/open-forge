using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Selection;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Selection;

internal sealed class ToolWriteSelector(ToolCatalogue catalogue)
{
    internal ToolWriteSelectionResult Select(ToolWriteSelectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Kind is not (ToolWriteSelectionKind.Install or ToolWriteSelectionKind.Update))
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Kind, "The writer selection kind is not defined.");
        }
        if (request.Spec is null && request.ChannelOverride is not null)
        {
            return new([], [new(ToolWriteSelectionIssueCode.NamedSpecRequired, null)]);
        }

        IReadOnlyList<string> ids = request.Spec is { } spec
            ? [spec.Id]
            : [.. request.Records.Settings.Keys.Order(StringComparer.Ordinal)];
        List<ToolWriteSelection> tools = [];
        List<ToolWriteSelectionIssue> issues = [];
        foreach (var id in ids)
        {
            if (!catalogue.Entries.TryGetValue(id, out var entry)
                || !string.Equals(id, entry.Identity.Id, StringComparison.Ordinal))
            {
                issues.Add(new(ToolWriteSelectionIssueCode.UnknownId, id));
                continue;
            }
            if (request.ChannelOverride is { } channel
                && !entry.Channels.Any(candidate => string.Equals(candidate.Id, channel, StringComparison.Ordinal)))
            {
                issues.Add(new(ToolWriteSelectionIssueCode.UnknownChannel, id));
                continue;
            }

            request.Records.Settings.TryGetValue(id, out var declaration);
            request.Records.Lock.TryGetValue(id, out var locked);
            if (request.Kind == ToolWriteSelectionKind.Update && declaration is null && locked is null)
            {
                issues.Add(new(ToolWriteSelectionIssueCode.UpdateRequiresRecord, id));
                continue;
            }

            var version = request.Spec?.Version ?? new ToolVersionSelection { Kind = ToolVersionSelectionKind.LatestSupported };
            var requiredChannel = request.ChannelOverride;
            if (request.Spec is null && request.Kind == ToolWriteSelectionKind.Install)
            {
                if (locked is not null)
                {
                    version = new() { Kind = ToolVersionSelectionKind.Exact, ExactVersion = locked.Version };
                }
                else if (declaration is not null)
                {
                    version = new() { Kind = ToolVersionSelectionKind.MinorRange, Range = declaration.Version };
                }
            }

            tools.Add(new()
            {
                Entry = entry,
                Version = version,
                RequiredChannel = requiredChannel,
                Declaration = declaration,
                Lock = locked,
            });
        }

        return issues.Count == 0 ? new(tools, []) : new([], issues);
    }
}
