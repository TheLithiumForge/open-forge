using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Serialization;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextJsonRenderer
{
    internal static string Render(CliPresentationRequest<ContextResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(
            ContextJsonProjector.Create(presentation.Result),
            CliJsonContext.Default.ContextJsonDocument);
    }
}
