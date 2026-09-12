using System.Globalization;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;

internal static partial class RepairPresentation
{
    internal static CliHelpContent CreateHelp()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge repair [--automatic] [--relink <source-location> <expected-destination> <target-path>]... [--dry-run] [global options]"),
            new CliHelpSection(
                "Selection",
                string.Join(
                    " ",
                    [
                        "  Run repair interactively to choose from the available repairs.",
                        "--automatic selects currently verified safe corrections;",
                        "--relink selects one current occurrence and contained target.",
                        "Guided candidates are never selected automatically.",
                    ])),
            new CliHelpSection(
                "Catalogue",
                """
                  Repair can correct path spelling, case, encoding, and unique fragments while preserving the target. You can also explicitly relink a missing target to a candidate reported by Doctor.
                It can also complete selected incomplete Library operations when current evidence proves the correction is safe.
                """),
            new CliHelpSection(
                "Library recovery",
                """
                  Library recovery requires a matching current-v1 recovery record, verified link identity without following the link, and current permission to change links outside .agents.
                It adds no recovery syntax, never follows or mutates source targets, and does not restore or widen permission grants.
                """),
            new CliHelpSection(
                "Preview and safety",
                string.Join(
                    " ",
                    [
                        "  --dry-run previews the complete selected plan and writes nothing.",
                        "Every eventual replacement carries complete expected and intended file states,",
                        "exact verification, and external recovery attribution.",
                    ])),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command. --view is ignored with --json."),
            new CliHelpSection("Results and streams", ResultsAndStreams()),
            new CliHelpSection(
                "Notes",
                "  Repair is stateless and local. It does not author content, change route or generated navigation, clean recovery artifacts, or invoke another command as a subprocess."),
        ]);

    internal static string? RenderDiagnostic(CliPresentationRequest<RepairResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"status={CliStatusDefinitions.Read(presentation.Result.Status).MachineName}; findings={presentation.Result.Findings.Count}; affected-paths={presentation.Result.AffectedPaths.Count}");
    }

    internal static CliRendererSet<RepairResult> CreateRenderers()
        => new(RenderHuman, RepairJsonRenderer.Render);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            string.Join(
                " ",
                [
                    $"  JSON writes one schema-version-{RepairDefinitions.SchemaVersion} envelope to stdout for every semantic status.",
                    "Verbose diagnostics use bounded stderr.",
                ]),
        };
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var definition = CliStatusDefinitions.Read(status);
            lines.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"  {definition.MachineName}: exit {definition.Disposition.ExitCode} and human {Stream(definition.Disposition.HumanOutputTarget)}."));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Stream(CliOutputTarget target)
        => target switch
        {
            CliOutputTarget.StandardOutput => "stdout",
            CliOutputTarget.StandardError => "stderr",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "The output target is not defined."),
        };
}
