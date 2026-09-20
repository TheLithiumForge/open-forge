using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;

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

        var facts = new MarkdownDocumentParser().Parse(normalized);
        if (facts.BodySpan is not { } body)
        {
            return false;
        }

        var sections = MarkdownSemanticSectionReader.Find(facts.Source, body, facts.Headings, "Axioms");
        if (sections.Count > 1)
        {
            return false;
        }

        if (sections.Count == 0)
        {
            state = AxiomsSectionState.Missing;
            return true;
        }

        var section = sections[0];
        var content = normalized[section.Heading.Span.End..section.Span.End]
            .Split('\n', StringSplitOptions.None)
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
