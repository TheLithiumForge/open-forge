using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

internal sealed partial class ToolBackupSession
{
    internal async ValueTask<ToolBackupResidual[]> RestoreAsync()
    {
        RequireLease();
        var residuals = new List<ToolBackupResidual>();
        foreach (var change in _changes.Reverse())
        {
            try
            {
                if (_attempted.Contains(change.LogicalPath))
                {
                    var current = await ToolFileReader.ReadAsync(_lease.Request.Workspace, change.LogicalPath, CancellationToken.None).ConfigureAwait(false);
                    if (current.Expectation != change.Expectation)
                    {
                        var physical = current.PhysicalPath ?? change.Expectation.PhysicalPath ?? change.LogicalPath;
                        await CheckAsync(Intended(change, physical), CancellationToken.None).ConfigureAwait(false);
                        if (_backups.TryGetValue(change.LogicalPath, out var saved))
                        {
                            await CheckAsync(saved.Expectation, CancellationToken.None).ConfigureAwait(false);
                            await WriteAsync(Intended(change, physical), physical, saved.Bytes.AsMemory(), CancellationToken.None).ConfigureAwait(false);
                        }
                        else if (_priors.TryGetValue(change.LogicalPath, out var prior))
                            await WriteAsync(Intended(change, physical), physical, prior.Bytes.AsMemory(), CancellationToken.None).ConfigureAwait(false);
                        else File.Delete(physical);
                        if (!OperatingSystem.IsWindows() && _priorModes.TryGetValue(change.LogicalPath, out var mode)) File.SetUnixFileMode(physical, mode);
                        await CheckAsync(change.Expectation, CancellationToken.None).ConfigureAwait(false);
                    }
                }
                if (_backups.TryGetValue(change.LogicalPath, out var backup))
                {
                    await CheckAsync(backup.Expectation, CancellationToken.None).ConfigureAwait(false);
                    File.Delete(backup.PhysicalPath ?? throw new IOException("The backup has no physical path."));
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
            {
                residuals.Add(new(change.LogicalPath, _backups.ContainsKey(change.LogicalPath) ? ToolBackupPaths.Beside(change.LogicalPath) : null, exception.Message));
            }
        }
        _closed = true;
        return [.. residuals];
    }

    private async ValueTask<ToolBackupResidual[]> DeleteBackupsAsync()
    {
        var residuals = new List<ToolBackupResidual>();
        foreach (var (target, backup) in _backups)
        {
            try
            {
                await CheckAsync(backup.Expectation, CancellationToken.None).ConfigureAwait(false);
                File.Delete(backup.PhysicalPath ?? throw new IOException("The backup has no physical path."));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                residuals.Add(new(target, backup.LogicalPath, exception.Message));
            }
        }
        return [.. residuals];
    }
}
