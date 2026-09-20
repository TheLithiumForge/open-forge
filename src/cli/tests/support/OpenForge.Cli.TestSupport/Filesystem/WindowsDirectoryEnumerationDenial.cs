using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace OpenForge.Cli.TestSupport.Filesystem;

[SupportedOSPlatform("windows")]
public sealed class WindowsDirectoryEnumerationDenial : IDisposable
{
    private readonly DirectoryInfo _directory;
    private readonly DirectorySecurity _original;
    private bool _disposed;

    private WindowsDirectoryEnumerationDenial(DirectoryInfo directory, DirectorySecurity original)
    {
        _directory = directory;
        _original = original;
    }

    public static WindowsDirectoryEnumerationDenial Create(string ownedRoot, string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownedRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        if (!Path.IsPathFullyQualified(ownedRoot) || !Path.IsPathFullyQualified(directoryPath))
        {
            throw new ArgumentException("Enumeration denial requires absolute owned fixture paths.");
        }

        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(ownedRoot));
        var target = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directoryPath));
        var relative = Path.GetRelativePath(root, target);
        if (Path.IsPathRooted(relative) || relative == ".."
            || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new ArgumentException("Enumeration denial must remain inside its owned fixture root.", nameof(directoryPath));
        }

        var directory = new DirectoryInfo(target);
        for (var current = directory; ; current = current.Parent
                 ?? throw new InvalidOperationException("The fixture directory ancestry changed."))
        {
            if (!current.Exists || (current.Attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException("Enumeration denial requires ordinary owned directories.");
            }

            if (string.Equals(current.FullName, root, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
        }

        var original = directory.GetAccessControl(AccessControlSections.Access);
        var denied = new DirectorySecurity();
        denied.SetSecurityDescriptorBinaryForm(original.GetSecurityDescriptorBinaryForm(), AccessControlSections.Access);
        using var identity = WindowsIdentity.GetCurrent();
        var user = identity.User
            ?? throw new InvalidOperationException("The fixture requires a current Windows user identity.");
        denied.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.ListDirectory,
            InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
        var scope = new WindowsDirectoryEnumerationDenial(directory, original);
        try
        {
            directory.SetAccessControl(denied);
            try
            {
                using var entries = Directory.EnumerateFileSystemEntries(target).GetEnumerator();
                _ = entries.MoveNext();
            }
            catch (UnauthorizedAccessException)
            {
                return scope;
            }

            throw new InvalidOperationException("The fixture did not deny directory enumeration.");
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var restore = new DirectorySecurity();
        restore.SetSecurityDescriptorBinaryForm(_original.GetSecurityDescriptorBinaryForm(), AccessControlSections.Access);
        _directory.SetAccessControl(restore);
        var restored = _directory.GetAccessControl(AccessControlSections.Access);
        if (restored.GetSecurityDescriptorSddlForm(AccessControlSections.Access)
            != _original.GetSecurityDescriptorSddlForm(AccessControlSections.Access))
        {
            throw new InvalidOperationException("The fixture directory access rules were not restored exactly.");
        }

        _disposed = true;
    }
}
