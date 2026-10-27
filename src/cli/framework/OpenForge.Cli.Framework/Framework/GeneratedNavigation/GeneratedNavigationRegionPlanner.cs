using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

internal sealed class GeneratedNavigationRegionPlanner
{
    internal GeneratedNavigationRegion Plan(
        GeneratedNavigationProjectionRequest request,
        GeneratedNavigationRegionInput input)
    {
        var document = ReadRegionDocument(input);
        if (!document.IsComplete)
        {
            return GeneratedNavigationRegion.Unavailable(
                input.Source,
                document.ReadUnavailableReason(),
                document.ReadCause());
        }

        var childProjection = ReadChildren(request, input.Source, document.ReadValue());
        if (!childProjection.IsComplete)
        {
            return GeneratedNavigationRegion.Unavailable(
                input.Source,
                childProjection.ReadUnavailableReason(),
                childProjection.ReadCause());
        }

        var entryProjection = BuildEntries(request, input.Source, childProjection.ReadValue());
        if (!entryProjection.IsComplete)
        {
            return GeneratedNavigationRegion.Unavailable(
                input.Source,
                entryProjection.ReadUnavailableReason(),
                entryProjection.ReadCause());
        }

        return BuildRegion(request.Formation.Catalogue.Layout, input.Source, document.ReadValue(), entryProjection.ReadValue(), ReadObservedLoader(request, input.Source),
            request);
    }

    // When an operation rewrites the Loader from the payload, the user's authored links live in the
    // Loader as it is on disk, so they are carried over from there.
    private static MarkdownDocumentFacts? ReadObservedLoader(
        GeneratedNavigationProjectionRequest request,
        SourceLogicalSource source)
    {
        return source.Base.Form == SourceDocumentForm.Loader && request.ObservedLoaderSource is { } observed
            ? new MarkdownDocumentParser().Parse(observed.Replace("\r\n", "\n", StringComparison.Ordinal))
            : null;
    }

    internal GeneratedNavigationRegion PlanProjectedEntries(WorkspaceLayout layout, GeneratedNavigationRegionInput input,
        IReadOnlyList<GeneratedNavigationEntry> entries)
    {
        var document = ReadRegionDocument(input);
        if (!document.IsComplete)
        {
            return GeneratedNavigationRegion.Unavailable(
                input.Source,
                document.ReadUnavailableReason(),
                document.ReadCause());
        }

        return BuildRegion(layout, input.Source, document.ReadValue(), entries, observedLoader: null);
    }

    private static GeneratedNavigationProjectionStage<MarkdownDocumentFacts> ReadRegionDocument(
        GeneratedNavigationRegionInput input)
    {
        if (!IsRegionSource(input.Source))
        {
            return GeneratedNavigationProjectionStage<MarkdownDocumentFacts>.Unavailable(
                GeneratedNavigationRegionUnavailableReason.RegionSourceUnsupported,
                "Generated navigation can only target the Loader or a recognized entrypoint.");
        }

        if (input.Document is not { } document)
        {
            return GeneratedNavigationProjectionStage<MarkdownDocumentFacts>.Unavailable(
                GeneratedNavigationRegionUnavailableReason.SourceDocumentUnavailable,
                input.UnavailableCause
                    ?? "The generated navigation source document is unavailable.");
        }

        if (document.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete
            || document.GeneratedRegion.EntriesBlock is not { } block)
        {
            return GeneratedNavigationProjectionStage<MarkdownDocumentFacts>.Unavailable(
                ReadGeneratedRegionReason(document.GeneratedRegion.State),
                document.GeneratedRegion.Cause
                    ?? "The document does not contain one complete generated Entries section.");
        }

        var beforeBody = document.Source[block.Span.Start..block.Span.End];
        if (ContainsUnsupportedLineEnding(beforeBody) || ContainsUnsupportedLineEnding(block.LineEnding))
        {
            return GeneratedNavigationProjectionStage<MarkdownDocumentFacts>.Unavailable(
                GeneratedNavigationRegionUnavailableReason.GeneratedRegionLineEndingUnsupported,
                "The generated Entries section uses an unsupported line ending.");
        }

        return GeneratedNavigationProjectionStage<MarkdownDocumentFacts>.Complete(document);
    }

    private static GeneratedNavigationRegion BuildRegion(
        WorkspaceLayout layout,
        SourceLogicalSource source,
        MarkdownDocumentFacts document,
        IReadOnlyList<GeneratedNavigationEntry> entries,
        MarkdownDocumentFacts? observedLoader,
        GeneratedNavigationProjectionRequest? request = null)
    {
        try
        {
            var roots = new SourceContentRoots(layout);
            var linkSource = observedLoader ?? document;
            var authoredLinks = ReadAuthoredLinks(layout, source, linkSource, entries, request);
            if (source.Base.Form == SourceDocumentForm.Loader && request?.LoaderEdit is { } loaderEdit)
            {
                authoredLinks = ApplyLoaderEdit(roots, authoredLinks, loaderEdit);
            }
            var expectedBody = BuildExpectedBlock(document, entries, authoredLinks);
            var change = BuildGeneratedChange(document, expectedBody);
            return GeneratedNavigationRegion.Available(source, entries, change) with { AuthoredLinkCount = authoredLinks.Count };
        }
        catch (ArgumentException exception)
        {
            return GeneratedNavigationRegion.Unavailable(
                source,
                GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable,
                exception.Message);
        }
    }

    // The Loader's Entries are the user's list of what is switched on. When membership is preserved,
    // a root folder that exists on disk but is no longer listed stays off, and an empty list stays empty.
    // Only roots the operation itself turns on join the list.
    private static IReadOnlyList<string> ReadLoaderRootPaths(
        GeneratedNavigationProjectionRequest request,
        MarkdownDocumentFacts document)
    {
        var structural = request.Topology.LoaderRootPaths
            .Where(path => !SourceLogicalPath.IsOutsideSource(request.Formation.Catalogue.Layout, path)).ToArray();
        if (!request.PreserveLoaderMembership)
        {
            return structural;
        }

        var listed = SourceLoaderEntriesParser.Parse(request.Formation.Catalogue.Layout, request.ObservedLoaderSource ?? document.Source);
        if (listed.State != SourceLoaderEntriesParseState.Valid)
        {
            return structural;
        }

        var listedRoots = listed.Destinations
            .Where(destination => destination.State == SourceLoaderDestinationParseState.Valid)
            .Select(destination => destination.CanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        return structural
            .Where(path => !request.TurnedOffLoaderRoots.Contains(path)
                && (listedRoots.Contains(path) || request.TurnedOnLoaderRoots.Contains(path)))
            .ToArray();
    }

    internal static bool IsRegionSource(SourceLogicalSource source)
    {
        return source.Base.Form == SourceDocumentForm.Loader
            || SourceFormClassifier.IsEntrypoint(source.Base.Form);
    }

    private static GeneratedNavigationProjectionStage<IReadOnlyList<SourceLogicalSource>> ReadChildren(
        GeneratedNavigationProjectionRequest request,
        SourceLogicalSource parent,
        MarkdownDocumentFacts document)
    {
        IReadOnlyList<string>? childPaths = parent.Base.Form == SourceDocumentForm.Loader
            ? ReadLoaderRootPaths(request, document)
            : request.Topology.FindByPath(parent.Identity.CanonicalBasePath)?.ChildPaths;
        if (childPaths is null)
        {
            return GeneratedNavigationProjectionStage<IReadOnlyList<SourceLogicalSource>>.Unavailable(
                GeneratedNavigationRegionUnavailableReason.TopologyUnavailable,
                "The generated navigation parent is not present in the accepted route topology.");
        }

        var selected = new List<SourceLogicalSource>();
        var physicalPaths = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
        var authoredOutsidePaths = new HashSet<string>(StringComparer.Ordinal);
        if (SourceLogicalPath.IsOutsideSource(request.Formation.Catalogue.Layout, parent.Base.CanonicalPath))
        {
            foreach (var entry in SourceGeneratedEntriesParser.Parse(document).Entries)
                if (SourceGeneratedDestinationResolver.ResolveLinked(request.Formation.Catalogue.Layout, parent.Base.CanonicalPath, false, entry.Destination) is { } path)
                    authoredOutsidePaths.Add(request.Formation.Catalogue.FindByPath(path)?.Base.CanonicalPath ?? path);
            foreach (var row in request.Formation.Catalogue.IncludedRows.Where(row => row.ParentPath == parent.Base.CanonicalPath))
                authoredOutsidePaths.Add(request.Formation.Catalogue.FindByPath(row.Path)?.Base.CanonicalPath ?? row.AliasOf ?? row.Path);
        }
        foreach (var childPath in childPaths)
        {
            if (authoredOutsidePaths.Contains(childPath)) continue;
            if (!request.Sharing.Includes(childPath)) continue;
            var child = request.FindSource(childPath);
            if (child is null)
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<SourceLogicalSource>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.TopologyUnavailable,
                    "The accepted route topology identifies a source unavailable to generated navigation.");
            }

            if (child.Base.Form == SourceDocumentForm.OverwriteCompanion)
            {
                continue;
            }

            if (PhysicalIdentityTracker.PathComparer.Equals(
                    child.Base.PhysicalPath,
                    parent.Base.PhysicalPath))
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<SourceLogicalSource>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.TopologyUnsafe,
                    "A direct routed child aliases its containing generated navigation region.");
            }

            if (physicalPaths.Add(child.Base.PhysicalPath))
            {
                selected.Add(child);
            }
        }

        return GeneratedNavigationProjectionStage<IReadOnlyList<SourceLogicalSource>>.Complete(selected);
    }

    private static GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>> BuildEntries(
        GeneratedNavigationProjectionRequest request,
        SourceLogicalSource parent,
        IReadOnlyList<SourceLogicalSource> children)
    {
        var projected = new List<GeneratedNavigationEntry>();
        var destinations = new HashSet<string>(StringComparer.Ordinal);
        foreach (var child in children)
        {
            var metadata = request.FindMetadata(child);
            if (metadata is not null && metadata.ApplyTo.State == ApplyToMetadataState.Invalid)
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.MetadataInvalid,
                    "Every direct routed child requires valid applyTo metadata.");
            }

            if (metadata is null
                || metadata.State == SourceAuthoredMetadataState.NotApplicable
                || (metadata.State == SourceAuthoredMetadataState.Missing
                    && !IsPartialMetadataSource(child.Base.Form)))
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.MetadataUnavailable,
                    "Every direct routed child requires complete authored source metadata or eligible observed metadata.");
            }

            if (metadata.State == SourceAuthoredMetadataState.Malformed)
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.MetadataInvalid,
                    "Every direct routed child requires complete authored source metadata.");
            }

            string description;
            IReadOnlyList<string> tags;
            IReadOnlyList<ApplyToPattern> applyTo;
            if (metadata.State == SourceAuthoredMetadataState.Complete)
            {
                if (metadata.Description is not { } completeDescription)
                {
                    return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                        GeneratedNavigationRegionUnavailableReason.MetadataInvalid,
                        "Every complete source metadata fact requires an authored description.");
                }

                description = completeDescription;
                applyTo = metadata.ApplyTo.Patterns;
                tags = child.Base.Form == SourceDocumentForm.Skill
                    ? ["Skill"]
                    : metadata.Tags;
            }
            else
            {
                description = metadata.ObservedDescription ?? child.Identity.AutomaticId;
                applyTo = metadata.ApplyTo.Patterns;
                tags = metadata.ObservedTags;
            }

            if (!TryReadDestination(request, parent, child, out var destination, out var cause))
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.DestinationUnsafe,
                    cause);
            }

            if (!IsSafeDescription(description, out cause))
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.MetadataUnrepresentable,
                    cause);
            }

            if (!destinations.Add(destination))
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.DestinationConflict,
                    "Direct routed children produce a duplicate generated navigation destination.");
            }

            var label = EncodeLinkLabel(description);
            var tagsSuffix = tags.Count == 0
                ? string.Empty
                : $" - {string.Join(' ', tags.Select(tag => $"#{tag}"))}";
            var line = $"- [{label}]({destination}){tagsSuffix}"
                + MarkdownEntryRowFormatter.FormatApplyToSuffix(applyTo);
            projected.Add(new GeneratedNavigationEntry(
                source: child,
                description: description,
                destination: destination,
                tags: tags,
                line: line)
            {
                ApplyTo = applyTo,
            });
        }

        var entries = projected
            .OrderBy(entry => entry.Destination, StringComparer.Ordinal)
            .ThenBy(entry => entry.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Complete(entries);
    }

    private static bool IsPartialMetadataSource(SourceDocumentForm form)
        => form == SourceDocumentForm.Markdown || SourceFormClassifier.IsEntrypoint(form);

    private static GeneratedNavigationRegionUnavailableReason ReadGeneratedRegionReason(
        MarkdownGeneratedRegionState state)
        => state switch
        {
            MarkdownGeneratedRegionState.Absent => GeneratedNavigationRegionUnavailableReason.GeneratedRegionMissing,
            MarkdownGeneratedRegionState.Invalid => GeneratedNavigationRegionUnavailableReason.GeneratedRegionInvalid,
            MarkdownGeneratedRegionState.Unavailable => GeneratedNavigationRegionUnavailableReason.GeneratedRegionUnavailable,
            MarkdownGeneratedRegionState.Complete => GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The generated region state is not defined."),
        };

    private static bool IsSafeDescription(
        string? description,
        out string cause)
    {
        cause = string.Empty;
        if (string.IsNullOrWhiteSpace(description))
        {
            cause = "A generated navigation entry requires a non-empty authored description.";
            return false;
        }

        if (description.Any(character => char.IsControl(character) || character is '[' or ']'))
        {
            cause = "An authored description cannot be represented as one canonical generated Markdown link label.";
            return false;
        }

        return true;
    }

    private static string EncodeLinkLabel(string description)
        => description.Replace("\\", "\\\\", StringComparison.Ordinal);

    private static bool TryReadDestination(
        GeneratedNavigationProjectionRequest request,
        SourceLogicalSource parent,
        SourceLogicalSource child,
        out string destination,
        out string cause)
    {
        destination = string.Empty;
        cause = string.Empty;
        var parentDirectory = SourceLogicalPath.ReadParent(parent.Identity.CanonicalBasePath);
        var prefix = parentDirectory + "/";
        var childPath = child.Identity.CanonicalBasePath;
        if (parent.Base.Form != SourceDocumentForm.Loader && !childPath.StartsWith(prefix, StringComparison.Ordinal))
        {
            cause = "A direct routed child does not have a safe containing-file-relative destination.";
            return false;
        }

        var layout = request.Formation.Catalogue.Layout;
        var relative = childPath.StartsWith(prefix, StringComparison.Ordinal)
            ? childPath[prefix.Length..]
            : Path.GetRelativePath(
                SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, parentDirectory),
                SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, childPath)).Replace('\\', '/');
        if (relative.Length == 0)
        {
            cause = "A direct routed child does not have a non-empty destination.";
            return false;
        }

        var segments = relative.Split('/');
        if (segments.Any(segment => !SourceLogicalPath.IsCanonicalSegment(segment)
                && !(segment == ".." && parent.Base.Form == SourceDocumentForm.Loader)))
        {
            cause = "A direct routed child destination contains a non-canonical path segment.";
            return false;
        }

        try
        {
            destination = SourceDestinationEncoder.EncodePath(relative);
            return SourceGeneratedDestinationResolver.Resolve(layout, parent.Identity.CanonicalBasePath, parent.Base.Form == SourceDocumentForm.Loader, destination) == childPath;
        }
        catch (EncoderFallbackException)
        {
            cause = "A direct routed child destination is not valid strict UTF-8.";
            return false;
        }
    }

    private static string BuildExpectedBlock(
        MarkdownDocumentFacts document,
        IReadOnlyList<GeneratedNavigationEntry> entries,
        IReadOnlyList<string> authoredLinks)
    {
        var block = document.GeneratedRegion.EntriesBlock
            ?? throw new InvalidOperationException("A complete Entries section requires its block boundary.");
        var lineEnding = block.LineEnding;
        var generated = entries.Select(entry => entry.Line).ToArray();
        string[] lines = generated.Length + authoredLinks.Count == 0
            ? [MarkdownEntriesSectionReader.EmptyEntry]
            : [.. generated, .. authoredLinks];
        var list = string.Join(lineEnding, lines);
        if (block.Exists)
        {
            return $"{list}{lineEnding}";
        }

        var content = document.GeneratedRegion.ContentSpan
            ?? throw new InvalidOperationException("A complete Entries section requires its content boundary.");
        var leadingBreaks = document.Source.AsSpan(content.Start, block.Span.Start - content.Start).Count('\n');
        var prefix = string.Concat(Enumerable.Repeat(lineEnding, Math.Max(0, 2 - leadingBreaks)));
        return $"{prefix}{list}{lineEnding}{lineEnding}";
    }

    // Index keeps authored link rows as written, after the generated rows and in their original order.
    private static IReadOnlyList<string> ReadAuthoredLinks(
        WorkspaceLayout layout,
        SourceLogicalSource source,
        MarkdownDocumentFacts document,
        IReadOnlyList<GeneratedNavigationEntry> entries,
        GeneratedNavigationProjectionRequest? request)
    {
        var roots = new SourceContentRoots(layout);
        if (document.GeneratedRegion.EntriesBlock is not { Exists: true } block)
        {
            return [];
        }

        var authored = new List<string>();
        var generatedPaths = entries.Select(entry => entry.CanonicalPath).ToHashSet(StringComparer.Ordinal);
        var text = document.Source[block.Span.Start..block.Span.End].Replace("\r\n", "\n", StringComparison.Ordinal);
        foreach (var line in text.Split('\n'))
        {
            if (!MarkdownEntryRowParser.TryParse(line, out var row, out _)) continue;
            var outsidePath = SourceGeneratedDestinationResolver.ResolveLinked(roots, source.Base.CanonicalPath, false, row.Destination);
            if (SourceLogicalPath.IsOutsideSource(layout, source.Base.CanonicalPath)
                && (outsidePath is null || !generatedPaths.Contains(outsidePath)))
            {
                authored.Add(line);
                continue;
            }
            var path = source.Base.Form == SourceDocumentForm.Loader
                ? SourceGeneratedDestinationResolver.Resolve(roots, source.Identity.CanonicalBasePath, true, row.Destination)
                : null;

            // Direct nested catalogue rows belong to the loader's authored membership.
            // Structural topology does not generate them or acquire their metadata.
            if (path is not null && !generatedPaths.Contains(path) && RetainNestedCatalogue(roots, path, request)
                || SourceGeneratedDestinationResolver.IsAuthoredLink(row.Destination)
                && (roots.IsDefault || source.Base.Form != SourceDocumentForm.Loader || path is null))
            {
                authored.Add(line);
            }
        }

        return authored;
    }

    private static bool RetainNestedCatalogue(SourceContentRoots roots, string path, GeneratedNavigationProjectionRequest? request)
    {
        if (!SourceFormClassifier.IsEntrypointPath(path)) return false;
        var directory = SourceLogicalPath.ReadParent(path);
        if (!SourceLogicalPath.IsPrimarySource(roots, directory)
            || SourceLogicalPath.IsPrimaryCategoryDirectory(roots, directory)) return false;
        if (request is null) return true;
        return request.FindSource(path) is { } source && source.Identity.CanonicalBasePath == path
            && SourceFormClassifier.IsEntrypoint(source.Base.Form)
            && request.Sharing.Includes(path) && !request.TurnedOffLoaderRoots.Contains(path);
    }

    private static IReadOnlyList<string> ApplyLoaderEdit(SourceContentRoots roots, IReadOnlyList<string> rows, GeneratedNavigationLoaderEdit edit)
    {
        var result = new List<string>();
        var replaced = false;
        foreach (var line in rows)
        {
            if (MarkdownEntryRowParser.TryParse(line, out var row, out _)
                && string.Equals(SourceLoaderDestinationParser.Parse(roots, row.Destination).CanonicalPath, edit.Path, StringComparison.Ordinal))
            {
                if (!replaced && edit.Row is { } replacement) result.Add(replacement);
                replaced = true;
            }
            else result.Add(line);
        }

        if (!replaced && edit.Row is { } appended) result.Add(appended);
        return result;
    }

    private static GeneratedNavigationBoundedChange BuildGeneratedChange(MarkdownDocumentFacts document, string expectedBlock)
    {
        var content = document.GeneratedRegion.ContentSpan
            ?? throw new InvalidOperationException("A complete Entries section requires its content boundary.");
        var block = document.GeneratedRegion.EntriesBlock
            ?? throw new InvalidOperationException("A complete Entries section requires its block boundary.");
        var guards = MarkdownEntriesSectionReader.ReadRetiredGuardSpans(document.Source, content);
        if (guards.Count == 0)
        {
            return BuildChange(document, block.Span, document.Source[block.Span.Start..block.Span.End], expectedBlock);
        }

        // Only known obsolete guard text is removed outside the generated list.
        // Authored prose and every surrounding line ending retain their bytes.
        var edits = guards.Select(span => (Span: span, Text: string.Empty))
            .Append((Span: block.Span, Text: expectedBlock))
            .OrderBy(edit => edit.Span.Start)
            .ThenBy(edit => edit.Span.Length);
        var builder = new StringBuilder();
        var cursor = content.Start;
        foreach (var edit in edits)
        {
            builder.Append(document.Source, cursor, edit.Span.Start - cursor);
            builder.Append(edit.Text);
            cursor = edit.Span.End;
        }

        builder.Append(document.Source, cursor, content.End - cursor);
        return BuildChange(document, content, document.Source[content.Start..content.End], builder.ToString());
    }

    private static bool ContainsUnsupportedLineEnding(string content)
    {
        for (var index = 0; index < content.Length; index++)
        {
            if (content[index] != '\r')
            {
                continue;
            }

            if (index + 1 >= content.Length || content[index + 1] != '\n')
            {
                return true;
            }

            index++;
        }

        return false;
    }

    private static GeneratedNavigationBoundedChange BuildChange(
        MarkdownDocumentFacts document,
        MarkdownTextSpan contentSpan,
        string beforeBody,
        string expectedBody)
    {
        var source = document.Source;
        var prefix = source[..contentSpan.Start];
        var suffix = source[contentSpan.End..];
        var location = new Utf8SourceMap(source).Map(
            contentSpan.Start,
            contentSpan.Length);
        return new GeneratedNavigationBoundedChange(
            new GeneratedNavigationBoundedChangeInput
            {
                ContentLocation = location,
                BeforeBody = beforeBody,
                ExpectedBody = expectedBody,
                Prefix = prefix,
                Suffix = suffix,
            });
    }

    private sealed record GeneratedNavigationProjectionStage<T>
        where T : class
    {
        private GeneratedNavigationProjectionStage(
            T? value,
            GeneratedNavigationRegionUnavailableReason? unavailableReason,
            string? cause)
        {
            Value = value;
            UnavailableReason = unavailableReason;
            Cause = cause;
        }

        internal bool IsComplete => UnavailableReason is null;

        private T? Value { get; }

        private GeneratedNavigationRegionUnavailableReason? UnavailableReason { get; }

        private string? Cause { get; }

        internal T ReadValue()
            => Value
                ?? throw new InvalidOperationException("A complete generated navigation stage requires its value.");

        internal GeneratedNavigationRegionUnavailableReason ReadUnavailableReason()
            => UnavailableReason
                ?? throw new InvalidOperationException("An unavailable generated navigation stage requires a typed reason.");

        internal string ReadCause()
            => Cause
                ?? throw new InvalidOperationException("An unavailable generated navigation stage requires a cause.");

        internal static GeneratedNavigationProjectionStage<T> Complete(T value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return new(value, null, null);
        }

        internal static GeneratedNavigationProjectionStage<T> Unavailable(
            GeneratedNavigationRegionUnavailableReason reason,
            string cause)
        {
            if (!Enum.IsDefined(reason))
            {
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "The unavailable reason is not defined.");
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
            return new(null, reason, cause);
        }
    }
}
