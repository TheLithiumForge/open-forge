using System.CommandLine;
using OpenForge.Cli.Core.Presentation.Extension.List.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help;
using TheLithium.Imprint;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.List.Shared.Rendering;

[Trait("Feature", "state-retirement-output"), Trait("Evidence", "Unit")]
public sealed class ExtensionListHelpSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact] public void ProductHelp() => Render().AssertSnapshot();

    private static string Render()
        => CliHelpRenderer.Render(new Command("list").Parse([]), ExtensionListHelpSections.Create(), 100);
}
