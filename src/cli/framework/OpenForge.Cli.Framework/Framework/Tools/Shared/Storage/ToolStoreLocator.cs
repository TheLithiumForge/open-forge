using OpenForge.Cli.Core.Framework.Tools.Models.Storage;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Storage;

internal static class ToolStoreLocator
{
    internal const string OverrideVariable = "OPENFORGE_TOOL_HOME";
    internal const string InstallsDirectory = "tools";
    internal const string ArchivesDirectory = "tool-cache";
    internal const string StateDirectory = "tool-state";
    private const string Application = "OpenForge";
    private const string UnixApplication = "open-forge";

    internal static ToolStorePaths FromRoot(string root)
    {
        if (!Path.IsPathFullyQualified(root)) throw new ArgumentException("The tool store override must be absolute.", nameof(root));
        root = Path.GetFullPath(root);
        return new(Path.Combine(root, InstallsDirectory), Path.Combine(root, ArchivesDirectory), Path.Combine(root, StateDirectory));
    }

    internal static ToolStorePaths ForCurrentUser()
    {
        var configured = Environment.GetEnvironmentVariable(OverrideVariable);
        if (!string.IsNullOrWhiteSpace(configured)) return FromRoot(configured);
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);
        if (OperatingSystem.IsWindows())
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
            return FromRoot(Required(Path.Combine(local, Application)));
        }
        if (OperatingSystem.IsMacOS())
        {
            var data = Required(Path.Combine(home, "Library", "Application Support", Application));
            return new(Path.Combine(data, InstallsDirectory), Path.Combine(Required(Path.Combine(home, "Library", "Caches", Application)), InstallsDirectory), Path.Combine(data, StateDirectory));
        }
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Tool stores aren't available on this platform.");
        return new(Path.Combine(Xdg("XDG_DATA_HOME", home, ".local/share"), UnixApplication, InstallsDirectory),
            Path.Combine(Xdg("XDG_CACHE_HOME", home, ".cache"), UnixApplication, InstallsDirectory),
            Path.Combine(Xdg("XDG_STATE_HOME", home, ".local/state"), UnixApplication, InstallsDirectory));
    }

    private static string Xdg(string variable, string home, string fallback)
    {
        var configured = Environment.GetEnvironmentVariable(variable);
        return Required(string.IsNullOrWhiteSpace(configured) ? Path.Combine(home, fallback.Replace('/', Path.DirectorySeparatorChar)) : configured);
    }

    private static string Required(string value)
        => Path.IsPathFullyQualified(value) ? Path.GetFullPath(value) : throw new IOException("The tool store location isn't available.");
}
