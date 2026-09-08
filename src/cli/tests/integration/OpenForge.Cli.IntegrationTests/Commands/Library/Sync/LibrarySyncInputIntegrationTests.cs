using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Sync;

public sealed class LibrarySyncInputIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("", null)]
    [InlineData("team-knowledge extra extra", "Unrecognized command or argument 'extra'.")]
    [InlineData("TEAM", null)]
    [InlineData("team--knowledge", null)]
    [InlineData("team/knowledge", null)]
    [InlineData("-team", null)]
    [InlineData("team-", null)]
    [InlineData("team-knowledge shared/team", "Unrecognized command or argument 'shared/team'.")]
    [InlineData("team-knowledge --force", "Unrecognized command or argument '--force'.")]
    [InlineData("team-knowledge --apply", "Unrecognized command or argument '--apply'.")]
    [InlineData("team-knowledge --yes", "Unrecognized command or argument '--yes'.")]
    public static async Task InvalidCardinalityGrammarAndFlagsHaveNoDomainEffects(string operands, string? nativeDiagnostic)
    {
        using var workspace = new LibraryMutationWorkspace();
        var before = workspace.Snapshot();
        string[] arguments = ["library", "sync", .. operands.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
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
        var result = await CliHostCapture.RunAsync(["library", "sync", "--help", "--workspace", workspace.Absolute("absent")], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("library sync", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true), InlineData(false)]
    public static async Task UnknownIdAndMissingRecordUseInvalidExitAndStandardError(bool completeRecord)
    {
        using var workspace = new LibraryMutationWorkspace();
        if (completeRecord)
        {
            workspace.Record(LibraryMutationWorkspace.Leaf);
        }

        var before = workspace.Snapshot();
        var libraryId = completeRecord ? "other-library" : "team-knowledge";

        var result = await CliHostCapture.RunAsync(["library", "sync", libraryId], workspace.Path);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        Assert.Contains("Status: invalid", result.Error, StringComparison.Ordinal);
        Assert.Contains("library-sync.unknown-id", result.Error, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }
}
