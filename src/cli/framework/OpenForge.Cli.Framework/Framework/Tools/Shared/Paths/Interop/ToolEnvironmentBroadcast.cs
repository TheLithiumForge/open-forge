using System.Runtime.InteropServices;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Paths.Interop;

// Tells running Windows apps that user environment variables changed, as the maintainer's 2026-11-16 PATH decision requires.
internal static partial class ToolEnvironmentBroadcast
{
    private const string EnvironmentArea = "Environment";
    private const nint BroadcastHandle = 0xffff;
    private const uint SettingChangeMessage = 0x001A;
    private const uint AbortIfHung = 0x0002;
    private const uint TimeoutMilliseconds = 5000;

    internal static void Announce()
    {
        if (!OperatingSystem.IsWindows()) return;
        var area = Marshal.StringToHGlobalUni(EnvironmentArea);
        try { _ = SendMessageTimeout(BroadcastHandle, SettingChangeMessage, 0, area, AbortIfHung, TimeoutMilliseconds, 0); }
        finally { Marshal.FreeHGlobal(area); }
    }

    [LibraryImport("user32.dll", EntryPoint = "SendMessageTimeoutW")]
    private static partial nint SendMessageTimeout(nint window, uint message, nuint wParam, nint lParam, uint flags, uint timeout, nint result);
}
