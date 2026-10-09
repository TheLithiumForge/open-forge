using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Update.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Wording;

public sealed class MutationConfirmationWordingTests
{
    [Theory(DisplayName = "Mutation confirmations approve the full plan and name only its destructive count in both input modes")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "cli-confirmation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task ConfirmationScreens(bool keys)
    {
        var sentences = new[]
        {
            UpdateWording.Confirmation(new UpdateConfirmationFacts(ReplacementCount: 0, DeletionCount: 0)),
            UpdateWording.Confirmation(new UpdateConfirmationFacts(ReplacementCount: 0, DeletionCount: 1)),
            UpdateWording.Confirmation(new UpdateConfirmationFacts(ReplacementCount: 0, DeletionCount: 3)),
            UpdateWording.Confirmation(new UpdateConfirmationFacts(ReplacementCount: 1, DeletionCount: 0)),
            UpdateWording.Confirmation(new UpdateConfirmationFacts(ReplacementCount: 2, DeletionCount: 0)),
            UpdateWording.Confirmation(new UpdateConfirmationFacts(ReplacementCount: 1, DeletionCount: 1)),
            UpdateWording.Confirmation(new UpdateConfirmationFacts(ReplacementCount: 2, DeletionCount: 3)),
            ExtensionUpdateWording.Apply(new(ReplacementCount: 0, DeletionCount: 0)),
            ExtensionUpdateWording.Apply(new(ReplacementCount: 0, DeletionCount: 1)),
            ExtensionUpdateWording.Apply(new(ReplacementCount: 0, DeletionCount: 3)),
            ExtensionUpdateWording.Apply(new(ReplacementCount: 1, DeletionCount: 0)),
            ExtensionUpdateWording.Apply(new(ReplacementCount: 2, DeletionCount: 3)),
            ExtensionInstallWording.Apply(new(ReplacementCount: 0)),
            ExtensionInstallWording.Apply(new(ReplacementCount: 1)),
            ExtensionInstallWording.Apply(new(ReplacementCount: 2)),
            ExtensionRemoveWording.Delete(0), ExtensionRemoveWording.Delete(1), ExtensionRemoveWording.Delete(3),
            RouteRemoveWording.Confirmation(0), RouteRemoveWording.Confirmation(1), RouteRemoveWording.Confirmation(3),
            LibraryDetachWording.Confirm(0), LibraryDetachWording.Confirm(1), LibraryDetachWording.Confirm(3),
            ExtensionInstallWording.ReplaceExisting(["a.txt"]),
            ExtensionInstallWording.ReplaceExisting(["a.txt", "b.txt", "c.txt"]),
        };
        var terminal = keys
            ? ScriptedCliTerminal.Keys(Enumerable.Repeat<CliKeyStroke?>(new(CliKey.Character, 'y'), sentences.Length))
            : ScriptedCliTerminal.Lines(Enumerable.Repeat<string?>("yes", sentences.Length));
        var prompts = new CliPrompts(terminal.Terminal);
        foreach (var sentence in sentences)
        {
            var reply = await prompts.ConfirmAsync(new(sentence), new(true), TestContext.Current.CancellationToken);
            Assert.Equal(CliPromptState.Answered, reply.State);
        }
        CommandOutputSnapshot.MatchSnapshot(terminal.Output.ToString(), keys ? "key-80x24" : "line");
    }
}
