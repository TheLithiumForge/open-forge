using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Binding;

internal sealed class ContextRequestParser
{
    internal ContextContentSelection ParseContent(ContextBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.ContentFacts.IsExplicit)
        {
            return new ContextContentSelection(
                supplied: [],
                effective:
                [
                    CreateFixed(ContextContentPartKind.Frontmatter, ContextDefinitions.Frontmatter),
                    CreateFixed(ContextContentPartKind.Body, ContextDefinitions.Body),
                ]);
        }

        if (input.ContentFacts.IdentifierCount != 1 || input.ContentFacts.ValueCount != 1)
        {
            throw new ContextBindingException(
                error: ContextBindingError.Content,
                subject: input.ContentValues.LastOrDefault(),
                message: "Content must occur exactly once with one comma-separated value.");
        }

        var spelling = input.ContentValues.SingleOrDefault()
            ?? throw new ContextBindingException(
                error: ContextBindingError.Content,
                subject: null,
                message: "Content must contain one comma-separated value.");
        var supplied = SplitContent(spelling).Select(ParsePart).ToArray();
        var effective = supplied
            .DistinctBy(part => part.CanonicalValue, StringComparer.Ordinal)
            .OrderBy(ReadRank)
            .ThenBy(part => part.Kind == ContextContentPartKind.Section
                ? Array.FindIndex(supplied, value => string.Equals(value.CanonicalValue, part.CanonicalValue, StringComparison.Ordinal))
                : 0)
            .ToArray();
        return new ContextContentSelection(supplied: supplied, effective: effective);
    }

    internal ContextLinkExpansion ParseLinkExpansion(ContextBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.FollowLinksFacts.IsExplicit)
        {
            return ContextLinkExpansion.None;
        }

        if (input.FollowLinksFacts.IdentifierCount != 1 || input.FollowLinksFacts.ValueCount != 1)
        {
            throw new ContextBindingException(
                error: ContextBindingError.LinkDepth,
                subject: input.FollowLinksValues.LastOrDefault(),
                message: "Follow-links must occur exactly once with one positive depth or all.");
        }

        var spelling = input.FollowLinksValues.SingleOrDefault();
        if (string.Equals(spelling, ContextDefinitions.All, StringComparison.Ordinal))
        {
            return ContextLinkExpansion.All;
        }

        if (spelling is null
            || !int.TryParse(spelling, NumberStyles.None, CultureInfo.InvariantCulture, out var depth)
            || depth <= 0)
        {
            throw new ContextBindingException(
                error: ContextBindingError.LinkDepth,
                subject: spelling,
                message: "Follow-links must be a positive base-10 integer or all; zero is invalid.");
        }

        return ContextLinkExpansion.Bounded(depth);
    }

    private static IReadOnlyList<string> SplitContent(string value)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        var escaped = false;
        foreach (var character in value)
        {
            if (escaped)
            {
                if (character is not ('\\' or ','))
                {
                    throw new ContextBindingException(
                        error: ContextBindingError.Content,
                        subject: value,
                        message: "Content escapes may precede only a comma or backslash.");
                }

                current.Append(character);
                escaped = false;
                continue;
            }

            if (character == '\\')
            {
                escaped = true;
            }
            else if (character == ',')
            {
                AddPart(parts, current, value);
            }
            else
            {
                current.Append(character);
            }
        }

        if (escaped)
        {
            throw new ContextBindingException(
                error: ContextBindingError.Content,
                subject: value,
                message: "Content cannot end with an incomplete escape.");
        }

        AddPart(parts, current, value);
        return parts;
    }

    private static void AddPart(List<string> parts, StringBuilder current, string supplied)
    {
        var value = current.ToString().Trim();
        current.Clear();
        if (value.Length == 0)
        {
            throw new ContextBindingException(
                error: ContextBindingError.Content,
                subject: supplied,
                message: "Content cannot contain an empty part.");
        }

        parts.Add(value);
    }

    private static ContextContentPart ParsePart(string value)
        => value switch
        {
            ContextDefinitions.Metadata => CreateFixed(ContextContentPartKind.Metadata, value),
            ContextDefinitions.Paths => CreateFixed(ContextContentPartKind.Paths, value),
            ContextDefinitions.Frontmatter => CreateFixed(ContextContentPartKind.Frontmatter, value),
            ContextDefinitions.Headings => CreateFixed(ContextContentPartKind.Headings, value),
            ContextDefinitions.Body => CreateFixed(ContextContentPartKind.Body, value),
            _ when value.StartsWith(ContextDefinitions.SectionPrefix, StringComparison.Ordinal)
                && value.Length > ContextDefinitions.SectionPrefix.Length => new ContextContentPart(
                    kind: ContextContentPartKind.Section,
                    name: value[ContextDefinitions.SectionPrefix.Length..],
                    canonicalValue: value),
            _ => throw new ContextBindingException(
                error: ContextBindingError.Content,
                subject: value,
                message: $"Content part '{value}' is not metadata, paths, frontmatter, headings, body, or section:<name>."),
        };

    private static ContextContentPart CreateFixed(ContextContentPartKind kind, string value)
        => new(kind: kind, name: null, canonicalValue: value);

    private static int ReadRank(ContextContentPart part)
        => part.Kind switch
        {
            ContextContentPartKind.Metadata => 0,
            ContextContentPartKind.Paths => 1,
            ContextContentPartKind.Frontmatter => 2,
            ContextContentPartKind.Headings => 3,
            ContextContentPartKind.Body => 4,
            ContextContentPartKind.Section => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(part), part.Kind, "The Context content part kind is not defined."),
        };
}

internal sealed class ContextBindingException(
    ContextBindingError error,
    string? subject,
    string message) : ArgumentException(message)
{
    internal ContextBindingError Error { get; } = error;

    internal string? Subject { get; } = subject;
}
