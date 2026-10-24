using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install;

public sealed class ExtensionInstallContractTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Install request and operation close the exact typed callable"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    public async Task RequestAndCallableAreExact()
    {
        var workspace = Workspace();
        var requestedIds = new[] { "toolkit" };
        var request = new ExtensionInstallRequest(
            workspace,
            ExtensionInstallMode.DryRun,
            requestedIds,
            true,
            null,
            false,
            true,
            false);
        var scripted = ScriptedCliTerminal.Lines([], canPrompt: false);
        var prompts = new CliPrompts(scripted.Terminal);
        var operation = ExtensionInstallOperationFactory.Create(
            ExtensionInteractionTestFactory.ForInstall(prompts));
        CliOperation<ExtensionInstallRequest, ExtensionInstallResult> callable = operation.ExecuteAsync;

        var result = await callable(request, TestContext.Current.CancellationToken);
        requestedIds[0] = "changed-after-construction";

        Assert.Same(workspace, request.Workspace);
        Assert.Equal(ExtensionInstallMode.DryRun, request.Mode);
        Assert.Equal(["toolkit"], request.RequestedIds);
        Assert.NotSame(requestedIds, request.RequestedIds);
        Assert.True(request.All);
        Assert.Null(request.SourcePath);
        Assert.False(request.Force);
        Assert.True(request.Automatic);
        Assert.False(request.AllowInteraction);
        Assert.Equal("extension install", result.Command);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Same(workspace, result.Workspace);
        AssertTypedResult(result);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionInstallFindingCode.InvalidInput);
        Assert.Equal(string.Empty, scripted.Output.ToString());
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Install findings preserve exact declaration order machine codes and statuses"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    public void FindingMappingsAreExact()
    {
        Assert.Equal(
            [ExtensionInstallMode.Apply, ExtensionInstallMode.DryRun],
            Enum.GetValues<ExtensionInstallMode>());
        (ExtensionInstallFindingCode Code, CliSemanticStatus Status)[] expected =
        [
            (ExtensionInstallFindingCode.InvalidInput, CliSemanticStatus.Invalid),
            (ExtensionInstallFindingCode.SelectionRequired, CliSemanticStatus.Invalid),
            (ExtensionInstallFindingCode.InteractionEnded, CliSemanticStatus.Invalid),
            (ExtensionInstallFindingCode.ConfirmationRequired, CliSemanticStatus.Invalid),
            (ExtensionInstallFindingCode.SourceUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionInstallFindingCode.SourceInvalid, CliSemanticStatus.Invalid),
            (ExtensionInstallFindingCode.FrameworkUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionInstallFindingCode.FrameworkUnsafe, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.LifecycleUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionInstallFindingCode.LifecycleBlocked, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.ManagedDivergence, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.PackageContentsChanged, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.InitialForceRequired, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.OwnershipConflict, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.PermissionRequired, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.PermissionDeclined, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.PermissionsInvalid, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.PermissionsUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionInstallFindingCode.PermissionsChanged, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.PermissionWriteFailed, CliSemanticStatus.Failed),
            (ExtensionInstallFindingCode.TargetUnsafe, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.ProjectionUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionInstallFindingCode.GeneratedRegionUnsafe, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.WorkspaceLockUnavailable, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.TargetChanged, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.RecoveryConflict, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.RecoveryUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionInstallFindingCode.LifecycleObservation, CliSemanticStatus.Attention),
            (ExtensionInstallFindingCode.PackageContentMissing, CliSemanticStatus.Attention),
            (ExtensionInstallFindingCode.RecoveryArtifactRetained, CliSemanticStatus.Attention),
            (ExtensionInstallFindingCode.RecoveryHistory, CliSemanticStatus.Attention),
            (ExtensionInstallFindingCode.RecoveryCoverageUnavailable, CliSemanticStatus.Attention),
            (ExtensionInstallFindingCode.WriteFailed, CliSemanticStatus.Failed),
            (ExtensionInstallFindingCode.TopologyVerificationFailed, CliSemanticStatus.Failed),
            (ExtensionInstallFindingCode.LifecyclePublicationFailed, CliSemanticStatus.Failed),
            (ExtensionInstallFindingCode.VerificationFailed, CliSemanticStatus.Failed),
            (ExtensionInstallFindingCode.RecoveryFailed, CliSemanticStatus.Failed),
            (ExtensionInstallFindingCode.OperationFailed, CliSemanticStatus.Failed),
            (ExtensionInstallFindingCode.Interrupted, CliSemanticStatus.Interrupted),
            (ExtensionInstallFindingCode.MetadataProjectionSkipped, CliSemanticStatus.Attention),
            (ExtensionInstallFindingCode.SettingsInvalid, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.SettingsUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionInstallFindingCode.RemovedExtension, CliSemanticStatus.Blocked),
            (ExtensionInstallFindingCode.PathExcluded, CliSemanticStatus.Complete),
            (ExtensionInstallFindingCode.ExcludedAncestor, CliSemanticStatus.Blocked),
        ];
        Assert.Equal(expected.Select(item => item.Code), Enum.GetValues<ExtensionInstallFindingCode>());
        Assert.Equal(expected.Select(item => item.Code), ExtensionInstallDefinitions.FindingCodes);
        Assert.All(expected, item => Assert.Equal(item.Status, ExtensionInstallDefinitions.ReadStatus(item.Code)));
        CommandOutputSnapshot.MatchSnapshot(string.Join("\n", ExtensionInstallDefinitions.FindingCodes.Select(code =>
            $"{code}: {ExtensionInstallDefinitions.ReadMachineName(code)}")), "finding-codes");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtensionInstallDefinitions.ReadMachineName((ExtensionInstallFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtensionInstallDefinitions.ReadStatus((ExtensionInstallFindingCode)int.MaxValue));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Install initial-force next preserves the exact normalized request"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    public void InitialForceNextPreservesNormalizedRequest()
    {
        var workspacePath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "extension install's workspace"));
        var request = new ExtensionInstallRequest(
            new CliWorkspace(
                workspacePath,
                workspacePath,
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            ExtensionInstallMode.DryRun,
            ["base", "toolkit"],
            all: false,
            sourcePath: "catalogue folder's source",
            force: false,
            automatic: true,
            allowInteraction: false);
        var finding = new ExtensionInstallFinding(
            ExtensionInstallFindingCode.InitialForceRequired,
            "Exact initial force is required.");

        var next = Assert.IsType<CliNextAction>(ExtensionInstallDefinitions.ReadNextAction(
            CliSemanticStatus.Blocked,
            [finding],
            request));

        Assert.Equal(
            $"open-forge extension install base toolkit --source {CliCommandText.Quote(request.SourcePath ?? throw new InvalidOperationException())} --workspace {CliCommandText.Quote(workspacePath)} --automatic --force --dry-run",
            next.Command);

        var allRequest = new ExtensionInstallRequest(
            Workspace(),
            ExtensionInstallMode.Apply,
            requestedIds: [],
            all: true,
            sourcePath: null,
            force: false,
            automatic: false,
            allowInteraction: false);
        Assert.Equal(
            $"open-forge extension install --all --workspace {CliCommandText.Quote(Workspace().LexicalRoot)} --force --dry-run",
            ExtensionInstallDefinitions.ReadNextAction(CliSemanticStatus.Blocked, [finding], allRequest)?.Command);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtensionInstallDefinitions.ReadNextAction(
                (CliSemanticStatus)int.MaxValue,
                [],
                request));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Extension Install classifies selected payload state before target policy"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    [InlineData((int)ExtensionPackageFileReadState.Available, false, (int)ExtensionInstallFindingCode.SourceUnavailable)]
    [InlineData((int)ExtensionPackageFileReadState.Missing, false, (int)ExtensionInstallFindingCode.SourceUnavailable)]
    [InlineData((int)ExtensionPackageFileReadState.Unavailable, false, (int)ExtensionInstallFindingCode.SourceUnavailable)]
    [InlineData((int)ExtensionPackageFileReadState.Invalid, false, (int)ExtensionInstallFindingCode.SourceInvalid)]
    [InlineData((int)ExtensionPackageFileReadState.Blocked, false, (int)ExtensionInstallFindingCode.SourceInvalid)]
    [InlineData((int)ExtensionPackageFileReadState.Cancelled, false, (int)ExtensionInstallFindingCode.Interrupted)]
    public void PayloadStatePrecedesTargetPolicy(
        int stateValue,
        bool includeReviewedBytes,
        int expectedCodeValue)
    {
        const string target = "outside-agents.txt";
        var bytes = includeReviewedBytes ? "payload"u8.ToArray() : null;
        var file = ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
        {
            Path = $"content/{target}",
            TargetPath = target,
            State = (ExtensionPackageFileReadState)stateValue,
            ByteLength = bytes?.Length,
            Sha256 = bytes is null ? null : FileExpectation.Hash(bytes),
            Bytes = bytes is null
                ? (ReadOnlyMemory<byte>?)null
                : new ReadOnlyMemory<byte>(bytes),
        });
        var package = ExtensionPackageFact.Create(
            new ExtensionPackageManifestFact
            {
                Id = "toolkit",
                Name = "toolkit",
                Description = "Toolkit package.",
                Version = "1.0.0",
                Dependencies = [],
            },
            new ExtensionPackageContentsFact
            {
                ManifestPath = "extension.json",
                Payload = [file],
            });

        var result = ExtensionInstallPayloadNormalizer.Normalize(
            [package],
            WorkspaceSettingsDocument.Empty);

        Assert.Empty(result.Packages);
        var finding = Assert.IsType<ExtensionInstallFinding>(result.Finding);
        Assert.Equal((ExtensionInstallFindingCode)expectedCodeValue, finding.Code);
        Assert.Equal(target, finding.Target);
    }

    private static void AssertTypedResult(ExtensionInstallResult result)
    {
        ExtensionInstallSelection? selection = result.Selection;
        ExtensionInstallSource? source = result.Source;
        IReadOnlyList<ExtensionInstallPackage> packages = result.Packages;
        ExtensionInstallFramework? framework = result.Framework;
        ExtensionInstallFootprint? footprint = result.Footprint;
        IReadOnlyList<ExtensionInstallEffect> effects = result.Effects;
        ExtensionInstallGeneratedNavigation? generatedNavigation = result.GeneratedNavigation;
        ExtensionInstallLifecycle lifecycle = result.Lifecycle;
        ExtensionInstallRecovery recovery = result.Recovery;
        ExtensionInstallVerification verification = result.Verification;
        IReadOnlyList<ExtensionInstallFinding> findings = result.Findings;
        CliNextAction? next = result.Next;

        Assert.Null(selection);
        Assert.Null(source);
        Assert.Empty(packages);
        Assert.Null(framework);
        Assert.Null(footprint);
        Assert.Empty(effects);
        Assert.Null(generatedNavigation);
        Assert.NotNull(lifecycle);
        Assert.NotNull(recovery);
        Assert.NotNull(verification);
        Assert.NotEmpty(findings);
        Assert.NotNull(next);
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "extension-install-unit"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
