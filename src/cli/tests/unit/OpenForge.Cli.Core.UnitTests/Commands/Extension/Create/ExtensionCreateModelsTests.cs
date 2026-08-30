using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Create;

public sealed class ExtensionCreateModelsTests
{
    [Fact(DisplayName = "Extension Create request retains every raw input and an immutable dependency snapshot"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void RequestRetainsImmutableRawInput()
    {
        var dependencies = new List<string> { "zeta", "alpha" };
        var request = new ExtensionCreateRequest
        {
            StableId = "development-toolkit",
            CataloguePath = "/catalogue",
            Name = "Development Toolkit",
            Description = "Adds workflows",
            PackageVersion = "0.2-preview",
            Dependencies = dependencies,
            AllowInteraction = true,
            Mode = ExtensionCreateMode.DryRun,
        };

        dependencies[0] = "changed";
        dependencies.Add("another");

        Assert.Equal("development-toolkit", request.StableId);
        Assert.Equal("/catalogue", request.CataloguePath);
        Assert.Equal("Development Toolkit", request.Name);
        Assert.Equal("Adds workflows", request.Description);
        Assert.Equal("0.2-preview", request.PackageVersion);
        Assert.Equal(["zeta", "alpha"], request.Dependencies);
        Assert.True(request.AllowInteraction);
        Assert.Equal(ExtensionCreateMode.DryRun, request.Mode);
    }

    [Fact(DisplayName = "Extension Create manifest retains exact identity metadata and dependency sequence"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void ManifestRetainsExactMetadata()
    {
        var dependencies = new[] { "alpha", "zeta" };
        var manifest = new ExtensionCreateManifest
        {
            Id = "development-toolkit",
            Name = "Development Toolkit",
            Description = "Open Forge Extension package development-toolkit.",
            Version = "0.1.0",
            Dependencies = dependencies,
        };

        dependencies[0] = "changed";

        Assert.Equal("development-toolkit", manifest.Id);
        Assert.Equal("Development Toolkit", manifest.Name);
        Assert.Equal("Open Forge Extension package development-toolkit.", manifest.Description);
        Assert.Equal("0.1.0", manifest.Version);
        Assert.Equal(["alpha", "zeta"], manifest.Dependencies);
    }

    [Fact(DisplayName = "Extension Create plan keeps catalogue and destination identity, manifest bytes, mode, and ordered effects"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void PlanRetainsExactScaffoldFacts()
    {
        var effects = new ExtensionCreateEffect[]
        {
            new() { Kind = ExtensionCreateEffectKind.ManifestFile, Path = "/catalogue/development-toolkit/extension.json" },
            new() { Kind = ExtensionCreateEffectKind.PayloadAgentsDirectory, Path = "/catalogue/development-toolkit/payload/.agents/" },
        };
        var plan = new ExtensionCreatePlan
        {
            Catalogue = "/catalogue",
            CataloguePhysicalIdentity = "/catalogue",
            Destination = "/catalogue/development-toolkit",
            DestinationPhysicalIdentity = null,
            Manifest = CreateManifest(),
            ManifestBytes = new byte[] { 1, 2, 3 },
            Mode = ExtensionCreateMode.Apply,
            IntendedEffects = effects,
            IsVerifiedNoOp = false,
        };

        Assert.Equal("/catalogue", plan.Catalogue);
        Assert.Equal("/catalogue", plan.CataloguePhysicalIdentity);
        Assert.Equal("/catalogue/development-toolkit", plan.Destination);
        Assert.Null(plan.DestinationPhysicalIdentity);
        Assert.Equal(ExtensionCreateMode.Apply, plan.Mode);
        Assert.Equal([ExtensionCreateEffectKind.ManifestFile, ExtensionCreateEffectKind.PayloadAgentsDirectory], plan.IntendedEffects.Select(effect => effect.Kind));
        Assert.Equal(["/catalogue/development-toolkit/extension.json", "/catalogue/development-toolkit/payload/.agents/"], plan.IntendedEffects.Select(effect => effect.Path));
        Assert.Equal([1, 2, 3], plan.ManifestBytes.ToArray());
        Assert.False(plan.IsVerifiedNoOp);
    }

    [Fact(DisplayName = "Extension Create complete result is workspace-free and retains intended and applied effect facts"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void ResultRetainsWorkspaceFreeFacts()
    {
        var result = CreateResult(
            CliSemanticStatus.Complete,
            appliedEffects:
            [
                new() { Kind = ExtensionCreateEffectKind.ManifestFile, Path = "/catalogue/development-toolkit/extension.json" },
            ]);

        Assert.Equal("extension create", result.Command);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Workspace);
        Assert.Equal("/catalogue", result.Catalogue);
        Assert.Equal("/catalogue/development-toolkit", result.Destination);
        Assert.Equal("development-toolkit", result.StableId);
        Assert.Equal("Development Toolkit", result.Manifest!.Name);
        Assert.Equal(2, result.IntendedEffects.Count);
        Assert.Single(result.AppliedEffects);
        Assert.Equal(ExtensionCreateVerificationState.Verified, result.Verification.Destination);
        Assert.Null(result.Next);
    }

    [Theory(DisplayName = "Extension Create next-action mapping has no attention action and one bounded action for every other named status"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData(CliSemanticStatus.Complete, "none")]
    [InlineData(CliSemanticStatus.Incomplete, "open-forge extension create --verbose")]
    [InlineData(CliSemanticStatus.Attention, "none")]
    [InlineData(CliSemanticStatus.Invalid, "open-forge extension create --help")]
    [InlineData(CliSemanticStatus.Blocked, "open-forge extension create --dry-run")]
    [InlineData(CliSemanticStatus.Failed, "open-forge extension create --verbose")]
    [InlineData(CliSemanticStatus.Interrupted, "open-forge extension create")]
    public void NextActionsAreExactAndBounded(
        object statusValue,
        string expectedCommand)
    {
        var status = Assert.IsType<CliSemanticStatus>(statusValue);
        var next = ExtensionCreateDefinitions.ReadNext(status);

        if (expectedCommand == "none")
        {
            Assert.Null(next);
            return;
        }

        Assert.NotNull(next);
        Assert.Equal(expectedCommand, next!.Command);
        Assert.False(string.IsNullOrWhiteSpace(next.Reason));
    }

    [Fact(DisplayName = "Extension Create next-action cases cover every named semantic status"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void NextActionCasesCoverEveryNamedStatus()
    {
        CliSemanticStatus[] specified =
        [
            CliSemanticStatus.Complete,
            CliSemanticStatus.Failed,
            CliSemanticStatus.Attention,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Interrupted,
        ];

        Assert.Equal(Enum.GetValues<CliSemanticStatus>(), specified);
    }

    [Fact(DisplayName = "Extension Create rejects an undefined semantic status at the next-action mapping"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void NextActionRejectsUndefinedStatus()
    {
        var undefined = (CliSemanticStatus)int.MaxValue;

        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionCreateDefinitions.ReadNext(undefined));
    }

    [Fact(DisplayName = "Extension Create result formation rejects the unreachable attention status"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void ResultFactoryRejectsAttention()
    {
        var request = new ExtensionCreateRequest
        {
            StableId = "development-toolkit",
            CataloguePath = "/catalogue",
            Name = null,
            Description = null,
            PackageVersion = null,
            Dependencies = [],
            AllowInteraction = false,
            Mode = ExtensionCreateMode.Apply,
        };

        Assert.Throws<ArgumentException>(
            () => ExtensionCreateResultFactory.Create(
                request,
                new ExtensionCreateResultOutcome
                {
                    Status = CliSemanticStatus.Attention,
                    Verification = Verified(),
                }));
    }

    private static ExtensionCreateResult CreateResult(
        CliSemanticStatus status,
        IEnumerable<ExtensionCreateEffect> appliedEffects)
        => new()
        {
            Status = status,
            Catalogue = "/catalogue",
            Destination = "/catalogue/development-toolkit",
            StableId = "development-toolkit",
            Manifest = CreateManifest(),
            Mode = ExtensionCreateMode.Apply,
            IntendedEffects =
            [
                new() { Kind = ExtensionCreateEffectKind.ManifestFile, Path = "/catalogue/development-toolkit/extension.json" },
                new() { Kind = ExtensionCreateEffectKind.PayloadAgentsDirectory, Path = "/catalogue/development-toolkit/payload/.agents/" },
            ],
            AppliedEffects = appliedEffects.ToArray(),
            Verification = Verified(),
            Findings = [],
            Next = status == CliSemanticStatus.Complete ? null : ExtensionCreateDefinitions.ReadNext(status),
        };

    private static ExtensionCreateManifest CreateManifest()
        => new()
        {
            Id = "development-toolkit",
            Name = "Development Toolkit",
            Description = "Open Forge Extension package development-toolkit.",
            Version = "0.1.0",
            Dependencies = [],
        };

    private static ExtensionCreateVerification Verified()
        => new()
        {
            Catalogue = ExtensionCreateVerificationState.Verified,
            Destination = ExtensionCreateVerificationState.Verified,
            Manifest = ExtensionCreateVerificationState.Verified,
            Payload = ExtensionCreateVerificationState.Verified,
            Cause = null,
        };
}
