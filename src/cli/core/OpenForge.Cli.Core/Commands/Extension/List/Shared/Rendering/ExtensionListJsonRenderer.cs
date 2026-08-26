using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Serialization;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;

internal static class ExtensionListJsonRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionListResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(
            ExtensionListJsonProjection.Create(presentation.Result),
            CliJsonContext.Default.ExtensionListJsonDocument);
    }
}
