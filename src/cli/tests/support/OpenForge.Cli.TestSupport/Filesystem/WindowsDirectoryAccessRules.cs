using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace OpenForge.Cli.TestSupport.Filesystem;

[SupportedOSPlatform("windows")]
internal static class WindowsDirectoryAccessRules
{
    internal static void Restore(DirectoryInfo directory, byte[] original)
    {
        var restore = new DirectorySecurity();
        restore.SetSecurityDescriptorBinaryForm(original, AccessControlSections.Access);
        directory.SetAccessControl(restore);
        var restored = directory.GetAccessControl(AccessControlSections.Access);
        var expectedDescriptor = new RawSecurityDescriptor(original, 0);
        var actualDescriptor = new RawSecurityDescriptor(restored.GetSecurityDescriptorBinaryForm(), 0);

        // Windows may mark a restored DACL as auto-inherited. That bookkeeping
        // flag is not an access rule. Protection and every ordered ACE byte
        // (trustee, rights, deny/allow and inheritance flags) must still match.
        if (restore.AreAccessRulesProtected != restored.AreAccessRulesProtected
            || !AclBytes(expectedDescriptor.DiscretionaryAcl).AsSpan().SequenceEqual(AclBytes(actualDescriptor.DiscretionaryAcl)))
        {
            throw new InvalidOperationException("The fixture directory access rules or inheritance protection were not restored exactly.");
        }
    }

    private static byte[] AclBytes(RawAcl? acl)
    {
        if (acl is null) return [];
        var bytes = new byte[acl.BinaryLength];
        acl.GetBinaryForm(bytes, 0);
        return bytes;
    }
}
