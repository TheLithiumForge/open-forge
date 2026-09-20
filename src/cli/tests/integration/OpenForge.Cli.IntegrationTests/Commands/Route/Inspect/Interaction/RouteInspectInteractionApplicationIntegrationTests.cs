using System.Text.Json;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using static OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Interaction.RouteInspectInteractionIntegrationFixture;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Interaction;

public sealed class RouteInspectInteractionApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root composition injects terminal-capable input and stderr into Route Inspect"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task RootCompositionInjectsTerminalCapableRouteInspectSession()
    {
        using var workspace = CreateCollisionWorkspace();
        using var standardInput = new StringReader("1\nremaining");
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"),
            new CliCompositionInputs
            {
                StandardInput = standardInput,
                PromptOutput = standardError,
                StandardInputRedirected = false,
                PromptOutputRedirected = false,
            });

        var completion = await application.RunAsync(
            ["route", "inspect", CollisionId, "--workspace", workspace.Path, "--detail=minimal"],
            new CliProcessEnvironment(workspace.Path),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(2, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, completion.Status);
        Assert.Equal(ExpectedPrompt(), standardError.ToString());
        Assert.Contains($"{CollisionId}  {FirstCandidate}", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Selection:", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal("remaining", await standardInput.ReadLineAsync(TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Inspect never calls an available interactive session for JSON collisions"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task JsonCollisionNeverPromptsOrConsumesInput()
    {
        using var workspace = CreateCollisionWorkspace();

        var run = await RouteInspectInteractionApplication.RunAsync(
            ["route", "inspect", CollisionId, "--workspace", workspace.Path, "--format", "json", "--detail", "full"],
            workspace.Path,
            "1\nremaining",
            canPrompt: true,
            TestContext.Current.CancellationToken);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Equal("1", run.RemainingInput);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            [FirstCandidate, SecondCandidate],
            document.RootElement.GetProperty("findings")[0].GetProperty("candidates")
                .EnumerateArray().Select(candidate => candidate.GetProperty("subject").GetProperty("path").GetString()));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Inspect never calls a redirected interactive session for human collisions"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task RedirectedHumanCollisionNeverPromptsOrConsumesInput()
    {
        using var workspace = CreateCollisionWorkspace();

        var run = await RouteInspectInteractionApplication.RunAsync(
            ["route", "inspect", CollisionId, "--workspace", workspace.Path, "--detail=minimal"],
            workspace.Path,
            "1\nremaining",
            canPrompt: false,
            TestContext.Current.CancellationToken);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Contains(
            $"Cannot inspect {CollisionId}: {CollisionId} matches more than one source. Use the exact path.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal("1", run.RemainingInput);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Inspect writes an interactive prompt only to stderr before the selected human result"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task InteractivePromptUsesStderrAndHumanResultUsesStdout()
    {
        using var workspace = CreateCollisionWorkspace();

        var run = await RouteInspectInteractionApplication.RunAsync(
            ["route", "inspect", CollisionId, "--workspace", workspace.Path, "--detail=minimal"],
            workspace.Path,
            "1\nremaining",
            canPrompt: true,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, run.Completion.Status);
        Assert.Equal(ExpectedPrompt(), run.StandardError);
        Assert.Contains($"{CollisionId}  {FirstCandidate}", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Selection:", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route inspect \".agents/root/collision.md\"", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal("remaining", run.RemainingInput);
    }
}
