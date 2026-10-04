using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Attach;
using OpenForge.Cli.Core.Presentation.Library.Detach;
using OpenForge.Cli.Core.Presentation.Library.Sync;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.GitIgnore;

public sealed class LibraryGitIgnorePartialPresentationTests
{
    [Theory, Trait("Feature", "library-git-ignore"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    [InlineData("attach", false, false)]
    [InlineData("attach", false, true)]
    [InlineData("attach", true, false)]
    [InlineData("attach", true, true)]
    [InlineData("detach", false, false)]
    [InlineData("detach", false, true)]
    [InlineData("detach", true, false)]
    [InlineData("detach", true, true)]
    [InlineData("sync", false, true)]
    public void IgnoreOnlyProgressSurvivesFailureAndCancellation(string command, bool failed, bool unknown)
    {
        var status = failed ? CliSemanticStatus.Failed : CliSemanticStatus.Interrupted;
        var application = LibraryMutationPresentationData.Application() with
        {
            State = failed ? LibraryApplicationState.Failed : LibraryApplicationState.Interrupted,
            Verification = LibraryVerificationState.Unavailable,
            Recovery = new() { State = LibraryRecoveryState.Retained, Path = ".agents/recovery/pending.json" },
            Residuals = [new() { Path = ".gitignore", Kind = LibraryResidualKind.GitIgnore, State = unknown ? LibraryResidualState.Unknown : LibraryResidualState.Retained }],
        };
        var plan = LibraryMutationPresentationData.Plan() with
        {
            State = LibraryPlanState.Complete,
            RecordEffect = LibraryRecordEffect.Replace,
            RecordExpected = new() { Kind = LibraryExpectedStateKind.OrdinaryFile, Length = 123, Sha256 = "record-before-hash", RawRelativeTarget = null },
            GitIgnore = new()
            {
                Path = ".gitignore",
                Action = "create",
                Paths = ["docs/existing.md"],
                Expected = new() { Kind = LibraryExpectedStateKind.Missing, Length = null, Sha256 = null, RawRelativeTarget = null },
            },
        };
        string Render(CliFormat format, CliDetail detail)
        {
            if (command == "sync")
            {
                var result = new LibrarySyncResult
                {
                    Status = status,
                    Workspace = LibraryMutationPlanningData.Workspace,
                    Next = null,
                    Result = new()
                    {
                        Identity = LibraryMutationPresentationData.Identity(false),
                        Permissions = LibraryPermissionView.NotEvaluated(),
                        Record = LibraryMutationPresentationData.Record(),
                        Source = LibraryMutationPresentationData.Source(),
                        Projection = LibraryMutationPresentationData.Projection(),
                        Plan = plan,
                        Application = application,
                        Findings = [new() { Code = LibrarySyncFindingCode.Interrupted,
                            Status = status, LibraryId = "team-knowledge", Path = ".gitignore", Cause = "Application stopped before ownership publication." }],
                    },
                };
                return CliRenderingStage.Render(new CliPresentationRequest<LibrarySyncResult>(result, new(format, detail, null)), LibrarySyncPresentation.Rendering).PrimaryContent;
            }
            if (command == "attach")
            {
                var result = new LibraryAttachResult
                {
                    Status = status,
                    Workspace = LibraryMutationPlanningData.Workspace,
                    Next = null,
                    Result = new()
                    {
                        Identity = LibraryMutationPresentationData.Identity(false),
                        Permissions = LibraryPermissionView.NotEvaluated(),
                        Record = LibraryMutationPresentationData.Record(),
                        Source = LibraryMutationPresentationData.Source(),
                        Projection = LibraryMutationPresentationData.Projection(),
                        Plan = plan,
                        Application = application,
                        Findings = [new() { Code = failed ? LibraryAttachFindingCode.ApplicationFailed : LibraryAttachFindingCode.Interrupted,
                            Status = status, LibraryId = "team-knowledge", Path = unknown ? ".gitignore" : ".agents/open-forge.lock.json", Cause = "Application stopped before ownership publication." }],
                    },
                };
                return CliRenderingStage.Render(new CliPresentationRequest<LibraryAttachResult>(result, new(format, detail, null)), LibraryAttachPresentation.Rendering).PrimaryContent;
            }
            var detach = new LibraryDetachResult
            {
                Status = status,
                Workspace = LibraryMutationPlanningData.Workspace,
                Next = null,
                Result = new()
                {
                    Identity = LibraryMutationPresentationData.Identity(true),
                    Permissions = LibraryPermissionView.NotEvaluated(),
                    Record = LibraryMutationPresentationData.Record(),
                    Projection = LibraryMutationPresentationData.Projection(),
                    Plan = plan,
                    Application = application,
                    Findings = [new() { Code = failed ? LibraryDetachFindingCode.ApplicationFailed : LibraryDetachFindingCode.Interrupted,
                        Status = status, LibraryId = "team-knowledge", Path = unknown ? ".gitignore" : ".agents/open-forge.lock.json", Cause = "Application stopped before ownership publication.", OccupantKind = null }],
                },
            };
            return CliRenderingStage.Render(new CliPresentationRequest<LibraryDetachResult>(detach, new(format, detail, null)), LibraryDetachPresentation.Rendering).PrimaryContent;
        }

        var expectedHeadline = $"Library {command} {(failed ? "stopped" : "was cancelled")} after {(unknown ? 0 : 1)} of 2 changes were verified.";
        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard })
        {
            var text = Render(CliFormat.Text, detail);
            Assert.Contains(expectedHeadline, text, StringComparison.Ordinal);
            Assert.Contains(unknown ? "The Library rules in .gitignore could not be verified" : "Updated Library rules in .gitignore", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Nothing was changed", text, StringComparison.Ordinal);
            Assert.DoesNotContain("0 of 0 links", text, StringComparison.Ordinal);
        }
        using var json = JsonDocument.Parse(Render(CliFormat.Json, CliDetail.Full));
        var root = json.RootElement;
        Assert.Equal(expectedHeadline, root.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Equal(unknown ? 0 : 1, root.GetProperty("counts").GetProperty("gitIgnoreFilesUpdated").GetInt32());
        var effect = Assert.Single(root.GetProperty("effects").EnumerateArray(), effect => effect.GetProperty("path").GetString() == ".gitignore");
        Assert.Equal("file", effect.GetProperty("kind").GetString());
        Assert.Equal(unknown ? "unknown" : "done", effect.GetProperty("outcome").GetString());
        var record = Assert.Single(root.GetProperty("effects").EnumerateArray(), effect => effect.GetProperty("kind").GetString() == "record");
        Assert.Equal("not-started", record.GetProperty("outcome").GetString());
        if (!failed)
            Assert.Equal(expectedHeadline, root.GetProperty("findings")[0].GetProperty("message").GetString());
        if (failed && command == "attach")
            Assert.Equal("Writing failed", root.GetProperty("findings")[0].GetProperty("title").GetString());
        if (failed && unknown && command == "attach")
        {
            Assert.Contains(".gitignore", root.GetProperty("findings")[0].GetProperty("message").GetString(), StringComparison.Ordinal);
            Assert.DoesNotContain("link", root.GetProperty("findings")[0].GetProperty("message").GetString(), StringComparison.Ordinal);
        }
    }

    [Fact, Trait("Feature", "library-git-ignore"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void LinkFailureWithoutIgnoreKeepsItsExistingFindingTitle()
    {
        var result = new LibraryAttachResult
        {
            Status = CliSemanticStatus.Failed,
            Workspace = LibraryMutationPlanningData.Workspace,
            Next = null,
            Result = new()
            {
                Identity = LibraryMutationPresentationData.Identity(false),
                Permissions = LibraryPermissionView.NotEvaluated(),
                Record = LibraryMutationPresentationData.Record(),
                Source = LibraryMutationPresentationData.Source(),
                Projection = LibraryMutationPresentationData.Projection(),
                Plan = LibraryHumanPresentationData.Plan(LibraryLinkEffectKind.Create),
                Application = LibraryMutationPresentationData.Application() with { State = LibraryApplicationState.Failed, Verification = LibraryVerificationState.Failed },
                Findings = [new() { Code = LibraryAttachFindingCode.ApplicationFailed, Status = CliSemanticStatus.Failed,
                    LibraryId = "team-knowledge", Path = "docs/first.md", Cause = "The mapped link could not be created." }],
            },
        };
        using var json = JsonDocument.Parse(CliRenderingStage.Render(
            new CliPresentationRequest<LibraryAttachResult>(result, new(CliFormat.Json, CliDetail.Minimal, null)), LibraryAttachPresentation.Rendering).PrimaryContent);
        Assert.Equal("Link creation failed", json.RootElement.GetProperty("findings")[0].GetProperty("title").GetString());
    }
}
