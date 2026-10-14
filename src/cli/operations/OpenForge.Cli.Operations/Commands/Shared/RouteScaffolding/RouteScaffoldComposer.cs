using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Composition;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;

/// <summary>
/// Neutral entrypoint composition from the Forgiving Maintenance technical design.
/// Callers supply title and metadata facts and retain their own authoring policy.
/// </summary>
internal sealed class RouteScaffoldComposer
{
    private readonly FrameworkMarkdownDocumentWriter _writer = new();

    internal ImmutableArray<byte> Compose(RouteScaffoldDocument document)
    {
        var definition = document.IncludeDescription && document.Metadata.Description is { } description
            ? $"\n{description}.\n"
            : string.Empty;
        var body = $"""

            # {document.Title}
            {definition}
            ## Axioms

            - inherited - No local axioms; loaded ancestor axioms remain active.

            ## Entries

            - none - No entries - #Empty
            """.TrimEnd('\n');
        return _writer.WriteOptional(document.Metadata, body, document.Form);
    }

    internal static RouteScaffoldDocument FolderDocument(
        string folderId,
        string sentenceCaseTitle,
        IEnumerable<string> categoryTags,
        FrontmatterForm form)
        => new()
        {
            Title = sentenceCaseTitle,
            Metadata = new FrameworkDocumentMetadataEmission($"Notes in {folderId}", categoryTags, responsibility: null),
            Form = form,
        };
}
