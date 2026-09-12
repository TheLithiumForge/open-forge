namespace OpenForge.Cli.Core.Shell.Parsing.Models.Input;

internal sealed class CliOptionResultFacts
{
    internal CliOptionResultFacts(
        bool isExplicit,
        int identifierCount,
        int valueCount)
    {
        if (identifierCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(identifierCount),
                identifierCount,
                "Identifier count cannot be negative.");
        }

        if (valueCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(valueCount),
                valueCount,
                "Value count cannot be negative.");
        }

        if (!isExplicit && (identifierCount != 0 || valueCount != 0))
        {
            throw new ArgumentException(
                "An implicit option result cannot contain identifier or value tokens.");
        }

        if (isExplicit && identifierCount == 0)
        {
            throw new ArgumentException(
                "An explicit option result must contain at least one identifier token.",
                nameof(identifierCount));
        }

        IsExplicit = isExplicit;
        IdentifierCount = identifierCount;
        ValueCount = valueCount;
    }

    internal bool IsExplicit { get; }

    internal bool IsExplicitWithoutValue => IsExplicit && ValueCount == 0;

    internal int IdentifierCount { get; }

    internal int ValueCount { get; }
}
