using System.CommandLine;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help;
using TheLithium.Imprint;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Inspect.Shared.Rendering;

[Trait("Feature", "state-retirement-output"), Trait("Evidence", "Unit")]
public sealed class ExtensionInspectHelpSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact] public void ProductHelp() => Render().AssertSnapshot();

    private static string Render()
        => CliHelpRenderer.Render(new Command("inspect").Parse([]), ExtensionInspectHelpSections.Create(), 100);
}
