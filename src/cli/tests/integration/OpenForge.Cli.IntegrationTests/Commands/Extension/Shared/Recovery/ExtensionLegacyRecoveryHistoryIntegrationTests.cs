using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;

[Trait("Feature", "extension-recovery-history"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ExtensionLegacyRecoveryHistoryIntegrationTests
{
    private const string Target = ".agents/toolkit.txt";

    [Theory(DisplayName = "Extension writes and no-ops report legacy problems while verified finals stay silent")]
    [InlineData("install", "draft", false), InlineData("install", "draft", true)]
    [InlineData("install", "damaged", false), InlineData("install", "damaged", true)]
    [InlineData("install", "unavailable", false), InlineData("install", "unavailable", true)]
    [InlineData("install", "verified", false), InlineData("install", "verified", true)]
    [InlineData("update", "draft", false), InlineData("update", "draft", true)]
    [InlineData("update", "damaged", false), InlineData("update", "damaged", true)]
    [InlineData("update", "unavailable", false), InlineData("update", "unavailable", true)]
    [InlineData("update", "verified", false), InlineData("update", "verified", true)]
    [InlineData("remove", "draft", false), InlineData("remove", "draft", true)]
    [InlineData("remove", "damaged", false), InlineData("remove", "damaged", true)]
    [InlineData("remove", "unavailable", false), InlineData("remove", "unavailable", true)]
    [InlineData("remove", "verified", false), InlineData("remove", "verified", true)]
    public async Task LegacyHistoryDoesNotVetoCurrentState(string command, string history, bool noOp)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-legacy-history");
        using var source = ExtensionInstallCatalogue.Create("extension-legacy-history-source");
        await workspace.SeedFrameworkAsync();
        source.AddPackage("toolkit", [], (Target, "version one\n"));
        if (command != "install" || noOp)
        {
            Assert.Equal(0, (await workspace.RunAsync(Arguments("install", source.Path))).ExitCode);
        }
        if (command == "remove" && noOp)
        {
            Assert.Equal(0, (await workspace.RunAsync(Arguments("remove", source.Path))).ExitCode);
        }
        if (command == "update" && !noOp) source.ReplacePayload("toolkit", Target, "version two\n");

        var legacy = Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace.Workspace));
        try
        {
            var path = await SeedHistoryAsync(workspace, legacy, history);
            var bytes = File.ReadAllBytes(path);
            Directory.Delete(workspace.Combine(".open-forge-state"), recursive: true);
            Assert.False(Directory.Exists(RecoveryStoreLocator.Local(workspace.Workspace).WorkspaceDirectory));
            var before = workspace.Snapshot();
            var sourceBefore = source.Snapshot();

            var run = await workspace.RunAsync([.. Arguments(command, source.Path), "--format", "json", "--detail", "full"]);

            var silent = history == "verified";
            Assert.Equal(silent ? 0 : 2, run.ExitCode);
            Assert.Equal(silent ? CliSemanticStatus.Complete : CliSemanticStatus.Attention, run.Status);
            Assert.Equal(string.Empty, run.StandardError);
            using var document = JsonDocument.Parse(run.StandardOutput);
            var findings = document.RootElement.GetProperty("findings").EnumerateArray();
            if (silent)
            {
                Assert.Empty(findings);
            }
            else
            {
                var finding = Assert.Single(findings);
                var code = history == "unavailable" ? "recovery-coverage-unavailable" : "recovery-history";
                Assert.Equal($"extension-{command}.{code}", finding.GetProperty("code").GetString());
                Assert.Equal("warning", finding.GetProperty("severity").GetString());
                Assert.Equal(path, finding.GetProperty("subject").GetProperty("path").GetString());
                Assert.Contains(path, finding.GetProperty("message").GetString(), StringComparison.Ordinal);
            }
            Assert.Equal(bytes, File.ReadAllBytes(path));
            Assert.Equal(sourceBefore, source.Snapshot());
            if (noOp)
            {
                Assert.Equal(before, workspace.Snapshot());
                Assert.False(Directory.Exists(workspace.Combine(".open-forge-state")));
            }
            else
            {
                if (command == "remove") Assert.False(File.Exists(workspace.Combine(Target)));
                else Assert.Equal(command == "update" ? "version two\n" : "version one\n", workspace.ReadText(Target));
                if (command != "install")
                {
                    var ownPath = Assert.IsType<string>(document.RootElement.GetProperty("recovery").GetProperty("path").GetString());
                    var local = RecoveryStoreLocator.Local(workspace.Workspace);
                    Assert.Equal(local.WorkspaceDirectory, Path.GetDirectoryName(ownPath));
                    var own = await RecoveryBundleReader.ReadFinalAsync(local, ownPath, TestContext.Current.CancellationToken);
                    Assert.Equal(RecoveryBundleReadState.Valid, own.State);
                }
            }
        }
        finally
        {
            if (Directory.Exists(legacy.WorkspaceDirectory)) Directory.Delete(legacy.WorkspaceDirectory, recursive: true);
            else File.Delete(legacy.WorkspaceDirectory);
        }
    }

    private static async Task<string> SeedHistoryAsync(
        ExtensionInstallIntegrationWorkspace workspace, RecoveryStoreLocation legacy, string history)
    {
        if (history == "unavailable")
        {
            Directory.CreateDirectory(legacy.RootPath);
            File.WriteAllText(legacy.WorkspaceDirectory, "unreadable legacy catalogue");
            return legacy.WorkspaceDirectory;
        }
        Directory.CreateDirectory(legacy.WorkspaceDirectory);
        if (history == "verified")
        {
            var preparation = await ExtensionRecoveryHistoryFixture.CreateVerifiedFinalAsync(
                workspace, "extension install", RecoveryBundleOperation.Install, TestContext.Current.CancellationToken);
            return await ExtensionRecoveryHistoryFixture.CopyToLegacyAsync(preparation, TestContext.Current.CancellationToken);
        }
        var fileName = history switch
        {
            "draft" => RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid()),
            "damaged" => RecoveryBundleFormatV1.FinalFileName(Guid.NewGuid()),
            _ => throw new ArgumentOutOfRangeException(nameof(history)),
        };
        var candidate = Path.Combine(legacy.WorkspaceDirectory, fileName);
        File.WriteAllText(candidate, "retained legacy recovery bytes");
        return candidate;
    }

    private static string[] Arguments(string command, string source)
        => command == "remove"
            ? ["extension", command, "toolkit", "--automatic"]
            : ["extension", command, "toolkit", "--source", source, "--automatic"];
}
