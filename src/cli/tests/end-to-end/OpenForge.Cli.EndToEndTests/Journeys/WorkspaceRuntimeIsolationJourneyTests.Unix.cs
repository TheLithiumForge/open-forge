using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.TestSupport.Isolation;

using Tokens = OpenForge.Cli.TestSupport.Contracts.PublishedTokens;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed partial class WorkspaceRuntimeIsolationJourneyTests
{
    private const string SentinelTarget = "target.txt";

    [Fact(DisplayName = "A Unix linked sentinel refuses a published writer without changing authored files")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task UnixLinkedSentinelRefusesWriter()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            Assert.Skip("Unix FIFO, socket and symlink qualification requires Linux or macOS.");
            return;
        }

        using var workspace = PublishedJourneyWorkspace.Create("special");
        workspace.ExpectCoreInstall();
        Directory.CreateDirectory(TestDataHome.LocalRuntimeDirectory(workspace.Path));
        File.WriteAllText(TestDataHome.LocalRuntimeIgnorePath(workspace.Path), Tokens.Runtime.IgnoreContent);
        var sentinel = TestDataHome.LocalWorkspaceLockPath(workspace.Path);
        workspace.WriteText(SentinelTarget, "Preserved target\n");
        File.CreateSymbolicLink(sentinel, workspace.Combine(SentinelTarget));
        try
        {
            var before = workspace.SnapshotState();
            var refused = await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic, Tokens.Syntax.FormatOption,
                Tokens.Syntax.JsonFormat));
            Assert.Equal(5, refused.ExitCode);
            Assert.Equal(before, workspace.SnapshotState());
            using var document = JsonDocument.Parse(refused.StandardOutput);
            // The exact refusal cause for each special target is proven in UnixWorkspaceLockIntegrationTests.
            Assert.Contains(document.RootElement.GetProperty(Tokens.Json.Findings).EnumerateArray(),
                finding => finding.GetProperty(Tokens.Json.Code).GetString() == Tokens.Install.WorkspaceUnsafeCode);
            Assert.False(Directory.Exists(workspace.Combine(Tokens.Sources.AgentsRoot)));
            Assert.True(File.Exists(sentinel));
        }
        finally
        {
            File.Delete(sentinel);
        }
    }
}
