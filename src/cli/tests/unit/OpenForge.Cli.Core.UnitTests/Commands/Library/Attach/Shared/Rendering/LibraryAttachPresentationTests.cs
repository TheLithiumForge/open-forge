using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Attach.Shared.Rendering;

public sealed class LibraryAttachPresentationTests
{
    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Attention, (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Incomplete, (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Invalid, (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Blocked, (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Failed, (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Interrupted, (int)CliOutputTarget.StandardError)]
    public void EveryStatusUsesConcreteHumanCoordinates(int statusValue, int humanTargetValue)
    {
        var result = Result(statusValue);
        var human = CliRenderingStage.Render(new CliPresentationRequest<LibraryAttachResult>(result,
            new(CliFormat.Text, CliDetail.Minimal, null)), LibraryAttachPresentation.Rendering);
        Assert.Equal((CliOutputTarget)humanTargetValue, human.PrimaryTarget);
        Assert.Contains("Library", human.PrimaryContent, StringComparison.Ordinal);
        if (statusValue is not (int)CliSemanticStatus.Failed and not (int)CliSemanticStatus.Interrupted)
        {
            Assert.Contains("team-knowledge", human.PrimaryContent, StringComparison.Ordinal);
        }
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, "completed")]
    [InlineData((int)CliSemanticStatus.Attention, "completed-with-warnings")]
    [InlineData((int)CliSemanticStatus.Incomplete, "incomplete")]
    [InlineData((int)CliSemanticStatus.Invalid, "invalid-input")]
    [InlineData((int)CliSemanticStatus.Blocked, "blocked")]
    [InlineData((int)CliSemanticStatus.Failed, "failed")]
    [InlineData((int)CliSemanticStatus.Interrupted, "cancelled")]
    public void EveryStatusUsesConcreteJsonCoordinates(int statusValue, string wireStatus)
    {
        var result = Result(statusValue);
        var json = CliRenderingStage.Render(new CliPresentationRequest<LibraryAttachResult>(result,
            new(CliFormat.Json, CliDetail.Standard, null)), LibraryAttachPresentation.Rendering);
        Assert.Equal(CliOutputTarget.StandardOutput, json.PrimaryTarget);
        using var document = JsonDocument.Parse(json.PrimaryContent);
        Assert.Equal(["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"],
            [.. document.RootElement.EnumerateObject().Select(property => property.Name)]);
        Assert.Equal(3, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("library attach", document.RootElement.GetProperty("command").GetString());
        Assert.Equal(wireStatus, document.RootElement.GetProperty("status").GetString());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(["mode", "id", "sourceFolder", "destinationFolder", "recorded", "permissions", "links", "inventory"],
            data.EnumerateObject().Select(property => property.Name));
        Assert.Equal("apply", data.GetProperty("mode").GetString());
        Assert.False(data.GetProperty("recorded").GetBoolean());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library Attach views preserve planned paths and interrupted recovery without claiming application"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    public void InterruptedViews(int value)
    {
        var seed = Result((int)CliSemanticStatus.Interrupted);
        var result = seed with
        {
            Next = new CliNextAction("open-forge doctor", "Review recovery before continuing."),
            Result = seed.Result with
            {
                Plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Create),
                Application = LibraryHumanPresentationData.Interrupted(),
                Permissions = LibraryHumanPresentationData.GrantedPermission(),
            },
        };
        var before = CliRenderingStage.Render(new CliPresentationRequest<LibraryAttachResult>(result,
            new(CliFormat.Json, CliDetail.Standard, null)), LibraryAttachPresentation.Rendering).PrimaryContent;
        var text = CliRenderingStage.Render(new CliPresentationRequest<LibraryAttachResult>(result,
            new(CliFormat.Text, (CliDetail)value, null)), LibraryAttachPresentation.Rendering).PrimaryContent;
        Assert.Contains("Library attach was cancelled. Stopped after 1 of 2 links were created.", text, StringComparison.Ordinal);
        Assert.Contains("docs/first.md", text, StringComparison.Ordinal);
        Assert.Contains("docs/second.md", text, StringComparison.Ordinal);
        Assert.Contains("The Library was not recorded. Recovery data: .agents/recovery/pending.json", text, StringComparison.Ordinal);
        Assert.Equal(1, text.Split("Next: open-forge doctor", StringSplitOptions.None).Length - 1);
        Assert.Equal(value == (int)CliDetail.Standard, text.Contains("Review recovery before continuing.", StringComparison.Ordinal));
        Assert.Equal(before, CliRenderingStage.Render(new CliPresentationRequest<LibraryAttachResult>(result,
            new(CliFormat.Json, CliDetail.Standard, null)), LibraryAttachPresentation.Rendering).PrimaryContent);
    }

    [Trait("Boundary", "Output")]
    [Fact(
        DisplayName = "Malformed ownership observation remains visible beside successful attach at minimal detail"),
     Trait("Feature", "library-mutation"),
     Trait("Evidence", "Unit")]
    public void MalformedOwnershipObservationRemainsVisibleAtMinimalDetail()
    {
        var seed = Result((int)CliSemanticStatus.Complete);
        var result = seed with
        {
            Result = seed.Result with
            {
                Record = seed.Result.Record with { State = LibraryMutationRecordState.Invalid },
                Findings =
                [
                    new LibraryAttachFinding
                    {
                        Code = LibraryAttachFindingCode.OwnershipObservation,
                        Status = CliSemanticStatus.Complete,
                        LibraryId = "team-knowledge",
                        Path = null,
                        Cause = "The ownership record is malformed.",
                    },
                ],
            },
        };
        const string warning = "The ownership record is malformed.";

        var minimal = CliRenderingStage.Render(new CliPresentationRequest<LibraryAttachResult>(result,
            new(CliFormat.Text, CliDetail.Minimal, null)), LibraryAttachPresentation.Rendering).PrimaryContent;
        Assert.Contains("Registered", minimal, StringComparison.Ordinal);
        Assert.Contains(seed.Result.Record.Path, minimal, StringComparison.Ordinal);
        Assert.Contains(warning, minimal, StringComparison.Ordinal);

        var standard = CliRenderingStage.Render(new CliPresentationRequest<LibraryAttachResult>(result,
            new(CliFormat.Text, CliDetail.Standard, null)), LibraryAttachPresentation.Rendering).PrimaryContent;
        Assert.Equal(1, standard.Split(warning, StringSplitOptions.None).Length - 1);
    }

    private static LibraryAttachResult Result(int statusValue)
        => new()
        {
            Status = (CliSemanticStatus)statusValue,
            Workspace = LibraryMutationPlanningData.Workspace,
            Next = null,
            Result = new LibraryAttachPayload
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
}
