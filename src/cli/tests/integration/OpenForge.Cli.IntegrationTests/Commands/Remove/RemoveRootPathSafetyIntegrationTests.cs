using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

public sealed class RemoveRootPathSafetyIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Root Remove deletes one recorded Library link object and preserves source bytes and registration")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task RegisteredLibraryRelativeFileLinkIsRemovedAsLink()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-library-link");
        var sourceBytes = new byte[] { 0, 255, 13, 10, 1, 128 };
        workspace.WriteBytes("library-source/guide.md", sourceBytes);
        SeedLibraryRegistration(workspace, "library-source", ".agents/library", ["guide.md"]);
        if (!workspace.TryCreateFileLink(".agents/library/guide.md", "../../library-source/guide.md"))
        {
            return;
        }
        var linkPath = workspace.Combine(".agents/library/guide.md");
        var rawLinkTarget = new FileInfo(linkPath).LinkTarget;
        Assert.Equal("../../library-source/guide.md", rawLinkTarget);

        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", ".agents/library/guide.md", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.False(File.Exists(linkPath));
        Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Combine("library-source/guide.md")));
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Contains(report.RootElement.GetProperty("effects").EnumerateArray(), effect =>
            effect.GetProperty("path").GetString() == ".agents/library/guide.md"
            && effect.GetProperty("kind").GetString() == "link"
            && effect.GetProperty("action").GetString() == "deleted");

        using (var ownership = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(".agents/open-forge.lock.json"))))
        {
            var library = Assert.Single(ownership.RootElement.GetProperty("libraries").EnumerateArray());
            Assert.Equal("team-knowledge", library.GetProperty("id").GetString());
            Assert.Empty(library.GetProperty("paths").EnumerateArray());
        }

        using var reportDocument = JsonDocument.Parse(output.ToString());
        var recoveryPath = reportDocument.RootElement.GetProperty("data").GetProperty("recoveryPath").GetString()
            ?? throw new InvalidOperationException("A deleted mapped link must have recovery evidence.");
        var finalRead = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            recoveryPath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, finalRead.State);
        var entry = Assert.Single(Assert.IsType<RecoveryBundleVerifiedRead>(finalRead.Verified).Entries,
            candidate => candidate.TargetPath == ".agents/library/guide.md");
        Assert.Equal(RecoveryEntryKind.RelativeFileLinkDelete, entry.Kind);
        Assert.Equal(rawLinkTarget, entry.Prior.RelativeFileLink?.RawRelativeTarget);
        Assert.Null(entry.PriorPayload);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove releases a missing Library mapping without treating it as a present link")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task MissingRegisteredLibraryDestinationReleasesMapping()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-missing-library-mapping");
        var sourceBytes = new byte[] { 0, 255, 13, 10, 6 };
        workspace.WriteBytes("library-source/guide.md", sourceBytes);
        SeedLibraryRegistration(workspace, "library-source", ".agents/library", ["guide.md"]);
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", ".agents/library/guide.md", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.False(File.Exists(workspace.Combine(".agents/library/guide.md")));
        Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Combine("library-source/guide.md")));
        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(".agents/open-forge.json"))))
        {
            Assert.Equal([".agents/library/guide.md"], settings.RootElement.GetProperty("removedFiles").EnumerateArray()
                .Select(path => path.GetString()).ToArray());
        }
        using (var ownership = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(".agents/open-forge.lock.json"))))
        {
            var library = Assert.Single(ownership.RootElement.GetProperty("libraries").EnumerateArray());
            Assert.Equal("team-knowledge", library.GetProperty("id").GetString());
            Assert.Empty(library.GetProperty("paths").EnumerateArray());
        }
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Empty(report.RootElement.GetProperty("data").GetProperty("removed").EnumerateArray());
        Assert.Contains(report.RootElement.GetProperty("effects").EnumerateArray(), effect =>
            effect.GetProperty("path").GetString() == ".agents/open-forge.lock.json"
            && effect.GetProperty("action").GetString() == "released");
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Root Remove blocks an unregistered relative link without following or deleting its source")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task UnknownRelativeFileLinkIsBlocked()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-unknown-link");
        var sourceBytes = new byte[] { 1, 2, 0, 255 };
        workspace.WriteBytes("source.bin", sourceBytes);
        if (!workspace.TryCreateFileLink(".agents/unregistered.bin", "../source.bin"))
        {
            return;
        }
        var linkPath = workspace.Combine(".agents/unregistered.bin");
        var rawTarget = new FileInfo(linkPath).LinkTarget;
        var before = workspace.SnapshotHashes();

        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", ".agents/unregistered.bin", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(rawTarget, new FileInfo(linkPath).LinkTarget);
        Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Combine("source.bin")));
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove", report.RootElement.GetProperty("command").GetString());
        Assert.Equal("remove.target-unsafe", report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Root Remove blocks a directory link without following its external target")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task DirectoryLinkIsBlockedWithoutFollowingTarget()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-directory-link");
        using var external = TemporaryWorkspace.Create("remove-root-directory-link-target");
        var sourceBytes = new byte[] { 0, 254, 10, 13, 5 };
        external.CreateFile("payload.bin", sourceBytes);
        workspace.WriteText("container/plain.md", "Keep ordinary content too.\n");
        if (!workspace.TryCreateDirectoryLink("container/external", external.Path))
        {
            return;
        }

        var linkPath = workspace.Combine("container/external");
        var rawTarget = new DirectoryInfo(linkPath).LinkTarget;
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", "container", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(rawTarget, new DirectoryInfo(linkPath).LinkTarget);
        Assert.True(File.Exists(external.Combine("payload.bin")));
        Assert.Equal(sourceBytes, File.ReadAllBytes(external.Combine("payload.bin")));
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove.target-unsafe", report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove protects a registered Library source path")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task RegisteredLibrarySourceTreeIsProtected()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-library-source-guard");
        workspace.WriteText("library-source/guide.md", "Source bytes stay outside the link destination.\n");
        SeedLibraryRegistration(workspace, "library-source", ".agents/library", ["guide.md"]);
        var before = workspace.SnapshotHashes();

        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", "library-source", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove.protected-path", report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove protects nested Git metadata during directory inventory")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task DirectorySelectionCannotDeleteNestedGitMetadata()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-nested-git");
        workspace.WriteText("container/content.txt", "Keep ordinary content too.\n");
        workspace.WriteText("container/nested/.git/config", "Git metadata.\n");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", "container", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove.target-unsafe", report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Root Remove refuses protected workspace paths and malformed selections with concrete reports")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    [InlineData(".agents/open-forge.json", 5, "protected-path")]
    [InlineData(".agents/open-forge.lock.json", 5, "protected-path")]
    [InlineData(".git/config", 5, "protected-path")]
    [InlineData("../escape", 4, "invalid-input")]
    [InlineData("scratch/*.md", 4, "invalid-input")]
    public async Task ProtectedAndInvalidInputsAreBounded(string target, int expectedExit, string finding)
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-invalid-path");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", target, "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(expectedExit, completion.ExitCode);
        Assert.NotEqual(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove", report.RootElement.GetProperty("command").GetString());
        Assert.Equal("remove." + finding, report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove uses stored canonical exclusion spelling to reject a portable case alias")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task KnownPortableCaseAliasIsBlocked()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-case-alias");
        workspace.WriteText("docs/README.md", "Exact physical spelling.\n");
        workspace.WriteText(".agents/open-forge.json", "{\"schemaVersion\":1,\"removedFiles\":[\"Docs/Readme.md\"]}");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", "docs/README.md", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove.target-unsafe", report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Contains("Docs/README.md", report.RootElement.GetProperty("findings")[0].GetProperty("message").GetString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Root Remove blocks case-varied reserved targets using their actual spellings")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task ActualCaseVariedReservedTargetsAreBlocked()
    {
        using (var agentsWorkspace = RemoveRootIntegrationWorkspace.Create("remove-root-uppercase-agents"))
        {
            agentsWorkspace.WriteText(".AGENTS/note.txt", "This control directory must remain intact.\n");
            await AssertProtectedTargetIsWriteFree(agentsWorkspace, ".AGENTS");
        }

        using var controlsWorkspace = RemoveRootIntegrationWorkspace.Create("remove-root-uppercase-controls");
        controlsWorkspace.WriteText(".agents/RECOVERY/bundle.zip", "Recovery storage stays protected.\n");
        controlsWorkspace.WriteText(".agents/OPEN-FORGE.JSON", "{}");
        controlsWorkspace.WriteText(".agents/OPEN-FORGE.LOCK.JSON", "{}");

        await AssertProtectedTargetIsWriteFree(controlsWorkspace, ".agents/RECOVERY");
        await AssertProtectedTargetIsWriteFree(controlsWorkspace, ".agents/OPEN-FORGE.JSON");
        await AssertProtectedTargetIsWriteFree(controlsWorkspace, ".agents/OPEN-FORGE.LOCK.JSON");
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Root Remove blocks descendants of case-varied reserved controls")]
    [InlineData(".agents/OPEN-FORGE.JSON/note.txt")]
    [InlineData(".agents/OPEN-FORGE.LOCK.JSON/note.txt")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task ReservedControlDescendantsAreBlocked(string target)
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-control-descendant");
        workspace.WriteText(target, "Reserved control descendants must remain intact.\n");
        await AssertProtectedTargetIsWriteFree(workspace, target);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove blocks invalid settings and malformed ownership instead of treating them as empty")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task InvalidSettingsAndOwnershipBlockRemoval()
    {
        using var settingsWorkspace = RemoveRootIntegrationWorkspace.Create("remove-root-invalid-settings");
        settingsWorkspace.WriteText("note.txt", "Do not delete.\n");
        settingsWorkspace.WriteText(".agents/open-forge.json", "{");
        var settingsBefore = settingsWorkspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();
        var settingsResult = await settingsWorkspace.RunAsync(
            ["remove", "note.txt", "--automatic", "--format", "json"], output, error);
        Assert.Equal(5, settingsResult.ExitCode);
        Assert.True(File.Exists(settingsWorkspace.Combine("note.txt")));
        Assert.Equal(settingsBefore, settingsWorkspace.SnapshotHashes());
        using (var report = JsonDocument.Parse(output.ToString()))
        {
            Assert.Equal("remove.settings-unavailable", report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        }

        using var ownershipWorkspace = RemoveRootIntegrationWorkspace.Create("remove-root-invalid-ownership");
        ownershipWorkspace.WriteText("note.txt", "Do not delete.\n");
        ownershipWorkspace.WriteText(".agents/open-forge.lock.json", "{");
        var ownershipBefore = ownershipWorkspace.SnapshotHashes();
        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        var ownershipResult = await ownershipWorkspace.RunAsync(
            ["remove", "note.txt", "--automatic", "--format", "json"], output, error);
        Assert.Equal(5, ownershipResult.ExitCode);
        Assert.True(File.Exists(ownershipWorkspace.Combine("note.txt")));
        Assert.Equal(ownershipBefore, ownershipWorkspace.SnapshotHashes());
        using var ownershipReport = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove.ownership-unavailable", ownershipReport.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }

    private static void SeedLibraryRegistration(
        RemoveRootIntegrationWorkspace workspace,
        string sourceRoot,
        string destinationRoot,
        ImmutableArray<string> paths)
    {
        workspace.WriteBytes(".agents/open-forge.lock.json", WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with
            {
                Libraries = [new LibraryOwnership("team-knowledge", sourceRoot, destinationRoot, paths)],
            }));
    }

    private static async Task AssertProtectedTargetIsWriteFree(
        RemoveRootIntegrationWorkspace workspace,
        string target)
    {
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", target, "--dry-run", "--format", "json"],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove", report.RootElement.GetProperty("command").GetString());
        Assert.Equal("remove.protected-path", report.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }
}
