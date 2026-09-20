using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

internal enum PathComponentState
{
    Ordinary,
    Link,
    Missing,
    Inaccessible,
    Unsupported,
    InputOutputFailure,
}

internal sealed record PathComponent(
    PathComponentState State,
    string Path,
    string? LinkTarget,
    FileAttributes? Attributes,
    FilesystemFailure? Failure);
