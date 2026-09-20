using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal sealed partial class CliPrompts
{
    internal CliPlanConfirmation<TResult, TQuestion> PlanConfirmation<TResult, TData, TQuestion>(
        CliReportRendering<TResult, TData> rendering, Func<TQuestion, CliConfirmQuestion> question)
        where TResult : ICliCommandResult where TData : class where TQuestion : notnull
    {
        ArgumentNullException.ThrowIfNull(rendering);
        ArgumentNullException.ThrowIfNull(question);
        return new PlanConfirmationOperation<TResult, TData, TQuestion>(this, rendering, question).InvokeAsync;
    }

    private sealed class PlanConfirmationOperation<TResult, TData, TQuestion>
        where TResult : ICliCommandResult
        where TData : class
        where TQuestion : notnull
    {
        private readonly CliPrompts _prompts;
        private readonly CliReportRendering<TResult, TData> _rendering;
        private readonly Func<TQuestion, CliConfirmQuestion> _question;

        internal PlanConfirmationOperation(
            CliPrompts prompts,
            CliReportRendering<TResult, TData> rendering,
            Func<TQuestion, CliConfirmQuestion> question)
        {
            _prompts = prompts;
            _rendering = rendering;
            _question = question;
        }

        internal ValueTask<CliPromptReply<bool>> InvokeAsync(
            TResult preview,
            TQuestion facts,
            CliPromptPolicy policy,
            CancellationToken cancellationToken)
        {
            var request = new PlanConfirmationRequest<TResult, TData, TQuestion>(
                _rendering,
                _question,
                preview,
                facts,
                policy);
            return _prompts.RunAsync<PlanConfirmationRequest<TResult, TData, TQuestion>, bool>(
                request,
                policy,
                cancellationToken,
                static (prompts, request, token) => prompts.PlanConfirmationCoreAsync(request, token));
        }
    }

    private async ValueTask<CliPromptReply<bool>> PlanConfirmationCoreAsync<TResult, TData, TQuestion>(
        PlanConfirmationRequest<TResult, TData, TQuestion> request,
        CancellationToken cancellationToken)
        where TResult : ICliCommandResult
        where TData : class
        where TQuestion : notnull
    {
        var selected = CliReportSelection.Select(request.Preview, new CliSelection(CliDetail.Minimal), request.Rendering);
        var document = CliTextRenderer.Render(selected, _style, request.Rendering.DataTextRenderer);
        foreach (var span in document.Spans)
            await _terminal.WriteAsync((span.Authored ? span.Content : CliText.PlatformLineEndings(span.Content)).AsMemory(), cancellationToken).ConfigureAwait(false);
        await WriteFrameAsync("\n", cancellationToken).ConfigureAwait(false);
        return await ConfirmAsync(request.Question(request.Facts), request.Policy, cancellationToken).ConfigureAwait(false);
    }
}
