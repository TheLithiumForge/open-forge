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
        "--format json: one minified schema-3 JSON result on stdout for every status.",
        "Exit codes: 0 completed, 1 failed, 2 completed with warnings, 3 incomplete, 4 invalid input, 5 blocked, 130 cancelled.",
        "Text exits 0, 2, 3: stdout. Others and bounded debug diagnostics: stderr.");

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Every command renders the shared status and stream body under Automation"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
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
            var section = Assert.Single(help.Sections, value => value.Heading == "Automation");
            Assert.Equal($"{command}: {Expected}", $"{command}: {section.Body}");
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Move and Update keep Automation separate from their command details"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    public void AutomationRemainsSeparateFromDetails()
    {
        foreach (var help in new[] { RouteMoveHelpSections.Create(), RouteUpdateHelpSections.Create() })
        {
            Assert.Equal(Expected, Assert.Single(help.Sections, section => section.Heading == "Automation").Body);
            Assert.DoesNotContain(help.Sections[0].Children, section => section.Body.Contains("130 cancelled", StringComparison.Ordinal));
        }
    }
}
