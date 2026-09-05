using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed class FrameworkLifecycleTargetIdentity
{
    private readonly FrameworkContentIdentity _contentIdentity = new();

    internal FrameworkLifecycleTargetSourceValidation ValidateSource(
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generated,
        FrameworkPayload payload)
    {
        if (IsGenerated(target, generated))
        {
            var valid = target.SourceAssetPath is null
                && string.Equals(
                    target.Region,
                    LifecycleSchema.GeneratedEntriesRegion,
                    StringComparison.Ordinal)
                && string.Equals(
                    target.FingerprintKind,
                    LifecycleSchema.ExactBytesFingerprintKind,
                    StringComparison.Ordinal);
            return valid
                ? Valid()
                : Blocked(
                    "A generated lifecycle Framework target has unsupported provenance or fingerprint identity.");
        }

        if (target.SourceAssetPath is not { } sourceAssetPath
            || payload.Find(sourceAssetPath) is not { } asset)
        {
            return SourceMismatch(
                "A lifecycle Framework target does not belong to the running embedded inventory.");
        }

        string sourceFingerprint;
        try
        {
            sourceFingerprint = _contentIdentity.ReadSourceFingerprint(
                asset.Bytes.AsSpan(),
                target.FingerprintKind);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            return SourceMismatch(
                $"A lifecycle Framework source asset cannot establish its running identity: {exception.Message}");
        }

        return string.Equals(
            sourceFingerprint,
            target.BaselineFingerprint,
            StringComparison.Ordinal)
            ? Valid()
            : SourceMismatch(
                "A lifecycle Framework target baseline does not match its running embedded source asset.");
    }

    internal bool IsGenerated(
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generated)
        => generated.Contains((target.Path, target.Region));

    internal string ReadFingerprint(
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generated,
        ReadOnlySpan<byte> bytes)
    {
        if (IsGenerated(target, generated))
        {
            return _contentIdentity.ReadGeneratedEntriesFingerprint(bytes, target.FingerprintKind);
        }

        if (target.Path is FrameworkPayloadAsset.RootAgentPath
            or FrameworkPayloadAsset.RootClaudePath)
        {
            var block = _contentIdentity.ReadManagedBlock(bytes);
            if (block.State != FrameworkManagedBlockState.Present
                || block.ExistingBlockBytes is null)
            {
                throw new InvalidDataException(
                    block.Cause
                        ?? "The managed host requires one complete ordered Open Forge marker boundary.");
            }

            return _contentIdentity.ReadSourceFingerprint(
                block.ExistingBlockBytes,
                target.FingerprintKind);
        }

        if (target.Region is not null)
        {
            throw new InvalidDataException(
                "Only entries regions and root managed blocks are supported Framework target regions.");
        }

        return _contentIdentity.ReadSourceFingerprint(bytes, target.FingerprintKind);
    }

    private static FrameworkLifecycleTargetSourceValidation Valid()
        => new(FrameworkLifecycleTargetSourceState.Valid, null);

    private static FrameworkLifecycleTargetSourceValidation SourceMismatch(string cause)
        => new(FrameworkLifecycleTargetSourceState.SourceMismatch, cause);

    private static FrameworkLifecycleTargetSourceValidation Blocked(string cause)
        => new(FrameworkLifecycleTargetSourceState.Blocked, cause);
}
