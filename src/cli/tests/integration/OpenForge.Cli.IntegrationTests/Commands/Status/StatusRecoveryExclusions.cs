using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal sealed class StatusRecoveryExclusions : IDisposable
{
    private readonly StatusIntegrationWorkspace _workspace;
    private readonly string _workspaceDirectory;
    private readonly List<string> _externalPaths = [];
    private readonly List<TemporaryWorkspace> _otherWorkspaces = [];
    private bool _disposed;

    private StatusRecoveryExclusions(StatusIntegrationWorkspace workspace)
    {
        _workspace = workspace;
        _workspaceDirectory = workspace.RecoveryDirectory();
    }

    internal static StatusRecoveryExclusions Create(StatusIntegrationWorkspace workspace)
        => new(workspace);

    internal string AddLookalike()
    {
        var path = Path.Combine(_workspaceDirectory, $"operation-{Guid.NewGuid():N}.zip.tmp");
        WriteExternal(path, "lookalike"u8.ToArray());
        return path;
    }

    internal string AddAdjacentLookalike()
    {
        var relativePath = $"operation-{Guid.NewGuid():N}.zip";
        _workspace.WriteBytes(relativePath, "adjacent"u8.ToArray());
        return _workspace.Combine(relativePath);
    }

    internal string AddDifferentWorkspaceCandidate()
    {
        var other = TemporaryWorkspace.Create("status-recovery-other");
        _otherWorkspaces.Add(other);
        var otherWorkspace = new CliWorkspace(
            other.Path,
            other.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var path = Path.Combine(
            RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(otherWorkspace),
            RecoveryBundleFormatV1.FinalFileName(Guid.NewGuid()));
        WriteExternal(path, "different workspace"u8.ToArray());
        return path;
    }

    internal IReadOnlyDictionary<string, byte[]> SnapshotExternalBytes()
        => _externalPaths.ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var path in _externalPaths.OrderByDescending(path => path.Length))
        {
            StatusRecoveryCatalogue.DeleteExact(path);
            var directory = Path.GetDirectoryName(path);
            if (directory is not null)
            {
                StatusRecoveryCatalogue.DeleteEmptyDirectory(directory);
            }
        }

        foreach (var other in _otherWorkspaces)
        {
            other.Dispose();
        }

        _disposed = true;
    }

    private void WriteExternal(string path, byte[] bytes)
    {
        var parent = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The excluded recovery path requires a parent directory.");
        Directory.CreateDirectory(parent);
        File.WriteAllBytes(path, bytes);
        _externalPaths.Add(path);
    }
}
