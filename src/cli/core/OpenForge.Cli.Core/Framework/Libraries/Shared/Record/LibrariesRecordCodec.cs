using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Record;

internal static class LibrariesRecordCodec
{
    internal static LibrariesRecordDecode Read(ReadOnlyMemory<byte> bytes)
    {
        try
        {
            using var json = JsonDocument.Parse(bytes, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 64,
            });
            ValidateDocument(json.RootElement);
            var document = JsonSerializer.Deserialize(
                bytes.Span,
                LibrariesRecordJsonContext.Default.LibrariesRecordDocument)
                ?? throw new JsonException("The Library record root is null.");
            if (document.SchemaVersion != LibrariesRecord.CurrentSchemaVersion)
            {
                throw new JsonException("The Library record schema version is unsupported.");
            }

            var candidates = document.Libraries.Select(library => (
                Id: LibraryId.Create(library.Id),
                SourceRoot: WorkspaceRelativeDirectory.Create(library.SourceRoot),
                DestinationRoot: LibraryDestinationRoot.Create(library.DestinationRoot),
                Paths: library.Paths.Select(SourceRelativeEligiblePath.Create).ToArray()))
                .ToArray();
            foreach (var candidate in candidates)
            {
                foreach (var path in candidate.Paths)
                {
                    if (LibraryEligiblePathPolicy.TryClassifyExclusion(candidate.SourceRoot, path, out _))
                    {
                        throw new ArgumentException(
                            "A Library record path names an ineligible manager or source-control path.");
                    }
                }

            }

            if (HasOutOfOrderMembers(document))
            {
                throw new ArgumentException(
                    "Library records and their paths must be in ordinal order.");
            }

            if (HasAmbiguousOwnership(document))
            {
                return new LibrariesRecordDecode
                {
                    State = LibrariesRecordReadState.Malformed,
                    Record = null,
                    Issue = LibrariesRecordDecodeIssue.AmbiguousOwnership,
                    Cause = "The Library record contains ambiguous duplicate ownership.",
                };
            }

            LibraryRecord[] libraries = [.. candidates.Select(candidate =>
                LibraryRecord.Create(
                    candidate.Id,
                    candidate.SourceRoot, candidate.DestinationRoot,
                    candidate.Paths))];
            return new LibrariesRecordDecode
            {
                State = LibrariesRecordReadState.Complete,
                Record = LibrariesRecord.Create(libraries),
                Issue = LibrariesRecordDecodeIssue.None,
                Cause = null,
            };
        }
        catch (Exception exception) when (exception is JsonException
            or ArgumentException
            or InvalidOperationException)
        {
            return new LibrariesRecordDecode
            {
                State = LibrariesRecordReadState.Malformed,
                Record = null,
                Issue = LibrariesRecordDecodeIssue.Malformed,
                Cause = "The Library record is not exact schema v1.",
            };
        }
    }

    internal static byte[] Write(LibrariesRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        var document = new LibrariesRecordDocument
        {
            SchemaVersion = LibrariesRecord.CurrentSchemaVersion,
            Libraries = [.. record.Libraries.Select(library => new LibraryRecordDocument
            {
                Id = library.Id.Value,
                SourceRoot = library.SourceRoot.Value,
                DestinationRoot = library.DestinationRoot.Value,
                Paths = [.. library.Paths.Select(path => path.Value)],
            })],
        };
        return JsonSerializer.SerializeToUtf8Bytes(
            document,
            LibrariesRecordJsonContext.Default.LibrariesRecordDocument);
    }

    private static void ValidateDocument(JsonElement root)
    {
        RequireObject(root, ["schemaVersion", "libraries"]);
        var schema = root.GetProperty("schemaVersion");
        if (schema.ValueKind != JsonValueKind.Number
            || !string.Equals(schema.GetRawText(), "1", StringComparison.Ordinal))
        {
            throw new JsonException("schemaVersion must be exactly the integer 1.");
        }

        var libraries = root.GetProperty("libraries");
        if (libraries.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("libraries must be an array.");
        }

        foreach (var library in libraries.EnumerateArray())
        {
            RequireObject(library, ["id", "sourceRoot", "destinationRoot", "paths"]);
            RequireString(library.GetProperty("id"));
            RequireString(library.GetProperty("sourceRoot"));
            RequireString(library.GetProperty("destinationRoot"));
            var paths = library.GetProperty("paths");
            if (paths.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("paths must be an array.");
            }

            foreach (var path in paths.EnumerateArray())
            {
                RequireString(path);
            }
        }
    }

    private static void RequireObject(JsonElement value, IReadOnlyList<string> expected)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("A Library record object has the wrong JSON kind.");
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            if (!expected.Contains(property.Name) || !names.Add(property.Name))
            {
                throw new JsonException("A Library record object has an unknown or duplicate property.");
            }
        }

        if (names.Count != expected.Count || expected.Any(name => !names.Contains(name)))
        {
            throw new JsonException("A Library record object is missing a required property.");
        }
    }

    private static void RequireString(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String || value.GetString() is null)
        {
            throw new JsonException("A Library record string property has the wrong JSON kind.");
        }
    }

    private static bool HasAmbiguousOwnership(LibrariesRecordDocument document)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var destinations = new HashSet<string>(StringComparer.Ordinal);
        foreach (var library in document.Libraries)
        {
            if (!ids.Add(library.Id))
            {
                return true;
            }

            foreach (var path in library.Paths)
            {
                var mapping = LibraryPathIdentity.Map(
                    WorkspaceRelativeDirectory.Create(library.SourceRoot),
                    LibraryDestinationRoot.Create(library.DestinationRoot),
                    SourceRelativeEligiblePath.Create(path));
                if (!destinations.Add(PortableWorkspacePath.CreatePortableKey(mapping.DestinationPath.Value)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasOutOfOrderMembers(LibrariesRecordDocument document)
    {
        string? previousId = null;
        foreach (var library in document.Libraries)
        {
            if (previousId is not null
                && string.CompareOrdinal(previousId, library.Id) > 0)
            {
                return true;
            }

            previousId = library.Id;
            string? previousPath = null;
            foreach (var path in library.Paths)
            {
                if (previousPath is not null
                    && string.CompareOrdinal(previousPath, path) > 0)
                {
                    return true;
                }

                previousPath = path;
            }
        }

        return false;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(LibrariesRecordDocument))]
internal sealed partial class LibrariesRecordJsonContext : JsonSerializerContext;
