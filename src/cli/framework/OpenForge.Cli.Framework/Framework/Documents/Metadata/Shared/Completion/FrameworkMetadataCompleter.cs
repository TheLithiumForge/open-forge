using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;

// The Metadata Completion in Index section of the Forgiving Maintenance design owns this shared mechanism.
// This pure stage has no file, lease, recovery or application authority.
internal sealed class FrameworkMetadataCompleter
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly MarkdownDocumentParser _markdown = new();
    private readonly YamlDocumentParser _yaml = new();
    private readonly FrameworkDocumentMetadataParser _metadata = new();

    internal FrameworkMetadataCompletionResult Complete(FrameworkMetadataCompletionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.CanonicalPath);
        if (input.Bytes.IsDefault) throw new ArgumentException("Completion requires current bytes.", nameof(input));
        if (!Enum.IsDefined(input.Form)) throw new ArgumentOutOfRangeException(nameof(input), "The metadata form is not defined.");
        var defaults = new FrameworkDocumentMetadataEmission(null, input.DefaultTags, responsibility: null);
        if (defaults.Tags.IsEmpty) throw new ArgumentException("Completion requires category tags.", nameof(input));

        string source;
        try { source = StrictUtf8.GetString(input.Bytes.AsSpan()); }
        catch (DecoderFallbackException) { return Failed(input, FrameworkMetadataCompletionState.Unsafe, "The file is not strict UTF-8."); }
        var document = _markdown.Parse(source);
        if (document.Frontmatter.State == MarkdownFrontmatterState.Unavailable)
            return Failed(input, FrameworkMetadataCompletionState.Malformed, "The frontmatter block is not closed.");

        var form = input.Form;
        FrameworkDocumentMetadataFacts? before = null;
        (int Offset, string Text) insertion;
        var fields = ImmutableArray.CreateBuilder<FrameworkMetadataField>();
        string? description = null;
        ImmutableArray<string>? tags = null;
        if (document.Frontmatter.YamlSpan is { } yamlSpan)
        {
            var yaml = source[yamlSpan.Start..yamlSpan.End];
            var syntax = _yaml.Parse(yaml);
            before = _metadata.Read(syntax, FrameworkMetadataReadScope.RoutedSource);
            if (FrameworkMetadataCompletionValidator.ReadMalformedCause(before, syntax) is { } cause)
                return Failed(input, FrameworkMetadataCompletionState.Malformed, cause);
            if (before.State == FrameworkDocumentMetadataState.Complete)
                return FrameworkMetadataCompletionResult.Unchanged(input.Bytes);

            form = before.Syntax.AuthoredForm ?? input.Form;
            DeriveMissing();
            var emission = new FrameworkDocumentMetadataEmission(description, tags ?? [], responsibility: null);
            var mapping = before.Syntax.SelectedMapping;
            var insertionForm = form;
            if (before.Syntax.AuthoredForm is null)
            {
                mapping = syntax.Root;
            }
            else
            {
                insertionForm = FrontmatterForm.Root;
            }

            if (syntax.Root is null || FrameworkMetadataCompletionValidator.IsEmptyDocument(syntax.Root))
            {
                insertion = (yamlSpan.Start, FrameworkMetadataCompletionEmitter.Emit(emission, form, flow: false).Replace("\n", FrameworkMetadataInsertionEditor.Newline(source)));
            }
            else if (mapping is not null && FrameworkMetadataInsertionEditor.TryInsertMapping(yaml, mapping, emission, insertionForm, out var relative))
            {
                insertion = (yamlSpan.Start + relative.Offset, relative.Text);
            }
            else
            {
                return Failed(input, FrameworkMetadataCompletionState.Unsafe, "The metadata mapping has no safe additive insertion boundary.");
            }
        }
        else
        {
            DeriveMissing();
            var newline = FrameworkMetadataInsertionEditor.Newline(source);
            var emitted = FrameworkMetadataCompletionEmitter.Emit(new(description, tags ?? [], responsibility: null), form, flow: false).Replace("\n", newline);
            insertion = (source.StartsWith('\uFEFF') ? 1 : 0, $"---{newline}{emitted}---{newline}{newline}");
        }

        var intended = FrameworkMetadataInsertionEditor.Insert(input.Bytes, source, insertion);
        var afterDocument = _markdown.Parse(StrictUtf8.GetString(intended.AsSpan()));
        if (afterDocument.Frontmatter.YamlSpan is not { } intendedYamlSpan)
            return Failed(input, FrameworkMetadataCompletionState.Unsafe, "The completed metadata has no safe YAML boundary.");
        var intendedSyntax = _yaml.Parse(afterDocument.Source[intendedYamlSpan.Start..intendedYamlSpan.End]);
        var after = _metadata.Read(intendedSyntax, FrameworkMetadataReadScope.RoutedSource);
        if (FrameworkMetadataCompletionValidator.ReadMalformedCause(after, intendedSyntax) is not null
            || after.State != FrameworkDocumentMetadataState.Complete || after.Metadata is not { } completed
            || description is not null && completed.Description != description
            || tags is { } insertedTags && !completed.Tags.SequenceEqual(insertedTags, StringComparer.Ordinal)
            || before?.ObservedDescription is { } suppliedDescription && completed.Description != suppliedDescription
            || before is { ObservedTags.IsEmpty: false } && !completed.Tags.SequenceEqual(before.ObservedTags, StringComparer.Ordinal)
            || before is not null && !after.ApplyTo.Patterns.Select(pattern => pattern.Text)
                .SequenceEqual(before.ApplyTo.Patterns.Select(pattern => pattern.Text), StringComparer.Ordinal))
            return Failed(input, FrameworkMetadataCompletionState.Unsafe, "The completed metadata did not preserve supplied values.");

        return new()
        {
            State = FrameworkMetadataCompletionState.Completed,
            Bytes = intended,
            Fields = fields.ToImmutable(),
            Form = form,
            Description = description,
            Tags = tags,
        };

        void DeriveMissing()
        {
            if (before?.Syntax.Members.Any(member => member.Field == FrameworkMetadataField.Description) != true)
            {
                description = FrameworkMetadataDescriptionDeriver.Derive(input.CanonicalPath, document);
                fields.Add(FrameworkMetadataField.Description);
            }

            if (before?.Syntax.Members.Any(member => member.Field == FrameworkMetadataField.Tags) != true)
            {
                tags = input.DefaultTags;
                fields.Add(FrameworkMetadataField.Tags);
            }
        }
    }

    private static FrameworkMetadataCompletionResult Failed(FrameworkMetadataCompletionInput input, FrameworkMetadataCompletionState state, string cause)
        => new() { State = state, Bytes = input.Bytes, Fields = [], Cause = cause };
}
