using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Documents;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Stripping;

internal enum StripManagedSectionState
{
    Absent,
    Removed,
    Emptied,
    Invalid,
}

internal sealed record StripManagedSectionRemoval(StripManagedSectionState State, ImmutableArray<byte> Bytes);

/// <summary>
/// Recognizes the document shapes uninstall removes: route scaffolds and the managed
/// sections of root hosts. The uninstall Interface contract defines both shapes.
/// </summary>
internal static class StripDocumentShapes
{
    private const string LevelOneHeadingPrefix = "# ";
    private const char DefinitionTerminator = '.';
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    internal static bool IsRouteScaffold(WorkspaceLayout layout, StripDocument document)
    {
        if (!SourceFormClassifier.TryClassify(layout, document.Path, out var form) || !SourceFormClassifier.IsEntrypoint(form))
        {
            return false;
        }

        var facts = new MarkdownDocumentParser().Parse(document.Source);
        var metadata = new FrameworkDocumentMetadataParser().Parse(facts, FrameworkMetadataReadScope.RoutedSource);
        if (metadata is not { State: FrameworkDocumentMetadataState.Complete, Metadata: { } values }
            || facts.Frontmatter.BodyStart is not { } bodyStart)
        {
            return false;
        }

        var frontmatter = StripMetadataEdits.Read(document.Path, facts);
        if (frontmatter.Edits is not [{ End: var end }] || end != bodyStart)
        {
            return false;
        }

        var entries = StripEntriesEdits.Read(facts);
        if (entries.Unsupported is not null)
        {
            return false;
        }

        var body = new StringBuilder(document.Source);
        foreach (var edit in entries.Edits.OrderByDescending(edit => edit.Start))
        {
            body.Remove(edit.Start, edit.End - edit.Start);
        }

        var lines = body.ToString(bodyStart, body.Length - bodyStart)
            .Split('\n')
            .Select(line => line.TrimEnd())
            .Where(line => line.Length > 0)
            .ToArray();
        if (lines.Length is not (3 or 4)
            || !lines[0].StartsWith(LevelOneHeadingPrefix, StringComparison.Ordinal)
            || lines[0].Length == LevelOneHeadingPrefix.Length
            || lines[^2] != RouteScaffoldComposer.AxiomsHeading
            || lines[^1] != RouteScaffoldComposer.InheritedAxiomsMarker)
        {
            return false;
        }

        return lines.Length == 3 || lines[1] == values.Description + DefinitionTerminator;
    }

    internal static StripManagedSectionRemoval RemoveManagedSection(string source)
    {
        var facts = new MarkdownDocumentParser().Parse(source);
        if (facts.ManagedHost.State == MarkdownManagedHostState.Absent)
        {
            return new StripManagedSectionRemoval(StripManagedSectionState.Absent, []);
        }

        if (facts.ManagedHost is not { State: MarkdownManagedHostState.Present, Span: { } span })
        {
            return new StripManagedSectionRemoval(StripManagedSectionState.Invalid, []);
        }

        var start = span.Start;
        int end;
        if (StripSourceLines.IsBlank(source, span.End, source.Length))
        {
            end = source.Length;
            start = StripSourceLines.BackOverBlankLines(source, start, 0);
        }
        else
        {
            end = StripSourceLines.ForwardOverBlankLines(source, span.End);
        }

        var remaining = string.Concat(source.AsSpan(0, start), source.AsSpan(end));
        return string.IsNullOrWhiteSpace(remaining)
            ? new StripManagedSectionRemoval(StripManagedSectionState.Emptied, [])
            : new StripManagedSectionRemoval(StripManagedSectionState.Removed, [.. Utf8.GetBytes(remaining)]);
    }
}
