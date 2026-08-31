using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed class FrameworkContentIdentity
{
    private const string ManagedStart = "<!-- open-forge:start -->";
    private const string ManagedEnd = "<!-- open-forge:end -->";

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly MarkdownDocumentParser _documentParser = new();
    private readonly MarkdownFingerprintReader _fingerprintReader = new();

    internal FrameworkManagedBlockRecognition ReadManagedBlock(
        ReadOnlySpan<byte> bytes)
    {
        try
        {
            return ReadManagedBlock(StrictUtf8.GetString(bytes));
        }
        catch (DecoderFallbackException exception)
        {
            return FrameworkManagedBlockRecognition.Blocked(
                $"The managed host is not valid UTF-8: {exception.Message}");
        }
    }

    internal FrameworkManagedBlockRecognition ReadManagedBlock(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var start = source.IndexOf(ManagedStart, StringComparison.Ordinal);
        var lastStart = source.LastIndexOf(ManagedStart, StringComparison.Ordinal);
        var end = source.IndexOf(ManagedEnd, StringComparison.Ordinal);
        var lastEnd = source.LastIndexOf(ManagedEnd, StringComparison.Ordinal);
        if (start < 0 && end < 0)
        {
            return FrameworkManagedBlockRecognition.Absent();
        }

        if (start < 0
            || end < 0
            || start != lastStart
            || end != lastEnd
            || end < start)
        {
            return FrameworkManagedBlockRecognition.Blocked(
                "The managed host contains an incomplete, duplicate, or reversed Open Forge marker boundary.");
        }

        var endExclusive = checked(end + ManagedEnd.Length);
        if (endExclusive < source.Length && source[endExclusive] == '\r')
        {
            endExclusive++;
        }

        if (endExclusive < source.Length && source[endExclusive] == '\n')
        {
            endExclusive++;
        }

        return FrameworkManagedBlockRecognition.Present(
            start,
            endExclusive,
            StrictUtf8.GetBytes(source[start..endExclusive]));
    }

    internal string ReadGeneratedEntriesFingerprint(ReadOnlySpan<byte> bytes)
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

    internal string ReadGeneratedEntriesFingerprint(
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

        return ReadGeneratedEntriesFingerprint(bytes);
    }

    internal MarkdownFingerprintFacts ReadSourceFingerprint(ReadOnlySpan<byte> bytes)
        => _fingerprintReader.Read(bytes);

    internal string ReadSourceFingerprint(
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
}

internal enum FrameworkManagedBlockState
{
    Absent,
    Present,
    Blocked,
}

internal sealed record FrameworkManagedBlockRecognition
{
    public required FrameworkManagedBlockState State { get; init; }

    public required int? Start { get; init; }

    public required int? EndExclusive { get; init; }

    public required byte[]? ExistingBlockBytes { get; init; }

    public required string? Cause { get; init; }

    internal static FrameworkManagedBlockRecognition Absent()
        => new()
        {
            State = FrameworkManagedBlockState.Absent,
            Start = null,
            EndExclusive = null,
            ExistingBlockBytes = null,
            Cause = null,
        };

    internal static FrameworkManagedBlockRecognition Present(
        int start,
        int endExclusive,
        byte[] existingBlockBytes)
        => new()
        {
            State = FrameworkManagedBlockState.Present,
            Start = start,
            EndExclusive = endExclusive,
            ExistingBlockBytes = existingBlockBytes,
            Cause = null,
        };

    internal static FrameworkManagedBlockRecognition Blocked(string cause)
        => new()
        {
            State = FrameworkManagedBlockState.Blocked,
            Start = null,
            EndExclusive = null,
            ExistingBlockBytes = null,
            Cause = cause,
        };
}
