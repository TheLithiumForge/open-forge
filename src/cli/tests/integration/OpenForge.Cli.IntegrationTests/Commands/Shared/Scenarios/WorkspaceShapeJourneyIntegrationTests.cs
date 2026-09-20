using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Commands.Install;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Scenarios;

[Trait("Feature", "workspace-shapes"), Trait("Evidence", "Integration")]
public sealed class WorkspaceShapeJourneyIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Dirty workspace blocks composed Install with its owning update action and no effects"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task DirtyWorkspaceBlocksInstallWithoutWrites()
    {
        using var workspace = InstallOperationWorkspace.Create("scenario-dirty-workspace");
        var installed = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Unavailable(),
                workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, installed.Status);

        workspace.ReplaceInstalledText(".agents/loader.md", "# User divergence\n");
        var before = workspace.SnapshotHashes();
        var recoveryBefore = workspace.RecoveryDirectoryExists();
        var run = await RunAsync(
            [
                "install",
                "--workspace",
                workspace.PhysicalPath,
                "--force",
                "--automatic",
                "--format",
                "json",
                "--detail",
                "full",
            ],
            workspace.PhysicalPath,
            workspace.LockStoreRoot);

        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = ParseJson(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("install", root.GetProperty("command").GetString());
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        Assert.Equal(workspace.PhysicalPath, root.GetProperty("workspace").GetProperty("path").GetString());

        var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());
        Assert.Equal("install.managed-divergence", finding.GetProperty("code").GetString());
        var subject = finding.GetProperty("subject");
        Assert.Equal("workspace", subject.GetProperty("kind").GetString());
        Assert.Equal(workspace.PhysicalPath, subject.GetProperty("id").GetString());
        Assert.Equal(JsonValueKind.Null, subject.GetProperty("path").ValueKind);
        Assert.Equal("open-forge update", finding.GetProperty("actions")[0].GetProperty("command").GetString());

        var next = root.GetProperty("next");
        Assert.Equal(JsonValueKind.Object, next.ValueKind);
        Assert.Equal("open-forge update", next.GetProperty("command").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(recoveryBefore, workspace.RecoveryDirectoryExists());
        Assert.Equal("not-required", root.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("recovery").GetProperty("path").ValueKind);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Relocated workspace keeps composed Status current without writes or recovery state"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task RelocatedWorkspaceRemainsCurrentWithoutWrites()
    {
        using var workspace = InstallOperationWorkspace.Create("scenario-relocated-workspace");
        var installed = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Unavailable(),
                workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, installed.Status);

        var originalPath = workspace.PhysicalPath;
        var parent = Path.GetDirectoryName(originalPath)
            ?? throw new InvalidOperationException("The relocated workspace fixture requires a parent directory.");
        var relocatedPath = Path.Combine(parent, Path.GetFileName(originalPath) + "-relocated");
        Assert.False(Directory.Exists(relocatedPath));
        Directory.Move(originalPath, relocatedPath);
        try
        {
            var before = SnapshotFiles(relocatedPath);
            using var lockStore = WorkspaceLockTestStore.Create("scenario-relocated-workspace-lock");
            var run = await RunAsync(
                [
                    "status",
                    "--workspace",
                    relocatedPath,
                    "--format",
                    "json",
                    "--detail",
                    "full",
                ],
                relocatedPath,
                lockStore.StoreRoot);

            Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
            Assert.Equal(0, run.Completion.ExitCode);
            Assert.Equal(string.Empty, run.StandardError);
            using var document = ParseJson(run.StandardOutput);
            var root = document.RootElement;
            Assert.Equal("status", root.GetProperty("command").GetString());
            Assert.Equal("completed", root.GetProperty("status").GetString());
            Assert.Equal(relocatedPath, root.GetProperty("workspace").GetProperty("path").GetString());
            Assert.Equal("explicit-workspace", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
            Assert.Empty(root.GetProperty("findings").EnumerateArray());
            Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
            Assert.Equal(JsonValueKind.Null, root.GetProperty("recovery").ValueKind);
            Assert.Equal(before, SnapshotFiles(relocatedPath));
        }
        finally
        {
            Directory.Move(relocatedPath, originalPath);
        }
    }

    private static async Task<WorkspaceShapeRun> RunAsync(
        string[] arguments,
        string currentDirectory,
        WorkspaceLockStoreRoot lockStoreRoot)
    {
        using var standardInput = new StringReader(string.Empty);
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", CliBuildVersion.InformationalVersion),
            new CliCompositionInputs
            {
                StandardInput = standardInput,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
                LockStoreRoot = lockStoreRoot,
            });
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(currentDirectory),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new WorkspaceShapeRun(completion, standardOutput.ToString(), standardError.ToString());
    }

    private static JsonDocument ParseJson(string content)
    {
        Assert.NotEmpty(content);
        Assert.DoesNotContain('\uFFFD', content);
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        Assert.Equal(content, encoding.GetString(encoding.GetBytes(content)));
        return JsonDocument.Parse(content);
    }

    private static IReadOnlyDictionary<string, string> SnapshotFiles(string root)
        => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .ToDictionary(
                path => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'),
                path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                StringComparer.Ordinal);

    private sealed record WorkspaceShapeRun(
        CliProcessCompletion Completion,
        string StandardOutput,
        string StandardError);
}
