using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal static class ExtensionCreateOperationFactory
{
    internal static ExtensionCreateOperation Create(CliInteractiveSession interactiveSession)
    {
        ArgumentNullException.ThrowIfNull(interactiveSession);
        return new ExtensionCreateOperation(interactiveSession);
    }
}
