using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Storage;

internal sealed class ToolStoreLease : IDisposable
{
    private readonly FileStream _handle;
    private readonly ToolStoreArea _area;
    private bool _disposed;

    private ToolStoreLease(ToolStoreArea area, FileStream handle) { _area = area; _handle = handle; }

    internal static ToolStoreLease Acquire(ToolStoreArea area, CancellationToken token)
    {
        area.EnsureRoot(token);
        var state = ToolFileReader.Observe(area.Scope, ToolStoreRegistryDefinitions.Lock, token);
        if (state is not NoFollowLeafState.Missing and not NoFollowLeafState.OrdinaryFile) throw new IOException("The tool store lock isn't an ordinary file.");
        token.ThrowIfCancellationRequested();
        var stream = new FileStream(area.At(ToolStoreRegistryDefinitions.Lock), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        try
        {
            area.ValidateRoot();
            if (ToolFileReader.Observe(area.Scope, ToolStoreRegistryDefinitions.Lock, token) != NoFollowLeafState.OrdinaryFile)
                throw new IOException("The tool store lock changed during acquisition.");
            return new(area, stream);
        }
        catch { stream.Dispose(); throw; }
    }

    internal void RequireHeldFor(ToolStoreArea area)
    {
        RequireHeld();
        if (!string.Equals(_area.Root, area.Root, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidOperationException("The tool store lease covers another registry.");
    }

    internal void RequireHeld()
    {
        if (_disposed || _handle.SafeFileHandle.IsClosed || _handle.SafeFileHandle.IsInvalid) throw new InvalidOperationException("The tool store lease is no longer held.");
        _area.ValidateRoot();
    }

    public void Dispose() { _disposed = true; _handle.Dispose(); }
}
