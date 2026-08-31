using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.Lifecycle;

public sealed class FrameworkLifecycleCurrentnessReaderRedTests
{
    [Fact(DisplayName = "Framework lifecycle currentness preserves coherent state details and bounded causes"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void CurrentnessModelPreservesCoherentStateDetailsAndBoundedCauses()
    {
        var current = new FrameworkLifecycleCurrentness(
            FrameworkLifecycleCurrentnessState.Current,
            path: null,
            cause: null);
        var cancelled = new FrameworkLifecycleCurrentness(
            FrameworkLifecycleCurrentnessState.Cancelled,
            path: null,
            cause: null);
        var longCause = new string('x', 512);
        var sourceMismatch = new FrameworkLifecycleCurrentness(
            FrameworkLifecycleCurrentnessState.SourceMismatch,
            path: null,
            cause: longCause);

        Assert.Equal(FrameworkLifecycleCurrentnessState.Current, current.State);
        Assert.Null(current.Path);
        Assert.Null(current.Cause);
        Assert.Equal(FrameworkLifecycleCurrentnessState.Cancelled, cancelled.State);
        Assert.Null(cancelled.Path);
        Assert.Null(cancelled.Cause);
        Assert.Equal(FrameworkLifecycleCurrentnessState.SourceMismatch, sourceMismatch.State);
        Assert.Null(sourceMismatch.Path);
        Assert.Equal(longCause[..256], sourceMismatch.Cause);

        foreach (var state in new[]
        {
            FrameworkLifecycleCurrentnessState.Changed,
            FrameworkLifecycleCurrentnessState.Missing,
            FrameworkLifecycleCurrentnessState.Unavailable,
            FrameworkLifecycleCurrentnessState.Blocked,
        })
        {
            var targetObservation = new FrameworkLifecycleCurrentness(state, "target", "cause");

            Assert.Equal(state, targetObservation.State);
            Assert.Equal("target", targetObservation.Path);
            Assert.Equal("cause", targetObservation.Cause);
            Assert.Throws<ArgumentException>(() =>
                new FrameworkLifecycleCurrentness(state, null, "cause"));
            Assert.Throws<ArgumentException>(() =>
                new FrameworkLifecycleCurrentness(state, "target", null));
        }

        Assert.Throws<ArgumentException>(() =>
            new FrameworkLifecycleCurrentness(
                FrameworkLifecycleCurrentnessState.Current,
                "target",
                null));
        Assert.Throws<ArgumentException>(() =>
            new FrameworkLifecycleCurrentness(
                FrameworkLifecycleCurrentnessState.SourceMismatch,
                null,
                "   "));
        Assert.Throws<ArgumentException>(() =>
            new FrameworkLifecycleCurrentness(
                FrameworkLifecycleCurrentnessState.Cancelled,
                null,
                "cause"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FrameworkLifecycleCurrentness(
                (FrameworkLifecycleCurrentnessState)int.MaxValue,
                null,
                null));
    }

    [Theory(DisplayName = "Framework lifecycle currentness reader reports each frozen physical boundary"),
        InlineData("current"),
        InlineData("source-mismatch"),
        InlineData("changed"),
        InlineData("missing"),
        InlineData("unavailable"),
        InlineData("blocked"),
        InlineData("cancelled")]
    [Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public async Task ReaderReportsEachFrozenPhysicalBoundary(string scenario)
    {
        var workspaceRoot = Path.Combine(
            Path.GetTempPath(),
            $"open-forge-route-init-currentness-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workspaceRoot);

        try
        {
            var payloadBytes = "embedded loader\n"u8.ToArray();
            var payload = FrameworkPayload.Create(
            [
                FrameworkPayloadAsset.Create(FrameworkPayloadAsset.RootAgentPath, "root agents\n"u8),
                FrameworkPayloadAsset.Create(FrameworkPayloadAsset.RootClaudePath, "root claude\n"u8),
                FrameworkPayloadAsset.Create(FrameworkPayloadAsset.LoaderPath, payloadBytes),
            ]);
            var logicalTargetPath = scenario == "blocked"
                ? "../outside.md"
                : FrameworkPayloadAsset.LoaderPath;
            var physicalTargetPath = Path.Combine(workspaceRoot, ".agents", "loader.md");

            if (scenario is "current" or "changed")
            {
                Directory.CreateDirectory(Path.GetDirectoryName(physicalTargetPath)!);
                File.WriteAllBytes(
                    physicalTargetPath,
                    scenario == "current" ? payloadBytes : "changed loader\n"u8.ToArray());
            }
            else if (scenario == "unavailable")
            {
                Directory.CreateDirectory(physicalTargetPath);
            }

            var lifecycle = new FrameworkLifecycleState
            {
                Coverage = LifecycleSchema.CompleteCoverage,
                Source = new FrameworkLifecycleSource
                {
                    Id = "open-forge",
                    Version = "1.0.0",
                    InventoryFingerprint = scenario == "source-mismatch"
                        ? new string('b', 64)
                        : payload.InventoryFingerprint,
                },
                Targets =
                [
                    new FrameworkLifecycleTarget
                    {
                        Path = logicalTargetPath,
                        SourceAssetPath = FrameworkPayloadAsset.LoaderPath,
                        Region = null,
                        BaselineFingerprint = payload.Find(FrameworkPayloadAsset.LoaderPath)!.Sha256,
                        FingerprintKind = LifecycleSchema.ExactBytesFingerprintKind,
                    },
                ],
                GeneratedRegions = [],
            };
            var workspace = new CliWorkspace(
                workspaceRoot,
                workspaceRoot,
                CliWorkspaceSelectionMethod.ExplicitWorkspace);
            var reader = new FrameworkLifecycleCurrentnessReader(new PhysicalPathResolver());
            using var cancellation = new CancellationTokenSource();
            if (scenario == "cancelled")
            {
                cancellation.Cancel();
            }

            var currentness = await reader.ReadAsync(
                workspace,
                lifecycle,
                payload,
                cancellation.Token);

            var expectedState = scenario switch
            {
                "current" => FrameworkLifecycleCurrentnessState.Current,
                "source-mismatch" => FrameworkLifecycleCurrentnessState.SourceMismatch,
                "changed" => FrameworkLifecycleCurrentnessState.Changed,
                "missing" => FrameworkLifecycleCurrentnessState.Missing,
                "unavailable" => FrameworkLifecycleCurrentnessState.Unavailable,
                "blocked" => FrameworkLifecycleCurrentnessState.Blocked,
                "cancelled" => FrameworkLifecycleCurrentnessState.Cancelled,
                _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario),
            };

            Assert.Equal(expectedState, currentness.State);
            if (expectedState is FrameworkLifecycleCurrentnessState.Current
                or FrameworkLifecycleCurrentnessState.Cancelled)
            {
                Assert.Null(currentness.Path);
                Assert.Null(currentness.Cause);
            }
            else if (expectedState == FrameworkLifecycleCurrentnessState.SourceMismatch)
            {
                Assert.Null(currentness.Path);
                Assert.False(string.IsNullOrWhiteSpace(currentness.Cause));
                Assert.InRange(currentness.Cause!.Length, 1, 256);
            }
            else
            {
                Assert.Equal(logicalTargetPath, currentness.Path);
                Assert.False(string.IsNullOrWhiteSpace(currentness.Cause));
                Assert.InRange(currentness.Cause!.Length, 1, 256);
            }
        }
        finally
        {
            if (Directory.Exists(workspaceRoot))
            {
                Directory.Delete(workspaceRoot, recursive: true);
            }
        }
    }
}
