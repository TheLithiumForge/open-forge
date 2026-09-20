using System.IO.Compression;
using System.Text;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.LibraryResiduals;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibrarySelectedFinalReadIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Selected recovery final readback returns fresh original preparation for exact ordinary and raw-link entries")]
    [InlineData("create"), InlineData("replace"), InlineData("delete"), InlineData("link-create"), InlineData("link-delete")]
    public static async Task ReadsExactOriginalFinal(string kind)
    {
        using var fixture = await LibraryRecoveryWorkspace.CreateAsync(kind);
        var original = fixture.Preparation;
        var before = File.ReadAllBytes(original.BundlePath);

        var result = await RecoveryBundleReader.ReadSelectedFinalAsync(fixture.Workspace, fixture.Candidate, TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleReadState.Valid, result.Read.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(result.Preparation);
        Assert.NotSame(original, preparation);
        Assert.Equal(original.BundlePath, preparation.BundlePath);
        Assert.Equal(original.OperationId, preparation.OperationId);
        Assert.Equal(original.Command, preparation.Command);
        Assert.Equal(original.Attribution, preparation.Attribution);
        Assert.Equal(original.Entries, preparation.Entries);
        Assert.Equal(before, File.ReadAllBytes(original.BundlePath));
        fixture.AssertUnrelatedPreserved();
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Selected final readback rejects missing corrupt linked or changed valid final instead of trusting the old candidate")]
    [InlineData("missing"), InlineData("corrupt"), InlineData("linked"), InlineData("changed-entry")]
    public static async Task RejectsStaleCandidateAuthority(string defect)
    {
        using var fixture = await LibraryRecoveryWorkspace.CreateAsync("replace");
        var path = fixture.Preparation.BundlePath;
        string? linkedTarget = null;
        switch (defect)
        {
            case "missing": File.Delete(path); break;
            case "corrupt": File.WriteAllText(path, "not a recovery archive"); break;
            case "linked":
                linkedTarget = fixture.CreateOrdinaryFile("bundle-copy.zip", File.ReadAllBytes(path));
                File.Delete(path);
                File.CreateSymbolicLink(path, linkedTarget);
                break;
            case "changed-entry":
                ChangeManifestTarget(path);
                var healthyChanged = await RecoveryBundleReader.ReadFinalAsync(fixture.Workspace, path, TestContext.Current.CancellationToken);
                Assert.Equal(RecoveryBundleReadState.Valid, healthyChanged.State);
                Assert.Equal(".agents/other.md", Assert.Single(Assert.IsType<RecoveryBundleVerifiedRead>(healthyChanged.Verified).Entries).TargetPath);
                break;
        }

        var result = await RecoveryBundleReader.ReadSelectedFinalAsync(fixture.Workspace, fixture.Candidate, TestContext.Current.CancellationToken);

        Assert.NotEqual(RecoveryBundleReadState.Valid, result.Read.State);
        Assert.Null(result.Preparation);
        Assert.Equal("intended", File.ReadAllText(fixture.TargetPath));
        fixture.AssertUnrelatedPreserved();
        if (linkedTarget is not null)
        {
            Assert.Equal(linkedTarget, new FileInfo(path).LinkTarget);
            Assert.True(File.Exists(linkedTarget));
        }
    }

    private static void ChangeManifestTarget(string path)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Update);
        var manifest = Assert.Single(archive.Entries, entry => entry.FullName == "manifest.json");
        string original;
        using (var reader = new StreamReader(manifest.Open(), Encoding.UTF8))
        {
            original = reader.ReadToEnd();
        }
        var changed = original.Replace(".agents/target.md", ".agents/other.md", StringComparison.Ordinal);
        Assert.NotEqual(original, changed);
        using var stream = manifest.Open();
        stream.SetLength(0);
        stream.Write(Encoding.UTF8.GetBytes(changed));
    }
}
