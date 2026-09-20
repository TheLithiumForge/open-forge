using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Observation;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryRecoveryEntryObservationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Recovery observation obtains ordinary content independently and preserves raw links without reading their targets")]
    [InlineData("missing"), InlineData("ordinary"), InlineData("relative"), InlineData("dangling"), InlineData("absolute"), InlineData("directory")]
    public static async Task ObservesExactObjectAndContentBoundary(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-recovery-observation");
        temporary.CreateDirectory(".agents");
        var source = temporary.CreateFile("source.json", "source payload");
        var path = temporary.Combine(".agents/record.json");
        switch (scenario)
        {
            case "ordinary": temporary.CreateFile(".agents/record.json", "record"); break;
            case "relative": temporary.CreateFileSymbolicLink(".agents/record.json", "../source.json"); break;
            case "dangling": temporary.CreateFileSymbolicLink(".agents/record.json", "../missing.json"); break;
            case "absolute": temporary.CreateFileSymbolicLink(".agents/record.json", source); break;
            case "directory": temporary.CreateDirectory(".agents/record.json"); break;
        }
        var context = Context(temporary);

        var result = await RecoveryEntryObservationReader.ReadAsync(new PhysicalPathResolver(), context, TestContext.Current.CancellationToken);

        Assert.Equal(context, result.Context);
        Assert.Equal(path, result.Leaf.LogicalPath);
        if (scenario == "ordinary")
        {
            Assert.Equal(NoFollowLeafState.OrdinaryFile, result.Leaf.State);
            var content = Assert.IsType<RecoveryOrdinaryContentObservation>(result.OrdinaryContent);
            var identity = Assert.IsType<RecoveryContentIdentity>(content.Identity);
            Assert.Equal(path, content.LogicalPath);
            Assert.Equal(6, identity.Length);
            Assert.Equal(Convert.ToHexStringLower(SHA256.HashData("record"u8)), identity.Sha256);
            Assert.Null(content.Failure);
        }
        else
        {
            Assert.Null(result.OrdinaryContent);
            var expected = scenario switch
            {
                "missing" => NoFollowLeafState.Missing,
                "relative" or "dangling" => NoFollowLeafState.RelativeFileLink,
                "absolute" => NoFollowLeafState.Link,
                "directory" => NoFollowLeafState.Directory,
                _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
            };
            Assert.Equal(expected, result.Leaf.State);
            if (scenario is "relative" or "dangling")
            {
                Assert.Equal(scenario == "relative" ? "../source.json" : "../missing.json",
                    Assert.IsType<RelativeFileLinkIdentity>(result.Leaf.RelativeFileLink).RawRelativeTarget);
            }
        }
        Assert.Equal("source payload", File.ReadAllText(source));
        Assert.False(File.Exists(temporary.Combine("missing.json")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Recovery observation never enters an external parent to read a same-named ordinary target")]
    public async Task BlocksExternalAncestryWithoutContentRead()
    {
        using var temporary = TemporaryWorkspace.Create("library-recovery-unsafe");
        using var external = TemporaryWorkspace.Create("library-recovery-external");
        var source = external.CreateFile("record.json", "outside");
        temporary.CreateDirectorySymbolicLink(".agents", external.Path);

        var result = await RecoveryEntryObservationReader.ReadAsync(new PhysicalPathResolver(), Context(temporary), TestContext.Current.CancellationToken);

        Assert.Null(result.OrdinaryContent);
        Assert.NotEqual(NoFollowLeafState.OrdinaryFile, result.Leaf.State);
        Assert.NotEqual(NoFollowLeafState.Missing, result.Leaf.State);
        Assert.Equal("outside", File.ReadAllText(source));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Recovery ordinary read failure remains explicit unavailable evidence")]
    public async Task RetainsIndependentReadFailure()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This evidence requires Unix file permissions.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create("library-recovery-unavailable");
        var path = temporary.CreateFile(".agents/record.json", "record");
        var mode = File.GetUnixFileMode(path);
        File.SetUnixFileMode(path, UnixFileMode.None);
        try
        {
            var result = await RecoveryEntryObservationReader.ReadAsync(new PhysicalPathResolver(), Context(temporary), TestContext.Current.CancellationToken);

            Assert.Equal(NoFollowLeafState.OrdinaryFile, result.Leaf.State);
            var content = Assert.IsType<RecoveryOrdinaryContentObservation>(result.OrdinaryContent);
            Assert.Null(content.Identity);
            Assert.NotNull(content.Failure);
        }
        finally
        {
            File.SetUnixFileMode(path, mode);
        }
    }

    private static RecoveryEntryComparisonContext Context(TemporaryWorkspace temporary)
    {
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var entry = RecoveryEntry.Create(ordinal: 0, logicalPath: CanonicalRelativePath.Create(".agents/record.json"),
            kind: RecoveryEntryKind.OrdinaryCreate, prior: RecoveryEntryState.Missing,
            intended: RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes("record"u8)));
        return new RecoveryEntryComparisonContext(workspace, entry);
    }
}
