using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

internal sealed class GeneratedNavigationRegionPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal GeneratedNavigationRegion Plan(
        GeneratedNavigationProjectionRequest request,
        GeneratedNavigationRegionInput input)
    {
        if (!IsRegionSource(input.Source))
        {
            return GeneratedNavigationRegion.Unavailable(
                input.Source,
                GeneratedNavigationRegionUnavailableReason.RegionSourceUnsupported,
                "Generated navigation can only target the Loader or a recognized entrypoint.");
        }

        if (input.Document is not { } document)
        {
            return GeneratedNavigationRegion.Unavailable(
                input.Source,
                GeneratedNavigationRegionUnavailableReason.SourceDocumentUnavailable,
                input.UnavailableCause
                    ?? "The generated navigation source document is unavailable.");
        }

        if (document.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete
            || document.GeneratedRegion.ContentSpan is not { } contentSpan)
        {
            return GeneratedNavigationRegion.Unavailable(
                input.Source,
                ReadGeneratedRegionReason(document.GeneratedRegion.State),
                document.GeneratedRegion.Cause
                    ?? "The document does not contain one complete final generated Entries region.");
        }

        var beforeBody = document.Source[contentSpan.Start..contentSpan.End];
        if (ContainsUnsupportedLineEnding(beforeBody))
        {
            return GeneratedNavigationRegion.Unavailable(
                input.Source,
                GeneratedNavigationRegionUnavailableReason.GeneratedRegionLineEndingUnsupported,
                "The generated Entries section uses an unsupported line ending.");
        }

        var childProjection = ReadChildren(request, input.Source);
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

        try
        {
            var entries = entryProjection.ReadValue();
            var expectedBody = BuildExpectedBody(source: document.Source, beforeBody: beforeBody, entries: entries);
            var change = BuildChange(
                document: document,
                contentSpan: contentSpan,
                beforeBody: beforeBody,
                expectedBody: expectedBody);
            return GeneratedNavigationRegion.Available(input.Source, entries, change);
        }
        catch (ArgumentException exception)
        {
            return GeneratedNavigationRegion.Unavailable(
                input.Source,
                GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable,
                exception.Message);
        }
    }

    private static bool IsRegionSource(SourceLogicalSource source)
    {
        return source.Base.Form == SourceDocumentForm.Loader
            || SourceFormClassifier.IsEntrypoint(source.Base.Form);
    }

    private static GeneratedNavigationProjectionStage<IReadOnlyList<SourceLogicalSource>> ReadChildren(
        GeneratedNavigationProjectionRequest request,
        SourceLogicalSource parent)
    {
        IReadOnlyList<string>? childPaths = parent.Base.Form == SourceDocumentForm.Loader
            ? request.Topology.LoaderRootPaths
            : request.Topology.FindByPath(parent.Identity.CanonicalBasePath)?.ChildPaths;
        if (childPaths is null)
        {
            return GeneratedNavigationProjectionStage<IReadOnlyList<SourceLogicalSource>>.Unavailable(
                GeneratedNavigationRegionUnavailableReason.TopologyUnavailable,
                "The generated navigation parent is not present in the accepted route topology.");
        }

        var selected = new List<SourceLogicalSource>();
        var physicalPaths = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
        foreach (var childPath in childPaths)
        {
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
            if (metadata is null
                || metadata.State is SourceAuthoredMetadataState.Missing or SourceAuthoredMetadataState.NotApplicable)
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.MetadataUnavailable,
                    "Every direct routed child requires complete authored source metadata.");
            }

            if (metadata.State == SourceAuthoredMetadataState.Malformed)
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.MetadataInvalid,
                    "Every direct routed child requires complete authored source metadata.");
            }

            if (metadata.Description is not { } description)
            {
                return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Unavailable(
                    GeneratedNavigationRegionUnavailableReason.MetadataInvalid,
                    "Every complete source metadata fact requires an authored description.");
            }

            if (!TryReadDestination(parent, child, out var destination, out var cause))
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

            var tags = child.Base.Form == SourceDocumentForm.Skill
                ? ["Skill"]
                : metadata.Tags;
            var label = EncodeLinkLabel(description);
            var line = $"- [{label}]({destination}) - {string.Join(' ', tags.Select(tag => $"#{tag}"))}";
            projected.Add(new GeneratedNavigationEntry(
                source: child,
                description: description,
                destination: destination,
                tags: tags,
                line: line));
        }

        var entries = projected
            .OrderBy(entry => entry.Destination, StringComparer.Ordinal)
            .ThenBy(entry => entry.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        return GeneratedNavigationProjectionStage<IReadOnlyList<GeneratedNavigationEntry>>.Complete(entries);
    }

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
        if (!childPath.StartsWith(prefix, StringComparison.Ordinal))
        {
            cause = "A direct routed child does not have a safe containing-file-relative destination.";
            return false;
        }

        var relative = childPath[prefix.Length..];
        if (relative.Length == 0)
        {
            cause = "A direct routed child does not have a non-empty destination.";
            return false;
        }

        var segments = relative.Split('/');
        if (segments.Any(segment => !SourceLogicalPath.IsCanonicalSegment(segment)))
        {
            cause = "A direct routed child destination contains a non-canonical path segment.";
            return false;
        }

        if (segments.Any(segment => segment.Any(character => character is '?' or '#' or ':')))
        {
            cause = "A direct routed child destination contains an unsafe path character.";
            return false;
        }

        try
        {
            destination = string.Join('/', segments.Select(EncodeSegment));
            return true;
        }
        catch (EncoderFallbackException)
        {
            cause = "A direct routed child destination is not valid strict UTF-8.";
            return false;
        }
    }

    private static string EncodeSegment(string segment)
    {
        _ = StrictUtf8.GetByteCount(segment);
        var builder = new StringBuilder(segment.Length);
        foreach (var rune in segment.EnumerateRunes())
        {
            if (rune.Value < 128 && IsUnescapedPathCharacter((char)rune.Value))
            {
                builder.Append((char)rune.Value);
                continue;
            }

            foreach (var value in StrictUtf8.GetBytes(rune.ToString()))
            {
                builder.Append('%');
                builder.Append(value.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        return builder.ToString();
    }

    private static bool IsUnescapedPathCharacter(char value)
    {
        return value is >= 'A' and <= 'Z'
            or >= 'a' and <= 'z'
            or >= '0' and <= '9'
            or '-' or '.' or '_' or '~';
    }

    private static string BuildExpectedBody(
        string source,
        string beforeBody,
        IReadOnlyList<GeneratedNavigationEntry> entries)
    {
        var lineEnding = ReadLineEnding(source: source, beforeBody: beforeBody);
        var lines = entries.Count == 0
            ? [MarkdownGeneratedRegionSyntax.EmptyEntry]
            : entries.Select(entry => entry.Line).ToArray();
        return $"{lineEnding}{string.Join(lineEnding, lines)}{lineEnding}";
    }

    private static string ReadLineEnding(string source, string beforeBody)
    {
        if (beforeBody.Contains("\r\n", StringComparison.Ordinal))
        {
            return "\r\n";
        }

        if (beforeBody.Contains('\n'))
        {
            return "\n";
        }

        return source.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";
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
