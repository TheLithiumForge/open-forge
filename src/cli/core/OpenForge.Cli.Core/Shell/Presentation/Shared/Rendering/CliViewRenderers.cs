using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

internal sealed class CliViewRenderers<TResult>(
    CliRenderer<TResult> expanded,
    CliRenderer<TResult>? compact = null)
    where TResult : ICliCommandResult
{
    internal string Render(CliPresentationRequest<TResult> request)
    {
        switch (request.Presentation.View)
        {
            case CliView.Expanded:
                return expanded(request);
            case CliView.Compact when compact is not null:
                return compact(request);
            case CliView.Compact:
                return expanded(request with
                {
                    Presentation = request.Presentation with { View = CliView.Expanded },
                });
            default:
                throw new ArgumentOutOfRangeException(nameof(request), request.Presentation.View, "The CLI view is not defined.");
        }
    }
}
