namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Shared.Links;

// Mapping identity is portable. Raw observations and mutation expectations stay exact.
internal static class RelativeFileLinkTarget
{
    internal static string PortableForm(string target)
        => target.Replace('\\', '/');

    internal static string PlatformForm(string target)
        => target.Replace('/', Path.DirectorySeparatorChar);

    internal static bool SameMapping(string? left, string? right)
        => left is not null && right is not null
            && string.Equals(PortableForm(left), PortableForm(right), StringComparison.Ordinal);

    internal static bool UnreadableOnWindows(string? target)
        => OperatingSystem.IsWindows() && target?.Contains('/') == true;
}
