using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindHumanRenderer
{
    internal static string Render(CliPresentationRequest<FindResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return presentation.Presentation.View switch
        {
            CliView.Compact => FindCompactRenderer.Render(presentation.Result),
            CliView.Expanded => FindExpandedRenderer.Render(presentation.Result),
            _ => throw new ArgumentOutOfRangeException(
                nameof(presentation),
                presentation.Presentation.View,
                "The Find human view is not defined."),
        };
    }
}
