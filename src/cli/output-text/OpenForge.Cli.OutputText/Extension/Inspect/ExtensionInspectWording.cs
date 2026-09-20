using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Inspect;

internal static class ExtensionInspectWording
{
    // @OpenForgeText extension.inspect.wording.extension-inspect-path-changed-extension-inspect-path-missing-extension-inspect-path-new-extension-inspect-path-retired
    internal static bool IsFileDifferenceCode(string code)
        => code is "extension-inspect.path-changed"
            or "extension-inspect.path-missing"
            or "extension-inspect.path-new"
            or "extension-inspect.path-retired";
}
