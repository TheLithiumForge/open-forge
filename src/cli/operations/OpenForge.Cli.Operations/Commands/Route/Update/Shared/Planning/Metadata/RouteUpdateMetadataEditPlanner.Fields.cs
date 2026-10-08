using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

internal sealed partial class RouteUpdateMetadataEditPlanner
{
    private static bool TryPlanResponsibility(
        RouteUpdateMetadataEditPlanningInput input,
        RouteUpdateCanonicalMetadataValues canonical,
        RouteUpdateMetadataEditDraft draft)
    {
        var request = input.Request.Responsibility;
        return request.Operation switch
        {
            RouteUpdateResponsibilityOperation.NotRequested => true,
            RouteUpdateResponsibilityOperation.Set when string.Equals(
                input.Layout.Responsibility,
                request.Value,
                StringComparison.Ordinal) => true,
            RouteUpdateResponsibilityOperation.Set => TrySetResponsibility(
                input.Layout,
                canonical.Responsibility
                    ?? throw new InvalidOperationException(
                        "A responsibility set requires one canonical value."),
                draft),
            RouteUpdateResponsibilityOperation.Remove => TryRemoveResponsibility(
                input.Layout,
                draft),
            _ => throw new ArgumentOutOfRangeException(
                nameof(input),
                request.Operation,
                "The Route Update responsibility operation is not defined."),
        };
    }

    private static bool TryPlanApplyTo(
        RouteUpdateMetadataEditPlanningInput input,
        RouteUpdateCanonicalMetadataValues canonical,
        RouteUpdateMetadataEditDraft draft)
    {
        var request = input.Request.ApplyTo;
        return request.Operation switch
        {
            RouteUpdateApplyToOperation.NotRequested => true,
            RouteUpdateApplyToOperation.Set when MatchesPatterns(
                input.Layout.ApplyToPatterns,
                request.Values) => true,
            RouteUpdateApplyToOperation.Set => TrySetApplyTo(
                input.Layout,
                canonical.ApplyTo
                    ?? throw new InvalidOperationException(
                        "An applyTo set requires one canonical list."),
                draft),
            RouteUpdateApplyToOperation.Clear => TryRemoveApplyTo(input.Layout, draft),
            _ => throw new ArgumentOutOfRangeException(
                nameof(input),
                request.Operation,
                "The Route Update applyTo operation is not defined."),
        };
    }

    private static bool MatchesPatterns(
        ImmutableArray<ApplyToPattern> current,
        ImmutableArray<string> requested)
        => current.Length == requested.Length
            && current.All(pattern => requested.Contains(pattern.Text, StringComparer.Ordinal));

    private static bool TrySetApplyTo(
        RouteUpdateMetadataLayout layout,
        string replacement,
        RouteUpdateMetadataEditDraft draft)
    {
        if (layout.ApplyToMembers.IsEmpty)
        {
            var anchor = layout.ResponsibilityMember
                ?? layout.TagsMember
                ?? layout.DescriptionMember;
            return TryInsertAfter(draft, anchor, "applyTo", replacement);
        }

        foreach (var member in layout.ApplyToMembers)
        {
            if (!TryReplace(member, replacement, draft))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryRemoveApplyTo(
        RouteUpdateMetadataLayout layout,
        RouteUpdateMetadataEditDraft draft)
    {
        foreach (var member in layout.ApplyToMembers)
        {
            if (!TryRemoveMember(member, draft))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryReplace(
        RouteUpdateMetadataMember member,
        string replacement,
        RouteUpdateMetadataEditDraft draft)
    {
        if (member.Line is not { } line)
        {
            return false;
        }

        if (string.Equals(line.RawValue, replacement, StringComparison.Ordinal))
        {
            return true;
        }

        draft.Edits.Add(new RouteUpdateMetadataEdit
        {
            DocumentStart = draft.Layout.DocumentOffset + line.ValueStart,
            DocumentLength = line.ValueEnd - line.ValueStart,
            Replacement = replacement,
        });
        draft.Preview.Add(new RouteUpdatePreviewHunk
        {
            Kind = RouteUpdatePreviewKind.MetadataField,
            Before = $"{member.Name}: {line.RawValue}",
            Expected = $"{member.Name}: {replacement}",
        });
        return true;
    }

    private static bool TryRemoveMember(
        RouteUpdateMetadataMember member,
        RouteUpdateMetadataEditDraft draft)
    {
        if (member.Line is not { } line)
        {
            return false;
        }

        if (line.IsMultilineValue && line.HasTrailingComment)
        {
            return false;
        }

        var start = line.IsMultilineValue
            ? line.Start
            : line.HasTrailingComment
                ? line.Start + line.Indentation.Length
                : line.Start;
        var length = line.IsMultilineValue
            ? line.End - start
            : line.HasTrailingComment
                ? line.ContentEnd - start
                : line.End - line.Start;
        var replacement = line.HasTrailingComment && !line.IsMultilineValue
            ? draft.Layout.Source[member.Entry.Value.Span.End..line.ContentEnd]
                .TrimStart()
            : string.Empty;
        draft.Edits.Add(new RouteUpdateMetadataEdit
        {
            DocumentStart = draft.Layout.DocumentOffset + start,
            DocumentLength = length,
            Replacement = replacement,
        });
        draft.Preview.Add(new RouteUpdatePreviewHunk
        {
            Kind = RouteUpdatePreviewKind.MetadataField,
            Before = $"{member.Name}: {line.RawValue}",
            Expected = string.Empty,
        });
        return true;
    }

    private static bool TrySetResponsibility(
        RouteUpdateMetadataLayout layout,
        string replacement,
        RouteUpdateMetadataEditDraft draft)
    {
        if (layout.ResponsibilityMember is { } responsibility)
        {
            return TryReplace(responsibility, replacement, draft);
        }

        if (layout.TagsMember is { } tags)
        {
            return TryInsertBefore(
                draft,
                tags,
                "responsibility",
                replacement);
        }

        return TryInsertAfter(
            draft,
            layout.DescriptionMember,
            "responsibility",
            replacement);
    }

    private static bool TryRemoveResponsibility(
        RouteUpdateMetadataLayout layout,
        RouteUpdateMetadataEditDraft draft)
    {
        if (layout.ResponsibilityMember is not { } responsibility)
        {
            return true;
        }

        return TryRemoveMember(responsibility, draft);
    }

    private static bool TryInsertBefore(
        RouteUpdateMetadataEditDraft draft,
        RouteUpdateMetadataMember? anchor,
        string name,
        string value)
        => TryInsert(draft, anchor, name, value, insertAfter: false);

    private static bool TryInsertAfter(
        RouteUpdateMetadataEditDraft draft,
        RouteUpdateMetadataMember? anchor,
        string name,
        string value)
        => TryInsert(draft, anchor, name, value, insertAfter: true);

    private static bool TryInsert(
        RouteUpdateMetadataEditDraft draft,
        RouteUpdateMetadataMember? anchor,
        string name,
        string value,
        bool insertAfter)
    {
        if (anchor?.Line is not { } line)
        {
            return false;
        }

        var lineEnding = ReadLineEnding(draft.Layout, line);
        draft.Edits.Add(new RouteUpdateMetadataEdit
        {
            DocumentStart = draft.Layout.DocumentOffset + (insertAfter ? line.End : line.Start),
            DocumentLength = 0,
            InsertionOrder = name switch
            {
                "description" => 0,
                "responsibility" => 1,
                "tags" => 2,
                "applyTo" => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(name), name, "The inserted metadata field is not defined."),
            },
            Replacement = $"{line.Indentation}{name}: {value}{lineEnding}",
        });
        draft.Preview.Add(new RouteUpdatePreviewHunk
        {
            Kind = RouteUpdatePreviewKind.MetadataField,
            Before = string.Empty,
            Expected = $"{name}: {value}",
        });
        return true;
    }

    private static string ReadLineEnding(
        RouteUpdateMetadataLayout layout,
        RouteUpdateMetadataMemberLine line)
    {
        var value = layout.Source[line.ContentEnd..line.End];
        return value.Length == 0 ? "\n" : value;
    }
}
