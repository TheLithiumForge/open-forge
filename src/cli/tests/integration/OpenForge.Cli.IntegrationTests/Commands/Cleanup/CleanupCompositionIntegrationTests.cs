using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class CleanupCompositionIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Root composition exposes one direct Cleanup leaf and truthful terminal help"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task RootCompositionExposesOneDirectCleanupLeaf()
    {
        using var workspace = CleanupIntegrationWorkspace.Create(
            "cleanup-composition-help",
            withEntry: false);
        var missing = workspace.Combine("missing-cleanup-help-workspace");
        var before = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var root = await workspace.RunAsync(
            ["--help"],
            TestContext.Current.CancellationToken);
        var leaf = await workspace.RunAsync(
            ["cleanup", "--help", "--workspace", missing],
            TestContext.Current.CancellationToken);

        var directLeaves = root.StandardOutput
            .Split(Environment.NewLine, StringSplitOptions.None)
            .SkipWhile(line => !line.Equals("Commands:", StringComparison.Ordinal))
            .Skip(1)
            .TakeWhile(line => line.StartsWith("  ", StringComparison.Ordinal))
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Select(line => line.Split(' ', 2)[0])
            .ToArray();
        Assert.Equal(0, root.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, root.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, root.PrimaryOutputTarget);
        Assert.Equal(string.Empty, root.StandardError);
        Assert.Equal(1, directLeaves.Count(name => name == "cleanup"));
        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, leaf.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, leaf.PrimaryOutputTarget);
        Assert.Equal(string.Empty, leaf.StandardError);
        Assert.Contains(
            "open-forge cleanup [--dry-run] [global options]",
            leaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("Catalogue", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Write policy", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
        Assert.Equal(before, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.False(workspace.LockInfrastructureExists);
    }

    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Cleanup rejects operands before workspace or recovery enumeration"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task OperandInputIsInvalidWithoutEffects()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-composition-operand");
        var before = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var run = await workspace.RunAsync(
            ["cleanup", "unexpected-operand", "--workspace", workspace.Path],
            TestContext.Current.CancellationToken);

        Assert.Equal(4, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Status);
        Assert.Equal(CliOutputTarget.StandardError, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Contains("unexpected-operand", run.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Unrecognized command or argument 'cleanup'.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.False(workspace.LockInfrastructureExists);
    }
}
