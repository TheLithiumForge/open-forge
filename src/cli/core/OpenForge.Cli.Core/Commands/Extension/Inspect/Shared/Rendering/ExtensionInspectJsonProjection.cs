using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectJsonProjection
{
    internal static ExtensionInspectJsonDocument Create(ExtensionInspectResult result)
        => ExtensionInspectJsonDocumentProjector.Create(result);
}
