using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
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
        if (string.IsNullOrWhiteSpace(description) || tags.IsEmpty)
        {
            intended = null;
            cause = "The intended document requires complete description and tag metadata.";
            return false;
        }

        try
        {
            intended = new FrameworkDocumentMetadata(description, tags, responsibility);
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

        plan = draft.Build();
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
            YamlStart = member.Entry.Value.Span.Start,
            YamlLength = member.Entry.Value.Span.Length,
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

        if (responsibility.Line is not { } line)
        {
            return false;
        }

        draft.Edits.Add(new RouteUpdateMetadataEdit
        {
            YamlStart = line.Start,
            YamlLength = line.End - line.Start,
            Replacement = string.Empty,
        });
        draft.Preview.Add(new RouteUpdatePreviewHunk
        {
            Kind = RouteUpdatePreviewKind.MetadataField,
            Before = $"responsibility: {line.RawValue}",
            Expected = string.Empty,
        });
        return true;
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
