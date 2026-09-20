using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Presentation.Library.Detach;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Detach.Shared.Rendering;

public sealed class LibraryDetachPresentationTests
{
    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, (int)CliOutputTarget.StandardOutput, 0)]
    [InlineData((int)CliSemanticStatus.Attention, (int)CliOutputTarget.StandardOutput, 2)]
    [InlineData((int)CliSemanticStatus.Incomplete, (int)CliOutputTarget.StandardOutput, 3)]
    [InlineData((int)CliSemanticStatus.Invalid, (int)CliOutputTarget.StandardError, 4)]
    [InlineData((int)CliSemanticStatus.Blocked, (int)CliOutputTarget.StandardError, 5)]
    [InlineData((int)CliSemanticStatus.Failed, (int)CliOutputTarget.StandardError, 1)]
    [InlineData((int)CliSemanticStatus.Interrupted, (int)CliOutputTarget.StandardError, 130)]
    public void StatusCoordinates(int statusValue, int targetValue, int exitCode)
    {
        var result = Result((CliSemanticStatus)statusValue);
        var rendering = CliRenderingStage.Render(
            new CliPresentationRequest<LibraryDetachResult>(
                result,
                new(CliFormat.Text, CliDetail.Minimal, null)),
            LibraryDetachPresentation.Rendering);

        Assert.Equal((CliOutputTarget)targetValue, rendering.PrimaryTarget);
        Assert.Equal(exitCode, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
        if (result.Status is not (CliSemanticStatus.Failed or CliSemanticStatus.Interrupted))
        {
            Assert.Contains("team-knowledge", rendering.PrimaryContent, StringComparison.Ordinal);
        }
        Assert.DoesNotContain("Record before change", rendering.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Source location", rendering.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Inside workspace", rendering.PrimaryContent, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void NativeDataKeepsEffectsAndSelectsFullFacts()
    {
        var seed = Result(CliSemanticStatus.Interrupted);
        var result = seed with
        {
            Result = seed.Result with
            {
                Plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Delete),
                Application = LibraryHumanPresentationData.Interrupted(),
                Permissions = LibraryHumanPresentationData.GrantedPermission(),
            },
        };

        using var document = JsonDocument.Parse(Render(result, CliFormat.Json, CliDetail.Full));
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(["mode", "id", "sourceFolder", "destinationFolder", "registrationRemoved", "permissions", "effects", "expectedStates", "verification", "recovery"],
            data.EnumerateObject().Select(property => property.Name));
        Assert.Equal("team-knowledge", data.GetProperty("id").GetString());
        Assert.Equal(3, data.GetProperty("effects").GetArrayLength());
        Assert.Equal(JsonValueKind.Array, data.GetProperty("expectedStates").ValueKind);
        Assert.DoesNotContain("identity", data.EnumerateObject().Select(property => property.Name));
        Assert.DoesNotContain("application", data.EnumerateObject().Select(property => property.Name));
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    [InlineData((int)CliDetail.Full)]
    [InlineData((int)CliDetail.Debug)]
    public void ForbiddenLegacyCoordinatesNeverAppear(int detailValue)
    {
        var text = Render(Result(CliSemanticStatus.Interrupted), CliFormat.Text, (CliDetail)detailValue);
        Assert.DoesNotContain("Record before change", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Source location", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Inside workspace", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)LibraryDetachOccupantKind.OrdinaryFile, "an ordinary file")]
    [InlineData((int)LibraryDetachOccupantKind.Folder, "a folder")]
    [InlineData((int)LibraryDetachOccupantKind.DifferentLink, "a different link")]
    public void MappingBlockedRendersTheObservedOccupantKind(int occupantKindValue, string wording)
    {
        var seed = Result(CliSemanticStatus.Blocked);
        var result = seed with
        {
            Result = seed.Result with
            {
                Plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Delete),
                Application = LibraryMutationPresentationData.Application() with
                {
                    State = LibraryApplicationState.Applied,
                },
                Findings =
                [
                    new LibraryDetachFinding
                    {
                        Code = LibraryDetachFindingCode.MappingBlocked,
                        Status = CliSemanticStatus.Blocked,
                        LibraryId = "team-knowledge",
                        Path = ".agents/directives/review.md",
                        Cause = "The destination is occupied.",
                        OccupantKind = (LibraryDetachOccupantKind)occupantKindValue,
                    },
                ],
            },
        };

        var text = Render(result, CliFormat.Text, CliDetail.Minimal);

        var message = $".agents/directives/review.md is {wording} and is not the link the Library created.";
        Assert.Contains(message, text, StringComparison.Ordinal);
        Assert.Equal(1, text.Split(message, StringSplitOptions.None).Length - 1);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void OrdinaryChangedDestinationRendersAsKeptWarning()
    {
        var seed = Result(CliSemanticStatus.Attention);
        var result = seed with
        {
            Result = seed.Result with
            {
                Plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Delete),
                Application = LibraryMutationPresentationData.Application() with
                {
                    State = LibraryApplicationState.Applied,
                },
                Findings =
                [
                    new LibraryDetachFinding
                    {
                        Code = LibraryDetachFindingCode.MappingBlocked,
                        Status = CliSemanticStatus.Attention,
                        LibraryId = "team-knowledge",
                        Path = ".agents/directives/review.md",
                        Cause = "The changed ordinary destination was kept.",
                        OccupantKind = LibraryDetachOccupantKind.OrdinaryFile,
                    },
                ],
            },
        };

        var text = Render(result, CliFormat.Text, CliDetail.Minimal);

        const string message = ".agents/directives/review.md is an ordinary file, so its bytes were kept.";
        Assert.Contains(message, text, StringComparison.Ordinal);
        Assert.Equal(1, text.Split(message, StringSplitOptions.None).Length - 1);
        Assert.Contains("link removed", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void MissingDestinationRendersAsAlreadyAbsentWarning()
    {
        var seed = Result(CliSemanticStatus.Attention);
        var result = seed with
        {
            Result = seed.Result with
            {
                Plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Delete),
                Application = LibraryMutationPresentationData.Application() with
                {
                    State = LibraryApplicationState.Applied,
                },
                Findings =
                [
                    new LibraryDetachFinding
                    {
                        Code = LibraryDetachFindingCode.RegisteredLinkMissing,
                        Status = CliSemanticStatus.Attention,
                        LibraryId = "team-knowledge",
                        Path = ".agents/directives/review.md",
                        Cause = "The registered Library mapping is already absent; no file was removed.",
                        OccupantKind = null,
                    },
                ],
            },
        };

        var text = Render(result, CliFormat.Text, CliDetail.Minimal);

        const string message = ".agents/directives/review.md is already absent, so no file was removed.";
        Assert.Contains(message, text, StringComparison.Ordinal);
        Assert.Equal(1, text.Split(message, StringSplitOptions.None).Length - 1);
        Assert.Contains("link removed", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void DryRunAttentionUsesWouldDetachHeadline()
    {
        var seed = Result(CliSemanticStatus.Attention);
        var result = seed with
        {
            Result = seed.Result with
            {
                Identity = LibraryMutationPresentationData.Identity(true) with { Mode = LibraryMode.DryRun },
                Plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Delete),
            },
        };

        var text = Render(result, CliFormat.Text, CliDetail.Minimal);

        Assert.Contains("Would detach team-knowledge", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Detached team-knowledge", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(
        (int)LibraryDetachFindingCode.DestinationProtected,
        ".agents/directives/review.md",
        ".agents/directives/review.md is protected, owned by the source, or registered to another Library.",
        "open-forge library list")]
    public void DestinationProtectedFindingRendersDedicatedMessageAndAction(
        int codeValue,
        string path,
        string message,
        string nextCommand)
    {
        var seed = Result(CliSemanticStatus.Blocked);
        var result = seed with
        {
            Result = seed.Result with
            {
                Findings =
                [
                    new LibraryDetachFinding
                    {
                        Code = (LibraryDetachFindingCode)codeValue,
                        Status = CliSemanticStatus.Blocked,
                        LibraryId = "team-knowledge",
                        Path = path,
                        Cause = message,
                        OccupantKind = null,
                    },
                ],
            },
        };

        var text = Render(result, CliFormat.Text, CliDetail.Minimal);

        Assert.Contains(message, text, StringComparison.Ordinal);
        Assert.Contains($"Next: {nextCommand}", text, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(Render(result, CliFormat.Json, CliDetail.Full));
        var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("library-detach.destination-protected", finding.GetProperty("code").GetString());
        Assert.Equal(message, finding.GetProperty("message").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void MappingBlockedWithUnclassifiedLeafRendersClassificationFailure()
    {
        var seed = Result(CliSemanticStatus.Blocked);
        var result = seed with
        {
            Result = seed.Result with
            {
                Findings =
                [
                    new LibraryDetachFinding
                    {
                        Code = LibraryDetachFindingCode.MappingBlocked,
                        Status = CliSemanticStatus.Blocked,
                        LibraryId = "team-knowledge",
                        Path = ".agents/directives/review.md",
                        Cause = ".agents/directives/review.md could not be classified as a supported destination occupant.",
                        OccupantKind = null,
                    },
                ],
            },
        };

        var text = Render(result, CliFormat.Text, CliDetail.Minimal);

        Assert.Contains(
            ".agents/directives/review.md could not be classified as a supported destination occupant.",
            text,
            StringComparison.Ordinal);
    }

    private static string Render(LibraryDetachResult result, CliFormat format, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryDetachResult>(result, new(format, detail, null)),
            LibraryDetachPresentation.Rendering).PrimaryContent;

    private static LibraryDetachResult Result(CliSemanticStatus status)
        => new()
        {
            Status = status,
            Workspace = LibraryMutationPlanningData.Workspace,
            Next = null,
            Result = new LibraryDetachPayload
            {
                Permissions = LibraryPermissionView.NotEvaluated(),
                Identity = LibraryMutationPresentationData.Identity(true),
                Record = LibraryMutationPresentationData.Record(),
                Projection = LibraryMutationPresentationData.Projection(),
                Plan = LibraryMutationPresentationData.Plan(),
                Application = LibraryMutationPresentationData.Application(),
                Findings = [],
            },
        };
}
