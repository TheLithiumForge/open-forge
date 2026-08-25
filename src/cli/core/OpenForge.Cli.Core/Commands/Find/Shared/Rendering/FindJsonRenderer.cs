using System.Text.Json;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Serialization;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindJsonRenderer
{
    internal static string Render(CliPresentationRequest<FindResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = FindJsonProjection.Create(presentation.Result);
        return JsonSerializer.Serialize(
            document,
            CliJsonContext.Default.FindJsonDocument);
    }
}
