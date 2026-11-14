using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Consent;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ToolWriteConsentOperationTests
{
    [Theory(DisplayName = "Tool yes accepts the plan without prompting in text and JSON")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task YesAcceptsPlan(bool json, bool terminal)
    {
        var result = await NeverPrompt().AskAsync(Request(ToolWriteConsentKind.Plan, yes: true, terminal: terminal, json: json), CancellationToken.None);

        Assert.Equal(ToolWriteConsentDecision.Accepted, result);
    }

    [Theory(DisplayName = "Tool apply without yes requires an interactive text session")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task UnattendedPlanRequiresConsent(bool json, bool terminal)
    {
        var result = await NeverPrompt().AskAsync(Request(ToolWriteConsentKind.Plan, terminal: terminal, json: json), CancellationToken.None);

        Assert.Equal(ToolWriteConsentDecision.MissingConsent, result);
    }

    [Theory(DisplayName = "Tool yes never accepts account uninstall shared deletion or PATH changes")]
    [InlineData((int)ToolWriteConsentKind.AccountUninstall)]
    [InlineData((int)ToolWriteConsentKind.SharedFileDeletion)]
    [InlineData((int)ToolWriteConsentKind.PathChange)]
    public async Task YesSkipsOptionalEffects(int kind)
    {
        var result = await NeverPrompt().AskAsync(Request((ToolWriteConsentKind)kind, yes: true, terminal: true), CancellationToken.None);

        Assert.Equal(ToolWriteConsentDecision.Skipped, result);
    }

    [Theory(DisplayName = "Tool unattended sessions leave optional effects unselected")]
    [InlineData((int)ToolWriteConsentKind.AccountUninstall, false, false)]
    [InlineData((int)ToolWriteConsentKind.AccountUninstall, true, true)]
    [InlineData((int)ToolWriteConsentKind.SharedFileDeletion, false, false)]
    [InlineData((int)ToolWriteConsentKind.SharedFileDeletion, true, true)]
    [InlineData((int)ToolWriteConsentKind.PathChange, false, false)]
    [InlineData((int)ToolWriteConsentKind.PathChange, true, true)]
    public async Task UnattendedSkipsOptionalEffects(int kind, bool json, bool terminal)
    {
        var result = await NeverPrompt().AskAsync(Request((ToolWriteConsentKind)kind, terminal: terminal, json: json), CancellationToken.None);

        Assert.Equal(ToolWriteConsentDecision.Skipped, result);
    }

    [Theory(DisplayName = "Tool preview never asks for plan or optional consent")]
    [InlineData((int)ToolWriteConsentKind.Plan)]
    [InlineData((int)ToolWriteConsentKind.AccountUninstall)]
    [InlineData((int)ToolWriteConsentKind.SharedFileDeletion)]
    [InlineData((int)ToolWriteConsentKind.PathChange)]
    public async Task PreviewNeverPrompts(int kind)
    {
        var request = Request((ToolWriteConsentKind)kind, terminal: true) with { Preview = true };
        var result = await NeverPrompt().AskAsync(request, CancellationToken.None);

        Assert.Equal(ToolWriteConsentDecision.Preview, result);
    }

    [Fact(DisplayName = "Tool account uninstall confirmation receives its complete declared effects")]
    public async Task AccountConfirmationReceivesDeclaredEffects()
    {
        var request = Request(ToolWriteConsentKind.AccountUninstall, terminal: true);
        var calls = 0;
        var operation = new ToolWriteConsentOperation((question, policy, token) =>
        {
            calls++;
            Assert.True(policy.Allowed);
            Assert.Same(request.Question, question);
            Assert.Equal(request.Question.Effects, question.Effects);
            Assert.Equal(CancellationToken.None, token);
            return ValueTask.FromResult(CliPromptReply<bool>.Answered(true));
        });

        Assert.Equal(ToolWriteConsentDecision.Accepted, await operation.AskAsync(request, CancellationToken.None));
        Assert.Equal(1, calls);
    }

    [Fact(DisplayName = "Declining Tool account uninstall still accepts a separate project cleanup plan")]
    public async Task DeclinedAccountUninstallDoesNotDeclineProjectCleanup()
    {
        var questions = new List<ToolWriteConsentKind>();
        var operation = new ToolWriteConsentOperation((question, _, _) =>
        {
            questions.Add(question.Kind);
            return ValueTask.FromResult(CliPromptReply<bool>.Answered(question.Kind == ToolWriteConsentKind.Plan));
        });

        var native = await operation.AskAsync(Request(ToolWriteConsentKind.AccountUninstall, terminal: true), CancellationToken.None);
        var project = await operation.AskAsync(Request(ToolWriteConsentKind.Plan, terminal: true), CancellationToken.None);

        Assert.Equal(ToolWriteConsentDecision.Skipped, native);
        Assert.Equal(ToolWriteConsentDecision.Accepted, project);
        Assert.Equal([ToolWriteConsentKind.AccountUninstall, ToolWriteConsentKind.Plan], questions);
    }

    [Theory(DisplayName = "Tool confirmations keep declines cancellations and unavailable prompts separate")]
    [InlineData(false, (int)CliPromptState.Answered, (int)ToolWriteConsentDecision.Declined)]
    [InlineData(true, (int)CliPromptState.Answered, (int)ToolWriteConsentDecision.Skipped)]
    [InlineData(false, (int)CliPromptState.Cancelled, (int)ToolWriteConsentDecision.Cancelled)]
    [InlineData(true, (int)CliPromptState.Cancelled, (int)ToolWriteConsentDecision.Cancelled)]
    [InlineData(false, (int)CliPromptState.Unavailable, (int)ToolWriteConsentDecision.MissingConsent)]
    [InlineData(true, (int)CliPromptState.Unavailable, (int)ToolWriteConsentDecision.Skipped)]
    public async Task PromptResultsStaySeparate(bool optional, int state, int expected)
    {
        var reply = (CliPromptState)state switch
        {
            CliPromptState.Answered => CliPromptReply<bool>.Answered(false),
            CliPromptState.Cancelled => CliPromptReply<bool>.Cancelled(),
            CliPromptState.Unavailable => CliPromptReply<bool>.Unavailable(),
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
        var operation = new ToolWriteConsentOperation((_, _, _) => ValueTask.FromResult(reply));
        var kind = optional ? ToolWriteConsentKind.AccountUninstall : ToolWriteConsentKind.Plan;

        Assert.Equal((ToolWriteConsentDecision)expected, await operation.AskAsync(Request(kind, terminal: true), CancellationToken.None));
    }

    [Fact(DisplayName = "Tool consent cancellation prevents prompting even with yes")]
    public async Task CancellationPreventsPrompting()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await NeverPrompt().AskAsync(Request(ToolWriteConsentKind.Plan, yes: true), cancellation.Token));
    }

    [Fact(DisplayName = "Tool consent rejects an undefined effect kind")]
    public async Task UndefinedKindIsRejected()
        => await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await NeverPrompt().AskAsync(Request((ToolWriteConsentKind)int.MaxValue), CancellationToken.None));

    private static ToolWriteConsentOperation NeverPrompt()
        => new((_, _, _) => throw new InvalidOperationException("This scenario must not prompt."));

    private static ToolWriteConsentRequest Request(ToolWriteConsentKind kind, bool yes = false, bool terminal = false, bool json = false)
        => new()
        {
            Consent = new(yes, terminal),
            Format = json ? CliFormat.Json : CliFormat.Text,
            Preview = false,
            Question = new()
            {
                Kind = kind,
                ToolId = "synthetic-tool",
                Effects =
                [
                    new ToolWriteEffectView("synthetic-binary", ToolEffectScope.Account.ToString(),
                        ToolDeclaredEffectKind.SharedFileDeletion.ToString(), "Remove the synthetic binary.", null),
                    new ToolWriteEffectView("synthetic-agent-entries", ToolEffectScope.Account.ToString(),
                        ToolDeclaredEffectKind.Configuration.ToString(), "Remove every synthetic agent entry.", null),
                ],
            },
        };
}
