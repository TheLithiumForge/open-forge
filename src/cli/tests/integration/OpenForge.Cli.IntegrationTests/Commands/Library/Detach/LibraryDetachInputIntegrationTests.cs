using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Detach;

public sealed class LibraryDetachInputIntegrationTests
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
        string[] arguments = ["library", "detach", .. operands.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);
        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        if (nativeDiagnostic is null)
        {
            Assert.Contains("Cannot detach", result.Error, StringComparison.Ordinal);
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
        var result = await CliHostCapture.RunAsync(["library", "detach", "--help", "--workspace", workspace.Absolute("absent")], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("library detach", result.Output, StringComparison.Ordinal);
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

        var result = await CliHostCapture.RunAsync(["library", "detach", libraryId], workspace.Path);

        Assert.Equal(completeRecord ? 4 : 0, result.ExitCode);
        if (completeRecord)
        {
            Assert.Equal(string.Empty, result.Output);
            Assert.Contains(
                "Cannot detach other-library: No Library has the ID other-library.",
                result.Error,
                StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(string.Empty, result.Error);
            Assert.Contains(
                "No ownership record exists, so team-knowledge cannot be detached. Nothing was changed.",
                result.Output,
                StringComparison.Ordinal);
        }
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task RepeatedDetachIsInvalidAndHasNoFurtherEffect()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);

        var applied = await CliHostCapture.RunAsync(["library", "detach", "team-knowledge", "--automatic"], workspace.Path);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.Error);
        var beforeRepeat = workspace.Snapshot();

        var repeated = await CliHostCapture.RunAsync(["library", "detach", "team-knowledge"], workspace.Path);

        Assert.Equal(4, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.Output);
        Assert.Contains(
            "Cannot detach team-knowledge: No Library has the ID team-knowledge.",
            repeated.Error,
            StringComparison.Ordinal);
        Assert.Equal(beforeRepeat, workspace.Snapshot());
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
            ? ["--workspace", badPath, "--format", "json", "library", "detach", "team-knowledge"]
            : ["library", "detach", "team-knowledge", "--workspace", badPath, "--format", "json"];

        var capture = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(ordinaryFile ? 5 : 3, capture.ExitCode);
        Assert.Empty(capture.Error);
        using var document = JsonDocument.Parse(capture.Output);
        var envelope = document.RootElement;
        var payload = envelope.GetProperty("data");
        Assert.Equal("library detach", envelope.GetProperty("command").GetString());
        Assert.Equal(ordinaryFile ? "blocked" : "incomplete", envelope.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, envelope.GetProperty("workspace").ValueKind);
        var finding = Assert.Single(envelope.GetProperty("findings").EnumerateArray());
        Assert.Equal(ordinaryFile ? "library-detach.record-blocked" : "library-detach.record-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal("team-knowledge", finding.GetProperty("subject").GetProperty("id").GetString());
        Assert.Contains(
            ordinaryFile ? "is invalid" : "could not be read completely",
            finding.GetProperty("message").GetString(),
            StringComparison.Ordinal);
        Assert.Equal("apply", payload.GetProperty("mode").GetString());
        Assert.Equal("team-knowledge", payload.GetProperty("id").GetString());
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("sourceFolder").ValueKind);
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("destinationFolder").ValueKind);
        Assert.False(payload.GetProperty("registrationRemoved").GetBoolean());
        Assert.Empty(payload.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
    }
}
