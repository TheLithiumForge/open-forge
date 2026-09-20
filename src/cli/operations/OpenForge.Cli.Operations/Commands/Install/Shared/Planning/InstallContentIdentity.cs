using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;


namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallContentIdentity
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly FrameworkContentIdentity _contentIdentity = new();

    internal bool IsCurrentBaseExact(
        IReadOnlyDictionary<string, InstallTargetRead> currentTargets,
        InstallIntendedState intended)
    {
        foreach (var target in intended.TargetBytes)
        {
            if (!currentTargets.TryGetValue(target.Key, out var read)
                || read.State != InstallTargetReadState.File
                || read.Snapshot is not { HasBytes: true } snapshot
                || !string.Equals(ReadSourceFingerprint(snapshot.Bytes.AsSpan()),
                    ReadSourceFingerprint(target.Value), StringComparison.Ordinal)
                || (intended.GeneratedRegionPaths.Contains(target.Key)
                    && !string.Equals(ReadGeneratedFingerprint(snapshot.Bytes.AsSpan()),
                        ReadGeneratedFingerprint(target.Value), StringComparison.Ordinal)))
            {
                return false;
            }
        }

        foreach (var block in intended.ManagedBlockBytes)
        {
            if (!currentTargets.TryGetValue(block.Key, out var read)
                || read.State != InstallTargetReadState.File
                || read.Snapshot is not { HasBytes: true } snapshot)
            {
                return false;
            }

            var resolution = ResolveManagedBlock(snapshot.Bytes.AsSpan(), block.Value);
            if (resolution.State != ManagedBlockState.Present
                || resolution.ExistingBlockBytes is not { } bytes
                || !string.Equals(ReadSourceFingerprint(bytes),
                    ReadSourceFingerprint(block.Value), StringComparison.Ordinal))
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

    internal string ReadSourceFingerprint(ReadOnlySpan<byte> bytes)
    {
        var facts = _contentIdentity.ReadSourceFingerprint(bytes);
        return facts.Sha256
            ?? throw new InvalidDataException(
                "A Framework source target requires a readable fingerprint.");
    }

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
