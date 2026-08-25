using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectAxiomsProfileBuilder
{
    private bool ReadSection(RouteSourceDocument document, out AxiomsSectionState state)
    {
        state = default;
        if (document.ReadState != FileReadState.Complete || document.Body is null)
        {
            return false;
        }

        var normalized = document.Body.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (normalized.Contains('\r'))
        {
            return false;
        }

        var lines = normalized.Split('\n', StringSplitOptions.None);
        var structural = RouteInspectMarkdownStructure.ReadStructuralLines(lines);
        var headings = Enumerable.Range(0, lines.Length)
            .Where(index => structural[index] && lines[index] == "## Axioms")
            .ToArray();
        if (headings.Length > 1)
        {
            return false;
        }

        if (headings.Length == 0)
        {
            state = AxiomsSectionState.Missing;
            return true;
        }

        var sectionEnd = Enumerable.Range(headings[0] + 1, lines.Length - headings[0] - 1)
            .FirstOrDefault(
                index => structural[index] && lines[index].StartsWith("## ", StringComparison.Ordinal),
                lines.Length);
        var content = lines[(headings[0] + 1)..sectionEnd]
            .Where(line => line.Length != 0)
            .ToArray();
        state = content switch
        {
            [] => AxiomsSectionState.Empty,
            [InheritedSentinel] => AxiomsSectionState.InheritedSentinel,
            _ => AxiomsSectionState.Substantive,
        };
        return true;
    }

    private static RouteInspectAxiomsLocalState ReadLocalState(AxiomsSectionState state)
    {
        return state switch
        {
            AxiomsSectionState.Substantive => RouteInspectAxiomsLocalState.Substantive,
            AxiomsSectionState.InheritedSentinel => RouteInspectAxiomsLocalState.InheritedSentinel,
            AxiomsSectionState.Empty => RouteInspectAxiomsLocalState.Empty,
            AxiomsSectionState.Missing => RouteInspectAxiomsLocalState.Missing,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Axioms section state is not defined."),
        };
    }

    private enum AxiomsSectionState
    {
        Substantive,
        InheritedSentinel,
        Empty,
        Missing,
    }
}
