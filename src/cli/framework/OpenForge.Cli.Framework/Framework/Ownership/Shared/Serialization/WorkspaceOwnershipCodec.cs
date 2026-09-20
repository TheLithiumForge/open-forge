using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

/// <summary>
/// Reads and writes the workspace lock.
///
/// Reading is forgiving on purpose. A missing section, a null array, or a key
/// this release has not heard of yields the entries the file does carry, because
/// the lock is never allowed to stop a command. Only a file that is not a JSON
/// object at all, or that carries an entry with no identity to act on, is
/// reported as unintelligible.
///
/// Writing is normalized so the file is reviewable in a diff: entries are sorted
/// by identity, paths and regions by ordinal path, and duplicates collapsed. Two
/// runs that owned the same things produce the same bytes.
/// </summary>
internal static class WorkspaceOwnershipCodec
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static WorkspaceOwnershipDecode Read(ReadOnlyMemory<byte> bytes)
    {
        try
        {
            _ = StrictUtf8.GetCharCount(bytes.Span);
            if (!IsJsonObject(bytes.Span))
            {
                return Invalid($"{WorkspaceOwnershipDefinitions.FileName} must contain a JSON object.");
            }

            var envelope = JsonSerializer.Deserialize(
                bytes.Span,
                WorkspaceOwnershipJsonContext.Default.WorkspaceOwnershipEnvelope);

            if (envelope is null)
            {
                return Invalid($"{WorkspaceOwnershipDefinitions.FileName} must contain a JSON object.");
            }

            return new WorkspaceOwnershipDecode(
                new WorkspaceOwnershipDocument(
                    envelope.SchemaVersion,
                    ReadFramework(envelope.Framework),
                    ReadExtensions(envelope.Extensions),
                    ReadLibraries(envelope.Libraries)),
                Cause: null);
        }
        catch (JsonException exception)
        {
            return Invalid(exception.Message);
        }
        catch (DecoderFallbackException)
        {
            return Invalid($"{WorkspaceOwnershipDefinitions.FileName} must be valid UTF-8.");
        }
        catch (ArgumentException exception)
        {
            return Invalid(exception.Message);
        }
    }

    internal static byte[] Write(WorkspaceOwnershipDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var envelope = new WorkspaceOwnershipEnvelope
        {
            Schema = WorkspaceOwnershipDefinitions.SchemaUrl,
            SchemaVersion = document.SchemaVersion,
            Framework = document.Framework is { } framework
                ? new FrameworkOwnershipEntry
                {
                    Source = new OwnedSourceEntry
                    {
                        Id = framework.Source.Id,
                        Version = framework.Source.Version,
                    },
                    Paths = SortPaths(framework.Paths),
                    Regions = SortRegions(framework.Regions),
                }
                : null,
            Extensions =
            [
                .. document.Extensions
                    .OrderBy(extension => extension.Id, StringComparer.Ordinal)
                    .Select(extension => new ExtensionOwnershipEntry
                    {
                        Id = extension.Id,
                        Version = extension.Version,
                        Source = extension.Source,
                        Dependencies = [.. extension.Dependencies.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
                        Paths = SortPaths(extension.Paths),
                        Regions = SortRegions(extension.Regions),
                    }),
            ],
            Libraries =
            [
                .. document.Libraries
                    .OrderBy(library => library.Id, StringComparer.Ordinal)
                    .Select(library => new LibraryOwnershipEntry
                    {
                        Id = library.Id,
                        SourceRoot = library.SourceRoot,
                        DestinationRoot = library.DestinationRoot,
                        Paths = SortPaths(library.Paths),
                    }),
            ],
        };

        return JsonSerializer.SerializeToUtf8Bytes(
            envelope,
            WorkspaceOwnershipJsonContext.Default.WorkspaceOwnershipEnvelope);
    }

    /// <summary>
    /// Whether the document starts as an object. Checked before deserializing so
    /// the cause names the file and the shape a person can act on, rather than the
    /// serializer's report about a CLR type they never asked about.
    /// </summary>
    private static bool IsJsonObject(ReadOnlySpan<byte> utf8Json)
    {
        var reader = new Utf8JsonReader(utf8Json);
        return reader.Read() && reader.TokenType == JsonTokenType.StartObject;
    }

    private static string[] SortPaths(ImmutableArray<string> paths)
        => [.. paths.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];

    private static OwnedRegionEntry[] SortRegions(ImmutableArray<OwnedRegion> regions)
        =>
        [
            .. regions
                .Distinct()
                .OrderBy(region => region.Path, StringComparer.Ordinal)
                .ThenBy(region => region.Region, StringComparer.Ordinal)
                .Select(region => new OwnedRegionEntry { Path = region.Path, Region = region.Region }),
        ];

    private static FrameworkOwnership? ReadFramework(FrameworkOwnershipEntry? entry)
    {
        if (entry is null)
        {
            return null;
        }

        var id = entry.Source?.Id;
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                $"\"{WorkspaceOwnershipDefinitions.FrameworkProperty}\" requires a source id.");
        }

        return new FrameworkOwnership(
            new OwnedSource(id, NullIfBlank(entry.Source?.Version)),
            ReadStrings(entry.Paths, WorkspaceOwnershipDefinitions.FrameworkProperty),
            ReadRegions(entry.Regions, WorkspaceOwnershipDefinitions.FrameworkProperty));
    }

    private static ImmutableArray<ExtensionOwnership> ReadExtensions(ExtensionOwnershipEntry[]? entries)
    {
        if (entries is null)
        {
            return [];
        }

        var extensions = ImmutableArray.CreateBuilder<ExtensionOwnership>();
        foreach (var entry in entries)
        {
            if (entry is null || string.IsNullOrWhiteSpace(entry.Id))
            {
                throw new ArgumentException(
                    $"\"{WorkspaceOwnershipDefinitions.ExtensionsProperty}\" requires an id on every entry.");
            }

            extensions.Add(new ExtensionOwnership(
                entry.Id,
                NullIfBlank(entry.Version),
                NullIfBlank(entry.Source),
                ReadStrings(entry.Dependencies, WorkspaceOwnershipDefinitions.ExtensionsProperty),
                ReadStrings(entry.Paths, WorkspaceOwnershipDefinitions.ExtensionsProperty),
                ReadRegions(entry.Regions, WorkspaceOwnershipDefinitions.ExtensionsProperty)));
        }

        return extensions.ToImmutable();
    }

    private static ImmutableArray<LibraryOwnership> ReadLibraries(LibraryOwnershipEntry[]? entries)
    {
        if (entries is null)
        {
            return [];
        }

        var libraries = ImmutableArray.CreateBuilder<LibraryOwnership>();
        foreach (var entry in entries)
        {
            if (entry is null
                || string.IsNullOrWhiteSpace(entry.Id)
                || string.IsNullOrWhiteSpace(entry.SourceRoot)
                || string.IsNullOrWhiteSpace(entry.DestinationRoot))
            {
                throw new ArgumentException(
                    $"\"{WorkspaceOwnershipDefinitions.LibrariesProperty}\" requires an id, source root, and destination root on every entry.");
            }

            libraries.Add(new LibraryOwnership(
                entry.Id,
                entry.SourceRoot,
                entry.DestinationRoot,
                ReadStrings(entry.Paths, WorkspaceOwnershipDefinitions.LibrariesProperty)));
        }

        return libraries.ToImmutable();
    }

    private static ImmutableArray<string> ReadStrings(string[]? values, string section)
    {
        if (values is null)
        {
            return [];
        }

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"\"{section}\" must not record an empty entry.");
            }
        }

        return [.. values];
    }

    private static ImmutableArray<OwnedRegion> ReadRegions(OwnedRegionEntry[]? values, string section)
    {
        if (values is null)
        {
            return [];
        }

        var regions = ImmutableArray.CreateBuilder<OwnedRegion>();
        foreach (var value in values)
        {
            if (value is null
                || string.IsNullOrWhiteSpace(value.Path)
                || string.IsNullOrWhiteSpace(value.Region))
            {
                throw new ArgumentException($"\"{section}\" requires a path and region on every generated region.");
            }

            regions.Add(new OwnedRegion(value.Path, value.Region));
        }

        return regions.ToImmutable();
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;

    private static WorkspaceOwnershipDecode Invalid(string cause)
        => new(Document: null, cause);
}
