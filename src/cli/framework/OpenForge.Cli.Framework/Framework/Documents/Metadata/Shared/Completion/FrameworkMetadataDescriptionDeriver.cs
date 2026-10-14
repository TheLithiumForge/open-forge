using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;

internal static class FrameworkMetadataDescriptionDeriver
{
    internal static string Derive(string canonicalPath, MarkdownDocumentFacts document)
    {
        var heading = document.Headings.FirstOrDefault(heading => heading.IsTopLevel && !string.IsNullOrWhiteSpace(heading.VisibleText));
        if (heading is { IsVisibleTextSupported: true })
        {
            return CollapseWhitespace(heading.VisibleText);
        }

        return SentenceCase(Path.GetFileNameWithoutExtension(canonicalPath));
    }

    internal static string SentenceCase(string stem)
    {
        var words = new StringBuilder(stem.Length);
        for (var index = 0; index < stem.Length; index++)
        {
            var character = stem[index];
            if (character is '-' or '_' || char.IsWhiteSpace(character))
            {
                words.Append(' ');
                continue;
            }

            if (index > 0 && char.IsUpper(character)
                && (char.IsLower(stem[index - 1]) || char.IsDigit(stem[index - 1])
                    || char.IsUpper(stem[index - 1]) && index + 1 < stem.Length && char.IsLower(stem[index + 1])))
            {
                words.Append(' ');
            }

            words.Append(char.ToLowerInvariant(character));
        }

        var text = CollapseWhitespace(words.ToString());
        if (!text.Any(char.IsLetterOrDigit)) return "Note";
        var firstLetter = text.TakeWhile(character => !char.IsLetter(character)).Count();
        if (firstLetter == text.Length) return text;
        return text[..firstLetter] + char.ToUpperInvariant(text[firstLetter]) + text[(firstLetter + 1)..];
    }

    private static string CollapseWhitespace(string text)
        => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
