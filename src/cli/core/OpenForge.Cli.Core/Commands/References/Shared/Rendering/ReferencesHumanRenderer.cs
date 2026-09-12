using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesHumanRenderer
{
    internal static string Render(CliPresentationRequest<ReferencesResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return presentation.Presentation.View switch
        {
            CliView.Compact => ReferencesCompactRenderer.Render(presentation.Result),
            CliView.Expanded => ReferencesExpandedRenderer.Render(presentation.Result),
            _ => throw new ArgumentOutOfRangeException(
                nameof(presentation),
                presentation.Presentation.View,
                "The References human view is not defined."),
        };
    }
}
