using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Writing;

[Trait("Feature", "tool-writers"), Trait("Evidence", "Integration")]
public sealed class ToolWriterSupplyTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Writer staging verifies local fixture artifacts before store publication")]
    [InlineData("file"), InlineData("zip"), InlineData("tar.gz")]
    public async Task StagesAndPublishes(string format)
    {
        using var fixture = new ToolWriterSupplyFixture();
        var extraction = fixture.Create(format);
        var definition = Definition(extraction.Artifact);
        var store = new ToolStoreRegistry(ToolStoreLocator.FromRoot(fixture.Temporary.Path));
        var before = fixture.Temporary.SnapshotHashes();
        var plan = await ToolWriterSupplyReader.ReadAsync(store, definition, Target, Token);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
        Assert.False(plan.Store.Verified);
        Assert.Null(Assert.Single(plan.Artifacts).Cached);
        var downloads = 0;
        var staged = await new ToolWriterSupplyStager(store, (request, token) =>
        {
            downloads++;
            Assert.Equal(plan.Artifacts[0].Source, request.Source);
            Assert.Equal(extraction.Artifact.Integrity, request.Integrity);
            File.Copy(extraction.ArchivePath, request.Destination);
            return ValueTask.CompletedTask;
        }).StageAsync(plan, Token);
        Assert.Equal(1, downloads);
        var install = Assert.IsType<ToolPreparedStoreUnit>(staged.Install);
        Assert.NotEmpty(install.Files);
        using var lease = ToolStoreLease.Acquire(store.State, Token);
        var publisher = new ToolStorePublisher(store);
        foreach (var archive in staged.Archives) await publisher.PublishAsync(lease, archive, null, Token);
        var publication = await publisher.PublishAsync(lease, install, null, Token);
        Assert.True(publication.Unit.Verified);
        Assert.True((await ToolStoreUnitReader.ReadAsync(store.Installs, publication.Unit, Token)).Verified);
        var repeated = await ToolWriterSupplyReader.ReadAsync(store, definition, Target, Token);
        Assert.True(repeated.Store.Verified);
        Assert.NotNull(Assert.Single(repeated.Artifacts).Cached);
        var reused = await new ToolWriterSupplyStager(store, static (_, _) => throw new InvalidOperationException("Offline reuse tried to download.")).StageAsync(repeated, Token);
        Assert.Null(reused.Install);
        Assert.Empty(reused.Archives);
    }

    [Theory(DisplayName = "Writer staging repairs changed or missing immutable files from verified offline archives")]
    [InlineData(false), InlineData(true)]
    public async Task RestoresFromOfflineArchive(bool missing)
    {
        using var fixture = new ToolWriterSupplyFixture();
        var extraction = fixture.Create("zip");
        var definition = Definition(extraction.Artifact);
        var store = new ToolStoreRegistry(ToolStoreLocator.FromRoot(fixture.Temporary.Path));
        var plan = await ToolWriterSupplyReader.ReadAsync(store, definition, Target, Token);
        var staged = await new ToolWriterSupplyStager(store, (request, _) =>
        {
            File.Copy(extraction.ArchivePath, request.Destination);
            return ValueTask.CompletedTask;
        }).StageAsync(plan, Token);
        using var lease = ToolStoreLease.Acquire(store.State, Token);
        var publisher = new ToolStorePublisher(store);
        foreach (var archive in staged.Archives) await publisher.PublishAsync(lease, archive, null, Token);
        var install = Assert.IsType<ToolPreparedStoreUnit>(staged.Install);
        var published = await publisher.PublishAsync(lease, install, null, Token);
        var path = store.Installs.At(published.Unit.Path + "/" + ToolArtifactFixture.Entry);
        if (missing) File.Delete(path);
        else await File.WriteAllBytesAsync(path, [1, 2, 3], Token);
        var repair = await ToolWriterSupplyReader.ReadAsync(store, definition, Target, Token);
        Assert.False(repair.Store.Verified);
        var replacement = await new ToolWriterSupplyStager(store, static (_, _) => throw new InvalidOperationException("Offline repair tried to download.")).StageAsync(repair, Token);
        Assert.Empty(replacement.Archives);
        var repaired = await publisher.PublishAsync(lease, Assert.IsType<ToolPreparedStoreUnit>(replacement.Install), null, Token);
        Assert.True((await ToolStoreUnitReader.ReadAsync(store.Installs, repaired.Unit, Token)).Verified);
        Assert.Equal(ToolArtifactFixture.Payload, await File.ReadAllBytesAsync(path, Token));
    }

    [Fact(DisplayName = "A transfer with incorrect pinned bytes produces no install or verified store unit")]
    public async Task RejectsChangedTransfer()
    {
        using var fixture = new ToolWriterSupplyFixture();
        var extraction = fixture.Create("zip");
        var store = new ToolStoreRegistry(ToolStoreLocator.FromRoot(fixture.Temporary.Path));
        var plan = await ToolWriterSupplyReader.ReadAsync(store, Definition(extraction.Artifact), Target, Token);
        await Assert.ThrowsAsync<InvalidDataException>(() => new ToolWriterSupplyStager(store, async (request, token) =>
        {
            var bytes = await File.ReadAllBytesAsync(extraction.ArchivePath, token);
            bytes[0] ^= 1;
            await File.WriteAllBytesAsync(request.Destination, bytes, token);
        }).StageAsync(plan, Token).AsTask());
        Assert.Empty((await store.ReadAsync(Token)).Document.Units);
        Assert.False(Directory.Exists(store.Installs.Root));
    }

    [Fact(DisplayName = "Artifact collision checks cover the entire target before any extraction")]
    public async Task RejectsCrossArtifactCollision()
    {
        using var fixture = new ToolWriterSupplyFixture();
        var extraction = fixture.Create("zip");
        var definition = Definition(extraction.Artifact);
        var manifest = definition.Manifest with
        {
            Artifacts = new(definition.Manifest.Artifacts, StringComparer.Ordinal) { [Second] = extraction.Artifact },
            Targets = new(definition.Manifest.Targets, StringComparer.Ordinal)
            { [Target] = definition.Manifest.Targets[Target] with { Artifacts = [Package, Second] } },
        };
        definition = Reviewed(manifest);
        var store = new ToolStoreRegistry(ToolStoreLocator.FromRoot(fixture.Temporary.Path));
        var plan = await ToolWriterSupplyReader.ReadAsync(store, definition, Target, Token);
        await Assert.ThrowsAsync<InvalidDataException>(() => new ToolWriterSupplyStager(store, (request, _) =>
        {
            File.Copy(extraction.ArchivePath, request.Destination);
            return ValueTask.CompletedTask;
        }).StageAsync(plan, Token).AsTask());
        Assert.Empty((await store.ReadAsync(Token)).Document.Units);
        Assert.False(Directory.Exists(store.Installs.Root));
    }

    [Fact(DisplayName = "Unrecognized members in a verified install block repair instead of being adopted")]
    public async Task RejectsForeignMember()
    {
        using var fixture = new ToolWriterSupplyFixture();
        var extraction = fixture.Create("file");
        var definition = Definition(extraction.Artifact);
        var store = new ToolStoreRegistry(ToolStoreLocator.FromRoot(fixture.Temporary.Path));
        var plan = await ToolWriterSupplyReader.ReadAsync(store, definition, Target, Token);
        var staged = await new ToolWriterSupplyStager(store, (request, _) =>
        {
            File.Copy(extraction.ArchivePath, request.Destination);
            return ValueTask.CompletedTask;
        }).StageAsync(plan, Token);
        using var lease = ToolStoreLease.Acquire(store.State, Token);
        var publisher = new ToolStorePublisher(store);
        foreach (var archive in staged.Archives) await publisher.PublishAsync(lease, archive, null, Token);
        var unit = await publisher.PublishAsync(lease, Assert.IsType<ToolPreparedStoreUnit>(staged.Install), null, Token);
        var foreign = store.Installs.At(unit.Unit.Path + "/" + Foreign);
        await File.WriteAllBytesAsync(foreign, [1], Token);
        var failure = await Assert.ThrowsAsync<ToolWriterFailure>(() => ToolWriterSupplyReader.ReadAsync(store, definition, Target, Token).AsTask());
        Assert.Equal(ToolWriteIssueCode.StoreUnavailable, failure.Issue.Code);
        Assert.Equal(new byte[] { 1 }, await File.ReadAllBytesAsync(foreign, Token));
    }

    private static ToolDefinition Definition(OpenForge.Cli.Core.Framework.Tools.Models.Supply.ToolArtifact artifact)
    {
        var manifest = new ToolCatalogueReader().Definition(ToolFixtureSource.ManifestBytes(), ToolFixtureSource.ManifestName, false).Manifest;
        manifest = manifest with
        {
            Admission = "enabled",
            AdmissionReason = null,
            Source = manifest.Source with { Repository = Repository },
            DownloadBase = Repository + "/releases/download/" + manifest.Source.Tag,
            Artifacts = new(StringComparer.Ordinal) { [Package] = artifact },
            Qualification = manifest.Qualification with { Targets = [Target] },
        };
        return Reviewed(manifest);
    }

    private static ToolDefinition Reviewed(OpenForge.Cli.Core.Framework.Tools.Models.Manifest.ToolManifest manifest)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(manifest, ToolJsonContext.Default.ToolManifest);
        var support = new ToolReleaseSupport([new(manifest.Id, manifest.Version, ToolCatalogueReader.Hash(bytes), manifest.Adapter.Revision)], [new(manifest.Adapter.Id, manifest.Adapter.Revision)]);
        return new ToolCatalogueReader(support).Definition(bytes, ToolFixtureSource.ManifestName, false);
    }

    private const string Target = "win-x64";
    private const string Package = "package";
    private const string Second = "second";
    private const string Foreign = "foreign.bin";
    private const string Repository = "https://github.com/fixture/tool";
}
