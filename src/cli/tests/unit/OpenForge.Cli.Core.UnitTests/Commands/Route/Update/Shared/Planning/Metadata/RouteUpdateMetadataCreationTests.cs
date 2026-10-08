using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update.Shared.Planning.Metadata;

[Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
public sealed class RouteUpdateMetadataCreationTests
{
    [Theory(DisplayName = "Route Update creates metadata in each workspace form and preserves foreign frontmatter"), Trait("Boundary", "Processing")]
    [InlineData(false)]
    [InlineData(true)]
    public void CreatesEachWorkspaceForm(bool root)
    {
        const string source = "---\ntitle: café 🙂 # retained\n---\n# Exact body\n";
        var members = root ? "description: After\ntags: [Memory]\n" : "open-forge:\n  description: After\n  tags: [Memory]\n";
        AssertCreation(source, $"---\ntitle: café 🙂 # retained\n{members}---\n# Exact body\n", root);
    }

    [Theory(DisplayName = "Route Update inserts new frontmatter after BOM without changing the body"), Trait("Boundary", "Processing")]
    [InlineData("\uFEFF# Exact body 🙂\r\n", "\r\n")]
    [InlineData("# Exact body\n", "\n")]
    [InlineData("\uFEFF", "\n")]
    [InlineData("", "\n")]
    public void InsertsAfterBomWithoutChangingBody(string source, string newline)
    {
        var bom = source.StartsWith('\uFEFF') ? "\uFEFF" : string.Empty;
        AssertCreation(source, $"{bom}---{newline}description: After{newline}tags: [Memory]{newline}---{newline}{source[bom.Length..]}", true);
    }

    [Theory(DisplayName = "Metadata creation enriches an empty frontmatter block without rewriting its boundary"), Trait("Boundary", "Processing")]
    [InlineData("")]
    [InlineData("{}\n")]
    public void EnrichesEmptyFrontmatter(string yaml)
        => AssertCreation($"---\n{yaml}---\n# Body\n", "---\ndescription: After\ntags: [Memory]\n---\n# Body\n", true);

    [Theory(DisplayName = "Metadata creation retains existing root applicability without a duplicate declaration"), Trait("Boundary", "Processing")]
    [InlineData(false)]
    [InlineData(true)]
    public void RetainsExistingRootApplyTo(bool root)
    {
        const string source = "---\napplyTo: [\"**/*.cs\"] # retained\n---\n# Body\n";
        var members = root ? "description: After\ntags: [Memory]\n" : "open-forge:\n  description: After\n  tags: [Memory]\n";
        AssertCreation(source, $"---\napplyTo: [\"**/*.cs\"] # retained\n{members}---\n# Body\n", root);
    }

    [Theory(DisplayName = "Incomplete metadata creation returns InvalidPatch with no plan"), Trait("Boundary", "Processing")]
    [InlineData(true)]
    [InlineData(false)]
    public void IncompleteMetadataHasNoPlan(bool description)
    {
        var request = description ? RouteUpdateTestData.DescriptionPatch("After")
            : RouteUpdateTestData.Patch(tags: new RouteUpdateTagsRequest { Requested = true, Values = ["Memory"] });
        var build = RouteUpdateTestData.MetadataPatcher().Build(RouteUpdateTestData.Observation(request, "# Body\n"));
        Assert.Null(build.Patch);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
        Assert.Contains(boundary.Formation.Findings, finding => finding.Code == RouteUpdateFindingCode.InvalidPatch);
    }

    private static void AssertCreation(string source, string expected, bool root)
    {
        var observation = RouteUpdateTestData.Observation(RouteUpdateTestData.Patch(
            description: new RouteUpdateDescriptionRequest { Requested = true, Value = "After" },
            tags: new RouteUpdateTagsRequest { Requested = true, Values = ["Memory"] }), source) with
        {
            MetadataSettings = new WorkspaceSettingsRead(WorkspaceSettingsReadState.Complete,
                WorkspaceSettingsDocument.Empty with { DeclaredFrontmatter = root ? FrontmatterForm.Root : FrontmatterForm.Scoped },
                ".agents/open-forge.json", Cause: null),
        };
        var build = RouteUpdateTestData.MetadataPatcher().Build(observation);
        Assert.True(build.Patch is not null, build.Boundary?.Formation.Findings[0].Cause);
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
    }
}
