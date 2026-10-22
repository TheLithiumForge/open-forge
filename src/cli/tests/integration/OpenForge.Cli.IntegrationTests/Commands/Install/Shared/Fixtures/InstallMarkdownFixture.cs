using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Markdown;

namespace OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Fixtures;

internal static class InstallMarkdownFixture
{
    internal const string OccupiedManagedHost = "# Open Forge\n\nSynthetic occupied host content.\n\n**End of Open Forge managed section.**\n";

    internal static string ReadEmbeddedText(string path)
    {
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        var asset = payload.Find(path)
            ?? throw new InvalidOperationException($"The embedded fixture target '{path}' is unavailable.");
        return Encoding.UTF8.GetString(asset.Bytes.AsSpan());
    }

    internal static string SeedStaleEntries(InstallOperationWorkspace workspace, string path)
    {
        var intended = ReadEmbeddedText(path);
        var document = new MarkdownDocumentParser().Parse(intended);
        if (document.GeneratedRegion.ContentSpan is not { } content)
        {
            throw new InvalidOperationException("The stale-navigation fixture requires an Entries body.");
        }

        var stale = $"{intended[..content.Start]}\n- [Stale generated entry](stale.md)\n{intended[content.End..]}";
        Assert.NotEqual(intended, stale);
        workspace.WriteText(path, stale);
        return intended;
    }
}
