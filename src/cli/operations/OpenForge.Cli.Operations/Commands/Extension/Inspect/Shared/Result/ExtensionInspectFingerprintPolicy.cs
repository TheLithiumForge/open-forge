using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectFingerprintPolicy
{
    internal const string LongMarkdownExtension = ".markdown";

    internal static ExtensionInspectFingerprintKind ReadKind(WorkspaceLayout layout, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return ExtensionDestinationPolicy.IsImplicit(layout, path) && IsMarkdown(path)
            ? ExtensionInspectFingerprintKind.Semantic
            : ExtensionInspectFingerprintKind.ExactBytes;
    }

    private static bool IsMarkdown(string path)
        => path.EndsWith(SourceFormClassifier.MarkdownExtension, StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(LongMarkdownExtension, StringComparison.OrdinalIgnoreCase);
}
