using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

internal static class ToolRegistrationFiles
{
    internal static async ValueTask<byte[]?> ReadAsync(string root, string file, CancellationToken token)
    {
        if (!ToolIdentity.IsRelativePath(file)) throw new InvalidDataException("A registration file must be workspace-relative.");
        var resolved = new PhysicalPathResolver().ResolveRoot(root);
        if (resolved.State != PhysicalPathState.Contained) throw new IOException("The physical registration workspace is unavailable.");
        var workspace = new CliWorkspace(root, resolved.GetContainedPhysicalPath(), CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var snapshot = await ToolFileReader.ReadAsync(workspace, Path.GetFullPath(file, root), token).ConfigureAwait(false);
        return snapshot.HasBytes ? snapshot.Bytes.ToArray() : null;
    }

    internal static bool IsReadFailure(Exception exception) => exception is IOException or UnauthorizedAccessException or NotSupportedException;
    internal static bool IsShapeFailure(Exception exception) => exception is InvalidDataException or System.Text.Json.JsonException or ArgumentException or InvalidOperationException;
}
