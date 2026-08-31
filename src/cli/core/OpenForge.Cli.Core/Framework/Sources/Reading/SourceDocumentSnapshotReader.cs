using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Sources.Reading;

internal sealed class SourceDocumentSnapshotReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal async ValueTask<FileStateSnapshot> ReadAsync(
        CliWorkspace workspace,
        SourceDocumentReadResult read,
        CancellationToken cancellationToken)
    {
        if (read.Verification.State != SourceLayerVerificationState.Verified
            || read.Verification.CurrentPhysicalPath is not { } physicalPath)
        {
            throw new ArgumentException(
                "An exact source snapshot requires a verified current physical path.",
                nameof(read));
        }

        var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
            .ConfigureAwait(false);
        _ = StrictUtf8.GetString(bytes);
        return FileStateSnapshot.File(
            SourceLogicalPath.ToLexicalPath(
                workspace.LexicalRoot,
                read.Layer.CanonicalPath),
            physicalPath,
            bytes);
    }
}
