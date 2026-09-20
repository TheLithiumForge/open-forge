using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Sync;

public sealed class LibrarySyncInputIntegrationTests
{
    [Trait("Boundary", "Host")]
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
            var expected = string.IsNullOrEmpty(operands)
                ? "Cannot synchronize the supplied ID: The supplied value is not a valid Library ID."
                : $"Cannot synchronize {operands}: {operands} is not a valid Library ID.";
            Assert.Contains(expected, result.Error, StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains(nativeDiagnostic, result.Error, StringComparison.Ordinal);
        }
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Host")]
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

    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true), InlineData(false)]
    public static async Task UnknownIdIsInvalidAndMissingOwnershipIsAnObservation(bool completeRecord)
    {
        using var workspace = new LibraryMutationWorkspace();
        if (completeRecord)
        {
            workspace.Record(LibraryMutationWorkspace.Leaf);
        }

        var before = workspace.Snapshot();
        var libraryId = completeRecord ? "other-library" : "team-knowledge";

        var result = await CliHostCapture.RunAsync(["library", "sync", libraryId], workspace.Path);

        Assert.Equal(completeRecord ? 4 : 0, result.ExitCode);
        if (completeRecord)
        {
            Assert.Equal(string.Empty, result.Output);
            Assert.Contains(
                "Cannot synchronize other-library: The Library ID is not registered.",
                result.Error,
                StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(string.Empty, result.Error);
            Assert.Contains(
                "No ownership record exists, so team-knowledge cannot be synchronized. Nothing was changed.",
                result.Output,
                StringComparison.Ordinal);
        }
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true, true), InlineData(true, false), InlineData(false, true), InlineData(false, false)]
    public static async Task WorkspaceFailureRetainsParsedLibrarySubject(bool globalsFirst, bool ordinaryFile)
    {
        using var workspace = new LibraryMutationWorkspace();
        if (ordinaryFile)
        {
            workspace.Write("bad-workspace", "An ordinary file cannot be a workspace.\n");
        }

        var badPath = workspace.Absolute("bad-workspace");
        var before = workspace.Snapshot();
        string[] arguments = globalsFirst
            ? ["--workspace", badPath, "--format", "json", "library", "sync", "team-knowledge"]
            : ["library", "sync", "team-knowledge", "--workspace", badPath, "--format", "json"];

        var capture = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(ordinaryFile ? 5 : 3, capture.ExitCode);
        Assert.Empty(capture.Error);
        using var document = JsonDocument.Parse(capture.Output);
        var envelope = document.RootElement;
        Assert.Equal("library sync", envelope.GetProperty("command").GetString());
        Assert.Equal(ordinaryFile ? "blocked" : "incomplete", envelope.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, envelope.GetProperty("workspace").ValueKind);
        var finding = Assert.Single(envelope.GetProperty("findings").EnumerateArray());
        Assert.Equal(ordinaryFile ? "library-sync.record-blocked" : "library-sync.record-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal(ordinaryFile ? "The selected workspace root is not a directory" : "The selected workspace is missing", finding.GetProperty("message").GetString());
        var data = envelope.GetProperty("data");
        Assert.Equal("apply", data.GetProperty("mode").GetString());
        Assert.Equal("team-knowledge", data.GetProperty("id").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("destinationFolder").ValueKind);
        Assert.Empty(data.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
    }
}
