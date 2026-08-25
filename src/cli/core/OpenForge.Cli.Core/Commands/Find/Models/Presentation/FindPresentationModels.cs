using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Models.Presentation;

internal enum FindContentPartKind
{
    Metadata,
    Frontmatter,
    Headings,
    Body,
    Section,
}

internal sealed record FindContentPart
{
    internal FindContentPart(FindContentPartKind kind, string? name, string canonicalValue)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find content part kind is not defined.");
        }

        ArgumentNullException.ThrowIfNull(canonicalValue);
        if (kind == FindContentPartKind.Section)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (!string.Equals(
                    canonicalValue,
                    $"{FindDefinitions.SectionPrefix}{name}",
                    StringComparison.Ordinal))
            {
                throw new ArgumentException("A section content part must use its canonical section: value.", nameof(canonicalValue));
            }
        }
        else if (name is not null)
        {
            throw new ArgumentException("Only a section content part can carry a name.", nameof(name));
        }

        var expected = kind switch
        {
            FindContentPartKind.Metadata => FindDefinitions.Metadata,
            FindContentPartKind.Frontmatter => FindDefinitions.Frontmatter,
            FindContentPartKind.Headings => FindDefinitions.Headings,
            FindContentPartKind.Body => FindDefinitions.Body,
            FindContentPartKind.Section => canonicalValue,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find content part kind is not defined."),
        };
        if (!string.Equals(expected, canonicalValue, StringComparison.Ordinal))
        {
            throw new ArgumentException("The Find content part canonical value does not match its kind.", nameof(canonicalValue));
        }

        Kind = kind;
        Name = name;
        CanonicalValue = canonicalValue;
    }

    internal FindContentPartKind Kind { get; }

    internal string? Name { get; }

    internal string CanonicalValue { get; }
}

internal sealed record FindContentSelection
{
    internal FindContentSelection(
        IEnumerable<FindContentPart> supplied,
        IEnumerable<FindContentPart> effective,
        bool isRequested = false)
    {
        Supplied = Snapshot(supplied, nameof(supplied), requireUnique: false);
        Effective = Snapshot(effective, nameof(effective), requireUnique: true);
        ValidateEffectiveOrder(Effective);
        ValidateEffectiveValues(Supplied, Effective);
        IsRequested = isRequested || Supplied.Count != 0;
    }

    internal IReadOnlyList<FindContentPart> Supplied { get; }

    internal IReadOnlyList<FindContentPart> Effective { get; }

    internal bool IsRequested { get; }

    private static IReadOnlyList<FindContentPart> Snapshot(
        IEnumerable<FindContentPart> values,
        string parameterName,
        bool requireUnique)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values.ToArray();
        if (materialized.Any(value => value is null))
        {
            throw new ArgumentException("Find content selections cannot contain null values.", parameterName);
        }

        if (requireUnique
            && materialized.Select(value => value.CanonicalValue)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != materialized.Length)
        {
            throw new ArgumentException("Find content selections cannot contain equivalent duplicate parts.", parameterName);
        }

        return Array.AsReadOnly(materialized);
    }

    private static void ValidateEffectiveValues(
        IReadOnlyList<FindContentPart> supplied,
        IReadOnlyList<FindContentPart> effective)
    {
        var firstSupplied = supplied
            .DistinctBy(value => value.CanonicalValue, StringComparer.OrdinalIgnoreCase)
            .OrderBy(ReadPartRank)
            .ToArray();
        if (effective.Count != firstSupplied.Length
            || effective.Where((value, index) =>
                    value.Kind != firstSupplied[index].Kind
                    || !string.Equals(value.Name, firstSupplied[index].Name, StringComparison.Ordinal)
                    || !string.Equals(value.CanonicalValue, firstSupplied[index].CanonicalValue, StringComparison.Ordinal))
                .Any())
        {
            throw new ArgumentException("Effective Find content must contain every unique supplied part.", nameof(effective));
        }
    }

    private static void ValidateEffectiveOrder(IReadOnlyList<FindContentPart> values)
    {
        var lastRank = -1;
        foreach (var value in values)
        {
            var rank = ReadPartRank(value);
            if (rank < lastRank)
            {
                throw new ArgumentException("Effective Find content parts must use canonical part order.", nameof(values));
            }

            lastRank = rank;
        }
    }

    private static int ReadPartRank(FindContentPart value)
        => value.Kind switch
        {
            FindContentPartKind.Metadata => 0,
            FindContentPartKind.Frontmatter => 1,
            FindContentPartKind.Headings => 2,
            FindContentPartKind.Body => 3,
            FindContentPartKind.Section => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value.Kind, "The Find content part kind is not defined."),
        };
}

internal sealed record FindPresentationSelection
{
    internal FindPresentationSelection(
        CliView? suppliedView,
        CliView effectiveView,
        FindContentSelection content)
    {
        if (suppliedView is { } requestedView && !Enum.IsDefined(requestedView))
        {
            throw new ArgumentOutOfRangeException(nameof(suppliedView), suppliedView, "The Find supplied view is not defined.");
        }

        if (!Enum.IsDefined(effectiveView))
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveView), effectiveView, "The Find effective view is not defined.");
        }

        ArgumentNullException.ThrowIfNull(content);
        SuppliedView = suppliedView;
        EffectiveView = effectiveView;
        Content = content;
    }

    internal CliView? SuppliedView { get; }

    internal CliView EffectiveView { get; }

    internal FindContentSelection Content { get; }
}
