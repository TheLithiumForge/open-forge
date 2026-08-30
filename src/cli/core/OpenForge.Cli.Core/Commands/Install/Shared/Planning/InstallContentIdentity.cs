using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallContentIdentity
{
    internal const string GeneratedRegionIdentity = "entries";

    private const string ManagedStart = "<!-- open-forge:start -->";
    private const string ManagedEnd = "<!-- open-forge:end -->";

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly MarkdownFingerprintReader _fingerprintReader = new();
    private readonly MarkdownDocumentParser _documentParser = new();

    internal FrameworkLifecycleState CreateLifecycle(
        FrameworkPayload payload,
        InstallIntendedState intended,
        FrameworkLifecycleState? current)
    {
        var baseTargets = new List<FrameworkLifecycleTarget>();
        foreach (var target in intended.TargetBytes.OrderBy(
                     value => value.Key,
                     StringComparer.Ordinal))
        {
            baseTargets.Add(CreateSourceTarget(target.Key, target.Key, target.Value));
            if (intended.GeneratedRegionPaths.Contains(target.Key))
            {
                baseTargets.Add(CreateGeneratedTarget(target.Key, target.Value));
            }
        }

        foreach (var block in intended.ManagedBlockBytes.OrderBy(
                     value => value.Key,
                     StringComparer.Ordinal))
        {
            baseTargets.Add(CreateSourceTarget(block.Key, block.Key, block.Value));
        }

        var baseKeys = baseTargets
            .Select(target => (target.Path, target.Region))
            .ToHashSet();
        var preservedTargets = current?.Targets
            .Where(target => !baseKeys.Contains((target.Path, target.Region)))
            ?? [];
        var generated = intended.GeneratedRegionPaths
            .Select(path => new FrameworkGeneratedRegion
            {
                Path = path,
                Region = GeneratedRegionIdentity,
            })
            .ToList();
        var generatedKeys = generated
            .Select(region => (region.Path, region.Region))
            .ToHashSet();
        if (current is not null)
        {
            generated.AddRange(current.GeneratedRegions.Where(region =>
                !generatedKeys.Contains((region.Path, region.Region))));
        }

        return new FrameworkLifecycleState
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Source = new FrameworkLifecycleSource
            {
                Id = "embedded-framework",
                Version = null,
                InventoryFingerprint = payload.InventoryFingerprint,
            },
            Targets = baseTargets
                .Concat(preservedTargets)
                .OrderBy(target => target.Path, StringComparer.Ordinal)
                .ThenBy(target => target.Region, StringComparer.Ordinal)
                .ToArray(),
            GeneratedRegions = generated
                .OrderBy(region => region.Path, StringComparer.Ordinal)
                .ThenBy(region => region.Region, StringComparer.Ordinal)
                .ToArray(),
        };
    }

    internal bool IsCurrentBaseExact(
        FrameworkLifecycleState current,
        FrameworkLifecycleState intended,
        IReadOnlyDictionary<string, InstallTargetRead> currentTargets,
        InstallIntendedState intendedState)
    {
        if (!string.Equals(current.Coverage, LifecycleSchema.CompleteCoverage, StringComparison.Ordinal)
            || !string.Equals(current.Source.Id, intended.Source.Id, StringComparison.Ordinal)
            || current.Source.Version is not null
            || !string.Equals(
                current.Source.InventoryFingerprint,
                intended.Source.InventoryFingerprint,
                StringComparison.Ordinal))
        {
            return false;
        }

        var currentByKey = current.Targets.ToDictionary(
            target => (target.Path, target.Region));
        foreach (var expected in ReadBaseTargets(intended, intendedState))
        {
            if (!currentByKey.TryGetValue((expected.Path, expected.Region), out var recorded)
                || !TargetFactsEqual(recorded, expected)
                || !currentTargets.TryGetValue(expected.Path, out var read)
                || read.State != InstallTargetReadState.File
                || read.Snapshot is not { HasBytes: true } snapshot
                || !ReadCurrentFingerprint(expected, snapshot.Bytes.AsSpan(), intendedState)
                    .Equals(expected.BaselineFingerprint, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    internal ManagedBlockResolution ResolveManagedBlock(
        ReadOnlySpan<byte> currentBytes,
        ReadOnlySpan<byte> intendedBlock)
    {
        string current;
        string block;
        try
        {
            current = StrictUtf8.GetString(currentBytes);
            block = StrictUtf8.GetString(intendedBlock);
        }
        catch (DecoderFallbackException exception)
        {
            return ManagedBlockResolution.Blocked(
                $"The managed host is not valid UTF-8: {exception.Message}");
        }

        var start = current.IndexOf(ManagedStart, StringComparison.Ordinal);
        var lastStart = current.LastIndexOf(ManagedStart, StringComparison.Ordinal);
        var end = current.IndexOf(ManagedEnd, StringComparison.Ordinal);
        var lastEnd = current.LastIndexOf(ManagedEnd, StringComparison.Ordinal);
        if (start < 0 && end < 0)
        {
            var separator = ReadManagedBlockSeparator(current);
            return ManagedBlockResolution.Absent(
                StrictUtf8.GetBytes(current + separator + block));
        }

        if (start < 0
            || end < 0
            || start != lastStart
            || end != lastEnd
            || end < start)
        {
            return ManagedBlockResolution.Blocked(
                "The managed host contains an incomplete, duplicate, or reversed Open Forge marker boundary.");
        }

        var endExclusive = checked(end + ManagedEnd.Length);
        if (endExclusive < current.Length && current[endExclusive] == '\r')
        {
            endExclusive++;
        }

        if (endExclusive < current.Length && current[endExclusive] == '\n')
        {
            endExclusive++;
        }

        var existingBlock = StrictUtf8.GetBytes(current[start..endExclusive]);
        var expectedDocument = StrictUtf8.GetBytes(string.Concat(
            current.AsSpan(0, start),
            block,
            current.AsSpan(endExclusive)));
        return ManagedBlockResolution.Present(existingBlock, expectedDocument);
    }

    internal string ReadGeneratedFingerprint(ReadOnlySpan<byte> bytes)
    {
        var document = _documentParser.Parse(StrictUtf8.GetString(bytes));
        if (document.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete
            || document.GeneratedRegion.ContentSpan is not { } content)
        {
            throw new InvalidDataException(
                "A generated lifecycle target requires one complete Entries region.");
        }

        return FileExpectation.Hash(StrictUtf8.GetBytes(
            document.Source[content.Start..content.End]));
    }

    internal string ReadPersistedGeneratedFingerprint(
        ReadOnlySpan<byte> bytes,
        string fingerprintKind)
    {
        if (!string.Equals(
                fingerprintKind,
                LifecycleSchema.ExactBytesFingerprintKind,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "A derived Entries target requires exact-bytes fingerprint identity.");
        }

        return ReadGeneratedFingerprint(bytes);
    }

    internal string ReadPersistedSourceFingerprint(
        ReadOnlySpan<byte> bytes,
        string fingerprintKind)
    {
        if (string.Equals(
                fingerprintKind,
                LifecycleSchema.ExactBytesFingerprintKind,
                StringComparison.Ordinal))
        {
            return FileExpectation.Hash(bytes);
        }

        if (!string.Equals(
                fingerprintKind,
                LifecycleSchema.SemanticFingerprintKind,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "The persisted Framework fingerprint kind is unsupported.");
        }

        var facts = _fingerprintReader.Read(bytes);
        if (!facts.IsSemantic || facts.Sha256 is null)
        {
            throw new InvalidDataException(
                facts.Cause
                    ?? "A semantic Framework target is no longer safely parseable.");
        }

        return facts.Sha256;
    }

    internal string ReadPersistedManagedBlockFingerprint(
        ReadOnlySpan<byte> bytes,
        string fingerprintKind)
    {
        var resolution = ResolveManagedBlock(bytes, ReadOnlySpan<byte>.Empty);
        if (resolution.State != ManagedBlockState.Present
            || resolution.ExistingBlockBytes is null)
        {
            throw new InvalidDataException(
                resolution.Cause
                    ?? "A persisted managed-block target requires one complete Open Forge boundary.");
        }

        return ReadPersistedSourceFingerprint(
            resolution.ExistingBlockBytes,
            fingerprintKind);
    }

    internal string ReadSourceFingerprint(ReadOnlySpan<byte> bytes)
    {
        var facts = _fingerprintReader.Read(bytes);
        return facts.Sha256
            ?? throw new InvalidDataException(
                "A Framework source target requires a readable fingerprint.");
    }

    private FrameworkLifecycleTarget CreateSourceTarget(
        string path,
        string sourceAssetPath,
        ReadOnlySpan<byte> bytes)
    {
        var fingerprint = _fingerprintReader.Read(bytes);
        return new FrameworkLifecycleTarget
        {
            Path = path,
            SourceAssetPath = sourceAssetPath,
            Region = null,
            BaselineFingerprint = fingerprint.Sha256
                ?? throw new InvalidDataException(
                    "A Framework source target requires a readable fingerprint."),
            FingerprintKind = fingerprint.IsSemantic
                ? LifecycleSchema.SemanticFingerprintKind
                : LifecycleSchema.ExactBytesFingerprintKind,
        };
    }

    private FrameworkLifecycleTarget CreateGeneratedTarget(
        string path,
        ReadOnlySpan<byte> bytes)
        => new()
        {
            Path = path,
            SourceAssetPath = null,
            Region = GeneratedRegionIdentity,
            BaselineFingerprint = ReadGeneratedFingerprint(bytes),
            FingerprintKind = LifecycleSchema.ExactBytesFingerprintKind,
        };

    private static IEnumerable<FrameworkLifecycleTarget> ReadBaseTargets(
        FrameworkLifecycleState intended,
        InstallIntendedState intendedState)
    {
        var basePaths = intendedState.TargetBytes.Keys
            .Concat(intendedState.ManagedBlockBytes.Keys)
            .ToHashSet(StringComparer.Ordinal);
        return intended.Targets.Where(target => basePaths.Contains(target.Path));
    }

    private string ReadCurrentFingerprint(
        FrameworkLifecycleTarget target,
        ReadOnlySpan<byte> currentBytes,
        InstallIntendedState intendedState)
    {
        if (target.Region == GeneratedRegionIdentity)
        {
            return ReadPersistedGeneratedFingerprint(
                currentBytes,
                target.FingerprintKind);
        }

        if (intendedState.ManagedBlockBytes.TryGetValue(
                target.Path,
                out var intendedBlock))
        {
            var resolution = ResolveManagedBlock(currentBytes, intendedBlock);
            if (resolution.State != ManagedBlockState.Present
                || resolution.ExistingBlockBytes is null)
            {
                return string.Empty;
            }

            return ReadPersistedSourceFingerprint(
                resolution.ExistingBlockBytes,
                target.FingerprintKind);
        }

        return ReadPersistedSourceFingerprint(
            currentBytes,
            target.FingerprintKind);
    }

    private static bool TargetFactsEqual(
        FrameworkLifecycleTarget left,
        FrameworkLifecycleTarget right)
        => string.Equals(left.Path, right.Path, StringComparison.Ordinal)
            && string.Equals(left.SourceAssetPath, right.SourceAssetPath, StringComparison.Ordinal)
            && string.Equals(left.Region, right.Region, StringComparison.Ordinal)
            && string.Equals(left.BaselineFingerprint, right.BaselineFingerprint, StringComparison.Ordinal)
            && string.Equals(left.FingerprintKind, right.FingerprintKind, StringComparison.Ordinal);

    private static string ReadManagedBlockSeparator(string current)
    {
        if (current.Length == 0
            || current.EndsWith("\n\n", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return current.EndsWith('\n') ? "\n" : "\n\n";
    }
}

internal enum ManagedBlockState
{
    Absent,
    Present,
    Blocked,
}

internal sealed record ManagedBlockResolution
{
    public required ManagedBlockState State { get; init; }

    public required byte[]? ExistingBlockBytes { get; init; }

    public required byte[]? IntendedDocumentBytes { get; init; }

    public required string? Cause { get; init; }

    internal static ManagedBlockResolution Absent(byte[] intendedDocumentBytes)
        => new()
        {
            State = ManagedBlockState.Absent,
            ExistingBlockBytes = null,
            IntendedDocumentBytes = intendedDocumentBytes,
            Cause = null,
        };

    internal static ManagedBlockResolution Present(
        byte[] existingBlockBytes,
        byte[] intendedDocumentBytes)
        => new()
        {
            State = ManagedBlockState.Present,
            ExistingBlockBytes = existingBlockBytes,
            IntendedDocumentBytes = intendedDocumentBytes,
            Cause = null,
        };

    internal static ManagedBlockResolution Blocked(string cause)
        => new()
        {
            State = ManagedBlockState.Blocked,
            ExistingBlockBytes = null,
            IntendedDocumentBytes = null,
            Cause = cause,
        };
}
