using OpenForge.Cli.Core.Presentation.Context.Shared.Help;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Help;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Help;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Help;
using OpenForge.Cli.Core.Presentation.Extension.List.Shared.Help;
using OpenForge.Cli.Core.Presentation.Find.Shared.Help;
using OpenForge.Cli.Core.Presentation.Index.Shared.Help;
using OpenForge.Cli.Core.Presentation.Install.Shared.Help;
using OpenForge.Cli.Core.Presentation.References.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.List.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Help;
using OpenForge.Cli.Core.Presentation.Status.Shared.Help;
using OpenForge.Cli.Core.Presentation.Update.Shared.Help;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Shell.Presentation.Shared.Help;

public sealed class CliResultHelpTests
{
    private static readonly string Expected = string.Join(Environment.NewLine,
        "  Text completed, completed-with-warnings, and incomplete results use stdout; invalid-input, blocked, failed, and cancelled results use stderr.",
        "  JSON writes one minified schema-version-3 envelope to stdout for every semantic status. Debug diagnostics use bounded stderr.",
        "  completed: exit 0 and text stdout.",
        "  failed: exit 1 and text stderr.",
        "  completed-with-warnings: exit 2 and text stdout.",
        "  incomplete: exit 3 and text stdout.",
        "  invalid-input: exit 4 and text stderr.",
        "  blocked: exit 5 and text stderr.",
        "  cancelled: exit 130 and text stderr.");

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Every command with a Results and streams section renders the one shared body"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    public void CommandsShareOneResultsAndStreamsBody()
    {
        (string Command, CliHelpContent Help)[] commands =
        [
            ("install", InstallHelpSections.Create()),
            ("update", UpdateHelpSections.Create()),
            ("index", IndexHelpSections.Create()),
            ("status", StatusHelpSections.Create()),
            ("doctor", DoctorHelpSections.Create()),
            ("find", FindHelpSections.Create()),
            ("context", ContextHelpSections.Create()),
            ("references", ReferencesHelpSections.Create()),
            ("route init", RouteInitHelpSections.Create()),
            ("route inspect", RouteInspectHelpSections.CreateInspect()),
            ("route list", RouteListHelpSections.CreateList()),
            ("route remove", RouteRemoveHelpSections.Create()),
            ("extension create", ExtensionCreateHelpSections.Create()),
            ("extension inspect", ExtensionInspectHelpSections.Create()),
            ("extension list", ExtensionListHelpSections.Create()),
        ];

        foreach (var (command, help) in commands)
        {
            var section = Assert.Single(help.Sections, value => value.Heading == "Results and streams");
            Assert.Equal($"{command}: {Expected}", $"{command}: {section.Body}");
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Commands that close their Notes with the shared body keep it verbatim"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    public void NotesSectionsAppendTheSameSharedBody()
    {
        var moveNotes = $"  Route Move never moves lifecycle-managed content, initializes a missing parent route, overwrites a destination, prompts, or invokes Index as a subprocess.\n{Expected}";
        var updateNotes = $"  Global workspace, format, detail, detail-filter, help, and version options retain their shared meaning.\n{Expected}";

        Assert.Equal(moveNotes, Assert.Single(RouteMoveHelpSections.Create().Sections, section => section.Heading == "Notes").Body);
        Assert.Equal(updateNotes, Assert.Single(RouteUpdateHelpSections.Create().Sections, section => section.Heading == "Notes").Body);
    }
}
