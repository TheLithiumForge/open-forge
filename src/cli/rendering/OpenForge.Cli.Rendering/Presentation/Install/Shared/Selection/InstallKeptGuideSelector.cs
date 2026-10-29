using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.OutputText.Install;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Selection;

/// <summary>Says that a file already at the guide path stays in place and the optional guide is left out.</summary>
internal static class InstallKeptGuideSelector
{
    private const string ConceptsCommand = "open-forge help concepts";

    internal static IReadOnlyList<string> Select(InstallResult result)
        => result.Input.KeptGuide is { } path ? [InstallSetupText.KeptGuide(CliText.Escape(path), ConceptsCommand)] : [];
}
