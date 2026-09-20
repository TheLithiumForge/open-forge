using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Shell.Interaction;

public sealed class CliTerminalTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Terminal capabilities are monotonic"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void CapabilitiesRejectInvalidCombinations(bool canPrompt, bool canReadKeys, bool canRedraw)
    {
        Assert.Throws<ArgumentException>(() => new CliTerminalCapabilities(canPrompt, canReadKeys, canRedraw));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Unavailable policy performs no prompt writes or reads"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task UnavailablePolicyDoesNotTouchTheTerminal()
    {
        var scripted = ScriptedCliTerminal.Lines(["yes"]);
        var prompts = new CliPrompts(scripted.Terminal);

        var reply = await prompts.ConfirmAsync(
            new CliConfirmQuestion("Apply these changes? [y/N]"),
            new CliPromptPolicy(Allowed: false),
            CancellationToken.None);

        Assert.Equal(CliPromptState.Unavailable, reply.State);
        Assert.Equal(0, scripted.WriteCalls);
        Assert.Equal(0, scripted.LineReadCalls);
        Assert.Equal(0, scripted.KeyReadCalls);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Unavailable physical prompting does not touch the terminal"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task PhysicalPromptingUnavailableDoesNotTouchTheTerminal()
    {
        var scripted = ScriptedCliTerminal.Lines(["yes"], canPrompt: false);
        var prompts = new CliPrompts(scripted.Terminal);

        var reply = await prompts.ConfirmAsync(
            new CliConfirmQuestion("Apply these changes? [y/N]"),
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);

        Assert.Equal(CliPromptState.Unavailable, reply.State);
        Assert.Equal(0, scripted.WriteCalls);
        Assert.Equal(0, scripted.LineReadCalls);
        Assert.Equal(0, scripted.KeyReadCalls);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Policy and physical capability guards cover every prompt primitive"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task EveryPrimitiveChecksPolicyAndCapabilityBeforeIo()
    {
        var policyDenied = ScriptedCliTerminal.Lines([]);
        await AssertAllUnavailableAsync(policyDenied, new CliPromptPolicy(Allowed: false));
        AssertNoIo(policyDenied);

        var capabilityDenied = ScriptedCliTerminal.Lines([], canPrompt: false);
        await AssertAllUnavailableAsync(capabilityDenied, new CliPromptPolicy(Allowed: true));
        AssertNoIo(capabilityDenied);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Cancelled prompt token returns a cancelled reply without I/O"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task CancelledTokenDoesNotTouchTheTerminal()
    {
        var scripted = ScriptedCliTerminal.Lines(["yes"]);
        var prompts = new CliPrompts(scripted.Terminal);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var reply = await prompts.ConfirmAsync(
            new CliConfirmQuestion("Apply these changes? [y/N]"),
            new CliPromptPolicy(Allowed: true),
            cancellation.Token);

        Assert.Equal(CliPromptState.Cancelled, reply.State);
        Assert.Equal(0, scripted.WriteCalls);
        Assert.Equal(0, scripted.LineReadCalls);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Key reads are rejected when the host cannot read keys"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task KeyReadsRequireTheCapability()
    {
        var scripted = ScriptedCliTerminal.Lines([]);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await scripted.Terminal.ReadKeyAsync(CancellationToken.None));
    }

    private static async Task AssertAllUnavailableAsync(ScriptedCliTerminal scripted, CliPromptPolicy policy)
    {
        var prompts = new CliPrompts(scripted.Terminal);
        Assert.Equal(CliPromptState.Unavailable, (await prompts.ConfirmAsync(
            new CliConfirmQuestion("Confirm?"), policy, CancellationToken.None)).State);
        Assert.Equal(CliPromptState.Unavailable, (await prompts.SelectAsync(
            new CliSelectQuestion<string>("Select?", [new CliChoice<string>("one", "one")]),
            policy, CancellationToken.None)).State);
        Assert.Equal(CliPromptState.Unavailable, (await prompts.MultiSelectAsync(
            new CliMultiSelectQuestion<string>("Select?", [new CliChoice<string>("one", "one")], [], new HashSet<string>()),
            policy, CancellationToken.None)).State);
        Assert.Equal(CliPromptState.Unavailable, (await prompts.TextAsync(
            new CliTextQuestion<string>("Text?", "rule", false,
                static value => new CliTextValidation<string>(true, value, null)),
            policy, CancellationToken.None)).State);
        Assert.Equal(CliPromptState.Unavailable, (await prompts.PermissionAsync(
            new CliPermissionQuestion("owner", [new CliPermissionPath("path", false)]),
            policy, CancellationToken.None)).State);
    }

    private static void AssertNoIo(ScriptedCliTerminal scripted)
    {
        Assert.Equal(0, scripted.WriteCalls);
        Assert.Equal(0, scripted.LineReadCalls);
        Assert.Equal(0, scripted.KeyReadCalls);
    }
}
