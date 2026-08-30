using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Serialization;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Rendering;

internal static class InstallJsonRenderer
{
    internal static string Render(CliPresentationRequest<InstallResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(
            InstallJsonProjection.Create(presentation.Result),
            CliJsonContext.Default.InstallJsonDocument);
    }
}
