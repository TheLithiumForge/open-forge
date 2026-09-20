using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Presentation.Library.Attach;
using OpenForge.Cli.Core.Presentation.Library.Attach.Models;
using OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Library.Detach;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Library.Detach.Models;
using OpenForge.Cli.Core.Presentation.Library.Sync;
using OpenForge.Cli.Core.Presentation.Library.Sync.Models;
using OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;

internal static class LibraryPermissionTestPrompt
{
    internal static CliPrompt<CliPermissionQuestion, CliPermissionChoice> Create(
        TextReader input,
        TextWriter output,
        bool canPrompt)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        return new CliPrompts(CreateTerminal(input, output, canPrompt)).PermissionAsync;
    }

    internal static TextReader CancelOnRead(CancellationTokenSource cancellation, Action read)
    {
        ArgumentNullException.ThrowIfNull(cancellation);
        ArgumentNullException.ThrowIfNull(read);
        return new CancellingInput(cancellation, read);
    }

    internal static CliPlanConfirmation<LibraryAttachResult, LibraryAttachPlan> AttachTerminalConfirmation(
        TextReader input,
        TextWriter output,
        bool canPrompt)
        => new CliPrompts(CreateTerminal(input, output, canPrompt)).PlanConfirmation<LibraryAttachResult, LibraryAttachData, LibraryAttachPlan>(
            LibraryAttachPresentation.Rendering,
            static plan => LibraryAttachInteractionPresentation.CreateConfirmationQuestion(plan));

    internal static CliPlanConfirmation<LibrarySyncResult, LibrarySyncPlan> SyncTerminalConfirmation(
        TextReader input,
        TextWriter output,
        bool canPrompt)
        => new CliPrompts(CreateTerminal(input, output, canPrompt)).PlanConfirmation<LibrarySyncResult, LibrarySyncData, LibrarySyncPlan>(
            LibrarySyncPresentation.Rendering,
            static plan => LibrarySyncInteractionPresentation.CreateConfirmationQuestion(plan));

    internal static CliPlanConfirmation<LibraryDetachResult, LibraryDetachPlan> DetachTerminalConfirmation(
        TextReader input,
        TextWriter output,
        bool canPrompt)
        => new CliPrompts(CreateTerminal(input, output, canPrompt)).PlanConfirmation<LibraryDetachResult, LibraryDetachData, LibraryDetachPlan>(
            LibraryDetachPresentation.Rendering,
            static plan => LibraryDetachInteractionPresentation.CreateConfirmationQuestion(plan));

    internal static CliPlanConfirmation<TResult, TQuestion> Confirm<TResult, TQuestion>()
        where TResult : ICliCommandResult
        where TQuestion : notnull
        => static (_, _, policy, _) => ValueTask.FromResult(
            policy.Allowed
                ? CliPromptReply<bool>.Answered(true)
                : CliPromptReply<bool>.Unavailable());

    internal static CliPlanConfirmation<TResult, TQuestion> Cancel<TResult, TQuestion>()
        where TResult : ICliCommandResult
        where TQuestion : notnull
        => static (_, _, policy, _) => ValueTask.FromResult(
            policy.Allowed
                ? CliPromptReply<bool>.Cancelled()
                : CliPromptReply<bool>.Unavailable());

    internal static CliPlanConfirmation<LibraryAttachResult, LibraryAttachPlan> AttachConfirmation()
        => Confirm<LibraryAttachResult, LibraryAttachPlan>();

    internal static CliPlanConfirmation<LibrarySyncResult, LibrarySyncPlan> SyncConfirmation()
        => Confirm<LibrarySyncResult, LibrarySyncPlan>();

    internal static CliPlanConfirmation<LibraryDetachResult, LibraryDetachPlan> DetachConfirmation()
        => Confirm<LibraryDetachResult, LibraryDetachPlan>();

    internal static CliPlanConfirmation<LibraryAttachResult, LibraryAttachPlan> AttachCancellation()
        => Cancel<LibraryAttachResult, LibraryAttachPlan>();

    internal static CliPlanConfirmation<LibrarySyncResult, LibrarySyncPlan> SyncCancellation()
        => Cancel<LibrarySyncResult, LibrarySyncPlan>();

    internal static CliPlanConfirmation<LibraryDetachResult, LibraryDetachPlan> DetachCancellation()
        => Cancel<LibraryDetachResult, LibraryDetachPlan>();

    private static CliTerminal CreateTerminal(TextReader input, TextWriter output, bool canPrompt)
        => new(
            new CliTerminalCapabilities(canPrompt, canReadKeys: false, canRedraw: false),
            async (content, cancellationToken) =>
                await output.WriteAsync(content, cancellationToken).ConfigureAwait(false),
            async cancellationToken => await input.ReadLineAsync(cancellationToken).ConfigureAwait(false),
            static _ => ValueTask.FromResult<CliKeyStroke?>(null));

    private sealed class CancellingInput(CancellationTokenSource cancellation, Action read)
        : StringReader("always\n")
    {
        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            read();
            var answer = ReadLine();
            cancellation.Cancel();
            return ValueTask.FromResult(answer);
        }
    }

}
