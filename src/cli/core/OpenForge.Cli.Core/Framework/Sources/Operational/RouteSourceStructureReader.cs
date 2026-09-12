using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal static class RouteSourceStructureReader
{
    internal static RouteSourceStructureObservation ReadStructure(
        MarkdownDocumentFacts? document,
        SourceDocumentForm form,
        Utf8SourceMap? locations)
        => new(
            ReadTitle(document, form, locations),
            ReadAxioms(document, form, locations));

    internal static IReadOnlyList<RouteWorkspaceSourceIssue> ReadWorkspaceIssues(
        string path,
        MarkdownDocumentFacts? document,
        FrameworkDocumentMetadataFacts metadata,
        Utf8SourceMap? locations)
    {
        if (document is null || locations is null)
        {
            return [RouteWorkspaceSourceIssue.WithoutLocation(RouteWorkspaceSourceIssueKind.ParseIncomplete, path)];
        }

        if (metadata.State != FrameworkDocumentMetadataState.Malformed)
        {
            return [];
        }

        if (metadata.FailureKind == FrameworkDocumentMetadataFailureKind.Duplicate
            && metadata.FailureSpan is { } duplicate
            && document.Frontmatter.YamlSpan is { } yaml)
        {
            return
            [
                RouteWorkspaceSourceIssue.At(
                    RouteWorkspaceSourceIssueKind.FrontmatterDuplicate,
                    path,
                    locations.Map(
                        checked(yaml.Start + duplicate.Start),
                        duplicate.Length)),
            ];
        }

        return
        [
            document.Frontmatter.BlockSpan is { } frontmatter
                ? RouteWorkspaceSourceIssue.At(
                    RouteWorkspaceSourceIssueKind.FrontmatterMalformed,
                    path,
                    locations.Map(frontmatter.Start, frontmatter.Length))
                : RouteWorkspaceSourceIssue.WithoutLocation(
                    RouteWorkspaceSourceIssueKind.FrontmatterMalformed,
                    path),
        ];
    }

    private static RouteTitleObservation ReadTitle(
        MarkdownDocumentFacts? document,
        SourceDocumentForm form,
        Utf8SourceMap? locations)
    {
        if (!SourceFormClassifier.IsEntrypoint(form))
        {
            return RouteTitleObservation.NotApplicable();
        }

        if (document is null || locations is null)
        {
            return RouteTitleObservation.Boundary(RouteTitleState.Unavailable);
        }

        var headings = document.Headings.Where(heading => heading.Level == 1).ToArray();
        if (headings.Length == 0)
        {
            return RouteTitleObservation.Boundary(RouteTitleState.Missing);
        }

        if (headings.Length != 1 || string.IsNullOrWhiteSpace(headings[0].VisibleText))
        {
            return RouteTitleObservation.Boundary(
                RouteTitleState.Invalid,
                locations.Map(headings[0].Span.Start, headings[0].Span.Length));
        }

        return RouteTitleObservation.Valid(
            headings[0].VisibleText,
            locations.Map(headings[0].Span.Start, headings[0].Span.Length));
    }

    private static RouteAxiomsObservation ReadAxioms(
        MarkdownDocumentFacts? document,
        SourceDocumentForm form,
        Utf8SourceMap? locations)
    {
        if (form != SourceDocumentForm.Loader && !SourceFormClassifier.IsEntrypoint(form))
        {
            return RouteAxiomsObservation.NotApplicable();
        }

        if (document is null || locations is null)
        {
            return RouteAxiomsObservation.Boundary(RouteAxiomsState.Unavailable);
        }

        var sections = document.Sections.Where(section => section.Heading.Level == 2
            && string.Equals(section.Heading.VisibleText, "Axioms", StringComparison.Ordinal)).ToArray();
        if (sections.Length == 0)
        {
            return RouteAxiomsObservation.Boundary(RouteAxiomsState.Missing);
        }

        var section = sections[0];
        var location = locations.Map(section.Heading.Span.Start, section.Heading.Span.Length);
        if (sections.Length != 1
            || section.Span.End <= section.Heading.Span.End
            || string.IsNullOrWhiteSpace(document.Source[section.Heading.Span.End..section.Span.End]))
        {
            return RouteAxiomsObservation.Boundary(RouteAxiomsState.Invalid, location);
        }

        return RouteAxiomsObservation.Valid(location);
    }
}
