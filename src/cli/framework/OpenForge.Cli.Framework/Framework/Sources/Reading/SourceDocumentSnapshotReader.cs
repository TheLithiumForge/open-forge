using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Reading;

internal sealed class SourceDocumentSnapshotReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal ValueTask<FileStateSnapshot> ReadAsync(
        CliWorkspace workspace,
        SourceDocumentReadResult read,
        CancellationToken cancellationToken)
        => ReadAsync(workspace, read.Verification, cancellationToken);

    internal async ValueTask<FileStateSnapshot> ReadAsync(
        CliWorkspace workspace,
        SourceLayerVerification verification,
        CancellationToken cancellationToken)
    {
        if (verification.State != SourceLayerVerificationState.Verified
            || verification.CurrentPhysicalPath is not { } physicalPath)
        {
            throw new ArgumentException(
                "An exact source snapshot requires a verified current physical path.",
                nameof(verification));
        }

        var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
            .ConfigureAwait(false);
        _ = StrictUtf8.GetString(bytes);
        return FileStateSnapshot.File(
            SourceLogicalPath.ToLexicalPath(
                workspace.LexicalRoot,
                verification.Layer.CanonicalPath),
            physicalPath,
            bytes);
    }
}
