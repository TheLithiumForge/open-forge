using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

public sealed class LibraryAttachInputIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("", null)]
    [InlineData("team-knowledge", null)]
    [InlineData("team-knowledge shared/team extra", "Unrecognized command or argument 'extra'.")]
    [InlineData("TEAM shared/team", null)]
    [InlineData("team--knowledge shared/team", null)]
    [InlineData("team-knowledge ../outside", null)]
    [InlineData("team-knowledge /absolute", null)]
    [InlineData("team-knowledge shared/./team", null)]
    [InlineData("team-knowledge shared/../team", null)]
    public static async Task InvalidCardinalityGrammarAndFlagsHaveNoDomainEffects(string operands, string? nativeDiagnostic)
    {
        using var workspace = new LibraryMutationWorkspace();
        var before = workspace.Snapshot();
        string[] arguments = ["library", "attach", .. operands.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);
        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        if (nativeDiagnostic is null)
        {
            Assert.Contains("invalid", result.Error, StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains(nativeDiagnostic, result.Error, StringComparison.Ordinal);
        }
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task HelpStopsBeforeMissingWorkspaceResolution()
    {
        using var workspace = new LibraryMutationWorkspace();
        var before = workspace.Snapshot();
        var result = await CliHostCapture.RunAsync(["library", "attach", "--help", "--workspace", workspace.Absolute("absent")], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("library attach", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }
}
