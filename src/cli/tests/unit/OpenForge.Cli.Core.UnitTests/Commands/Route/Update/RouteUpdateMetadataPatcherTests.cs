using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateMetadataPatcherTests
{
    [Fact(DisplayName = "Route Update metadata patcher replaces only the parsed description span"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void ReplacesOnlyParsedDescriptionSpan()
    {
        const string source = "---\r\nopen-forge:\r\n  description: Before\r\n  responsibility: Keep\r\n  tags: [Memory]\r\n  custom: preserve # exact\r\n---\r\n\r\n# Body\r\n";
        const string expected = "---\r\nopen-forge:\r\n  description: After\r\n  responsibility: Keep\r\n  tags: [Memory]\r\n  custom: preserve # exact\r\n---\r\n\r\n# Body\r\n";

        var build = Build(source, RouteUpdateTestData.DescriptionPatch("After"));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(expected, Encoding.UTF8.GetString(patch.IntendedTargetBytes.AsSpan()));
        var preview = Assert.Single(patch.Preview);
        Assert.Equal(RouteUpdatePreviewKind.MetadataField, preview.Kind);
        Assert.Equal("description: Before", preview.Before);
        Assert.Equal("description: After", preview.Expected);
    }

    [Fact(DisplayName = "Route Update metadata patcher adds responsibility at the adjacent member convention"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void AddsResponsibilityAtAdjacentMemberConvention()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  custom: preserve\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: Before\n  responsibility: Owns decisions\n  tags: [Memory]\n  custom: preserve\n---\n\n# Body\n";

        var build = Build(
            source,
            RouteUpdateTestData.ResponsibilityPatch("Owns decisions"));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(expected, Encoding.UTF8.GetString(patch.IntendedTargetBytes.AsSpan()));
        var preview = Assert.Single(patch.Preview);
        Assert.Equal(string.Empty, preview.Before);
        Assert.Equal("responsibility: Owns decisions", preview.Expected);
    }

    [Fact(DisplayName = "Route Update metadata patcher adds a missing description beside scoped metadata"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void AddsMissingDescriptionBesideScopedMetadata()
    {
        const string source = "---\nopen-forge:\n  responsibility: Keep\n  tags: [Memory]\n  custom: preserve\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: After\n  responsibility: Keep\n  tags: [Memory]\n  custom: preserve\n---\n\n# Body\n";

        var build = Build(source, RouteUpdateTestData.DescriptionPatch("After"));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(expected, Encoding.UTF8.GetString(patch.IntendedTargetBytes.AsSpan()));
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.Description.State);
        var preview = Assert.Single(patch.Preview);
        Assert.Equal(string.Empty, preview.Before);
        Assert.Equal("description: After", preview.Expected);
    }

    [Fact(DisplayName = "Route Update metadata patcher adds missing tags beside scoped metadata"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void AddsMissingTagsBesideScopedMetadata()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  responsibility: Keep\n  custom: preserve\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: Before\n  responsibility: Keep\n  tags: [Memory, Decision]\n  custom: preserve\n---\n\n# Body\n";

        var build = Build(
            source,
            RouteUpdateTestData.TagsPatch("Memory", "Decision"));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(expected, Encoding.UTF8.GetString(patch.IntendedTargetBytes.AsSpan()));
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.Tags.State);
        var preview = Assert.Single(patch.Preview);
        Assert.Equal(string.Empty, preview.Before);
        Assert.Equal("tags: [Memory, Decision]", preview.Expected);
    }

    [Fact(DisplayName = "Route Update metadata patcher removes only the responsibility member"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void RemovesOnlyResponsibilityMember()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  responsibility: Remove me\n  tags: [Memory]\n  custom: preserve\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  custom: preserve\n---\n\n# Body\n";

        var build = Build(
            source,
            RouteUpdateTestData.ResponsibilityPatch(null));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(expected, Encoding.UTF8.GetString(patch.IntendedTargetBytes.AsSpan()));
        var preview = Assert.Single(patch.Preview);
        Assert.Equal("responsibility: Remove me", preview.Before);
        Assert.Equal(string.Empty, preview.Expected);
    }

    [Fact(DisplayName = "Route Update metadata patcher omits unrequested fields and preserves opaque YAML"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void OmitsUnrequestedFieldsAndPreservesOpaqueYaml()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  responsibility: Preserve exactly\n  tags: [Memory]\n  custom: [opaque, exact]\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: Before\n  responsibility: Preserve exactly\n  tags: [Memory, Decision]\n  custom: [opaque, exact]\n---\n\n# Body\n";

        var build = Build(
            source,
            RouteUpdateTestData.TagsPatch("Memory", "Decision"));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(expected, Encoding.UTF8.GetString(patch.IntendedTargetBytes.AsSpan()));
        Assert.Contains(
            "responsibility: Preserve exactly",
            Encoding.UTF8.GetString(patch.IntendedTargetBytes.AsSpan()),
            StringComparison.Ordinal);
        Assert.Contains(
            "custom: [opaque, exact]",
            Encoding.UTF8.GetString(patch.IntendedTargetBytes.AsSpan()),
            StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Route Update metadata patcher preserves semantic no-op spelling byte-exact"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    [InlineData("description")]
    [InlineData("tags")]
    public void PreservesSemanticNoOpSpellingByteExact(string field)
    {
        const string quotedDescription = "---\nopen-forge:\n  description: \"Before\"\n  tags: [Memory]\n---\n\n# Body\n";
        const string alternateTags = "---\nopen-forge:\n  description: Before\n  tags: [ Memory , Before ]\n---\n\n# Body\n";
        var source = field switch
        {
            "description" => quotedDescription,
            "tags" => alternateTags,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
        };
        var request = field switch
        {
            "description" => RouteUpdateTestData.DescriptionPatch("Before"),
            "tags" => RouteUpdateTestData.TagsPatch("Memory", "Before"),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
        };

        var build = Build(source, request);
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);
        var state = field switch
        {
            "description" => patch.Patch.Description.State,
            "tags" => patch.Patch.Tags.State,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
        };

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(source), patch.IntendedTargetBytes);
        Assert.Empty(patch.Preview);
        Assert.Equal(RouteUpdatePatchState.Unchanged, state);
    }

    [Fact(DisplayName = "Route Update metadata patcher preserves duplicate and nested unknown metadata"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void PreservesDuplicateAndNestedUnknownMetadata()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  custom:\n    nested: keep\n  custom: preserve-too\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: After\n  tags: [Memory]\n  custom:\n    nested: keep\n  custom: preserve-too\n---\n\n# Body\n";

        var build = Build(source, RouteUpdateTestData.DescriptionPatch("After"));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.Description.State);
        Assert.Single(patch.Preview);
    }

    [Theory(DisplayName = "Route Update metadata patcher blocks unsafe parsed YAML preservation"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    [InlineData("open-forge: {description: Before, tags: [Memory]}")]
    [InlineData("open-forge:\n  description: Before # attached comment\n  tags: [Memory]")]
    [InlineData("open-forge:\n  description : Before\n  tags: [Memory]")]
    [InlineData("open-forge:\n  description: |\n    Before\n  tags: [Memory]")]
    [InlineData("open-forge:\n  description: Before\n  description: Duplicate\n  tags: [Memory]")]
    [InlineData("open-forge:\n  description: &value Before\n  tags: [Memory]\n  custom: *value")]
    public void BlocksUnsafeParsedYamlPreservation(string yaml)
    {
        var source = $"---\n{yaml}\n---\n\n# Body\n";

        var build = Build(source, RouteUpdateTestData.DescriptionPatch("After"));
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);

        Assert.Null(build.Patch);
        Assert.Contains(
            boundary.Formation.Findings,
            finding => finding.Code == RouteUpdateFindingCode.MetadataPreservationUnsafe);
    }

    [Theory(DisplayName = "Route Update metadata patcher blocks flow-style YAML field addition and removal"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    [InlineData("open-forge: {description: Before, tags: [Memory]}", true)]
    [InlineData("open-forge: {description: Before, responsibility: Keep, tags: [Memory]}", false)]
    public void BlocksFlowStyleYamlFieldAdditionAndRemoval(
        string yaml,
        bool addResponsibility)
    {
        var source = $"---\n{yaml}\n---\n\n# Body\n";
        var patch = addResponsibility
            ? RouteUpdateTestData.ResponsibilityPatch("Owns decisions")
            : RouteUpdateTestData.ResponsibilityPatch(null);

        var build = Build(source, patch);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);

        Assert.Null(build.Patch);
        Assert.Contains(
            boundary.Formation.Findings,
            finding => finding.Code == RouteUpdateFindingCode.MetadataPreservationUnsafe);
    }

    [Fact(DisplayName = "Route Update metadata patcher preserves comments around responsibility removal"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void PreservesCommentsAroundResponsibilityRemoval()
    {
        const string source = """
            ---
            open-forge:
              description: Before
              # Ownership rationale remains authored
              responsibility: Remove me
              # Tags remain authored
              tags: [Memory]
            ---

            # Body
            """;
        const string expected = """
            ---
            open-forge:
              description: Before
              # Ownership rationale remains authored
              # Tags remain authored
              tags: [Memory]
            ---

            # Body
            """;

        var build = Build(
            source,
            RouteUpdateTestData.ResponsibilityPatch(null));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
    }

    [Fact(DisplayName = "Route Update metadata patcher blocks responsibility removal with an attached comment"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void BlocksResponsibilityRemovalWithAttachedComment()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  responsibility: Remove me # Keep ownership note\n  tags: [Memory]\n---\n\n# Body\n";

        var build = Build(
            source,
            RouteUpdateTestData.ResponsibilityPatch(null));
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);

        Assert.Null(build.Patch);
        Assert.Contains(
            boundary.Formation.Findings,
            finding => finding.Code == RouteUpdateFindingCode.MetadataPreservationUnsafe);
    }

    private static RouteUpdateMetadataPatchBuild Build(
        string source,
        RouteUpdatePatchRequest patch)
        => RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(patch, source));
}
