using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationFormationAmbiguityTests
{
    [Fact(DisplayName = "Root-entrypoint ambiguity accepts exact intended entrypoints for one represented folder")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void RootEntrypointKindValidatesItsSemanticShape()
    {
        var canonical = GeneratedNavigationTestData.Source(
            ".agents/root/_root.md",
            SourceDocumentForm.CanonicalEntrypoint);
        var compatibility = GeneratedNavigationTestData.Source(
            ".agents/root/index.md",
            SourceDocumentForm.IndexEntrypoint);

        var ambiguity = new GeneratedNavigationFormationAmbiguity(
            kind: GeneratedNavigationFormationAmbiguityKind.RootEntrypoint,
            subject: ".agents/root",
            intendedSources: [canonical, compatibility],
            observedCandidates: []);

        Assert.Equal(GeneratedNavigationFormationAmbiguityKind.RootEntrypoint, ambiguity.Kind);
        Assert.Equal([canonical, compatibility], ambiguity.IntendedSources);
    }

    [Fact(DisplayName = "Route-parent ambiguity accepts exact intended parents for a canonical subject")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void RouteParentKindValidatesItsSemanticShape()
    {
        var first = GeneratedNavigationTestData.Source(
            ".agents/first/_first.md",
            SourceDocumentForm.CanonicalEntrypoint);
        var second = GeneratedNavigationTestData.Source(
            ".agents/second/_second.md",
            SourceDocumentForm.CanonicalEntrypoint);

        var ambiguity = new GeneratedNavigationFormationAmbiguity(
            kind: GeneratedNavigationFormationAmbiguityKind.RouteParent,
            subject: ".agents/child.md",
            intendedSources: [first, second],
            observedCandidates: []);

        Assert.Equal(GeneratedNavigationFormationAmbiguityKind.RouteParent, ambiguity.Kind);
        Assert.Equal([first, second], ambiguity.IntendedSources);
    }

    [Fact(DisplayName = "Physical-alias ambiguity accepts its observed canonical representative")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void PhysicalAliasKindValidatesItsSemanticShape()
    {
        var target = GeneratedNavigationTestData.Physical("shared/alias.md");
        var first = GeneratedNavigationTestData.Candidate(
            ".agents/first.md",
            SourceDocumentForm.Markdown,
            target);
        var second = GeneratedNavigationTestData.Candidate(
            ".agents/second.md",
            SourceDocumentForm.Markdown,
            target);

        var ambiguity = new GeneratedNavigationFormationAmbiguity(
            kind: GeneratedNavigationFormationAmbiguityKind.PhysicalAlias,
            subject: first.CanonicalPath,
            intendedSources: [],
            observedCandidates: [first, second]);

        Assert.Equal(GeneratedNavigationFormationAmbiguityKind.PhysicalAlias, ambiguity.Kind);
        Assert.Equal([first, second], ambiguity.Candidates);
    }

    [Fact(DisplayName = "An undefined generated-navigation ambiguity kind is rejected")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void UndefinedKindIsRejected()
    {
        var undefined = (GeneratedNavigationFormationAmbiguityKind)int.MaxValue;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new GeneratedNavigationFormationAmbiguity(
            kind: undefined,
            subject: ".agents/root/_root.md",
            intendedSources: [],
            observedCandidates: []));

        Assert.Equal("kind", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }
}
