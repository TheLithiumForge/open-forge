using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests;

public sealed class TemporaryWorkspaceIntegrationTests
{
    [Fact(DisplayName = "Temporary workspaces expose a readable purpose and unique identifier"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void RootNamesContainPurposeAndUniqueIdentifier()
    {
        using var first = TemporaryWorkspace.Create("readable-purpose");
        using var second = TemporaryWorkspace.Create("readable-purpose");

        Assert.NotEqual(first.Path, second.Path);
        AssertRootName(first.Path, "readable-purpose");
        AssertRootName(second.Path, "readable-purpose");
    }

    [Fact(DisplayName = "Temporary workspaces reject rooted and traversal paths without outside writes"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void RejectsPathsOutsideOwnedRoot()
    {
        using var workspace = TemporaryWorkspace.Create("path-safety");
        var outsideName = $"open-forge-outside-{Guid.NewGuid():N}";
        var outsidePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(workspace.Path)!, outsideName);
        var traversalPath = $"..{System.IO.Path.DirectorySeparatorChar}{outsideName}";

        try
        {
            Assert.Throws<ArgumentException>(() => workspace.Combine(outsidePath));
            Assert.Throws<ArgumentException>(() => workspace.CreateDirectory(traversalPath));
            Assert.Throws<ArgumentException>(() => workspace.WriteText(outsidePath, "outside"));
            Assert.Throws<ArgumentException>(() => workspace.WriteBytes(traversalPath, [0x01]));
            Assert.False(File.Exists(outsidePath));
            Assert.False(Directory.Exists(outsidePath));
        }
        finally
        {
            if (File.Exists(outsidePath))
            {
                File.Delete(outsidePath);
            }

            if (Directory.Exists(outsidePath))
            {
                Directory.Delete(outsidePath, recursive: true);
            }
        }
    }

    [Fact(DisplayName = "Temporary workspace text writes use strict UTF-8 without a byte-order mark"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void WritesStrictUtf8WithoutBom()
    {
        using var workspace = TemporaryWorkspace.Create("utf8");
        const string contents = "Forge π 路線";

        workspace.WriteText("content.txt", contents);

        Assert.Equal(new UTF8Encoding(false, true).GetBytes(contents), File.ReadAllBytes(workspace.Combine("content.txt")));
    }

    [Fact(DisplayName = "Temporary workspace snapshots are sorted deterministic hashes without ownership metadata"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void SnapshotsAreSortedDeterministicAndExcludeMarker()
    {
        using var workspace = TemporaryWorkspace.Create("snapshot");
        workspace.WriteText("z.txt", "zulu");
        workspace.WriteText("nested/middle.txt", "middle");
        workspace.WriteText("a.txt", "alpha");

        var first = workspace.SnapshotHashes();
        var second = workspace.SnapshotHashes();

        Assert.Equal(["a.txt", "nested/middle.txt", "z.txt"], first.Keys);
        Assert.Equal(first, second);
        Assert.Equal(Hash("alpha"), first["a.txt"]);
        Assert.Equal(Hash("middle"), first["nested/middle.txt"]);
        Assert.Equal(Hash("zulu"), first["z.txt"]);
    }

    [Fact(DisplayName = "Temporary workspace cleanup removes its normally owned root and is idempotent"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void CleanupRemovesOwnedRootAndIsIdempotent()
    {
        var workspace = TemporaryWorkspace.Create("cleanup");
        var rootPath = workspace.Path;
        workspace.WriteText("nested/content.txt", "owned");

        workspace.Dispose();
        workspace.Dispose();

        Assert.False(Directory.Exists(rootPath));
    }

    [Fact(DisplayName = "Temporary workspace cleanup refuses a replaced root"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void CleanupRefusesReplacedRoot()
    {
        var workspace = TemporaryWorkspace.Create("replaced-root");
        var rootPath = workspace.Path;
        var sentinelPath = System.IO.Path.Combine(rootPath, "unowned.txt");

        try
        {
            Directory.Delete(rootPath, recursive: true);
            Directory.CreateDirectory(rootPath);
            File.WriteAllText(sentinelPath, "unowned");

            Assert.Throws<InvalidOperationException>(() => workspace.Dispose());
            Assert.True(File.Exists(sentinelPath));
        }
        finally
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }
        }
    }

    [Fact(DisplayName = "Temporary workspace cleanup refuses an ownership marker mismatch"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void CleanupRefusesOwnershipMarkerMismatch()
    {
        var workspace = TemporaryWorkspace.Create("marker-mismatch");
        var rootPath = workspace.Path;
        var markerPath = Assert.Single(Directory.EnumerateFiles(rootPath));

        try
        {
            var ownershipToken = File.ReadAllText(markerPath, new UTF8Encoding(false, true));
            var changedFirstCharacter = ownershipToken[0] == '0' ? '1' : '0';
            File.WriteAllText(
                markerPath,
                $"{changedFirstCharacter}{ownershipToken[1..]}",
                new UTF8Encoding(false, true));

            Assert.Throws<InvalidOperationException>(() => workspace.Dispose());
            Assert.True(Directory.Exists(rootPath));
        }
        finally
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }
        }
    }

    private static void AssertRootName(string rootPath, string purpose)
    {
        var name = System.IO.Path.GetFileName(rootPath);
        var prefix = $"open-forge-{purpose}-";
        Assert.StartsWith(prefix, name, StringComparison.Ordinal);
        Assert.True(Guid.TryParseExact(name[prefix.Length..], "N", out _));
        Assert.True(Directory.Exists(rootPath));
    }

    private static string Hash(string contents)
    {
        return Convert.ToHexString(SHA256.HashData(new UTF8Encoding(false, true).GetBytes(contents)));
    }
}
