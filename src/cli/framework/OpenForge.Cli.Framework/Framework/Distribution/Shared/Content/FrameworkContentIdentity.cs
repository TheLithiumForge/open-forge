using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Content;

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
            || document.GeneratedRegion.EntriesBlock?.Span is not { } content)
        {
            throw new InvalidDataException(
                "A generated navigation target requires one complete Entries region.");
        }

        return FileExpectation.Hash(StrictUtf8.GetBytes(
            document.Source[content.Start..content.End]));
    }

    internal MarkdownFingerprintFacts ReadSourceFingerprint(ReadOnlySpan<byte> bytes)
        => _fingerprintReader.Read(bytes);

    internal string ReadSourceFingerprint(
        ReadOnlySpan<byte> bytes,
        MarkdownFingerprintState fingerprintKind)
    {
        if (fingerprintKind == MarkdownFingerprintState.ExactBytes)
        {
            return FileExpectation.Hash(bytes);
        }
        if (fingerprintKind != MarkdownFingerprintState.Semantic)
        {
            throw new ArgumentOutOfRangeException(nameof(fingerprintKind), fingerprintKind, "A source comparison requires a semantic or exact-byte identity.");
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
