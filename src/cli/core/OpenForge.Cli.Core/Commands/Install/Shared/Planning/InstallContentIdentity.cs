using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallContentIdentity
{
    internal const string GeneratedRegionIdentity = "entries";

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly FrameworkContentIdentity _contentIdentity = new();

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

        var recognition = _contentIdentity.ReadManagedBlock(current);
        if (recognition.State == FrameworkManagedBlockState.Absent)
        {
            var separator = ReadManagedBlockSeparator(current);
            return ManagedBlockResolution.Absent(
                StrictUtf8.GetBytes(current + separator + block));
        }

        if (recognition.State == FrameworkManagedBlockState.Blocked)
        {
            return ManagedBlockResolution.Blocked(
                recognition.Cause
                    ?? "The managed host contains an unsupported Open Forge marker boundary.");
        }

        var start = recognition.Start
            ?? throw new InvalidOperationException(
                "A present managed block must establish its start.");
        var endExclusive = recognition.EndExclusive
            ?? throw new InvalidOperationException(
                "A present managed block must establish its end.");
        var existingBlock = recognition.ExistingBlockBytes
            ?? throw new InvalidOperationException(
                "A present managed block must retain its exact bytes.");
        var expectedDocument = StrictUtf8.GetBytes(string.Concat(
            current.AsSpan(0, start),
            block,
            current.AsSpan(endExclusive)));
        return ManagedBlockResolution.Present(existingBlock, expectedDocument);
    }

    internal string ReadGeneratedFingerprint(ReadOnlySpan<byte> bytes)
        => _contentIdentity.ReadGeneratedEntriesFingerprint(bytes);

    internal string ReadPersistedGeneratedFingerprint(
        ReadOnlySpan<byte> bytes,
        string fingerprintKind)
    {
        return _contentIdentity.ReadGeneratedEntriesFingerprint(bytes, fingerprintKind);
    }

    internal string ReadPersistedSourceFingerprint(
        ReadOnlySpan<byte> bytes,
        string fingerprintKind)
    {
        return _contentIdentity.ReadSourceFingerprint(bytes, fingerprintKind);
    }

    internal string ReadPersistedManagedBlockFingerprint(
        ReadOnlySpan<byte> bytes,
        string fingerprintKind)
    {
        var recognition = _contentIdentity.ReadManagedBlock(bytes);
        if (recognition.State != FrameworkManagedBlockState.Present
            || recognition.ExistingBlockBytes is null)
        {
            throw new InvalidDataException(
                recognition.Cause
                    ?? "A persisted managed-block target requires one complete Open Forge boundary.");
        }

        return ReadPersistedSourceFingerprint(
            recognition.ExistingBlockBytes,
            fingerprintKind);
    }

    internal string ReadSourceFingerprint(ReadOnlySpan<byte> bytes)
    {
        var facts = _contentIdentity.ReadSourceFingerprint(bytes);
        return facts.Sha256
            ?? throw new InvalidDataException(
                "A Framework source target requires a readable fingerprint.");
    }

    private FrameworkLifecycleTarget CreateSourceTarget(
        string path,
        string sourceAssetPath,
        ReadOnlySpan<byte> bytes)
    {
        var fingerprint = _contentIdentity.ReadSourceFingerprint(bytes);
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
