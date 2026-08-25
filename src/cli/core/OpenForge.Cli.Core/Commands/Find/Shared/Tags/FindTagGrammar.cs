using System.Text;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Tags;

internal static class FindTagGrammar
{
    internal static bool IsValid(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var isFirst = true;
        var previousWasHyphen = false;
        foreach (var rune in value.EnumerateRunes())
        {
            if (isFirst)
            {
                if (!Rune.IsLetter(rune))
                {
                    return false;
                }

                isFirst = false;
                continue;
            }

            if (rune.Value == '-')
            {
                if (previousWasHyphen)
                {
                    return false;
                }

                previousWasHyphen = true;
                continue;
            }

            if (!Rune.IsLetterOrDigit(rune))
            {
                return false;
            }

            previousWasHyphen = false;
        }

        return !previousWasHyphen;
    }
}
