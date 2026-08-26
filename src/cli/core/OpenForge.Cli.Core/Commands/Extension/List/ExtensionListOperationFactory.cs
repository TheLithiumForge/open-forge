using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;

namespace OpenForge.Cli.Core.Commands.Extension.List;

internal static class ExtensionListOperationFactory
{
    internal static ExtensionListOperation Create()
        => new(
            new ExtensionSourceReader(new PhysicalPathResolver()),
            new LifecycleDocumentReader(new PhysicalPathResolver()));
}
