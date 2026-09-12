using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

internal static class LibraryHumanPresentationData
{
    internal static LibraryMutationPlanView Plan(LibraryLinkEffectKind effect)
    {
        var expected = new LibraryExpectedState
        {
            Kind = effect == LibraryLinkEffectKind.Create ? LibraryExpectedStateKind.Missing : LibraryExpectedStateKind.RelativeFileLink,
            Length = null,
            Sha256 = null,
            RawRelativeTarget = effect == LibraryLinkEffectKind.Create ? null : "../shared/first.md",
        };
        return new()
        {
            State = LibraryPlanState.Complete,
            Directories = [],
            Links =
            [
                new() { Path = "docs/first.md", Kind = effect, RawRelativeTarget = "../shared/first.md", Expected = expected },
                new() { Path = "docs/second.md", Kind = effect, RawRelativeTarget = "../shared/second.md", Expected = expected with { RawRelativeTarget = effect == LibraryLinkEffectKind.Create ? null : "../shared/second.md" } },
            ],
            GeneratedRegions = [],
            RecordEffect = LibraryRecordEffect.Replace,
            RecordExpected = new() { Kind = LibraryExpectedStateKind.OrdinaryFile, Length = 123, Sha256 = "record-before-hash", RawRelativeTarget = null },
        };
    }

    internal static LibraryMutationApplication Interrupted()
        => new()
        {
            State = LibraryApplicationState.Interrupted,
            Verification = LibraryVerificationState.Unavailable,
            Recovery = new() { State = LibraryRecoveryState.Retained, Path = ".agents/recovery/pending.json" },
            Residuals = [new() { Kind = LibraryResidualKind.Link, Path = "docs/first.md", State = LibraryResidualState.Retained }],
            RecordPublication = new() { State = LibraryRecordPublicationState.NotStarted, PublishedLast = null },
        };

    internal static LibraryPermissionView GrantedPermission()
        => LibraryPermissionView.NotEvaluated() with
        {
            Required = [new("team-knowledge", "shared/team-knowledge", "docs/second.md")],
            ApprovedScopes = [new("team-knowledge", "shared/team-knowledge", "directory", "docs")],
            Decision = "granted",
        };

    internal static void AssertInterrupted(string text, bool expanded)
    {
        Assert.Contains("Application: interrupted; verification unavailable", text, StringComparison.Ordinal);
        Assert.Contains("Recovery: retained", text, StringComparison.Ordinal);
        Assert.Contains(".agents/recovery/pending.json", text, StringComparison.Ordinal);
        Assert.Contains("Record publication: not started", text, StringComparison.Ordinal);
        Assert.Contains("Remaining link: docs/first.md (retained)", text, StringComparison.Ordinal);
        Assert.Contains("Plan: complete", text, StringComparison.Ordinal);
        Assert.True(text.IndexOf("Application: interrupted", StringComparison.Ordinal) < text.IndexOf("Plan: complete", StringComparison.Ordinal));
        Assert.Contains("docs/first.md -> ../shared/first.md", text, StringComparison.Ordinal);
        Assert.Contains("docs/second.md -> ../shared/second.md", text, StringComparison.Ordinal);
        Assert.Contains("Approved: directory docs; Library team-knowledge; source shared/team-knowledge", text, StringComparison.Ordinal);
        Assert.Equal(expanded, text.Contains("record-before-hash", StringComparison.Ordinal));
        Assert.DoesNotContain("Application: applied", text, StringComparison.Ordinal);
        Assert.DoesNotContain("verification verified", text, StringComparison.Ordinal);
    }
}
