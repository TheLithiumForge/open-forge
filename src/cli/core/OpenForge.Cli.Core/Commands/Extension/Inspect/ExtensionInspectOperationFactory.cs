using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Reading;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect;

internal static class ExtensionInspectOperationFactory
{
    internal static ExtensionInspectOperation Create()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        return new ExtensionInspectOperation(
            new ExtensionSourceReader(physicalPathResolver),
            new LifecycleDocumentReader(physicalPathResolver),
            new ExtensionInspectCurrentPathReader(physicalPathResolver),
            new ExtensionInspectResultBuilder(
                new ExtensionInspectComparisonBuilder(new MarkdownFingerprintReader())));
    }
}
