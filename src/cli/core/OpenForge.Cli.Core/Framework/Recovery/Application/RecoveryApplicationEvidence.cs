using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery.Application;

internal static class RecoveryApplicationEvidence
{
    internal static async ValueTask<RecoveryBundleReadResult> ReadExactFinalAsync(
        CliWorkspace workspace,
        RecoveryBundlePreparation preparation,
        CancellationToken cancellationToken)
    {
        var read = await RecoveryBundleReader.ReadFinalAsync(
            workspace,
            preparation.BundlePath,
            cancellationToken).ConfigureAwait(false);
        if (read.Verified is null || Matches(preparation, read.Verified))
        {
            return read;
        }

        return RecoveryBundleReadResult.Classified(
            RecoveryBundleReadState.Malformed,
            "The recovery final no longer matches the verified preparation.");
    }

    internal static async ValueTask<byte[]> ReadPriorPayloadAsync(
        RecoveryBundlePreparation preparation,
        RecoveryEntry entry,
        CancellationToken cancellationToken)
    {
        var payloadName = entry.PriorPayload
            ?? throw new ArgumentException(
                "An ordinary prior state requires a recovery payload.",
                nameof(entry));
        var identity = entry.Prior.OrdinaryFile
            ?? throw new ArgumentException(
                "A recovery payload requires an ordinary prior identity.",
                nameof(entry));
        RecoveryBundleStorage.ValidateOrdinaryFile(preparation.BundlePath);
        await using var stream = RecoveryBundleStorage.OpenReadFile(preparation.BundlePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        var payload = archive.GetEntry(payloadName)
            ?? throw new InvalidDataException("The recovery payload is missing.");
        if (payload.Length != identity.Length || identity.Length > int.MaxValue)
        {
            throw new InvalidDataException("The recovery payload length is invalid.");
        }

        await using var payloadStream = payload.Open();
        using var buffer = new MemoryStream((int)identity.Length);
        await payloadStream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        var bytes = buffer.ToArray();
        if (!identity.Matches(bytes))
        {
            throw new InvalidDataException("The recovery payload identity is invalid.");
        }

        return bytes;
    }

    private static bool Matches(
        RecoveryBundlePreparation preparation,
        RecoveryBundleVerifiedRead verified)
        => string.Equals(preparation.BundlePath, verified.BundlePath, PathComparison())
            && string.Equals(
                preparation.WorkspacePhysicalPath,
                verified.WorkspacePhysicalPath,
                PathComparison())
            && string.Equals(preparation.WorkspaceKey, verified.WorkspaceKey, StringComparison.Ordinal)
            && string.Equals(preparation.Command, verified.Command, StringComparison.Ordinal)
            && preparation.Attribution == verified.Attribution
            && preparation.OperationId == verified.OperationId
            && preparation.Entries.SequenceEqual(verified.Entries);

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}
