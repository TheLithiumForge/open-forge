using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
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
        Assert.Equal(
        [
            ("extension-install.invalid-input", CliSemanticStatus.Invalid),
            ("extension-install.selection-required", CliSemanticStatus.Invalid),
            ("extension-install.interaction-ended", CliSemanticStatus.Invalid),
            ("extension-install.confirmation-required", CliSemanticStatus.Invalid),
            ("extension-install.source-unavailable", CliSemanticStatus.Incomplete),
            ("extension-install.source-invalid", CliSemanticStatus.Invalid),
            ("extension-install.framework-unavailable", CliSemanticStatus.Incomplete),
            ("extension-install.framework-unsafe", CliSemanticStatus.Blocked),
            ("extension-install.lifecycle-unavailable", CliSemanticStatus.Incomplete),
            ("extension-install.lifecycle-blocked", CliSemanticStatus.Blocked),
            ("extension-install.managed-divergence", CliSemanticStatus.Blocked),
            ("extension-install.package-contents-changed", CliSemanticStatus.Blocked),
            ("extension-install.initial-force-required", CliSemanticStatus.Blocked),
            ("extension-install.ownership-conflict", CliSemanticStatus.Blocked),
            ("extension-install.permission-required", CliSemanticStatus.Blocked),
            ("extension-install.permission-declined", CliSemanticStatus.Blocked),
            ("extension-install.permissions-invalid", CliSemanticStatus.Blocked),
            ("extension-install.permissions-unavailable", CliSemanticStatus.Incomplete),
            ("extension-install.permissions-changed", CliSemanticStatus.Blocked),
            ("extension-install.permission-write-failed", CliSemanticStatus.Failed),
            ("extension-install.target-unsafe", CliSemanticStatus.Blocked),
            ("extension-install.projection-unavailable", CliSemanticStatus.Incomplete),
            ("extension-install.generated-region-unsafe", CliSemanticStatus.Blocked),
            ("extension-install.workspace-lock-unavailable", CliSemanticStatus.Blocked),
            ("extension-install.target-changed", CliSemanticStatus.Blocked),
            ("extension-install.recovery-conflict", CliSemanticStatus.Blocked),
            ("extension-install.recovery-unavailable", CliSemanticStatus.Incomplete),
            ("extension-install.lifecycle-observation", CliSemanticStatus.Attention),
            ("extension-install.package-content-missing", CliSemanticStatus.Attention),
            ("extension-install.recovery-artifact-retained", CliSemanticStatus.Attention),
            ("extension-install.write-failed", CliSemanticStatus.Failed),
            ("extension-install.topology-verification-failed", CliSemanticStatus.Failed),
            ("extension-install.lifecycle-publication-failed", CliSemanticStatus.Failed),
            ("extension-install.verification-failed", CliSemanticStatus.Failed),
            ("extension-install.recovery-failed", CliSemanticStatus.Failed),
            ("extension-install.operation-failed", CliSemanticStatus.Failed),
            ("extension-install.interrupted", CliSemanticStatus.Interrupted),
            ("extension-install.metadata-projection-skipped", CliSemanticStatus.Attention),
        ],
            ExtensionInstallDefinitions.FindingCodes.Select(code => (
                ExtensionInstallDefinitions.ReadMachineName(code),
                ExtensionInstallDefinitions.ReadStatus(code))));

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

        var quotedWorkspace = $"'{workspacePath.Replace("'", "'\"'\"'", StringComparison.Ordinal)}'";
        Assert.Equal(
            $"open-forge extension install base toolkit --source 'catalogue folder'\"'\"'s source' --force --automatic --dry-run --workspace {quotedWorkspace}",
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
            "open-forge extension install --all --force --workspace '"
                + Workspace().LexicalRoot
                + "'",
            ExtensionInstallDefinitions.ReadNextAction(
                CliSemanticStatus.Blocked,
                [finding],
                allRequest)?.Command);
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

        var result = ExtensionInstallPayloadNormalizer.Normalize([package]);

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
