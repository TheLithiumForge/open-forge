namespace OpenForge.Cli.Core.Framework.Sources.Models.Selection;

internal enum SourceUniverseSelectorRole
{
    Include,
    Exclude,
}

internal sealed record SourceUniverseSelectorOccurrence
{
    internal SourceUniverseSelectorOccurrence(
        SourceUniverseSelectorRole role,
        string value,
        int position)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "The source-universe selector role is not defined.");
        }

        ArgumentNullException.ThrowIfNull(value);
        if (position < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "A selector position must be positive.");
        }

        Role = role;
        Value = value;
        Position = position;
    }

    internal SourceUniverseSelectorRole Role { get; }

    internal string Value { get; }

    internal int Position { get; }
}
