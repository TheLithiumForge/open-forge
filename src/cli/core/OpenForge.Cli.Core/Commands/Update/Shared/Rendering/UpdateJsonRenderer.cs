using System.Text.Json;
using OpenForge.Cli.Core.Commands.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static class UpdateJsonRenderer
{
    internal static string Render(CliPresentationRequest<UpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(
            UpdateJsonProjection.Create(presentation.Result),
            UpdateJsonContext.Default.UpdateJsonDocument);
    }
}
