using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.LibraryResiduals;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Application;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryOrdinaryRecoverySafetyIntegrationTests
{
    [Theory(DisplayName = "Explicit ordinary recovery never adopts third ordinary objects links or directories")]
    [InlineData("create", "third"), InlineData("replace", "third"), InlineData("generated", "third"), InlineData("delete", "third")]
    [InlineData("create", "relative-link"), InlineData("replace", "relative-link"), InlineData("delete", "relative-link")]
    [InlineData("create", "absolute-link"), InlineData("replace", "absolute-link"), InlineData("delete", "absolute-link")]
    [InlineData("create", "directory"), InlineData("replace", "directory"), InlineData("delete", "directory")]
    public static async Task RejectsNonIntendedLeaf(string kind, string occupant)
    {
        using var fixture = await LibraryRecoveryWorkspace.CreateAsync(kind);
        await using var lease = await fixture.AcquireLaterLeaseAsync();
        File.Delete(fixture.TargetPath);
        var raw = occupant == "absolute-link" ? fixture.SourcePath : LibraryRecoveryWorkspace.RelativeTarget;
        switch (occupant)
        {
            case "third": File.WriteAllText(fixture.TargetPath, "third object"); break;
            case "relative-link":
            case "absolute-link": File.CreateSymbolicLink(fixture.TargetPath, raw); break;
            case "directory": Directory.CreateDirectory(fixture.TargetPath); break;
        }
        try
        {
            var result = await OrdinaryFileRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease,
                fixture.Preparation, Assert.Single(fixture.Preparation.Entries), TestContext.Current.CancellationToken);

            Assert.Equal(FilesystemEffectState.NotStarted, result.Effect);
            Assert.Null(result.After);
            if (occupant == "third")
            {
                Assert.Equal("third object", File.ReadAllText(fixture.TargetPath));
            }
            else if (occupant == "directory")
            {
                Assert.True(Directory.Exists(fixture.TargetPath));
                Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.TargetPath));
            }
            else
            {
                Assert.Equal(raw, new FileInfo(fixture.TargetPath).LinkTarget);
            }
            Assert.True(File.Exists(fixture.Preparation.BundlePath));
            fixture.AssertUnrelatedPreserved();
        }
        finally
        {
            if (occupant == "directory")
            {
                Directory.Delete(fixture.TargetPath, recursive: false);
            }
            else
            {
                File.Delete(fixture.TargetPath);
            }
        }
    }

    [Theory(DisplayName = "Ordinary recovery revalidates original final and prior payload before applying any inverse effect")]
    [InlineData("create", false), InlineData("replace", false), InlineData("delete", false)]
    [InlineData("replace", true), InlineData("delete", true)]
    public static async Task RejectsChangedRecoveryEvidence(string kind, bool payloadOnly)
    {
        using var fixture = await LibraryRecoveryWorkspace.CreateAsync(kind);
        await using var lease = await fixture.AcquireLaterLeaseAsync();
        if (payloadOnly)
        {
            using var archive = ZipFile.Open(fixture.Preparation.BundlePath, ZipArchiveMode.Update);
            var payload = Assert.Single(archive.Entries, entry => entry.FullName == "payloads/00000000.bin");
            using var stream = payload.Open();
            stream.SetLength(0);
            stream.Write("changed prior payload"u8);
        }
        else
        {
            File.WriteAllText(fixture.Preparation.BundlePath, "corrupted after preparation");
        }
        var before = File.ReadAllBytes(fixture.Preparation.BundlePath);
        var invalid = await RecoveryBundleReader.ReadFinalAsync(fixture.Workspace, fixture.Preparation.BundlePath, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Malformed, invalid.State);

        var result = await OrdinaryFileRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease,
            fixture.Preparation, Assert.Single(fixture.Preparation.Entries), TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, result.Effect);
        Assert.Null(result.After);
        if (kind == "delete")
        {
            Assert.False(File.Exists(fixture.TargetPath));
        }
        else
        {
            Assert.Equal("intended", File.ReadAllText(fixture.TargetPath));
        }
        Assert.Equal(before, File.ReadAllBytes(fixture.Preparation.BundlePath));
        fixture.AssertUnrelatedPreserved();
    }
    [Fact(DisplayName = "Ordinary recovery blocks an external parent transition before touching a same-named intended file")]
    public static async Task RejectsExternalParentEvenWhenBytesMatch()
    {
        using var fixture = await LibraryRecoveryWorkspace.CreateAsync("replace");
        using var external = TemporaryWorkspace.Create("library-recovery-source");
        var source = external.CreateFile("target.md", "intended");
        await using var lease = await fixture.AcquireLaterLeaseAsync();
        var agents = Path.Combine(fixture.Workspace.LexicalRoot, ".agents");
        var saved = Path.Combine(fixture.Workspace.LexicalRoot, "saved-agents");
        Directory.Move(agents, saved);
        Directory.CreateSymbolicLink(agents, external.Path);
        try
        {
            var result = await OrdinaryFileRecoveryApplier.ApplyAsync(new PhysicalPathResolver(), lease,
                fixture.Preparation, Assert.Single(fixture.Preparation.Entries), TestContext.Current.CancellationToken);

            Assert.Equal(FilesystemEffectState.NotStarted, result.Effect);
            Assert.Null(result.After);
            Assert.Equal("intended", File.ReadAllText(source));
            Assert.Equal(external.Path, new DirectoryInfo(agents).LinkTarget);
            Assert.True(File.Exists(fixture.Preparation.BundlePath));
        }
        finally
        {
            Directory.Delete(agents);
            Directory.Move(saved, agents);
        }
    }

}
