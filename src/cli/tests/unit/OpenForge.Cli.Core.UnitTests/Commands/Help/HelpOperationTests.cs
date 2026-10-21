using OpenForge.Cli.Core.Commands.Help;
using OpenForge.Cli.Core.Commands.Help.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Help;

public sealed class HelpOperationTests
{
    [Fact, Trait("Feature", "help"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public async Task NoTopicListsTheOnlyTopicWithoutWorkspaceOrEffects()
    {
        var result = await new HelpOperation().ExecuteAsync(new HelpRequest(null), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(["concepts"], result.Topics);
        Assert.Null(result.Topic);
        Assert.Null(result.Authoring);
        Assert.Null(result.Workspace);
        Assert.Null(result.Next);
    }

    [Theory, InlineData("unknown", false), InlineData("install", true), InlineData("Concepts", false)]
    [Trait("Feature", "help"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public async Task UnknownTopicsAreInvalidAndCommandNamesSuggestTheirOptions(string topic, bool namesCommand)
    {
        var result = await new HelpOperation().ExecuteAsync(new HelpRequest(topic, namesCommand), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(topic, result.Topic);
        Assert.Equal(["concepts"], result.Topics);
        Assert.Null(result.Authoring);
        Assert.Null(result.Workspace);
        Assert.Equal(namesCommand ? $"open-forge {topic} --help" : null, result.SuggestedCommand);
        Assert.Equal(result.SuggestedCommand ?? "open-forge help --help", result.Next?.Command);
    }

    [Fact, Trait("Feature", "help"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public async Task CancellationStopsBeforeReadingTheTopic()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await new HelpOperation().ExecuteAsync(new HelpRequest("concepts"), cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal("help.interrupted", result.FindingCode);
        Assert.Null(result.Authoring);
        Assert.Null(result.Workspace);
    }
}
