using System.Text;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata;

internal static class FrameworkDocumentMetadataTagGrammar
{
    internal static bool IsValid(string? tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return false;
        }

        var runes = tag.EnumerateRunes().ToArray();
        if (runes.Length == 0 || !Rune.IsLetter(runes[0]))
        {
            return false;
        }

        var previousWasHyphen = false;
        for (var index = 1; index < runes.Length; index++)
        {
            var rune = runes[index];
            if (rune.Value == '-')
            {
                if (previousWasHyphen || index == runes.Length - 1)
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

        return true;
    }
}
