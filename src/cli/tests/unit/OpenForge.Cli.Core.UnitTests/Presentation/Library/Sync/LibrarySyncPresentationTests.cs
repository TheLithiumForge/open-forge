using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Sync;
using OpenForge.Cli.Core.Presentation.Library.Sync.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Library.Sync;

public sealed class LibrarySyncPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library sync keeps the minimal no-op headline to one line")]
    public void UpToDateMinimalIsOneLine()
    {
        var result = Result(CliSemanticStatus.Complete);
        var output = Render(result, CliFormat.Text, CliDetail.Minimal);

        Assert.Equal("The team-knowledge Library is up to date. Nothing to do.\n", output);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library sync exposes the native change data at each JSON detail level")]
    public void JsonDataFollowsDetailPolicy()
    {
        var result = Result(CliSemanticStatus.Complete) with
        {
            Result = Result(CliSemanticStatus.Complete).Result with
            {
                Plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Create),
                Application = LibraryHumanPresentationData.Interrupted(),
                Permissions = LibraryHumanPresentationData.GrantedPermission(),
            },
        };

        using var minimal = JsonDocument.Parse(Render(result, CliFormat.Json, CliDetail.Minimal));
        var minimalData = minimal.RootElement.GetProperty("data");
        Assert.True(minimalData.GetProperty("effects").GetArrayLength() > 0);
        Assert.False(minimalData.TryGetProperty("unchanged", out _));
        Assert.False(minimalData.TryGetProperty("inventory", out _));

        using var standard = JsonDocument.Parse(Render(result, CliFormat.Json, CliDetail.Standard));
        var standardData = standard.RootElement.GetProperty("data");
        Assert.True(standardData.TryGetProperty("unchanged", out _));
        Assert.True(standardData.GetProperty("effects")[0].TryGetProperty("target", out _));

        using var full = JsonDocument.Parse(Render(result, CliFormat.Json, CliDetail.Full));
        var fullData = full.RootElement.GetProperty("data");
        Assert.True(fullData.TryGetProperty("inventory", out _));
        Assert.True(fullData.TryGetProperty("expectedStates", out _));
        Assert.True(fullData.TryGetProperty("verification", out _));
        Assert.True(fullData.TryGetProperty("recovery", out _));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library sync uses the destination-specific blocked resolution")]
    public void ChangedDestinationNamesItsObservedOccupant()
    {
        var seed = Result(CliSemanticStatus.Blocked);
        var result = seed with
        {
            Result = seed.Result with
            {
                Projection = seed.Result.Projection with
                {
                    Mappings =
                    [
                        new()
                        {
                            SourcePath = ".agents/directives/review.md",
                            DestinationPath = "docs/review.md",
                            SourceId = "directives/review",
                            ExpectedRelativeLink = "../../shared/team-knowledge/.agents/directives/review.md",
                            ObservedRelativeLink = null,
                            State = OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation.LibraryLinkViewState.Changed,
                            Relation = OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation.LibraryComparisonRelation.Changed,
                        },
                    ],
                },
                Findings =
                [
                    new()
                    {
                        Code = LibrarySyncFindingCode.DestinationCollision,
                        Status = CliSemanticStatus.Blocked,
                        LibraryId = "team-knowledge",
                        Path = "docs/review.md",
                        Cause = "Library sync never adopts or replaces an unowned destination.",
                    },
                ],
            },
        };

        var output = Render(result, CliFormat.Text, CliDetail.Minimal);
        Assert.Contains("Cannot synchronize team-knowledge: docs/review.md is no longer the link the Library created. Nothing was changed.", output, StringComparison.Ordinal);
        Assert.Contains("It is now an ordinary file. Move it away or restore the link, then rerun.", output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library sync shows safe effects beside an incomplete retained destination")]
    public void IncompleteRetainedDestinationStillShowsSafeEffects()
    {
        var seed = Result(CliSemanticStatus.Incomplete);
        var result = seed with
        {
            Result = seed.Result with
            {
                Projection = seed.Result.Projection with
                {
                    Mappings =
                    [
                        new()
                        {
                            SourcePath = ".agents/directives/review.md",
                            DestinationPath = "docs/retained.md",
                            SourceId = "directives/review",
                            ExpectedRelativeLink = "../../shared/team-knowledge/.agents/directives/review.md",
                            ObservedRelativeLink = null,
                            State = OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation.LibraryLinkViewState.Changed,
                            Relation = OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation.LibraryComparisonRelation.Changed,
                        },
                    ],
                },
                Plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Create),
                Application = LibraryMutationPresentationData.Application() with
                {
                    State = LibraryApplicationState.Applied,
                },
                Findings =
                [
                    new()
                    {
                        Code = LibrarySyncFindingCode.MappingBlocked,
                        Status = CliSemanticStatus.Incomplete,
                        LibraryId = "team-knowledge",
                        Path = "docs/retained.md",
                        Cause = "The changed ordinary destination was kept; Library sync never replaces it.",
                    },
                ],
            },
        };

        var output = Render(result, CliFormat.Text, CliDetail.Minimal);

        Assert.Contains(
            "docs/retained.md is an ordinary file and is not the link the Library created.",
            output,
            StringComparison.Ordinal);
        Assert.Contains("docs/first.md", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Synchronized team-knowledge", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Nothing was changed.", output, StringComparison.Ordinal);
        Assert.Contains("could not be fully synchronized:", output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library sync names an incomplete source inventory without effects")]
    public void IncompleteInventoryNamesSourceAndStatesNoEffects()
    {
        var seed = Result(CliSemanticStatus.Incomplete);
        var result = seed with
        {
            Result = seed.Result with
            {
                Findings =
                [
                    new()
                    {
                        Code = LibrarySyncFindingCode.InventoryIncomplete,
                        Status = CliSemanticStatus.Incomplete,
                        LibraryId = "team-knowledge",
                        Path = "shared/team-knowledge",
                        Cause = "A required source subtree could not be listed.",
                    },
                ],
            },
        };

        var output = Render(result, CliFormat.Text, CliDetail.Minimal);

        Assert.Contains(
            "team-knowledge could not be synchronized: some source files under shared/team-knowledge could not be listed. Nothing was changed.",
            output,
            StringComparison.Ordinal);
        using var document = JsonDocument.Parse(Render(result, CliFormat.Json, CliDetail.Minimal));
        Assert.Empty(document.RootElement.GetProperty("data").GetProperty("effects").EnumerateArray());
    }

    private static LibrarySyncResult Result(CliSemanticStatus status)
        => new()
        {
            Status = status,
            Workspace = null,
            Next = null,
            Result = new LibrarySyncPayload
            {
                Permissions = LibraryPermissionView.NotEvaluated(),
                Identity = LibraryMutationPresentationData.Identity(false),
                Record = LibraryMutationPresentationData.Record(),
                Source = LibraryMutationPresentationData.Source(),
                Projection = LibraryMutationPresentationData.Projection(),
                Plan = LibraryMutationPresentationData.Plan(),
                Application = LibraryMutationPresentationData.Application(),
                Findings = [],
            },
        };

    private static string Render(LibrarySyncResult result, CliFormat format, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibrarySyncResult>(result, new(format, detail, null)),
            LibrarySyncPresentation.Rendering).PrimaryContent;
}
