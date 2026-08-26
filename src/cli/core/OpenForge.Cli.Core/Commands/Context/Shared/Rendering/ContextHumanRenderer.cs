using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextHumanRenderer
{
    internal static string Render(CliPresentationRequest<ContextResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return presentation.Presentation.View switch
        {
            CliView.Compact => ContextCompactHumanRenderer.Render(presentation.Result),
            CliView.Expanded => ContextExpandedHumanRenderer.Render(presentation.Result),
            _ => throw new ArgumentOutOfRangeException(
                nameof(presentation),
                presentation.Presentation.View,
                "The Context human view is not defined."),
        };
    }
}
