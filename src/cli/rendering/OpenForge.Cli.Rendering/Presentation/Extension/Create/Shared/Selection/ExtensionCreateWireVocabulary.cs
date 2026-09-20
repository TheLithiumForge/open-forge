using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;

namespace OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Selection;

internal static class ExtensionCreateWireVocabulary
{
    internal static string Name(ExtensionCreateMode mode)
        => mode switch
        {
            ExtensionCreateMode.Apply => "apply",
            ExtensionCreateMode.DryRun => "dry-run",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Extension Create mode is not defined."),
        };
}
