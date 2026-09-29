using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdateMetadataEditPlanner(
    FrameworkDocumentMetadataEmitter emitter,
    YamlDocumentParser yamlParser)
{
    private const string UnsafeCause =
        "The requested metadata member cannot be changed without reserializing unrelated YAML.";

    private readonly FrameworkDocumentMetadataEmitter _emitter = emitter;
    private readonly YamlDocumentParser _yamlParser = yamlParser;

    internal RouteUpdateMetadataEditPlanBuild Build(RouteUpdateMetadataEditPlanningInput input)
    {
        if (!TryReadIntendedMetadata(input, out var intended, out var cause))
        {
            return RouteUpdateMetadataEditPlanBuild.Unsafe(cause);
        }

        var canonical = ReadCanonicalValues(intended);
        return TryBuildEdits(input, canonical, out var plan)
            ? RouteUpdateMetadataEditPlanBuild.Complete(plan)
            : RouteUpdateMetadataEditPlanBuild.Unsafe(UnsafeCause);
    }

    private static bool TryReadIntendedMetadata(
        RouteUpdateMetadataEditPlanningInput input,
        [NotNullWhen(true)] out FrameworkDocumentMetadata? intended,
        out string cause)
    {
        var request = input.Request;
        var layout = input.Layout;
        var description = request.Description.Requested
            ? request.Description.Value
            : layout.Description;
        var tags = request.Tags.Requested ? request.Tags.Values : layout.Tags;
        var responsibility = request.Responsibility.Operation switch
        {
            RouteUpdateResponsibilityOperation.NotRequested => layout.Responsibility,
            RouteUpdateResponsibilityOperation.Set => request.Responsibility.Value,
            RouteUpdateResponsibilityOperation.Remove => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(input),
                request.Responsibility.Operation,
                "The Route Update responsibility operation is not defined."),
        };
        if (!TryReadApplyToPatterns(
                request.ApplyTo,
                layout.ApplyToPatterns,
                out var applyTo,
                out cause))
        {
            intended = null;
            return false;
        }

        if (string.IsNullOrWhiteSpace(description) || tags.IsEmpty)
        {
            intended = null;
            cause = "The intended document requires complete description and tag metadata.";
            return false;
        }

        try
        {
            intended = new FrameworkDocumentMetadata(description, tags, responsibility, applyTo);
            cause = string.Empty;
            return true;
        }
        catch (ArgumentException exception)
        {
            intended = null;
            cause = exception.Message;
            return false;
        }
    }

    private static bool TryReadApplyToPatterns(
        RouteUpdateApplyToRequest request,
        ImmutableArray<ApplyToPattern> current,
        out ImmutableArray<ApplyToPattern> patterns,
        out string cause)
    {
        switch (request.Operation)
        {
            case RouteUpdateApplyToOperation.NotRequested:
                patterns = current;
                cause = string.Empty;
                return true;
            case RouteUpdateApplyToOperation.Clear:
                patterns = [];
                cause = string.Empty;
                return true;
            case RouteUpdateApplyToOperation.Set:
                var values = ImmutableArray.CreateBuilder<ApplyToPattern>();
                foreach (var value in request.Values)
                {
                    var parsed = ApplyToPatternMatcher.Parse(value);
                    if (parsed.Pattern is not { } pattern)
                    {
                        patterns = [];
                        cause = "The requested applyTo pattern is invalid.";
                        return false;
                    }

                    values.Add(pattern);
                }

                if (values.Count == 0)
                {
                    patterns = [];
                    cause = "The requested applyTo list cannot be empty.";
                    return false;
                }

                patterns = values.ToImmutable();
                cause = string.Empty;
                return true;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(request),
                    request.Operation,
                    "The Route Update applyTo operation is not defined.");
        }
    }

    private RouteUpdateCanonicalMetadataValues ReadCanonicalValues(
        FrameworkDocumentMetadata metadata)
    {
        var source = _emitter.Emit(metadata);
        var parsed = _yamlParser.Parse(source);
        var openForge = parsed.Root?.Mapping?
            .Single(entry => string.Equals(
                entry.Key.Scalar?.Value,
                "open-forge",
                StringComparison.Ordinal))
            .Value;
        var values = openForge?.Mapping?.ToDictionary(
            entry => entry.Key.Scalar?.Value
                ?? throw new InvalidOperationException(
                    "Canonical metadata requires scalar member keys."),
            entry => source[entry.Value.Span.Start..entry.Value.Span.End],
            StringComparer.Ordinal)
            ?? throw new InvalidOperationException(
                "Canonical metadata emission requires one Open Forge mapping.");
        return new RouteUpdateCanonicalMetadataValues
        {
            Description = values["description"],
            Tags = values["tags"],
            Responsibility = values.GetValueOrDefault("responsibility"),
            ApplyTo = values.GetValueOrDefault("applyTo"),
        };
    }

    private static bool TryBuildEdits(
        RouteUpdateMetadataEditPlanningInput input,
        RouteUpdateCanonicalMetadataValues canonical,
        [NotNullWhen(true)] out RouteUpdateMetadataEditPlan? plan)
    {
        var request = input.Request;
        var layout = input.Layout;
        var draft = new RouteUpdateMetadataEditDraft(layout);
        if (layout.EmptyMapping is { } emptyMapping)
        {
            if (!TryExpandEmptyMapping(input, emptyMapping, canonical, draft)
                || !layout.ApplyToMembers.IsEmpty
                    && !TryPlanApplyTo(input, canonical, draft))
            {
                plan = null;
                return false;
            }
        }
        else
        {
            if (request.Description.Requested
                && !string.Equals(layout.Description, request.Description.Value, StringComparison.Ordinal)
                && !(layout.DescriptionMember is { } description
                    ? TryReplace(description, canonical.Description, draft)
                    : TryInsertBefore(
                        draft,
                        layout.ResponsibilityMember ?? layout.TagsMember,
                        "description",
                        canonical.Description)))
            {
                plan = null;
                return false;
            }

            if (request.Tags.Requested
                && !layout.Tags.SequenceEqual(request.Tags.Values)
                && !(layout.TagsMember is { } tags
                    ? TryReplace(tags, canonical.Tags, draft)
                    : TryInsertAfter(
                        draft,
                        layout.ResponsibilityMember ?? layout.DescriptionMember,
                        "tags",
                        canonical.Tags)))
            {
                plan = null;
                return false;
            }

            if (!TryPlanResponsibility(input, canonical, draft))
            {
                plan = null;
                return false;
            }

            if (!TryPlanApplyTo(input, canonical, draft))
            {
                plan = null;
                return false;
            }
        }

        plan = draft.Build();
        return true;
    }

    private static bool TryExpandEmptyMapping(
        RouteUpdateMetadataEditPlanningInput input,
        RouteUpdateMetadataEmptyMapping emptyMapping,
        RouteUpdateCanonicalMetadataValues canonical,
        RouteUpdateMetadataEditDraft draft)
    {
        var lineEnding = ReadLineEnding(draft.Layout, emptyMapping.Line);
        var memberIndentation = $"{emptyMapping.Line.Indentation}  ";
        var members = new List<string>
        {
            $"{memberIndentation}description: {canonical.Description}",
            $"{memberIndentation}tags: {canonical.Tags}",
        };
        if (canonical.Responsibility is { } responsibility)
        {
            members.Add($"{memberIndentation}responsibility: {responsibility}");
        }

        var insertApplyTo = input.Layout.ApplyToMembers.IsEmpty
            && input.Request.ApplyTo.Operation == RouteUpdateApplyToOperation.Set;
        if (insertApplyTo && canonical.ApplyTo is { } applyTo)
        {
            members.Add($"{memberIndentation}applyTo: {applyTo}");
        }

        var replacement = $"{lineEnding}{string.Join(lineEnding, members)}";
        draft.Edits.Add(new RouteUpdateMetadataEdit
        {
            // The layout reader established the exact colon-space separator.
            YamlStart = emptyMapping.Span.Start - 1,
            YamlLength = emptyMapping.Span.Length + 1,
            Replacement = replacement,
        });
        draft.Preview.Add(new RouteUpdatePreviewHunk
        {
            Kind = RouteUpdatePreviewKind.MetadataField,
            Before = $"{emptyMapping.Line.Indentation}open-forge: {emptyMapping.Line.RawValue}",
            Expected = $"{emptyMapping.Line.Indentation}open-forge:{replacement}",
        });
        return true;
    }

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
            YamlStart = line.ValueStart,
            YamlLength = line.ValueEnd - line.ValueStart,
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
            YamlStart = start,
            YamlLength = length,
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
            YamlStart = insertAfter ? line.End : line.Start,
            YamlLength = 0,
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
