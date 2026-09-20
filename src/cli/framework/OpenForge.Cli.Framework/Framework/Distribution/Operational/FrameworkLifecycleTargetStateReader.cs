using OpenForge.Cli.Core.Framework.Filesystem.Shared.Reading;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Distribution.Operational;

internal sealed class FrameworkLifecycleTargetStateReader(
    ManagedTargetReader managedTargetReader)
{
    private readonly FrameworkContentIdentity _contentIdentity = new();

    internal async ValueTask<FrameworkManagedTargetDoctorRead> ReadAsync(
        CliWorkspace workspace,
        FrameworkManagedTargetIntent target,
        CancellationToken cancellationToken)
    {
        var read = await managedTargetReader.ReadAsync(workspace, target.Path, cancellationToken)
            .ConfigureAwait(false);
        if (read.State != ManagedTargetReadState.Available)
        {
            return FrameworkManagedTargetDoctorRead.Boundary(read.State, cancellationToken);
        }

        if (target.Source.State != FrameworkTargetSourceState.Valid)
        {
            return FrameworkManagedTargetDoctorRead.Unavailable(
                target.Source.Cause ?? "The intended Framework target is unavailable.");
        }

        try
        {
            var fingerprint = ReadFingerprint(target.Kind, read.Bytes.Span);
            var state = target.Kind != FrameworkManagedTargetKind.GeneratedRegion
                && string.Equals(fingerprint, target.IntendedFingerprint, StringComparison.Ordinal)
                ? OperationalTargetState.Current
                : OperationalTargetState.Changed;
            return FrameworkManagedTargetDoctorRead.Observed(state, fingerprint);
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException or InvalidDataException or InvalidOperationException)
        {
            return FrameworkManagedTargetDoctorRead.Blocked(exception.Message);
        }
    }

    internal string ReadFingerprint(FrameworkManagedTargetKind kind, ReadOnlySpan<byte> bytes)
    {
        if (kind == FrameworkManagedTargetKind.GeneratedRegion)
        {
            return _contentIdentity.ReadGeneratedEntriesFingerprint(bytes);
        }

        if (kind == FrameworkManagedTargetKind.ManagedRegion)
        {
            var block = _contentIdentity.ReadManagedBlock(bytes);
            if (block.State != FrameworkManagedBlockState.Present || block.ExistingBlockBytes is not { } content)
            {
                throw new InvalidDataException(block.Cause ?? "The Open Forge managed block is missing.");
            }

            return ReadSemanticFingerprint(content);
        }

        return ReadSemanticFingerprint(bytes);
    }

    private string ReadSemanticFingerprint(ReadOnlySpan<byte> bytes)
    {
        var facts = _contentIdentity.ReadSourceFingerprint(bytes);
        return facts.IsSemantic && facts.Sha256 is { } fingerprint
            ? fingerprint
            : throw new InvalidDataException(facts.Cause ?? "The Framework target has no readable semantic identity.");
    }
}
