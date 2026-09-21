using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace OpenForge.Cli.TestSupport.Filesystem;

[SupportedOSPlatform("windows")]
public sealed class WindowsChildDirectoryCreationDenial : IDisposable
{
    private readonly DirectoryInfo _catalogue;
    private readonly byte[] _original;
    private bool _disposed;

    private WindowsChildDirectoryCreationDenial(DirectoryInfo catalogue, DirectorySecurity original)
    {
        _catalogue = catalogue;
        _original = original.GetSecurityDescriptorBinaryForm();
    }

    public static WindowsChildDirectoryCreationDenial Create(string ownedRoot, string cataloguePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownedRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(cataloguePath);
        if (!Path.IsPathFullyQualified(ownedRoot) || !Path.IsPathFullyQualified(cataloguePath))
        {
            throw new ArgumentException("Child-directory denial requires absolute owned fixture paths.");
        }

        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(ownedRoot));
        var target = Path.TrimEndingDirectorySeparator(Path.GetFullPath(cataloguePath));
        var relative = Path.GetRelativePath(root, target);
        if (Path.IsPathRooted(relative) || relative == ".."
            || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new ArgumentException("The catalogue must remain inside its owned fixture root.", nameof(cataloguePath));
        }

        var catalogue = new DirectoryInfo(target);
        for (var current = catalogue; ; current = current.Parent
                 ?? throw new InvalidOperationException("The fixture catalogue ancestry changed."))
        {
            if (!current.Exists || (current.Attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException("Child-directory denial requires ordinary owned directories.");
            }

            if (string.Equals(current.FullName, root, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
        }

        if (catalogue.EnumerateFileSystemInfos().Any())
        {
            throw new InvalidOperationException("Child-directory denial requires an initially empty fixture catalogue.");
        }

        var original = catalogue.GetAccessControl(AccessControlSections.Access);
        var denied = new DirectorySecurity();
        denied.SetSecurityDescriptorBinaryForm(original.GetSecurityDescriptorBinaryForm(), AccessControlSections.Access);
        denied.SetAccessRuleProtection(isProtected: true, preserveInheritance: true);
        using var identity = WindowsIdentity.GetCurrent();
        var user = identity.User
            ?? throw new InvalidOperationException("The fixture requires a current Windows user identity.");
        denied.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.CreateDirectories,
            InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly | PropagationFlags.NoPropagateInherit,
            AccessControlType.Deny));
        var scope = new WindowsChildDirectoryCreationDenial(catalogue, original);
        try
        {
            catalogue.SetAccessControl(denied);
            return scope;
        }
        catch (Exception failure)
        {
            try
            {
                scope.Dispose();
            }
            catch (Exception restorationFailure)
            {
                throw new AggregateException("The denial fixture and its restoration both failed.", failure, restorationFailure);
            }
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        WindowsDirectoryAccessRules.Restore(_catalogue, _original);

        _disposed = true;
    }
}
