using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Rendering;

public sealed class LibraryHumanRendererTests
{
    [Fact(DisplayName = "Library retirement presentation describes intended membership and prior observations without claiming a source scan"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void RetirementIsNotASourceObservation()
    {
        var projection = LibraryMutationPresentationData.Projection() with
        {
            Mappings = [new()
            {
                SourcePath = "guide.md", DestinationPath = "docs/guide.md", SourceId = null,
                ExpectedRelativeLink = "../shared/guide.md", ObservedRelativeLink = "../shared/guide.md",
                State = LibraryLinkViewState.Current, Relation = LibraryComparisonRelation.Retired,
            }],
        };
        var builder = new StringBuilder();
        LibraryObservationHumanRenderer.AppendRecord(builder, LibraryMutationPresentationData.Record(), CliView.Compact);
        LibraryObservationHumanRenderer.AppendProjection(builder, projection, CliView.Compact);
        var text = builder.ToString();
        Assert.Contains("Record before change: not checked", text, StringComparison.Ordinal);
        Assert.Contains("Links before change: checks not started", text, StringComparison.Ordinal);
        Assert.Contains("Link: current; comparison: not in the intended Library", text, StringComparison.Ordinal);
        Assert.DoesNotContain("source comparison", text, StringComparison.Ordinal);
        Assert.DoesNotContain("eligible", text, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Compact Library output retains blocked ownership even without a duplicate collision finding"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void CompactOwnershipBlocker()
    {
        var projection = LibraryMutationPresentationData.Projection() with
        {
            Ownership = [new() { Path = "docs/guide.md", Kind = LibraryOwnershipKind.Blocked, OwnerId = "another-library", Cause = "Ownership cannot be established." }],
        };
        var builder = new StringBuilder();
        LibraryObservationHumanRenderer.AppendProjection(builder, projection, CliView.Compact);
        Assert.Contains("Ownership: docs/guide.md; blocked; owner another-library", builder.ToString(), StringComparison.Ordinal);
        Assert.Contains("Ownership cannot be established.", builder.ToString(), StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Library plan views keep every effect path and show hashes only as supporting expanded facts"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void PlannedEffects(int value)
    {
        var expected = new LibraryExpectedState { Kind = LibraryExpectedStateKind.Missing, Length = null, Sha256 = null, RawRelativeTarget = null };
        var plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Create) with
        {
            Directories = [new() { Path = "docs", Expected = expected }],
            GeneratedRegions = [new() { Path = ".agents/directives/_directives.md", ExpectedSha256 = "before-hash", IntendedSha256 = "after-hash", Expected = expected with { Kind = LibraryExpectedStateKind.OrdinaryFile, Length = 100, Sha256 = "before-hash" } }],
        };
        var builder = new StringBuilder();
        LibraryPlanHumanRenderer.Append(builder, plan, ".agents/open-forge.libraries.json", (CliView)value);
        var text = builder.ToString();
        Assert.Contains("Create directory: docs", text, StringComparison.Ordinal);
        Assert.Contains("Create link: docs/first.md -> ../shared/first.md", text, StringComparison.Ordinal);
        Assert.Contains("Create link: docs/second.md -> ../shared/second.md", text, StringComparison.Ordinal);
        Assert.Contains("Update generated navigation: .agents/directives/_directives.md", text, StringComparison.Ordinal);
        Assert.Contains("Record: replace (.agents/open-forge.libraries.json)", text, StringComparison.Ordinal);
        Assert.Equal(value == (int)CliView.Expanded, text.Contains("before-hash -> after-hash", StringComparison.Ordinal));
        Assert.DoesNotContain("Applied", text, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Library permission views retain missing approval paths and exact source rebinding facts"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void MissingPermissions(int value)
    {
        var permissions = LibraryPermissionView.NotEvaluated() with
        {
            Missing = [new("team", "vendor/team", "docs/guide.md")],
            Rebinding = new("vendor/previous", "vendor/team"),
        };
        var builder = new StringBuilder();
        LibraryPermissionHumanRenderer.Append(builder, permissions, (CliView)value);
        Assert.Contains("MISSING: docs/guide.md; Library team; source vendor/team", builder.ToString(), StringComparison.Ordinal);
        Assert.Contains("Source permission change: vendor/previous -> vendor/team", builder.ToString(), StringComparison.Ordinal);
    }
}
