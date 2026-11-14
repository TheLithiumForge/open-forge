using System.Text;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

internal sealed class ToolConfigFixture : IDisposable
{
    private const string DirectoryName = "workspace";
    private readonly TemporaryWorkspace _temporary;
    internal string Path { get; }

    internal ToolConfigFixture(string purpose)
    {
        _temporary = TemporaryWorkspace.Create(purpose);
        Path = _temporary.CreateDirectory(DirectoryName);
    }

    internal string Combine(string relative) => _temporary.Combine(DirectoryName, relative);
    internal void WriteText(string relative, string contents) => File.WriteAllText(Combine(relative), contents, new UTF8Encoding(false));
    internal void CreateFileSymbolicLink(string relative, string target) => File.CreateSymbolicLink(Combine(relative), target);

    public void Dispose()
    {
        Assert.Equal(_temporary.Combine(DirectoryName), Path);
        Assert.Null(new DirectoryInfo(Path).LinkTarget);
        Directory.Delete(Path, recursive: true);
        _temporary.Dispose();
    }
}
