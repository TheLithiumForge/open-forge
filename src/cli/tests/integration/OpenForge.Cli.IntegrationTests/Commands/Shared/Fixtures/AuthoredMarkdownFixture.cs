using OpenForge.Cli.Core.Framework.Documents.Markdown;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Fixtures;

internal static class AuthoredMarkdownFixture
{
    internal static string InsertParagraph(string source, string paragraph)
    {
        var document = new MarkdownDocumentParser().Parse(source);
        if (document.BodySpan is not { } body
            || document.GeneratedRegion.RegionSpan is not { } entries
            || body.Start >= entries.Start)
        {
            throw new InvalidOperationException("The authored-change fixture requires a body before Entries.");
        }

        return $"{source[..body.Start]}\n{paragraph}\n\n{source[body.Start..]}";
    }
}
