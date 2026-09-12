using System.Text.Json;
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
            ? ["--workspace", badPath, "--json", "library", "attach", "team-knowledge", "shared/team"]
            : ["library", "attach", "team-knowledge", "shared/team", "--workspace", badPath, "--json"];

        var capture = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(ordinaryFile ? 5 : 3, capture.ExitCode);
        Assert.Empty(capture.Error);
        using var document = JsonDocument.Parse(capture.Output);
        var envelope = document.RootElement;
        var payload = envelope.GetProperty("result");
        Assert.Equal("library attach", envelope.GetProperty("command").GetString());
        Assert.Equal(ordinaryFile ? "blocked" : "incomplete", envelope.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, envelope.GetProperty("workspace").ValueKind);
        var finding = Assert.Single(payload.GetProperty("findings").EnumerateArray());
        Assert.Equal(ordinaryFile ? "library-attach.record-blocked" : "library-attach.record-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal(ordinaryFile ? "The selected workspace root is not a directory." : "The selected workspace is missing.", finding.GetProperty("cause").GetString());
        var plan = payload.GetProperty("plan");
        Assert.Equal("not-started", plan.GetProperty("state").GetString());
        Assert.Empty(plan.GetProperty("directories").EnumerateArray());
        Assert.Empty(plan.GetProperty("links").EnumerateArray());
        Assert.Empty(plan.GetProperty("generatedRegions").EnumerateArray());
        Assert.Equal("none", plan.GetProperty("recordEffect").GetString());
        var application = payload.GetProperty("application");
        Assert.Equal("not-started", application.GetProperty("state").GetString());
        Assert.Equal("not-started", application.GetProperty("verification").GetString());
        Assert.Empty(application.GetProperty("residuals").EnumerateArray());
        Assert.Equal("not-started", application.GetProperty("recordPublication").GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, application.GetProperty("recordPublication").GetProperty("publishedLast").ValueKind);
        var identity = payload.GetProperty("identity");
        Assert.Equal("apply", identity.GetProperty("mode").GetString());
        Assert.Equal(JsonValueKind.Null, identity.GetProperty("destinationRoot").ValueKind);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal("team-knowledge", identity.GetProperty("libraryId").GetString());
        Assert.Equal("team-knowledge", finding.GetProperty("libraryId").GetString());
        Assert.Equal("shared/team", identity.GetProperty("sourceRoot").GetString());
    }
}
