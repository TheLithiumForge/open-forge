using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Framework.Sources.Metadata;

internal sealed class SourceOpenForgeMetadataParser
{
    private readonly FrameworkDocumentMetadataParser _parser = new();

    internal SourceOpenForgeMetadataFacts Parse(MarkdownDocumentFacts document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return Project(_parser.Parse(document));
    }

    internal static SourceOpenForgeMetadataFacts Project(FrameworkDocumentMetadataFacts facts)
    {
        return facts.State switch
        {
            FrameworkDocumentMetadataState.Complete => Complete(facts),
            FrameworkDocumentMetadataState.Missing => SourceOpenForgeMetadataFacts.WithoutValues(
                SourceOpenForgeMetadataState.Missing),
            FrameworkDocumentMetadataState.Malformed => SourceOpenForgeMetadataFacts.WithoutValues(
                SourceOpenForgeMetadataState.Malformed),
            _ => throw new ArgumentOutOfRangeException(
                nameof(facts),
                facts.State,
                "The Framework document metadata state is not defined."),
        };
    }

    internal static bool IsValidTag(string? tag)
        => FrameworkDocumentMetadataTagGrammar.IsValid(tag);

    private static SourceOpenForgeMetadataFacts Complete(
        FrameworkDocumentMetadataFacts facts)
    {
        var metadata = facts.Metadata
            ?? throw new InvalidOperationException(
                "Complete Framework document metadata facts require authored values.");
        return SourceOpenForgeMetadataFacts.Complete(
            metadata.Description,
            metadata.Tags);
    }
}
