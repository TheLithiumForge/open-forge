using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Lifecycle;

public sealed class FrameworkLifecycleCurrentnessReaderIntegrationTests
{
    [Theory(DisplayName = "Framework lifecycle currentness reader reports each frozen physical boundary"),
        InlineData("current"),
        InlineData("source-mismatch"),
        InlineData("changed"),
        InlineData("missing"),
        InlineData("unavailable"),
        InlineData("blocked"),
        InlineData("cancelled")]
    [Trait("Feature", "route-init"), Trait("Evidence", "Integration")]
    public async Task ReaderReportsEachFrozenPhysicalBoundary(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("route-init-currentness");
        var workspaceRoot = temporary.Path;
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
        if (scenario is "current" or "changed")
        {
            temporary.CreateFile(
                ".agents/loader.md",
                scenario == "current" ? payloadBytes : "changed loader\n"u8.ToArray());
        }
        else if (scenario == "unavailable")
        {
            temporary.CreateDirectory(".agents/loader.md");
        }

        var loaderAsset = payload.Find(FrameworkPayloadAsset.LoaderPath)
            ?? throw new InvalidOperationException("The embedded loader asset is unavailable.");
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
                    BaselineFingerprint = loaderAsset.Sha256,
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
            var cause = Assert.IsType<string>(currentness.Cause);
            Assert.False(string.IsNullOrWhiteSpace(cause));
            Assert.InRange(cause.Length, 1, 256);
        }
        else
        {
            Assert.Equal(logicalTargetPath, currentness.Path);
            var cause = Assert.IsType<string>(currentness.Cause);
            Assert.False(string.IsNullOrWhiteSpace(cause));
            Assert.InRange(cause.Length, 1, 256);
        }
    }
}
