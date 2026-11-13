using OpenForge.Cli.Core.Commands.Tool.Models.Interaction;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Writing;

[Trait("Feature", "tool-writers"), Trait("Evidence", "Unit")]
public sealed class ToolWriterComponentsTests
{
    private const string Component = "fixture-hook";
    private static readonly ToolWriterDisclosure Disclosure = new("fixture", "Fixture", ToolOrigin.ThirdParty, "https://github.com/fixture/tool", "MIT", false);
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Supplied component choices override saved choices without prompting")]
    [InlineData(true), InlineData(false)]
    public async Task KeepsExplicitChoice(bool enabled)
    {
        var request = Request(new() { AllowInteraction = true }) with
        { Saved = new Dictionary<string, bool> { [Component] = !enabled }, Supplied = [new(ToolComponentFlags.PromptHook, enabled)] };
        var result = await ToolWriterComponents.ResolveAsync(request, ToolWriterInteraction.Unavailable, Token);
        Assert.Empty(result.Issues);
        Assert.Equal(enabled, result.Choices[Component]);
    }

    [Theory(DisplayName = "Saved choices remain available to automatic team setup and preview")]
    [InlineData(true), InlineData(false)]
    public async Task KeepsSavedChoice(bool preview)
    {
        var request = Request(new() { Automatic = true, DryRun = preview }) with { Saved = new Dictionary<string, bool> { [Component] = false } };
        var result = await ToolWriterComponents.ResolveAsync(request, ToolWriterInteraction.Unavailable, Token);
        Assert.Empty(result.Issues);
        Assert.False(result.Choices[Component]);
    }

    [Theory(DisplayName = "An unresolved choice needs its flag when prompting isn't allowed")]
    [InlineData(false, false, false), InlineData(true, true, false), InlineData(true, false, true)]
    public async Task RequiresUnresolvedChoice(bool interaction, bool automatic, bool preview)
    {
        var asked = false;
        var prompts = ToolWriterInteraction.Unavailable with { Component = (_, _, _) => { asked = true; return ValueTask.FromResult(CliPromptReply<bool>.Answered(true)); } };
        var result = await ToolWriterComponents.ResolveAsync(Request(new() { AllowInteraction = interaction, Automatic = automatic, DryRun = preview }), prompts, Token);
        Assert.False(asked);
        Assert.Empty(result.Choices);
        Assert.Equal(ToolWriteIssueCode.ComponentChoiceMissing, Assert.Single(result.Issues).Code);
    }

    [Fact(DisplayName = "Interactive component consent carries the definition's default and saves the chosen value")]
    public async Task AsksInteractiveChoice()
    {
        ToolComponentQuestion? observed = null;
        var prompts = ToolWriterInteraction.Unavailable with
        {
            Component = (question, policy, _) =>
            {
                observed = question;
                return ValueTask.FromResult(CliPromptReply<bool>.Answered(false));
            },
        };
        var result = await ToolWriterComponents.ResolveAsync(Request(new() { AllowInteraction = true }), prompts, Token);
        Assert.Equal(Component, observed?.Id);
        Assert.True(observed?.DefaultEnabled);
        Assert.Empty(result.Issues);
        Assert.False(result.Choices[Component]);
    }

    [Fact(DisplayName = "Cancelling an optional component choice reports interruption")]
    public async Task CancelsChoice()
    {
        var prompts = ToolWriterInteraction.Unavailable with { Component = static (_, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Cancelled()) };
        var result = await ToolWriterComponents.ResolveAsync(Request(new() { AllowInteraction = true }), prompts, Token);
        Assert.Empty(result.Choices);
        Assert.Equal(ToolWriteIssueCode.Interrupted, Assert.Single(result.Issues).Code);
    }

    [Theory(DisplayName = "Unknown and duplicate component choices are invalid before prompting")]
    [InlineData(true), InlineData(false)]
    public async Task RejectsInvalidChoices(bool duplicate)
    {
        var choices = duplicate ? new[] { new ToolComponentChoice(Component, true), new ToolComponentChoice(ToolComponentFlags.PromptHook, false) }
            : [new ToolComponentChoice("unknown", true)];
        var failure = await Assert.ThrowsAsync<ToolWriterFailure>(() => ToolWriterComponents.ResolveAsync(Request(new()) with { Supplied = choices }, ToolWriterInteraction.Unavailable, Token).AsTask());
        Assert.Equal(ToolWriteIssueCode.InvalidInput, failure.Issue.Code);
    }

    private static ToolWriterComponentRequest Request(ToolWriteOptions options) => new(
        [new() { Id = Component, Flag = ToolComponentFlags.PromptHook, DefaultEnabled = true, Description = "Fixture prompt context", DeclinedEntries = [] }],
        [], null, Disclosure, options);
}
