namespace OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

internal sealed record YamlTextSpan
{
    internal YamlTextSpan(int start, int length)
    {
        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start), start, "A YAML span start cannot be negative.");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "A YAML span length cannot be negative.");
        }

        _ = checked(start + length);
        Start = start;
        Length = length;
    }

    internal int Start { get; }

    internal int Length { get; }

    internal int End => checked(Start + Length);
}
