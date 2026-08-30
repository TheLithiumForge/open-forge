using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;
using static OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Interaction.RouteInspectInteractionIntegrationFixture;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Interaction;

public sealed class RouteInspectInteractiveCollisionIntegrationTests
{
    [Fact(DisplayName = "Route Inspect selects an ambiguous source from one one-based interactive answer"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task OneBasedAnswerSelectsDisplayedSourceOnce()
    {
        using var workspace = CreateCollisionWorkspace();
        var before = workspace.Snapshot();

        var run = await RunOperationAsync(
            workspace,
            CollisionId,
            "1\nremaining",
            allowInteractiveSourceSelection: true,
            canPrompt: true);

        AssertInteractiveAttention(run.Result, FirstCandidate);
        Assert.Equal(ExpectedPrompt(), run.Prompt);
        Assert.Equal("remaining", run.RemainingInput);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Route Inspect selects an ambiguous source from one exact displayed path answer"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task ExactDisplayedPathAnswerSelectsThatSourceOnce()
    {
        using var workspace = CreateCollisionWorkspace();

        var run = await RunOperationAsync(
            workspace,
            CollisionId,
            $"{SecondCandidate}\nremaining",
            allowInteractiveSourceSelection: true,
            canPrompt: true);

        AssertInteractiveAttention(run.Result, SecondCandidate);
        Assert.Equal(ExpectedPrompt(), run.Prompt);
        Assert.Equal("remaining", run.RemainingInput);
    }

    [Theory(DisplayName = "Route Inspect retains a blocked collision after one unusable interactive answer"),
        InlineData("invalid\nremaining", "remaining"),
        InlineData("", null),
        Trait("Feature", "route-inspect"),
        Trait("Evidence", "Integration")]
    public async Task InvalidAnswerOrEndOfInputRetainsBlockedCollision(
        string standardInput,
        string? expectedRemainingInput)
    {
        using var workspace = CreateCollisionWorkspace();

        var run = await RunOperationAsync(
            workspace,
            CollisionId,
            standardInput,
            allowInteractiveSourceSelection: true,
            canPrompt: true);

        AssertBlockedCollision(run.Result);
        Assert.Equal(ExpectedPrompt(), run.Prompt);
        Assert.Equal(expectedRemainingInput, run.RemainingInput);
    }

    [Fact(DisplayName = "Route Inspect retains known collision facts when interactive input is cancelled"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task CancellationWhileReadingInteractiveAnswerIsInterrupted()
    {
        using var workspace = CreateCollisionWorkspace();
        using var cancellation = new CancellationTokenSource();
        using var input = new CancellingTextReader(cancellation);
        using var prompt = new StringWriter();
        var session = new CliInteractiveSession(input, prompt, canPrompt: true);
        var operation = RouteInspectOperationFactory.Create(session);

        var result = await operation(
            Request(workspace, CollisionId, allowInteractiveSourceSelection: true),
            cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(RouteInspectSelectionMethod.Unresolved, result.Selection.SelectionMethod);
        Assert.Equal([FirstCandidate, SecondCandidate], result.Selection.CandidatePaths);
        Assert.Equal(RouteInspectConditionCode.Interrupted, Assert.Single(result.Conditions).Code);
        Assert.Equal(ExpectedPrompt(), prompt.ToString());
        Assert.Equal(1, input.ReadCount);
    }

    [Fact(DisplayName = "Route Inspect interactive source selection does not repair an ambiguous route"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task InteractiveSourceChoiceRetainsAmbiguousRouteBlock()
    {
        using var workspace = CreateCollisionWorkspace(ambiguousRoute: true);

        var run = await RunOperationAsync(
            workspace,
            CollisionId,
            "1\nremaining",
            allowInteractiveSourceSelection: true,
            canPrompt: true);

        Assert.Equal(CliSemanticStatus.Blocked, run.Result.Status);
        Assert.Equal(RouteInspectSelectionMethod.Interactive, run.Result.Selection.SelectionMethod);
        Assert.Equal(FirstCandidate, Assert.IsType<RouteInspectIdentity>(run.Result.Identity).CanonicalWorkspaceRelativePath);
        Assert.Contains(
            run.Result.Conditions,
            condition => condition.Code == RouteInspectConditionCode.AmbiguousRoute);
        Assert.Equal(ExpectedPrompt(), run.Prompt);
        Assert.Equal("remaining", run.RemainingInput);
    }

    [Fact(DisplayName = "Route Inspect never calls an available interactive session for an unambiguous source"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task UniqueSourceNeverPromptsOrConsumesInput()
    {
        using var workspace = CreateCollisionWorkspace();

        var run = await RunOperationAsync(
            workspace,
            "root",
            "1\nremaining",
            allowInteractiveSourceSelection: true,
            canPrompt: true);

        Assert.Equal(CliSemanticStatus.Complete, run.Result.Status);
        Assert.Equal(RouteInspectSelectionMethod.AutomaticId, run.Result.Selection.SelectionMethod);
        Assert.Equal(string.Empty, run.Prompt);
        Assert.Equal("1", run.RemainingInput);
    }

    private static void AssertInteractiveAttention(
        RouteInspectResult result,
        string expectedPath)
    {
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(RouteInspectReferenceKind.SourceId, result.Selection.ReferenceKind);
        Assert.Equal(RouteInspectSelectionMethod.Interactive, result.Selection.SelectionMethod);
        Assert.Equal(CollisionId, result.Selection.RequestedReference);
        Assert.Empty(result.Selection.CandidatePaths);
        Assert.Equal(expectedPath, Assert.IsType<RouteInspectIdentity>(result.Identity).CanonicalWorkspaceRelativePath);
        Assert.Equal(
            RouteInspectObservationCode.AutomaticIdNotUnique,
            Assert.Single(result.Observations, observation => observation.Code == RouteInspectObservationCode.AutomaticIdNotUnique).Code);
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.Equal($"open-forge route inspect \"{expectedPath}\"", next.Command);
        Assert.Equal("Rerun with the exact path for non-interactive use.", next.Reason);
    }

    private static void AssertBlockedCollision(RouteInspectResult result)
    {
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInspectSelectionMethod.Unresolved, result.Selection.SelectionMethod);
        Assert.Equal(CollisionId, result.Selection.RequestedReference);
        Assert.Equal([FirstCandidate, SecondCandidate], result.Selection.CandidatePaths);
        var condition = Assert.Single(
            result.Conditions,
            candidate => candidate.Code == RouteInspectConditionCode.AmbiguousSource);
        Assert.Equal([FirstCandidate, SecondCandidate], condition.Paths);
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.Equal($"open-forge route inspect \"{FirstCandidate}\"", next.Command);
        Assert.Equal("Rerun with one listed exact path to resolve the source collision.", next.Reason);
    }

    private sealed class CancellingTextReader(CancellationTokenSource cancellation) : TextReader
    {
        internal int ReadCount { get; private set; }

        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            ReadCount++;
            cancellation.Cancel();
            return ValueTask.FromCanceled<string?>(cancellationToken);
        }
    }
}
