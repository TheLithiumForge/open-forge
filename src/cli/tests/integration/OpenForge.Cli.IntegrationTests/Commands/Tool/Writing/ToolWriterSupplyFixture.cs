using OpenForge.Cli.Core.Framework.Tools.Models.Supply;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Writing;

internal sealed class ToolWriterSupplyFixture : IDisposable
{
    private readonly ToolArtifactFixture _artifact = new();
    internal TemporaryWorkspace Temporary => _artifact.Temporary;
    internal ToolArtifactExtraction Create(string format) => _artifact.Create(format);

    public void Dispose()
    {
        var paths = ToolStoreLocator.FromRoot(Temporary.Path);
        foreach (var root in new[] { paths.Installs, paths.Archives, paths.State })
        {
            var relative = Path.GetRelativePath(Temporary.Path, Path.GetFullPath(root));
            if (Path.IsPathRooted(relative) || relative.StartsWith("..", StringComparison.Ordinal))
                throw new InvalidOperationException("The fixture store cleanup escaped its owned directory.");
            if (Directory.Exists(root)) Delete(new(root));
        }
        _artifact.Dispose();
    }

    private static void Delete(DirectoryInfo directory)
    {
        if (directory.LinkTarget is null && (directory.Attributes & FileAttributes.ReparsePoint) == 0)
            foreach (var member in directory.EnumerateFileSystemInfos())
            {
                if (member is DirectoryInfo child) Delete(child);
                else member.Delete();
            }
        directory.Delete(recursive: false);
    }
}
