using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;
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

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

internal sealed partial class RouteUpdateMetadataEditPlanner(
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

        if (input.Layout.Syntax.AuthoredForm == FrontmatterForm.Root
            && (input.Request.Description.Requested && intended.Description.IndexOfAny(['\r', '\n']) >= 0
                || input.Request.Responsibility.Operation == RouteUpdateResponsibilityOperation.Set
                    && intended.Responsibility?.IndexOfAny(['\r', '\n']) >= 0))
        {
            return RouteUpdateMetadataEditPlanBuild.Unsafe(UnsafeCause);
        }

        var form = input.Layout.Syntax.AuthoredForm ?? input.DefaultForm;
        var canonical = ReadCanonicalValues(intended, form);
        if (input.Layout.Syntax.AuthoredForm is null)
        {
            return BuildCreation(input, intended, canonical);
        }

        if (form == FrontmatterForm.Root)
        {
            canonical = PreserveRootScalarStyles(input, intended, canonical);
        }
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
        FrameworkDocumentMetadata metadata,
        FrontmatterForm form)
    {
        var source = _emitter.Emit(metadata, form);
        var parsed = _yamlParser.Parse(source);
        var mapping = parsed.Root;
        if (form == FrontmatterForm.Scoped)
        {
            mapping = parsed.Root?.Mapping?.Single(entry => string.Equals(
                entry.Key.Scalar?.Value, "open-forge", StringComparison.Ordinal)).Value;
        }
        var values = mapping?.Mapping?.ToDictionary(
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
            DocumentStart = draft.Layout.DocumentOffset + emptyMapping.Span.Start - 1,
            DocumentLength = emptyMapping.Span.Length + 1,
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

}
