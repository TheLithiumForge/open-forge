using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global flags]"),
            new CliHelpSection(
                "Subject and source",
                "  The operand is one exact lowercase stable ID. Without --source, available facts use the embedded catalogue; with --source, only that exact disjoint local package or catalogue is read. No fallback, network, registry, cache, or fuzzy identity is used."),
            new CliHelpSection(
                "Inspection",
                "  Inspect reports independently trusted lifecycle, available package, dependency, path, current-byte, fingerprint, generated-boundary, and comparison facts. It never writes, mutates, executes package content, or invokes another command."),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view=<compact|expanded>, --verbose, --help, and --version retain their shared Shell meaning. --view is a no-op with --json."),
            new CliHelpSection(
                "Examples",
                """
                  open-forge extension inspect development-toolkit
                  open-forge extension inspect development-toolkit --source ./packages/toolkit --json
                """),
            new CliHelpSection(
                "Results and streams",
                ResultsAndStreams()),
            new CliHelpSection(
                "Fingerprint boundary",
                "  open-forge-markdown-v1 uses strict UTF-8, LF-only normalization, exact final Entries marker exclusion, and exact-byte fallback. An authored lifecycle exact-bytes record remains read-only evidence and never creates mutation authority."),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  JSON writes one schema-version-1 envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr.",
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
