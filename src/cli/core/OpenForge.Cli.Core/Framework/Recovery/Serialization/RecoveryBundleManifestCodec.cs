using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Serialization;

internal static class RecoveryBundleManifestCodec
{
    private const string ReplaceKind = "replace";
    private const string DeleteKind = "delete";
    private const string ReplaceGeneratedRegionKind = "replace-generated-region";

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
    private static readonly RecoveryBundleJsonContext SerializerContext = new(SerializerOptions);

    internal static byte[] Serialize(
        RecoveryBundleInput input,
        ImmutableArray<RecoveryBundleEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Serialize(CreateDocument(input, entries));
    }

    internal static byte[] Serialize(RecoveryBundleManifestV1 document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return JsonSerializer.SerializeToUtf8Bytes(
            document,
            SerializerContext.RecoveryBundleManifestV1);
    }

    internal static RecoveryBundleManifestDecodeResult Decode(ReadOnlySpan<byte> bytes)
    {
        try
        {
            var document = JsonSerializer.Deserialize(
                bytes,
                SerializerContext.RecoveryBundleManifestV1);
            return Validate(document);
        }
        catch (JsonException exception)
        {
            return Malformed(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return Malformed(exception.Message);
        }
    }

    internal static async ValueTask<RecoveryBundleManifestDecodeResult> DecodeAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        try
        {
            var document = await JsonSerializer.DeserializeAsync(
                stream,
                SerializerContext.RecoveryBundleManifestV1,
                cancellationToken).ConfigureAwait(false);
            return Validate(document);
        }
        catch (JsonException exception)
        {
            return Malformed(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return Malformed(exception.Message);
        }
    }

    internal static bool TryParseOperationId(
        RecoveryBundleManifestV1 document,
        out Guid operationId)
    {
        operationId = Guid.Empty;
        return Guid.TryParseExact(
                document.OperationId,
                RecoveryBundleFormatV1.OperationIdFormat,
                out operationId)
            && string.Equals(
                document.OperationId,
                operationId.ToString(RecoveryBundleFormatV1.OperationIdFormat),
                StringComparison.Ordinal);
    }

    private static RecoveryBundleManifestV1 CreateDocument(
        RecoveryBundleInput input,
        ImmutableArray<RecoveryBundleEntry> entries)
    {
        if (entries.IsDefaultOrEmpty || entries.Length != input.RecoveryTargets.Length)
        {
            throw new ArgumentException(
                "A recovery manifest requires the complete existing-target entry set.",
                nameof(entries));
        }

        var serializedEntries = new RecoveryBundleManifestEntryV1[entries.Length];
        for (var index = 0; index < entries.Length; index++)
        {
            var entry = entries[index];
            if (entry.Ordinal != index)
            {
                throw new ArgumentException(
                    "Recovery manifest entries must retain exact ordinal order.",
                    nameof(entries));
            }

            serializedEntries[index] = new RecoveryBundleManifestEntryV1
            {
                Ordinal = entry.Ordinal,
                Target = entry.TargetPath,
                Kind = Kind(entry.ChangeKind),
                PriorLength = entry.Prior.Length,
                PriorSha256 = entry.Prior.Sha256,
                Payload = entry.PayloadName,
                IntendedAbsent = entry.IntendedAbsent,
                IntendedLength = entry.Intended?.Length,
                IntendedSha256 = entry.Intended?.Sha256,
            };
        }

        var workspacePath = RecoveryBundlePathIdentity.NormalizeWorkspacePath(
            input.Workspace.PhysicalRoot);
        return new RecoveryBundleManifestV1
        {
            SchemaVersion = RecoveryBundleFormatV1.SchemaVersion,
            Command = input.Command,
            OperationId = input.OperationId.ToString(RecoveryBundleFormatV1.OperationIdFormat),
            WorkspacePath = workspacePath,
            WorkspaceKey = RecoveryBundlePathIdentity.WorkspaceKey(workspacePath),
            Entries = serializedEntries,
        };
    }

    private static RecoveryBundleManifestDecodeResult Validate(RecoveryBundleManifestV1? document)
    {
        if (document is null)
        {
            return Malformed("The recovery manifest did not contain a JSON document.");
        }

        if (document.SchemaVersion != RecoveryBundleFormatV1.SchemaVersion)
        {
            return new RecoveryBundleManifestDecodeResult
            {
                State = RecoveryBundleManifestState.Unsupported,
                Entries = [],
                Cause = "The recovery manifest schema version is unsupported.",
            };
        }

        if (string.IsNullOrWhiteSpace(document.Command)
            || !TryParseOperationId(document, out _)
            || string.IsNullOrWhiteSpace(document.WorkspacePath)
            || string.IsNullOrWhiteSpace(document.WorkspaceKey))
        {
            return Malformed("The recovery manifest operation or workspace identity is invalid.");
        }

        var normalizedWorkspace = RecoveryBundlePathIdentity.NormalizeWorkspacePath(
            document.WorkspacePath);
        if (!string.Equals(normalizedWorkspace, document.WorkspacePath, PathComparison())
            || !string.Equals(
                RecoveryBundlePathIdentity.WorkspaceKey(normalizedWorkspace),
                document.WorkspaceKey,
                StringComparison.Ordinal))
        {
            return Malformed("The recovery manifest workspace identity is invalid.");
        }

        if (document.Entries is not { Length: > 0 })
        {
            return Malformed("The recovery manifest requires a non-empty target set.");
        }

        var builder = ImmutableArray.CreateBuilder<RecoveryBundleEntry>(document.Entries.Length);
        var targetPaths = new HashSet<string>(PathComparer());
        for (var index = 0; index < document.Entries.Length; index++)
        {
            var serialized = document.Entries[index];
            if (serialized is null || serialized.Ordinal != index)
            {
                return Malformed("The recovery manifest target order is invalid.");
            }

            if (!TryKind(serialized.Kind, out var kind))
            {
                return Malformed("The recovery manifest change kind is invalid.");
            }

            var prior = RecoveryContentIdentity.Create(
                serialized.PriorLength,
                serialized.PriorSha256);
            RecoveryContentIdentity? intended = null;
            if (serialized.IntendedAbsent)
            {
                if (serialized.IntendedLength is not null
                    || serialized.IntendedSha256 is not null)
                {
                    return Malformed(
                        "An absent intended recovery identity cannot carry content facts.");
                }
            }
            else
            {
                if (serialized.IntendedLength is not { } intendedLength
                    || serialized.IntendedSha256 is null)
                {
                    return Malformed("A replacement recovery entry requires intended content identity.");
                }

                intended = RecoveryContentIdentity.Create(
                    intendedLength,
                    serialized.IntendedSha256);
            }

            var entry = RecoveryBundleEntry.Create(
                ordinal: serialized.Ordinal,
                targetPath: serialized.Target,
                changeKind: kind,
                prior: prior,
                intended: intended);
            if (!string.Equals(entry.PayloadName, serialized.Payload, StringComparison.Ordinal)
                || !targetPaths.Add(entry.TargetPath))
            {
                return Malformed("The recovery manifest payload or target identity is invalid.");
            }

            builder.Add(entry);
        }

        return new RecoveryBundleManifestDecodeResult
        {
            State = RecoveryBundleManifestState.Valid,
            Document = document,
            Entries = builder.MoveToImmutable(),
        };
    }

    private static RecoveryBundleManifestDecodeResult Malformed(string cause)
        => new()
        {
            State = RecoveryBundleManifestState.Malformed,
            Entries = [],
            Cause = cause,
        };

    private static string Kind(PlannedFileChangeKind kind)
        => kind switch
        {
            PlannedFileChangeKind.Replace => ReplaceKind,
            PlannedFileChangeKind.Delete => DeleteKind,
            PlannedFileChangeKind.ReplaceGeneratedRegion => ReplaceGeneratedRegionKind,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The recovery change kind is not defined."),
        };

    private static bool TryKind(string value, out PlannedFileChangeKind kind)
    {
        kind = value switch
        {
            ReplaceKind => PlannedFileChangeKind.Replace,
            DeleteKind => PlannedFileChangeKind.Delete,
            ReplaceGeneratedRegionKind => PlannedFileChangeKind.ReplaceGeneratedRegion,
            _ => default,
        };
        return value is ReplaceKind or DeleteKind or ReplaceGeneratedRegionKind;
    }

    private static JsonSerializerOptions CreateSerializerOptions()
        => new()
        {
            AllowDuplicateProperties = false,
            AllowTrailingCommas = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            NumberHandling = JsonNumberHandling.Strict,
            PropertyNameCaseInsensitive = false,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            WriteIndented = false,
        };

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static StringComparer PathComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}
