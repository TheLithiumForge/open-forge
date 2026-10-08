using System.Text;

namespace OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Configuration;

internal sealed class InstallFrontmatterSources(InstallOperationWorkspace workspace) : IDisposable
{
    private readonly List<string> _files = [];
    private readonly HashSet<string> _directories = new(StringComparer.Ordinal);

    internal void Add(string relativePath, string content)
    {
        var path = workspace.Combine(relativePath);
        if (File.Exists(path)) throw new InvalidOperationException("An added conversion fixture must be absent.");
        var directory = Path.GetDirectoryName(path) ?? throw new InvalidOperationException();
        for (var parent = directory; parent != workspace.PhysicalPath && !Directory.Exists(parent); parent = Path.GetDirectoryName(parent) ?? throw new InvalidOperationException())
            _directories.Add(parent);
        Directory.CreateDirectory(directory);
        _files.Add(path);
        File.WriteAllText(path, content, new UTF8Encoding(false, true));
    }

    public void Dispose()
    {
        foreach (var path in _files)
        {
            if ((File.GetAttributes(path) & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
                throw new InvalidOperationException("An added conversion fixture must remain an ordinary file.");
            File.Delete(path);
        }
        foreach (var path in _directories.OrderByDescending(path => path.Length))
        {
            if (!Directory.EnumerateFileSystemEntries(path).Any()) Directory.Delete(path);
        }
    }
}
