using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

public sealed class LibraryAttachInputIntegrationTests
{
    [Trait("Boundary", "Host")]
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
            Assert.StartsWith("Cannot attach", result.Error, StringComparison.Ordinal);
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
        var result = await CliHostCapture.RunAsync(["library", "attach", "--help", "--workspace", workspace.Absolute("absent")], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("library attach", result.Output, StringComparison.Ordinal);
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
            ? ["--workspace", badPath, "--format", "json", "library", "attach", "team-knowledge", "shared/team"]
            : ["library", "attach", "team-knowledge", "shared/team", "--workspace", badPath, "--format", "json"];

        var capture = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(ordinaryFile ? 5 : 3, capture.ExitCode);
        Assert.Empty(capture.Error);
        using var document = JsonDocument.Parse(capture.Output);
        var envelope = document.RootElement;
        Assert.Equal("library attach", envelope.GetProperty("command").GetString());
        Assert.Equal(ordinaryFile ? "blocked" : "incomplete", envelope.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, envelope.GetProperty("workspace").ValueKind);
        var data = envelope.GetProperty("data");
        var finding = Assert.Single(envelope.GetProperty("findings").EnumerateArray());
        Assert.Equal(ordinaryFile ? "library-attach.record-blocked" : "library-attach.record-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal(
            ordinaryFile
                ? ".agents/open-forge.lock.json is invalid: The selected workspace root is not a directory."
                : ".agents/open-forge.lock.json could not be read completely.",
            finding.GetProperty("message").GetString());
        Assert.Equal("apply", data.GetProperty("mode").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("destinationFolder").ValueKind);
        Assert.False(data.GetProperty("recorded").GetBoolean());
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal("team-knowledge", data.GetProperty("id").GetString());
        Assert.Equal("shared/team", data.GetProperty("sourceFolder").GetString());
    }
}
