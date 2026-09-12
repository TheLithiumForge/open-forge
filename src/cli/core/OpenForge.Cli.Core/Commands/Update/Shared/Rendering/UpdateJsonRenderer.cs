using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Presentation.Models;
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
        var document = UpdateJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                UpdateJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document,
            UpdateJsonContext.Default.UpdateJsonDocument);
    }
}
