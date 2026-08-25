using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Query;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Query;

public sealed class FindQueryParserRedTests
{
    [Theory(DisplayName = "Find tags use the accepted scalar grammar and equivalent predicates keep their first occurrence")]
    [InlineData("tag-unicode")]
    [InlineData("heading-unicode")]
    [InlineData("mixed-predicate-order")]
    [InlineData("invalid-grammar")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void TagGrammarAndEquivalentPredicatesUseFirstOccurrence(string scenario)
    {
        var parser = new FindQueryParser();

        switch (scenario)
        {
            case "tag-unicode":
                {
                    var query = parser.Parse(QueryInput(
                        ["#Élan-2", "élan-2", "Ångström-2"],
                        [],
                        [
                            new FindPredicateOccurrence(FindPredicateKind.Tag, "#Élan-2", 1),
                        new FindPredicateOccurrence(FindPredicateKind.Tag, "élan-2", 2),
                        new FindPredicateOccurrence(FindPredicateKind.Tag, "Ångström-2", 3),
                        ]));

                    Assert.Equal(
                        ["#Élan-2", "élan-2", "Ångström-2"],
                        query.Predicates.Select(predicate => predicate.SuppliedValue));
                    Assert.Equal(
                        ["Élan-2", "élan-2", "Ångström-2"],
                        query.Predicates.Select(predicate => predicate.ComparisonValue));
                    Assert.Equal(
                        ["#Élan-2", "Ångström-2"],
                        query.EffectivePredicates.Select(predicate => predicate.SuppliedValue));
                    Assert.Equal(
                        ["Élan-2", "Ångström-2"],
                        query.EffectivePredicates.Select(predicate => predicate.ComparisonValue));
                    break;
                }

            case "heading-unicode":
                {
                    var query = parser.Parse(QueryInput(
                        [],
                        ["Résumé, Current State", "résumé, current state", "Release, Notes"],
                        [
                            new FindPredicateOccurrence(
                            FindPredicateKind.Heading,
                            "Résumé, Current State",
                            1),
                        new FindPredicateOccurrence(
                            FindPredicateKind.Heading,
                            "résumé, current state",
                            2),
                        new FindPredicateOccurrence(FindPredicateKind.Heading, "Release, Notes", 3),
                        ]));

                    Assert.Equal(
                        ["Résumé, Current State", "résumé, current state", "Release, Notes"],
                        query.Predicates.Select(predicate => predicate.SuppliedValue));
                    Assert.Equal(
                        ["Résumé, Current State", "résumé, current state", "Release, Notes"],
                        query.Predicates.Select(predicate => predicate.ComparisonValue));
                    Assert.Equal(
                        ["Résumé, Current State", "Release, Notes"],
                        query.EffectivePredicates.Select(predicate => predicate.SuppliedValue));
                    break;
                }

            case "mixed-predicate-order":
                {
                    var query = parser.Parse(QueryInput(
                        ["#Architecture", "architecture"],
                        ["Résumé, Current State", "résumé, current state"],
                        [
                            new FindPredicateOccurrence(
                            FindPredicateKind.Heading,
                            "Résumé, Current State",
                            1),
                        new FindPredicateOccurrence(FindPredicateKind.Tag, "#Architecture", 2),
                        new FindPredicateOccurrence(
                            FindPredicateKind.Heading,
                            "résumé, current state",
                            3),
                        new FindPredicateOccurrence(FindPredicateKind.Tag, "architecture", 4),
                        ]));

                    Assert.Equal(
                        [FindPredicateKind.Heading, FindPredicateKind.Tag, FindPredicateKind.Heading, FindPredicateKind.Tag],
                        query.Predicates.Select(predicate => predicate.Kind));
                    Assert.Equal(
                        ["Résumé, Current State", "#Architecture", "résumé, current state", "architecture"],
                        query.Predicates.Select(predicate => predicate.SuppliedValue));
                    Assert.Equal(
                        ["Résumé, Current State", "#Architecture"],
                        query.EffectivePredicates.Select(predicate => predicate.SuppliedValue));
                    Assert.Equal(
                        ["Résumé, Current State", "Architecture"],
                        query.EffectivePredicates.Select(predicate => predicate.ComparisonValue));
                    break;
                }

            case "invalid-grammar":
                {
                    var invalidValues = new (FindPredicateKind Kind, string Value)[]
                    {
                    (FindPredicateKind.Tag, "#"),
                    (FindPredicateKind.Tag, "##Architecture"),
                    (FindPredicateKind.Tag, "Architecture#"),
                    (FindPredicateKind.Tag, "-Architecture"),
                    (FindPredicateKind.Tag, "Architecture-"),
                    (FindPredicateKind.Tag, "Architecture--Current"),
                    (FindPredicateKind.Tag, "1Architecture"),
                    (FindPredicateKind.Tag, "Architecture_Name"),
                    (FindPredicateKind.Tag, "Architecture Current"),
                    (FindPredicateKind.Tag, "Architecture,Current"),
                    (FindPredicateKind.Tag, " Architecture"),
                    (FindPredicateKind.Tag, "Architecture "),
                    (FindPredicateKind.Heading, ""),
                    };

                    foreach (var invalidValue in invalidValues)
                    {
                        var exception = Record.Exception(() => parser.Parse(
                            invalidValue.Kind == FindPredicateKind.Tag
                                ? QueryInput(
                                    [invalidValue.Value],
                                    [],
                                    [new FindPredicateOccurrence(invalidValue.Kind, invalidValue.Value, 1)])
                                : QueryInput(
                                    [],
                                    [invalidValue.Value],
                                    [new FindPredicateOccurrence(invalidValue.Kind, invalidValue.Value, 1)])));

                        Assert.IsType<ArgumentException>(exception);
                    }

                    break;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Find query scenario is not defined.");
        }
    }

    [Theory(DisplayName = "Find requirement remains flat for all and any and enforces predicate dependencies")]
    [InlineData("valid-modes")]
    [InlineData("invalid-dependencies")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void RequireAllAndAnyRemainFlatAndDependencyChecked(string scenario)
    {
        var parser = new FindQueryParser();
        if (scenario == "valid-modes")
        {
            var modes = new (string Name, string? Require, FindRequirement Expected)[]
            {
                ("omitted", null, FindRequirement.All),
                ("all", "all", FindRequirement.All),
                ("any", "any", FindRequirement.Any),
            };

            foreach (var mode in modes)
            {
                var query = parser.Parse(QueryInput(
                    ["Architecture"],
                    ["Instructions"],
                    [
                        new FindPredicateOccurrence(FindPredicateKind.Tag, "Architecture", 1),
                        new FindPredicateOccurrence(FindPredicateKind.Heading, "Instructions", 2),
                    ],
                    requireValue: mode.Require));

                Assert.Equal(mode.Expected, query.Requirement);
                Assert.Equal(
                    ["Architecture", "Instructions"],
                    query.Predicates.Select(predicate => predicate.SuppliedValue));
                Assert.Equal(query.Predicates, query.EffectivePredicates);
            }

            return;
        }

        if (scenario == "invalid-dependencies")
        {
            var invalidDependencies = new (string? Require, string? Within)[]
            {
                ("all", null),
                ("any", null),
                ("maybe", null),
                (null, "body"),
                (null, "document"),
            };

            foreach (var invalidDependency in invalidDependencies)
            {
                var exception = Record.Exception(() => parser.Parse(QueryInput(
                    [],
                    [],
                    [],
                    requireValue: invalidDependency.Require,
                    withinValue: invalidDependency.Within)));

                Assert.IsType<ArgumentException>(exception);
            }

            return;
        }

        throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Find requirement scenario is not defined.");
    }

    [Theory(DisplayName = "Find escaped region and content lists preserve supplied input and canonical effective order")]
    [InlineData("escaped-order")]
    [InlineData("escaped-unicode")]
    [InlineData("invalid-lists")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void EscapedRegionAndContentListsPreserveSuppliedAndEffectiveOrder(string scenario)
    {
        var parser = new FindQueryParser();
        if (scenario == "escaped-order")
        {
            const string within = "section:Rules\\, Limits, frontmatter, section:Other, section:Other";
            var query = parser.Parse(QueryInput(
                ["Architecture"],
                ["Intro"],
                [
                    new FindPredicateOccurrence(FindPredicateKind.Tag, "Architecture", 1),
                    new FindPredicateOccurrence(FindPredicateKind.Heading, "Intro", 2),
                ],
                withinValue: within,
                contentValue: "section:Rules\\, Limits, body, metadata, body, section:Other"));

            Assert.Equal(
                ["section:Rules, Limits", "frontmatter", "section:Other", "section:Other"],
                query.Within.Supplied.Select(region => region.CanonicalValue));
            Assert.Equal(
                ["frontmatter", "section:Rules, Limits", "section:Other"],
                query.Within.Tag.Select(region => region.CanonicalValue));
            Assert.Equal(
                ["section:Rules, Limits", "section:Other"],
                query.Within.Heading.Select(region => region.CanonicalValue));

            var request = BindContent(
                within,
                "section:Rules\\, Limits, body, metadata, body, section:Other");
            Assert.Equal(
                ["section:Rules, Limits", "body", "metadata", "body", "section:Other"],
                request.Presentation.Content.Supplied.Select(part => part.CanonicalValue));
            Assert.Equal(
                ["metadata", "body", "section:Rules, Limits", "section:Other"],
                request.Presentation.Content.Effective.Select(part => part.CanonicalValue));
            return;
        }

        if (scenario == "escaped-unicode")
        {
            const string within = "frontmatter, section:Résumé\\, Notes, section:Résumé\\, Notes";
            var query = parser.Parse(QueryInput(
                ["Élan-2"],
                ["Résumé, Notes"],
                [
                    new FindPredicateOccurrence(FindPredicateKind.Tag, "Élan-2", 1),
                    new FindPredicateOccurrence(FindPredicateKind.Heading, "Résumé, Notes", 2),
                ],
                withinValue: within,
                contentValue: "section:Résumé\\, Notes, headings, frontmatter, headings"));

            Assert.Equal(
                ["frontmatter", "section:Résumé, Notes", "section:Résumé, Notes"],
                query.Within.Supplied.Select(region => region.CanonicalValue));
            Assert.Equal(
                ["frontmatter", "section:Résumé, Notes"],
                query.Within.Tag.Select(region => region.CanonicalValue));
            Assert.Equal(
                ["section:Résumé, Notes"],
                query.Within.Heading.Select(region => region.CanonicalValue));

            var request = BindContent(
                within,
                "section:Résumé\\, Notes, headings, frontmatter, headings");
            Assert.Equal(
                ["section:Résumé, Notes", "headings", "frontmatter", "headings"],
                request.Presentation.Content.Supplied.Select(part => part.CanonicalValue));
            Assert.Equal(
                ["frontmatter", "headings", "section:Résumé, Notes"],
                request.Presentation.Content.Effective.Select(part => part.CanonicalValue));
            return;
        }

        if (scenario == "invalid-lists")
        {
            var invalidWithinValues = new[]
            {
                "body,,frontmatter",
                ",body",
                "body,",
                "section:",
                "section:Name\\",
                "section:Name\\q",
                "unknown",
                "metadata",
                " ",
            };
            var invalidContentValues = new[]
            {
                "metadata,,body",
                ",body",
                "body,",
                "section:",
                "section:Name\\",
                "section:Name\\q",
                "unknown",
                "paths",
                " ",
            };

            foreach (var invalidWithin in invalidWithinValues)
            {
                var exception = Record.Exception(() => parser.Parse(QueryInput(
                    ["Architecture"],
                    ["Intro"],
                    [
                        new FindPredicateOccurrence(FindPredicateKind.Tag, "Architecture", 1),
                        new FindPredicateOccurrence(FindPredicateKind.Heading, "Intro", 2),
                    ],
                    withinValue: invalidWithin)));

                Assert.IsType<ArgumentException>(exception);
            }

            foreach (var invalidContent in invalidContentValues)
            {
                var exception = Record.Exception(() => parser.Parse(QueryInput(
                    ["Architecture"],
                    ["Intro"],
                    [
                        new FindPredicateOccurrence(FindPredicateKind.Tag, "Architecture", 1),
                        new FindPredicateOccurrence(FindPredicateKind.Heading, "Intro", 2),
                    ],
                    withinValue: "body",
                    contentValue: invalidContent)));

                Assert.IsType<ArgumentException>(exception);
            }

            return;
        }

        throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Find escaped-list scenario is not defined.");
    }

    [Theory(DisplayName = "Find region compatibility and document or body subsumption remain exact")]
    [InlineData("valid-subsumption")]
    [InlineData("invalid-incompatibility")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void RegionCompatibilityAndSubsumptionAreExact(string scenario)
    {
        var parser = new FindQueryParser();
        if (scenario == "valid-subsumption")
        {
            var validSelections = new (string Within, string[] Tag, string[] Heading)[]
            {
                (
                    "document,frontmatter,body,section:Intro,section:Intro",
                    ["document"],
                    ["document"]),
                ("body,section:Intro,body", ["body"], ["body"]),
                ("frontmatter,body", ["frontmatter", "body"], ["body"]),
            };

            foreach (var selection in validSelections)
            {
                var query = parser.Parse(QueryInput(
                    ["Architecture"],
                    ["Intro"],
                    [
                        new FindPredicateOccurrence(FindPredicateKind.Tag, "Architecture", 1),
                        new FindPredicateOccurrence(FindPredicateKind.Heading, "Intro", 2),
                    ],
                    withinValue: selection.Within));

                Assert.Equal(
                    selection.Tag,
                    query.Within.Tag.Select(region => region.CanonicalValue));
                Assert.Equal(
                    selection.Heading,
                    query.Within.Heading.Select(region => region.CanonicalValue));
            }

            return;
        }

        if (scenario == "invalid-incompatibility")
        {
            var exception = Record.Exception(() => parser.Parse(QueryInput(
                [],
                ["Intro"],
                [new FindPredicateOccurrence(FindPredicateKind.Heading, "Intro", 1)],
                withinValue: "frontmatter")));

            Assert.IsType<ArgumentException>(exception);
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Find compatibility scenario is not defined.");
    }

    private static FindQueryInput QueryInput(
        IEnumerable<string> tags,
        IEnumerable<string> headings,
        IEnumerable<FindPredicateOccurrence> occurrences,
        string? requireValue = null,
        string? withinValue = null,
        string? contentValue = null)
    {
        var tagValues = tags.ToArray();
        var headingValues = headings.ToArray();
        var predicateOccurrences = occurrences.ToArray();
        return new FindQueryInput(
            [],
            [],
            tagValues,
            headingValues,
            requireValue,
            withinValue,
            contentValue,
            predicateOccurrences,
            Facts(false, 0, 0),
            Facts(false, 0, 0),
            Facts(tagValues.Length != 0, tagValues.Length, tagValues.Length),
            Facts(headingValues.Length != 0, headingValues.Length, headingValues.Length),
            Facts(requireValue is not null, requireValue is null ? 0 : 1, requireValue is null ? 0 : 1),
            Facts(withinValue is not null, withinValue is null ? 0 : 1, withinValue is null ? 0 : 1),
            Facts(contentValue is not null, contentValue is null ? 0 : 1, contentValue is null ? 0 : 1),
            requireValue,
            withinValue,
            contentValue,
            null,
            CliView.Expanded);
    }

    private static FindRequest BindContent(string withinValue, string contentValue)
    {
        var symbols = FindSymbols.Create();
        var parse = ParseFullCommand(
            symbols,
            [
                "find",
                "--tag=Architecture",
                "--heading=Intro",
                $"--within={withinValue}",
                $"--content={contentValue}",
            ]);
        Assert.Empty(parse.Result.Errors);
        var bound = new FindRequestBinder(symbols, new FindResultBuilder()).Bind(
            new CliBindingParse(parse.Result),
            Invocation(Workspace()));
        return Assert.IsType<FindRequest>(bound.Request);
    }

    private static CliOptionResultFacts Facts(
        bool explicitValue,
        int identifiers,
        int values)
        => new(explicitValue, identifiers, values);

    private static CliParseOutcome ParseFullCommand(
        FindSymbols symbols,
        string[] arguments)
    {
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(symbols.FindCommand, CliHelpContent.Empty, [])],
            []);
        return tree.Parse(arguments);
    }

    private static CliInvocation Invocation(CliWorkspace workspace)
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-query-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
