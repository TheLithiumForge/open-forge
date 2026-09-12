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
            ["route", "inspect", CollisionId, "--workspace", workspace.Path, "--view=compact"],
            new CliProcessEnvironment(workspace.Path),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(2, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, completion.Status);
        Assert.Equal(ExpectedPrompt(), standardError.ToString());
        Assert.Contains("Status: requires attention", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains(
            $"Selection: source ID; interactive selection; requested \"{CollisionId}\"",
            standardOutput.ToString(),
            StringComparison.Ordinal);
        Assert.Equal("remaining", await standardInput.ReadLineAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Route Inspect never calls an available interactive session for JSON collisions"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task JsonCollisionNeverPromptsOrConsumesInput()
    {
        using var workspace = CreateCollisionWorkspace();

        var run = await RouteInspectInteractionApplication.RunAsync(
            ["route", "inspect", CollisionId, "--workspace", workspace.Path, "--json"],
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
            document.RootElement.GetProperty("result").GetProperty("selection")
                .GetProperty("candidatePaths").EnumerateArray().Select(value => value.GetString()));
    }

    [Fact(DisplayName = "Route Inspect never calls a redirected interactive session for human collisions"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task RedirectedHumanCollisionNeverPromptsOrConsumesInput()
    {
        using var workspace = CreateCollisionWorkspace();

        var run = await RouteInspectInteractionApplication.RunAsync(
            ["route", "inspect", CollisionId, "--workspace", workspace.Path, "--view=compact"],
            workspace.Path,
            "1\nremaining",
            canPrompt: false,
            TestContext.Current.CancellationToken);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.DoesNotContain("matches more than one source", run.StandardError, StringComparison.Ordinal);
        Assert.Contains("Status: blocked", run.StandardError, StringComparison.Ordinal);
        Assert.Equal("1", run.RemainingInput);
    }

    [Fact(DisplayName = "Route Inspect writes an interactive prompt only to stderr before the selected human result"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task InteractivePromptUsesStderrAndHumanResultUsesStdout()
    {
        using var workspace = CreateCollisionWorkspace();

        var run = await RouteInspectInteractionApplication.RunAsync(
            ["route", "inspect", CollisionId, "--workspace", workspace.Path, "--view=compact"],
            workspace.Path,
            "1\nremaining",
            canPrompt: true,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, run.Completion.Status);
        Assert.Equal(ExpectedPrompt(), run.StandardError);
        Assert.DoesNotContain("matches more than one source", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Status: requires attention", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            $"Selection: source ID; interactive selection; requested \"{CollisionId}\"",
            run.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route inspect \".agents/root/collision.md\"", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal("remaining", run.RemainingInput);
    }
}
