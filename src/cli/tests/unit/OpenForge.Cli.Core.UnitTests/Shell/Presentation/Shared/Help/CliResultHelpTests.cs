using OpenForge.Cli.Core.Commands.Context.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Find.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Index.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Install.Shared.Rendering;
using OpenForge.Cli.Core.Commands.References.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Status.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Shell.Presentation.Shared.Help;

public sealed class CliResultHelpTests
{
    [Fact(DisplayName = "Standard command help retains the complete Results and streams body"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    public void MatchingCommandsRetainCompleteResultsAndStreams()
    {
        var expected = string.Join(Environment.NewLine,
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  JSON writes one schema-version-1 envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr.",
            "  complete: exit 0 and human stdout.",
            "  failed: exit 1 and human stderr.",
            "  attention: exit 2 and human stdout.",
            "  incomplete: exit 3 and human stdout.",
            "  invalid: exit 4 and human stderr.",
            "  blocked: exit 5 and human stderr.",
            "  interrupted: exit 130 and human stderr.");

        Assert.Equal(expected, Assert.Single(InstallHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
        Assert.Equal(expected, Assert.Single(UpdateHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
        Assert.Equal(expected, Assert.Single(IndexHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
        Assert.Equal(expected, Assert.Single(StatusHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
        Assert.Equal(expected, Assert.Single(RouteInitHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
        var moveNotes = $"  Route Move never moves lifecycle-managed content, initializes a missing parent route, overwrites a destination, prompts, or invokes Index as a subprocess.\n{expected}";
        Assert.Equal(moveNotes, Assert.Single(RouteMoveHelpSections.Create().Sections, section => section.Heading == "Notes").Body);
        var updateNotes = $"  Global workspace, JSON, view, verbosity, help, and version options retain their shared meaning.\n{expected}";
        Assert.Equal(updateNotes, Assert.Single(RouteUpdateHelpSections.Create().Sections, section => section.Heading == "Notes").Body);
        Assert.Equal(expected, Assert.Single(ContextHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
        Assert.Equal(expected, Assert.Single(ReferencesHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
        Assert.Equal(expected, Assert.Single(ExtensionCreateHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
        Assert.Equal(expected, Assert.Single(ExtensionInspectHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
        Assert.Equal(expected, Assert.Single(ExtensionListHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
    }

    [Fact(DisplayName = "Find help retains its distinct complete-envelope wording and status order"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    public void FindRetainsItsDistinctResultsAndStreams()
    {
        var expected = string.Join(Environment.NewLine,
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  JSON writes one complete schema-version-1 envelope to stdout for every semantic status. Verbose diagnostics use stderr and remain bounded.",
            "  complete: exit 0 and human stdout.",
            "  attention: exit 2 and human stdout.",
            "  incomplete: exit 3 and human stdout.",
            "  invalid: exit 4 and human stderr.",
            "  blocked: exit 5 and human stderr.",
            "  failed: exit 1 and human stderr.",
            "  interrupted: exit 130 and human stderr.");

        Assert.Equal(expected, Assert.Single(FindHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
    }

    [Fact(DisplayName = "Doctor help retains its distinct JSON sentence and status rows"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    public void DoctorRetainsItsDistinctResultsAndStreams()
    {
        var expected = string.Join(Environment.NewLine,
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  JSON writes one schema-version-1 envelope to stdout for every semantic status.",
            "  complete: exit 0.",
            "  failed: exit 1.",
            "  attention: exit 2.",
            "  incomplete: exit 3.",
            "  invalid: exit 4.",
            "  blocked: exit 5.",
            "  interrupted: exit 130.");

        Assert.Equal(expected, Assert.Single(DoctorHelpSections.Create().Sections, section => section.Heading == "Results and streams").Body);
    }

    [Fact(DisplayName = "Route Remove help retains its distinct status-row template"), Trait("Feature", "command-help"), Trait("Evidence", "Unit")]
    public void RouteRemoveRetainsItsDistinctResultsAndStreams()
    {
        var results = string.Join(Environment.NewLine,
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  JSON writes one schema-version-1 envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr.",
            "  complete: 0 (stdout)",
            "  failed: 1 (stderr)",
            "  attention: 2 (stdout)",
            "  incomplete: 3 (stdout)",
            "  invalid: 4 (stderr)",
            "  blocked: 5 (stderr)",
            "  interrupted: 130 (stderr)");

        var expected = $"  Route Remove never removes lifecycle-managed content, follows remote links, prompts, or invokes another command as a subprocess.\n{results}";
        Assert.Equal(expected, Assert.Single(RouteRemoveHelpSections.Create().Sections, section => section.Heading == "Notes").Body);
    }
}
