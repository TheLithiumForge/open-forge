using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update;

public sealed class UpdateMigrationResultTests
{
    [Fact(DisplayName = "Update migration facts preserve empty defaults and positional build constructors"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void PreservesEmptyDefaultsAndExistingBuildConstructors()
    {
        var facts = UpdateMigrationFacts.Create([], []);
        var unplannedEffect = UpdateMigrationFacts.Create(
            [],
            [FileEffect("docs/no-migration.md", UpdatePhysicalEffectOutcome.Verified)]);
        var navigation = new UpdateGeneratedNavigationBuild(new Dictionary<string, byte[]>(), [], null);
        var intended = new UpdateIntendedStateBuild([], [], null, Cancelled: false);

        Assert.Empty(facts);
        Assert.Empty(unplannedEffect);
        Assert.Empty(navigation.AdoptionTargets);
        Assert.Empty(navigation.Migrations);
        Assert.Empty(intended.AdoptionTargets);
        Assert.Empty(intended.Migrations);
    }

    [Fact(DisplayName = "Update migration facts preserve the default non-user-owned source flag without changing outcome"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void PreservesDefaultUserOwnedSourceFlag()
    {
        var migration = Assert.Single(UpdateMigrationFacts.Create(
            [Plan("docs/framework.md")],
            [FileEffect("docs/framework.md", UpdatePhysicalEffectOutcome.Verified)]));

        Assert.False(migration.IsUserOwnedSource);
        Assert.Equal(UpdateMigrationOutcome.Applied, migration.Outcome);
    }

    [Fact(DisplayName = "Update migration facts preserve user-owned source flag without changing outcome"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void PreservesUserOwnedSourceFlag()
    {
        var migration = Assert.Single(UpdateMigrationFacts.Create(
            [Plan("docs/adopted.md", isUserOwnedSource: true)],
            [FileEffect("docs/adopted.md", UpdatePhysicalEffectOutcome.NotStarted)]));

        Assert.True(migration.IsUserOwnedSource);
        Assert.Equal(UpdateMigrationOutcome.Planned, migration.Outcome);
    }

    [Fact(DisplayName = "Update migration facts use matching verified file effects and deterministic path order"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void OrdersUniquePathsAndAppliesOnlyMatchingVerifiedFileEffects()
    {
        var facts = UpdateMigrationFacts.Create(
            [Plan("docs/z.md"), Plan("docs/a.md"), Plan("docs/missing.md")],
            [
                FileEffect("docs/z.md", UpdatePhysicalEffectOutcome.Verified),
                FileEffect("docs/a.md", UpdatePhysicalEffectOutcome.NotStarted),
                FileEffect("docs/unmatched.md", UpdatePhysicalEffectOutcome.Verified),
            ]);

        Assert.Equal(["docs/a.md", "docs/missing.md", "docs/z.md"], facts.Select(fact => fact.Path));
        Assert.Equal(UpdateMigrationOutcome.Planned, facts[0].Outcome);
        Assert.Equal(UpdateMigrationOutcome.Planned, facts[1].Outcome);
        Assert.Equal(UpdateMigrationOutcome.Applied, facts[2].Outcome);
    }

    [Fact(DisplayName = "Nonverified effects keep Update migration planned"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void NonverifiedEffectsKeepMigrationPlanned()
    {
        foreach (var outcome in new[]
        {
            UpdatePhysicalEffectOutcome.Planned,
            UpdatePhysicalEffectOutcome.NotStarted,
            UpdatePhysicalEffectOutcome.VerificationFailed,
            UpdatePhysicalEffectOutcome.CompletionUnknown,
        })
        {
            var facts = UpdateMigrationFacts.Create(
                [Plan("docs/custom.md")],
                [FileEffect("docs/custom.md", outcome)]);

            Assert.Equal(UpdateMigrationOutcome.Planned, Assert.Single(facts).Outcome);
        }
    }

    [Fact(DisplayName = "Update migrations remain planned for cancellation and verified directory effects"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void MissingFileEffectAndVerifiedDirectoryKeepMigrationPlanned()
    {
        var noEffect = UpdateMigrationFacts.Create([Plan("docs/cancelled.md")], []);
        var directoryEffect = UpdateMigrationFacts.Create(
            [Plan("docs/folder.md")],
            [UpdatePhysicalEffect.Directory(
                "docs/folder.md",
                UpdatePhysicalEffectOutcome.Verified,
                UpdatePhysicalEffectResidual.None)]);

        Assert.Equal(UpdateMigrationOutcome.Planned, Assert.Single(noEffect).Outcome);
        Assert.Equal(UpdateMigrationOutcome.Planned, Assert.Single(directoryEffect).Outcome);
    }

    [Fact(DisplayName = "Update migration facts reject null duplicate and noncanonical inputs"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RejectsInvalidMigrationFactInputs()
    {
        Assert.Throws<ArgumentNullException>(() => UpdateMigrationFacts.Create(null!, []));
        Assert.Throws<ArgumentNullException>(() => UpdateMigrationFacts.Create([], null!));
        Assert.Throws<ArgumentException>(() => UpdateMigrationFacts.Create([Plan("docs/a.md"), Plan("docs/a.md")], []));
        Assert.Throws<ArgumentException>(() => UpdateMigrationFacts.Create([Plan("../docs/a.md")], []));
        Assert.Throws<ArgumentException>(() => UpdateMigrationFacts.Create([null!], []));
        Assert.Throws<ArgumentException>(() => UpdateMigrationFacts.Create([], [null!]));
        Assert.Throws<ArgumentException>(() => UpdateMigrationFacts.Create(
            [Plan("docs/a.md")],
            [FileEffect("docs/a.md", UpdatePhysicalEffectOutcome.Verified), FileEffect("docs/a.md", UpdatePhysicalEffectOutcome.Planned)]));
    }

    [Fact(DisplayName = "Update migration constructors reject invalid enum and null facts"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void MigrationConstructorRejectsInvalidFacts()
    {
        Assert.Throws<ArgumentException>(() => new UpdateMigration(
            "docs/custom.md",
            [(WorkspaceAdoptionAction)int.MaxValue],
            ["description"],
            [Derivation()],
            UpdateMigrationOutcome.Planned));
        Assert.Throws<ArgumentOutOfRangeException>(() => new UpdateMigration(
            "docs/custom.md",
            [WorkspaceAdoptionAction.MetadataCompleted],
            ["description"],
            [Derivation()],
            (UpdateMigrationOutcome)int.MaxValue));
        Assert.Throws<ArgumentNullException>(() => new UpdateMigration(
            "docs/custom.md",
            null!,
            ["description"],
            [Derivation()],
            UpdateMigrationOutcome.Planned));
        Assert.Throws<ArgumentException>(() => new UpdateMigration(
            "docs/custom.md",
            [WorkspaceAdoptionAction.MetadataCompleted],
            ["description"],
            [null!],
            UpdateMigrationOutcome.Planned));
        Assert.Throws<ArgumentException>(() => new UpdateMigration(
            "../docs/custom.md",
            [WorkspaceAdoptionAction.MetadataCompleted],
            ["description"],
            [Derivation()],
            UpdateMigrationOutcome.Planned));
        Assert.Throws<ArgumentOutOfRangeException>(() => Derivation((WorkspaceAdoptionDerivationSource)int.MaxValue));
    }

    [Fact(DisplayName = "Update migration outcome machine names are stable"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void ReadsEveryMigrationOutcomeName()
    {
        Assert.Equal("planned", UpdateDefinitions.ReadMachineName(UpdateMigrationOutcome.Planned));
        Assert.Equal("applied", UpdateDefinitions.ReadMachineName(UpdateMigrationOutcome.Applied));
    }

    [Fact(DisplayName = "Update migration outcome machine names reject undefined values"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RejectsUndefinedMigrationOutcomeName()
        => Assert.Throws<ArgumentOutOfRangeException>(() => UpdateDefinitions.ReadMachineName((UpdateMigrationOutcome)int.MaxValue));

    [Fact(DisplayName = "Update migration action machine names are stable"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void ReadsEveryMigrationActionName()
    {
        Assert.Equal("metadata-completed", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionAction.MetadataCompleted));
        Assert.Equal("entrypoint-created", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionAction.EntrypointCreated));
        Assert.Equal("entries-section-added", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionAction.EntriesSectionAdded));
        Assert.Equal("navigation-updated", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionAction.NavigationUpdated));
        Assert.Equal("content-preserved", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionAction.ContentPreserved));
    }

    [Fact(DisplayName = "Update migration action machine names reject undefined values"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RejectsUndefinedMigrationActionName()
        => Assert.Throws<ArgumentOutOfRangeException>(() => UpdateDefinitions.ReadMachineName((WorkspaceAdoptionAction)int.MaxValue));

    [Fact(DisplayName = "Update migration derivation source machine names are stable"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void ReadsEveryMigrationDerivationSourceName()
    {
        Assert.Equal("existing-description", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionDerivationSource.ExistingDescription));
        Assert.Equal("existing-title", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionDerivationSource.ExistingTitle));
        Assert.Equal("heading", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionDerivationSource.Heading));
        Assert.Equal("relative-path", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionDerivationSource.RelativePath));
        Assert.Equal("directory-name", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionDerivationSource.DirectoryName));
        Assert.Equal("required-tag", UpdateDefinitions.ReadMachineName(WorkspaceAdoptionDerivationSource.RequiredTag));
    }

    [Fact(DisplayName = "Update migration derivation source names reject undefined values"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RejectsUndefinedMigrationDerivationSourceName()
        => Assert.Throws<ArgumentOutOfRangeException>(() => UpdateDefinitions.ReadMachineName(
            (WorkspaceAdoptionDerivationSource)int.MaxValue));

    [Fact(DisplayName = "Every migration vocabulary member has an explicit name"), Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void MigrationVocabularyCoversEveryNamedEnumMember()
    {
        Assert.Equal(
            Enum.GetValues<UpdateMigrationOutcome>().OrderBy(value => value),
            new[] { UpdateMigrationOutcome.Planned, UpdateMigrationOutcome.Applied }.OrderBy(value => value));
        Assert.Equal(
            Enum.GetValues<WorkspaceAdoptionAction>().OrderBy(value => value),
            new[]
            {
                WorkspaceAdoptionAction.MetadataCompleted,
                WorkspaceAdoptionAction.EntrypointCreated,
                WorkspaceAdoptionAction.EntriesSectionAdded,
                WorkspaceAdoptionAction.NavigationUpdated,
                WorkspaceAdoptionAction.ContentPreserved,
            }.OrderBy(value => value));
        Assert.Equal(
            Enum.GetValues<WorkspaceAdoptionDerivationSource>().OrderBy(value => value),
            new[]
            {
                WorkspaceAdoptionDerivationSource.ExistingDescription,
                WorkspaceAdoptionDerivationSource.ExistingTitle,
                WorkspaceAdoptionDerivationSource.Heading,
                WorkspaceAdoptionDerivationSource.RelativePath,
                WorkspaceAdoptionDerivationSource.DirectoryName,
                WorkspaceAdoptionDerivationSource.RequiredTag,
            }.OrderBy(value => value));
    }

    private static UpdateMigrationPlan Plan(string path, bool isUserOwnedSource = false)
        => new()
        {
            Path = path,
            Actions = [WorkspaceAdoptionAction.MetadataCompleted],
            Fields = ["description"],
            Derivation = [Derivation()],
            IsUserOwnedSource = isUserOwnedSource,
        };

    private static WorkspaceAdoptionDerivation Derivation(
        WorkspaceAdoptionDerivationSource source = WorkspaceAdoptionDerivationSource.RelativePath)
        => new("description", source);

    private static UpdatePhysicalEffect FileEffect(string path, UpdatePhysicalEffectOutcome outcome)
        => new(
            path,
            UpdatePhysicalEffectAction.Replace,
            [new UpdateLogicalChange(UpdateComparisonTargetKind.File, UpdateLogicalChangeAction.Replace, null, null)],
            outcome,
            UpdatePhysicalEffectResidual.None);
}
