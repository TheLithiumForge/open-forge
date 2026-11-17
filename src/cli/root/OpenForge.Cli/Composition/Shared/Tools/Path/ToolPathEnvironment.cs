using System.Runtime.Versioning;
using Microsoft.Win32;
using OpenForge.Cli.Core.Framework.Tools.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Shared.Paths.Interop;

namespace OpenForge.Cli.Composition.Shared.Tools.Path;

/// <summary>The user Path value as stored, with its registry kind and presence.</summary>
internal sealed record ToolUserPathValue(string Text, bool Expandable, bool Present);

internal abstract class ToolPathEnvironment
{
    internal abstract bool IsWindows { get; }
    internal abstract string? ProcessPath { get; }
    // Null means the stored value couldn't be read safely, so nothing may be written.
    internal abstract ToolUserPathValue? ReadUserPath();
    internal abstract bool WriteUserPath(ToolUserPathValue value);
    internal abstract void AnnounceEnvironmentChange();
}

// The Windows user Path is edited in HKCU\Environment directly, never through an API that expands or rewrites it.
internal sealed class ToolUserPathEnvironment : ToolPathEnvironment
{
    private const string EnvironmentKey = "Environment";
    private const string PathValueName = "Path";

    internal override bool IsWindows => OperatingSystem.IsWindows();
    internal override string? ProcessPath => Environment.GetEnvironmentVariable(ToolPathPolicy.PathVariable);

    internal override ToolUserPathValue? ReadUserPath()
    {
        if (!OperatingSystem.IsWindows()) return null;
        try { return Read(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException) { return null; }
    }

    internal override bool WriteUserPath(ToolUserPathValue value)
    {
        if (!OperatingSystem.IsWindows()) return false;
        try { return Write(value); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException) { return false; }
    }

    internal override void AnnounceEnvironmentChange() => ToolEnvironmentBroadcast.Announce();

    [SupportedOSPlatform("windows")]
    private static ToolUserPathValue? Read()
    {
        using var key = Registry.CurrentUser.OpenSubKey(EnvironmentKey, writable: false);
        if (key is null) return null;
        if (key.GetValue(PathValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames) is not { } stored) return new(string.Empty, Expandable: true, Present: false);
        var kind = key.GetValueKind(PathValueName);
        // Only string kinds can be edited without changing their meaning.
        if (kind is not (RegistryValueKind.String or RegistryValueKind.ExpandString) || stored is not string text) return null;
        return new(text, kind == RegistryValueKind.ExpandString, Present: true);
    }

    [SupportedOSPlatform("windows")]
    private static bool Write(ToolUserPathValue value)
    {
        using var key = Registry.CurrentUser.OpenSubKey(EnvironmentKey, writable: true);
        if (key is null) return false;
        key.SetValue(PathValueName, value.Text, value.Expandable ? RegistryValueKind.ExpandString : RegistryValueKind.String);
        return true;
    }
}
