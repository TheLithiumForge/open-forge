using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectFingerprintPolicy
{
    internal static ExtensionInspectFingerprintKind ReadKind(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return ExtensionDestinationPolicy.IsImplicit(path) && IsMarkdown(path)
            ? ExtensionInspectFingerprintKind.Semantic
            : ExtensionInspectFingerprintKind.ExactBytes;
    }

    private static bool IsMarkdown(string path)
        => path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase);
}
