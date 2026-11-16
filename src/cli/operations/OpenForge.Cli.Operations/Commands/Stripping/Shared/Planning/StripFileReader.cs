using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Stripping.Shared.Planning;

/// <summary>Reads one contained workspace file as an exact snapshot for planning and recovery.</summary>
internal sealed class StripFileReader(PhysicalPathResolver resolver)
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    internal string LexicalPath(CliWorkspace workspace, string path)
        => SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path);

    internal async ValueTask<FileStateSnapshot?> ReadAsync(CliWorkspace workspace, string path, CancellationToken token)
    {
        var lexical = LexicalPath(workspace, path);
        var resolution = resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, lexical);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return FileStateSnapshot.Missing(lexical);
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return null;
        }

        var physical = resolution.GetContainedPhysicalPath();
        if (Directory.Exists(physical))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(physical, token).ConfigureAwait(false);
        return FileStateSnapshot.File(lexical, physical, bytes);
    }

    internal FileExpectation? ReadDirectory(CliWorkspace workspace, string path)
    {
        var lexical = LexicalPath(workspace, path);
        var resolution = resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, lexical);
        return resolution.State == PhysicalPathState.Contained && Directory.Exists(resolution.GetContainedPhysicalPath())
            ? FileExpectation.Directory(lexical, resolution.GetContainedPhysicalPath())
            : null;
    }

    internal static bool TryDecode(FileStateSnapshot snapshot, out string text)
    {
        try
        {
            text = StrictUtf8.GetString(snapshot.Bytes.AsSpan());
            return true;
        }
        catch (DecoderFallbackException)
        {
            text = string.Empty;
            return false;
        }
    }
}
