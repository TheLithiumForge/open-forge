using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery.Serialization;

internal static class RecoveryBundleManifestCodec
{
    private const string OrdinaryCreateKind = "ordinary-create";
    private const string OrdinaryReplaceKind = "ordinary-replace";
    private const string OrdinaryDeleteKind = "ordinary-delete";
    private const string OrdinaryReplaceGeneratedRegionKind = "ordinary-replace-generated-region";
    private const string RelativeFileLinkCreateKind = "relative-file-link-create";
    private const string RelativeFileLinkDeleteKind = "relative-file-link-delete";
    private const string MissingStateKind = "missing";
    private const string OrdinaryFileStateKind = "ordinary-file";
    private const string RelativeFileLinkStateKind = "relative-file-link";
    private const string RelativeFileSymbolicLinkKind = "relative-file-symbolic-link";

    private static readonly FrozenDictionary<RecoveryEntryKind, string> KindWire =
        new Dictionary<RecoveryEntryKind, string>
        {
            [RecoveryEntryKind.OrdinaryCreate] = OrdinaryCreateKind,
            [RecoveryEntryKind.OrdinaryReplace] = OrdinaryReplaceKind,
            [RecoveryEntryKind.OrdinaryReplaceGeneratedRegion] = OrdinaryReplaceGeneratedRegionKind,
            [RecoveryEntryKind.OrdinaryDelete] = OrdinaryDeleteKind,
            [RecoveryEntryKind.RelativeFileLinkCreate] = RelativeFileLinkCreateKind,
            [RecoveryEntryKind.RelativeFileLinkDelete] = RelativeFileLinkDeleteKind,
        }.ToFrozenDictionary();
    private static readonly FrozenDictionary<string, RecoveryEntryKind> WireKind =
        KindWire.ToFrozenDictionary(static pair => pair.Value, static pair => pair.Key, StringComparer.Ordinal);

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
    private static readonly RecoveryBundleJsonContext SerializerContext = new(SerializerOptions);

    internal static byte[] Serialize(
        RecoveryBundleInput input,
        ImmutableArray<RecoveryEntry> entries)
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
        ImmutableArray<RecoveryEntry> entries)
    {
        if (entries.IsDefaultOrEmpty || entries.Length != input.RecoveryTargets.Length)
        {
            throw new ArgumentException(
                "A recovery manifest requires the complete target entry set.",
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
                LogicalPath = entry.LogicalPath.Value,
                Kind = Kind(entry.Kind),
                Prior = State(entry.Prior),
                Intended = State(entry.Intended),
                PriorPayload = entry.PriorPayload,
            };
        }

        var workspacePath = WorkspaceIdentity.NormalizePhysicalPath(
            input.Workspace.PhysicalRoot);
        return new RecoveryBundleManifestV1
        {
            SchemaVersion = RecoveryBundleFormatV1.SchemaVersion,
            Command = input.Command,
            OperationId = input.OperationId.ToString(RecoveryBundleFormatV1.OperationIdFormat),
            WorkspacePath = workspacePath,
            WorkspaceKey = WorkspaceIdentity.Key(workspacePath),
            Attribution = RecoveryBundleAttributionCodec.Serialize(input.Attribution),
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
            || string.IsNullOrWhiteSpace(document.WorkspaceKey)
            || !RecoveryBundleAttributionCodec.TryRead(
                document.Attribution,
                out var attribution)
            || attribution is null)
        {
            return Malformed("The recovery manifest operation, workspace identity, or attribution is invalid.");
        }

        var normalizedWorkspace = WorkspaceIdentity.NormalizePhysicalPath(
            document.WorkspacePath);
        if (!string.Equals(normalizedWorkspace, document.WorkspacePath, PathComparison())
            || !string.Equals(
                WorkspaceIdentity.Key(normalizedWorkspace),
                document.WorkspaceKey,
                StringComparison.Ordinal))
        {
            return Malformed("The recovery manifest workspace identity is invalid.");
        }

        if (!string.Equals(
            attribution.Subject.Identity,
            document.WorkspaceKey,
            StringComparison.Ordinal))
        {
            return Malformed("The recovery manifest subject does not identify its workspace.");
        }

        if (document.Entries is not { Length: > 0 })
        {
            return Malformed("The recovery manifest requires a non-empty target set.");
        }

        var builder = ImmutableArray.CreateBuilder<RecoveryEntry>(document.Entries.Length);
        var targetPaths = new HashSet<string>(PathComparer());
        for (var index = 0; index < document.Entries.Length; index++)
        {
            var serialized = document.Entries[index];
            if (serialized is null || serialized.Ordinal != index)
            {
                return Malformed("The recovery manifest target order is invalid.");
            }

            if (!TryKind(serialized.Kind, out var kind)
                || serialized.Prior is null
                || serialized.Intended is null)
            {
                return Malformed("The recovery manifest entry kind or state identity is invalid.");
            }

            if (!TryState(serialized.Prior, out var prior)
                || !TryState(serialized.Intended, out var intended))
            {
                return Malformed("The recovery manifest state identity is invalid.");
            }

            RecoveryEntry entry;
            try
            {
                entry = RecoveryEntry.Create(
                    ordinal: serialized.Ordinal,
                    logicalPath: CanonicalRelativePath.Create(serialized.LogicalPath),
                    kind: kind,
                    prior: prior,
                    intended: intended,
                    priorPayload: serialized.PriorPayload);
            }
            catch (ArgumentException exception)
            {
                return Malformed(exception.Message);
            }

            if (!targetPaths.Add(entry.TargetPath))
            {
                return Malformed("The recovery manifest target identity is duplicated.");
            }

            builder.Add(entry);
        }

        return new RecoveryBundleManifestDecodeResult
        {
            State = RecoveryBundleManifestState.Valid,
            Document = document,
            Attribution = attribution,
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

    private static string Kind(RecoveryEntryKind kind)
        => KindWire.TryGetValue(kind, out var wire)
            ? wire
            : throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The recovery entry kind is not defined.");

    private static RecoveryBundleManifestStateV1 State(RecoveryEntryState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.Kind switch
        {
            RecoveryEntryStateKind.Missing => new RecoveryBundleManifestStateV1
            {
                Kind = MissingStateKind,
                Length = null,
                Sha256 = null,
                LinkKind = null,
                RawRelativeTarget = null,
            },
            RecoveryEntryStateKind.OrdinaryFile when state.OrdinaryFile is { } ordinary =>
                new RecoveryBundleManifestStateV1
                {
                    Kind = OrdinaryFileStateKind,
                    Length = ordinary.Length,
                    Sha256 = ordinary.Sha256,
                    LinkKind = null,
                    RawRelativeTarget = null,
                },
            RecoveryEntryStateKind.RelativeFileLink when state.RelativeFileLink is { } link =>
                new RecoveryBundleManifestStateV1
                {
                    Kind = RelativeFileLinkStateKind,
                    Length = null,
                    Sha256 = null,
                    LinkKind = LinkKind(link.LinkKind),
                    RawRelativeTarget = link.RawRelativeTarget,
                },
            _ => throw new ArgumentException(
                "A recovery entry state does not carry its typed identity.",
                nameof(state)),
        };
    }

    private static bool TryKind(string? value, out RecoveryEntryKind kind)
    {
        kind = default;
        if (value is null)
        {
            return false;
        }

        return WireKind.TryGetValue(value, out kind);
    }

    private static bool TryState(
        RecoveryBundleManifestStateV1 serialized,
        [NotNullWhen(true)] out RecoveryEntryState? state)
    {
        state = null;
        if (serialized is null)
        {
            return false;
        }

        switch (serialized.Kind)
        {
            case MissingStateKind:
                if (serialized.Length is not null
                    || serialized.Sha256 is not null
                    || serialized.LinkKind is not null
                    || serialized.RawRelativeTarget is not null)
                {
                    return false;
                }

                state = RecoveryEntryState.Missing;
                return true;

            case OrdinaryFileStateKind:
                if (serialized.Length is not { } length
                    || length < 0
                    || serialized.Sha256 is null
                    || serialized.LinkKind is not null
                    || serialized.RawRelativeTarget is not null)
                {
                    return false;
                }

                try
                {
                    state = RecoveryEntryState.Ordinary(
                        RecoveryContentIdentity.Create(length, serialized.Sha256));
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }

            case RelativeFileLinkStateKind:
                if (serialized.Length is not null
                    || serialized.Sha256 is not null
                    || serialized.LinkKind != RelativeFileSymbolicLinkKind
                    || serialized.RawRelativeTarget is null)
                {
                    return false;
                }

                try
                {
                    state = RecoveryEntryState.RelativeLink(
                        RelativeFileLinkIdentity.Create(
                            NoFollowLinkKind.SymbolicLink,
                            serialized.RawRelativeTarget));
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }

            default:
                return false;
        }
    }

    private static string LinkKind(NoFollowLinkKind kind)
        => kind switch
        {
            NoFollowLinkKind.SymbolicLink => RelativeFileSymbolicLinkKind,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The recovery link kind is not defined."),
        };

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
