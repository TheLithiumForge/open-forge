using System.Text.Json;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Serialization;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesJsonRenderer
{
    internal static string Render(CliPresentationRequest<ReferencesResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = ReferencesJsonProjection.Create(presentation.Result);
        return JsonSerializer.Serialize(document, CliJsonContext.Default.ReferencesJsonDocument);
    }
}
