using OpenForge.Cli.Core.Commands.Shared.Models;
using OpenForge.Cli.Core.Commands.Context.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal enum ContextProjectionPart
{
    Frontmatter,
    Headings,
    Body,
    Section,
}

internal enum ContextProjectionState
{
    Available,
    Missing,
    Unavailable,
    Ambiguous,
}

internal enum ContextHeadingForm
{
    Atx,
    Setext,
}

internal sealed record ContextProjectedHeading
{
    internal ContextProjectedHeading(
        string text,
        int level,
        ContextHeadingForm form,
        SourceLocation location,
        bool canonical)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(location);
        if (level is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, "A Context heading level must be between one and six.");
        }

        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The Context heading form is not defined.");
        }

        Text = text;
        Level = level;
        Form = form;
        Location = location;
        Canonical = canonical;
    }

    internal string Text { get; }

    internal int Level { get; }

    internal ContextHeadingForm Form { get; }

    internal SourceLocation Location { get; }

    internal CommandSourceLocation LocationView => CommandSourceLocation.From(Location);

    internal bool Canonical { get; }
}

internal sealed record ContextProjection
{
    internal ContextProjection(
        ContextProjectionPart part,
        string? name,
        ContextProjectionState state,
        string? text,
        IEnumerable<ContextProjectedHeading> headings,
        SourceLocation? location)
    {
        if (!Enum.IsDefined(part) || !Enum.IsDefined(state))
        {
            throw new ArgumentException("The Context projection part and state must be defined.");
        }

        ArgumentNullException.ThrowIfNull(headings);
        Part = part;
        Name = name;
        State = state;
        Text = text;
        Headings = ContextResultCollections.Snapshot(headings, nameof(headings));
        Location = location;
    }

    internal ContextProjectionPart Part { get; }

    internal string? Name { get; }

    internal ContextProjectionState State { get; }

    internal string? Text { get; }

    internal IReadOnlyList<ContextProjectedHeading> Headings { get; }

    internal SourceLocation? Location { get; }
}
