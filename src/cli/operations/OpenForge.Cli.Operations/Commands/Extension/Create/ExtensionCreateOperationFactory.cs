using OpenForge.Cli.Core.Commands.Extension.Create.Models.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal static class ExtensionCreateOperationFactory
{
    internal static ExtensionCreateOperation Create(ExtensionCreateInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        return new ExtensionCreateOperation(interaction);
    }
}
