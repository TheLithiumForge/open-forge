using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.IntegrationTests.Framework.Libraries.Operational.Shared.Attribution;
using OpenForge.Cli.IntegrationTests.Framework.Libraries.Operational.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Operational;

[Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
public sealed class LibraryResidualAttributionReaderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact]
    public static async Task LibraryResidualNeverAttributesPermissionDocumentForAutomaticRepair()
    {
        using var fixture = new LibraryObservationWorkspace();
        fixture.Write(LibraryObservationWorkspace.RecordPath, LibraryObservationWorkspace.SingleRecord);
        var residual = await LibraryResidualArchive.CreateAsync(fixture,
            expectedPriorRecord: """{"allowInstallPaths":[]}""",
            recordTarget: ".agents/open-forge.json");
        var before = fixture.Snapshot();

        var result = await LibraryResidualAttributionReader.ReadAsync(fixture.Workspace, fixture.DoctorView(true),
            LibraryResidualArchive.View(residual), TestContext.Current.CancellationToken);

        Assert.Equal(".agents/a.md", Assert.Single(result).Entry.Input.Context.Entry.TargetPath);
        Assert.DoesNotContain(result, item => item.Entry.Input.Context.Entry.TargetPath == ".agents/open-forge.json");
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library residual reader binds current-v1 record membership to the exact observed residual entry")]
    public static async Task AttributesFromCurrentRecord()
    {
        using var fixture = new LibraryObservationWorkspace();
        fixture.Write(LibraryObservationWorkspace.RecordPath, LibraryObservationWorkspace.SingleRecord);
        var libraries = fixture.DoctorView(true);
        var residual = await LibraryResidualArchive.CreateAsync(fixture);
        var before = fixture.Snapshot();

        var result = await LibraryResidualAttributionReader.ReadAsync(
            fixture.Workspace, libraries, LibraryResidualArchive.View(residual), TestContext.Current.CancellationToken);

        var evidence = Assert.Single(result);
        Assert.Equal("team", evidence.LibraryId.Value);
        Assert.Same(libraries.Record, evidence.CurrentRecord);
        Assert.Same(residual, evidence.Residual);
        Assert.Same(residual.Entries[0], evidence.Entry);
        Assert.Null(evidence.VerifiedPriorRecord);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library recovery uses verified prior lock membership when the current lock is missing or the final registration was removed")]
    [InlineData(false), InlineData(true)]
    public static async Task DecodesVerifiedPriorPayloadBeforeAttributingMissingCurrentRecord(bool retainEmptyLock)
    {
        using var fixture = new LibraryObservationWorkspace();
        const string payload = """
            {"schemaVersion":1,"libraries":[{"id":"team","sourceRoot":"shared/team","destinationRoot":".","paths":[".agents/a.md",".agents/z.md"]}]}
            """;
        var residual = await LibraryResidualArchive.CreateAsync(fixture, payload, retainEmptyLock: retainEmptyLock);
        var current = await OpenForge.Cli.Core.Framework.Libraries.Shared.Observation.LibraryRegistrationReader.ReadAsync(
            new OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.PhysicalPathResolver(), fixture.Workspace,
            TestContext.Current.CancellationToken);
        var libraries = fixture.DoctorView(false) with { Record = current };
        var before = fixture.Snapshot();

        var result = await LibraryResidualAttributionReader.ReadAsync(
            fixture.Workspace, libraries, LibraryResidualArchive.View(residual), TestContext.Current.CancellationToken);

        var evidence = Assert.Single(result, item => item.Entry.Input.Context.Entry.TargetPath == ".agents/a.md");
        Assert.Equal("team", evidence.LibraryId.Value);
        Assert.Same(libraries.Record, evidence.CurrentRecord);
        Assert.Equal(retainEmptyLock ? LibraryRegistrationReadState.Complete : LibraryRegistrationReadState.Missing, evidence.CurrentRecord.State);
        if (retainEmptyLock)
        {
            Assert.Empty(Assert.IsType<LibraryRegistrationSet>(evidence.CurrentRecord.Record).Libraries);
        }
        else
        {
            Assert.Null(evidence.CurrentRecord.Record);
        }
        var prior = Assert.IsType<LibraryRecoveryPriorRecord>(evidence.VerifiedPriorRecord);
        var library = Assert.Single(prior.Record.Libraries);
        Assert.Equal("team", library.Id.Value);
        Assert.Equal("shared/team", library.SourceRoot.Value);
        Assert.Equal([".agents/a.md", ".agents/z.md"], library.Paths.Select(path => path.Value));
        Assert.Same(residual.Entries[0].Input.Context.Entry, prior.Entry);
        Assert.Equal(Encoding.UTF8.GetByteCount(payload), prior.PayloadIdentity.Length);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant(), prior.PayloadIdentity.Sha256);
        Assert.Same(residual.Entries[1], evidence.Entry);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library residual reader refuses unverified or invalid prior record payloads instead of inferring path-only membership")]
    [InlineData("changed-bytes"), InlineData("missing-payload"), InlineData("malformed-record"), InlineData("wrong-record-target")]
    public static async Task RefusesUnboundPriorPayload(string scenario)
    {
        using var fixture = new LibraryObservationWorkspace();
        var expected = scenario == "malformed-record" ? "{" : LibraryObservationWorkspace.SingleRecord;
        var residual = await LibraryResidualArchive.CreateAsync(fixture, expected,
            storedPayload: scenario == "changed-bytes" ? "{\"schemaVersion\":1,\"libraries\":[]}" : null,
            includePayload: scenario != "missing-payload",
            recordTarget: scenario == "wrong-record-target" ? ".agents/unrelated.json" : ".agents/open-forge.lock.json");
        var before = fixture.Snapshot();

        var result = await LibraryResidualAttributionReader.ReadAsync(
            fixture.Workspace, fixture.DoctorView(false), LibraryResidualArchive.View(residual), TestContext.Current.CancellationToken);

        Assert.Empty(result);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Decoded prior record membership cannot be borrowed by a link naming a different Library source")]
    public static async Task RejectsPriorPayloadWithDifferentLibraryAttribution()
    {
        using var fixture = new LibraryObservationWorkspace();
        const string otherRecord = """
            {"schemaVersion":1,"libraries":[{"id":"other","sourceRoot":"shared/other","destinationRoot":".","paths":[".agents/a.md"]}]}
            """;
        var residual = await LibraryResidualArchive.CreateAsync(fixture, otherRecord);
        var before = fixture.Snapshot();

        var result = await LibraryResidualAttributionReader.ReadAsync(
            fixture.Workspace, fixture.DoctorView(false), LibraryResidualArchive.View(residual), TestContext.Current.CancellationToken);

        Assert.DoesNotContain(result, item => item.LibraryId.Value == "team");
        Assert.DoesNotContain(result, item => item.Entry.Input.Context.Entry.TargetPath == ".agents/a.md");
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library residual reader requires current membership and the exact registered link identity")]
    [InlineData(false, "../shared/team/.agents/a.md"), InlineData(true, "../shared/other/.agents/a.md")]
    public static async Task RejectsUnattributedCurrentEntry(bool currentPresent, string target)
    {
        using var fixture = new LibraryObservationWorkspace();
        if (currentPresent)
        {
            fixture.Write(LibraryObservationWorkspace.RecordPath, LibraryObservationWorkspace.SingleRecord);
        }
        var residual = await LibraryResidualArchive.CreateAsync(fixture, linkTarget: target);
        var before = fixture.Snapshot();

        var result = await LibraryResidualAttributionReader.ReadAsync(
            fixture.Workspace, fixture.DoctorView(currentPresent), LibraryResidualArchive.View(residual), TestContext.Current.CancellationToken);

        Assert.Empty(result);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Safely missing Library record and empty recovery catalogue require no invented residual attribution")]
    public static async Task NoCandidatesProduceNoAttribution()
    {
        using var fixture = new LibraryObservationWorkspace();
        var before = fixture.Snapshot();

        var result = await LibraryResidualAttributionReader.ReadAsync(fixture.Workspace, fixture.DoctorView(false),
            new RecoveryResidualDoctorView(OperationalViewState.Complete, [], null), TestContext.Current.CancellationToken);

        Assert.Empty(result);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library residual reader honors cancellation before inspecting payload bytes and preserves residuals")]
    public static async Task CancellationDoesNotConsumePayload()
    {
        using var fixture = new LibraryObservationWorkspace();
        var residual = await LibraryResidualArchive.CreateAsync(fixture, LibraryObservationWorkspace.SingleRecord);
        var before = fixture.Snapshot();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await LibraryResidualAttributionReader.ReadAsync(
            fixture.Workspace, fixture.DoctorView(false), LibraryResidualArchive.View(residual), cancellation.Token));

        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Malformed current Library record does not grant attribution to a matching verified residual path")]
    public static async Task MalformedCurrentRecordGrantsNoMembership()
    {
        using var fixture = new LibraryObservationWorkspace();
        const string malformed = "{\"schemaVersion\":1,\"libraries\":null}";
        var path = fixture.Write(LibraryObservationWorkspace.RecordPath, malformed);
        var libraries = fixture.DoctorView(false) with
        {
            State = OperationalViewState.Blocked,
            Record = new LibraryRegistrationRead
            {
                State = LibraryRegistrationReadState.Malformed,
                Record = null,
                Snapshot = FileStateSnapshot.File(path, path, Encoding.UTF8.GetBytes(malformed)),
                Cause = "The libraries member is not an array.",
            },
        };
        var residual = await LibraryResidualArchive.CreateAsync(fixture);
        var before = fixture.Snapshot();

        var result = await LibraryResidualAttributionReader.ReadAsync(
            fixture.Workspace, libraries, LibraryResidualArchive.View(residual), TestContext.Current.CancellationToken);

        Assert.Empty(result);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Current Library membership does not turn an unverified recovery draft into residual authority")]
    public static async Task UnverifiedCandidateGrantsNoAttribution()
    {
        using var fixture = new LibraryObservationWorkspace();
        fixture.Write(LibraryObservationWorkspace.RecordPath, LibraryObservationWorkspace.SingleRecord);
        var draft = fixture.Files.CreateFile("recovery/operation-123456781234123412341234567890ab.draft", "untrusted draft");
        var candidate = RecoveryBundleCandidateSnapshot.IncompleteDraft(draft);
        var recovery = new RecoveryResidualDoctorView(OperationalViewState.Complete,
            [RecoveryDoctorCandidateObservation.Create(candidate, null)], null);
        var before = fixture.Snapshot();

        var result = await LibraryResidualAttributionReader.ReadAsync(
            fixture.Workspace, fixture.DoctorView(true), recovery, TestContext.Current.CancellationToken);

        Assert.Empty(result);
        Assert.Equal(before, fixture.Snapshot());
    }
}
