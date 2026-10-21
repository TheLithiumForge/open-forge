using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;

// A SKILL.md carries native name and description metadata at its frontmatter root. Workspace adoption already owns
// how that metadata is completed safely, so maintenance reuses it and only maps its plan to the shared completion result.
internal sealed class RoutedSkillMetadataCompleter
{
    private const string NameField = "name";
    private const string DescriptionField = "description";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private readonly WorkspaceAdoptionSkillMetadataPlanner _planner = new();
    private readonly MarkdownDocumentParser _parser = new();
    private readonly SourceAuthoredMetadataParser _metadata = new();

    internal FrameworkMetadataCompletionResult Complete(RoutedMetadataCompletionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        string source;
        try { source = StrictUtf8.GetString(input.Bytes.AsSpan()); }
        catch (DecoderFallbackException) { return Failed(input, FrameworkMetadataCompletionState.Unsafe, "The file is not strict UTF-8."); }

        var document = input.Document is { } retained && string.Equals(retained.Source, source, StringComparison.Ordinal)
            ? retained : _parser.Parse(source);
        if (document.Frontmatter.State == MarkdownFrontmatterState.Unavailable)
            return Failed(input, FrameworkMetadataCompletionState.Malformed, "The frontmatter block is not closed.");
        var authored = _metadata.Parse(document, SourceDocumentForm.Skill);
        var plan = _planner.Plan(input.File.CanonicalPath, source, document, authored, input.Bytes.AsMemory());
        if (plan.Cause is { } cause)
        {
            var state = authored.State == SourceAuthoredMetadataState.Malformed
                ? FrameworkMetadataCompletionState.Malformed : FrameworkMetadataCompletionState.Unsafe;
            return Failed(input, state, cause);
        }

        if (plan.Actions.Count == 0) return FrameworkMetadataCompletionResult.Unchanged(input.Bytes) with { Document = document };
        var bytes = ImmutableArray.Create(plan.IntendedBytes ?? throw new InvalidOperationException("A completed plan requires bytes."));
        var completed = _parser.Parse(StrictUtf8.GetString(bytes.AsSpan()));
        return new()
        {
            State = FrameworkMetadataCompletionState.Completed,
            Bytes = bytes,
            Fields = [.. plan.Fields.Select(Field)],
            Form = FrontmatterForm.Root,
            Description = plan.Fields.Contains(DescriptionField) ? _metadata.Parse(completed, SourceDocumentForm.Skill).Description : null,
            Document = completed,
        };
    }

    private static FrameworkMetadataField Field(string field) => field switch
    {
        NameField => FrameworkMetadataField.Name,
        DescriptionField => FrameworkMetadataField.Description,
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, "The native Skill field is not defined."),
    };

    private static FrameworkMetadataCompletionResult Failed(RoutedMetadataCompletionInput input, FrameworkMetadataCompletionState state, string cause)
        => new() { State = state, Bytes = input.Bytes, Fields = [], Cause = cause };
}
