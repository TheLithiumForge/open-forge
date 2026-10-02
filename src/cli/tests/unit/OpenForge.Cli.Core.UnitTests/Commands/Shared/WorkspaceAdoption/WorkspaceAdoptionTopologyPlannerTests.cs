using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.WorkspaceAdoption;

public sealed class WorkspaceAdoptionTopologyPlannerTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption routes Skill resources without creating an entrypoint in the native Skill root")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void NativeSkillResourcesUseTheirOwnCatalogueBoundary()
    {
        var skill = Source(".agents/skills/local-skill/SKILL.md");
        var rootReadme = Source(".agents/skills/local-skill/README.md");
        var reference = Source(".agents/skills/local-skill/references/setup.md");

        var plan = Plan(
            [reference, rootReadme, skill],
            [Source(".agents/skills/_skills.md")]);

        Assert.Null(plan.Cause);
        Assert.Equal(
            [skill.Identity.CanonicalBasePath, reference.Identity.CanonicalBasePath],
            plan.EligibleSourcePaths);
        Assert.Equal(
            [".agents/skills/local-skill/references/_references.md"],
            plan.CreatedEntrypointPaths);
        Assert.DoesNotContain(".agents/skills/local-skill/_local-skill.md", plan.CreatedEntrypointPaths);
        Assert.DoesNotContain(rootReadme.Identity.CanonicalBasePath, plan.EligibleSourcePaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption creates every missing route host for nested Markdown")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void NestedMarkdownCreatesRequiredHostChain()
    {
        var guide = Source(".agents/directives/csharp/api/v2/guide.md");

        var plan = Plan([guide], [Source(".agents/directives/_directives.md")]);

        Assert.Null(plan.Cause);
        Assert.Equal(
            [
                ".agents/directives/csharp/_csharp.md",
                ".agents/directives/csharp/api/_api.md",
                ".agents/directives/csharp/api/v2/_v2.md",
            ],
            plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption reuses one compatibility entrypoint for a payload route")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void UniqueCompatibilityEntrypointIsReused()
    {
        const string payloadPath = ".agents/guidance/_guidance.md";
        const string existingPath = ".agents/guidance/index.md";

        var plan = Plan([Source(existingPath)], [Source(payloadPath)]);

        Assert.Null(plan.Cause);
        Assert.Equal(existingPath, plan.ReusedEntrypoints[payloadPath]);
        Assert.Empty(plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption blocks multiple entrypoint candidates without returning a partial plan")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void MultipleExistingEntryPointsAreAmbiguous()
    {
        var plan = Plan(
            [
                Source(".agents/guidance/index.md"),
                Source(".agents/guidance/_guidance.md"),
            ],
            [Source(".agents/guidance/_guidance.md")]);

        AssertBlocked(plan, "Ambiguous recognized entrypoints");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption creates no routes for a complete existing chain")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void ExistingRouteChainNeedsNoNewEntrypoints()
    {
        var root = Source(".agents/directives/_directives.md");
        var language = Source(".agents/directives/csharp/_csharp.md");
        var topic = Source(".agents/directives/csharp/dotnet/_dotnet.md");
        var guide = Source(".agents/directives/csharp/dotnet/guide.md");

        var plan = Plan(
            [guide, topic, root, language],
            [root]);

        Assert.Null(plan.Cause);
        Assert.Empty(plan.CreatedEntrypointPaths);
        Assert.Equal(root.Identity.CanonicalBasePath, plan.ReusedEntrypoints[root.Identity.CanonicalBasePath]);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption does not select a root whose exact entrypoint file was removed")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void RemovedRootIsNotSelected()
    {
        var plan = WorkspaceAdoptionTopologyPlanner.Plan(
            [Source(".agents/directives/guide.md")],
            [Source(".agents/directives/_directives.md")],
            [".AGENTS/DIRECTIVES/_DIRECTIVES.md"],
            WorkspaceOwnershipRead.Absent(LockPath));

        Assert.Null(plan.Cause);
        Assert.Empty(plan.ReusedEntrypoints);
        Assert.Empty(plan.EligibleSourcePaths);
        Assert.Empty(plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "An exact RemovedFiles path does not exclude longer descendant paths")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void RemovedFileDoesNotExpandToItsPathPrefix()
    {
        var plan = WorkspaceAdoptionTopologyPlanner.Plan(
            [Source(".agents/directives/nested.md/guide.md")],
            [Source(".agents/directives/_directives.md")],
            [".agents/directives/nested.md"],
            WorkspaceOwnershipRead.Absent(LockPath));

        Assert.Null(plan.Cause);
        Assert.Contains(
            ".agents/directives/nested.md/_nested.md.md",
            plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption blocks when a required nested canonical entrypoint was removed")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void RemovedRequiredNestedEntrypointBlocks()
    {
        const string removedHost = ".agents/directives/nested/_nested.md";
        var plan = WorkspaceAdoptionTopologyPlanner.Plan(
            [Source(".agents/directives/nested/guide.md")],
            [Source(".agents/directives/_directives.md")],
            [removedHost],
            WorkspaceOwnershipRead.Absent(LockPath));

        AssertBlocked(plan, removedHost);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption ignores unrelated harness sources")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void UnrelatedHarnessIsOutsideSelectedRoots()
    {
        var harness = Source(".agents/harness/ci/check.md");

        var plan = Plan([harness], [Source(".agents/directives/_directives.md")]);

        Assert.Null(plan.Cause);
        Assert.Empty(plan.EligibleSourcePaths);
        Assert.Empty(plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption excludes whole-file and foreign-managed sources while retaining a Framework region host")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void ManagerOwnershipExcludesAdoptionCandidates()
    {
        const string frameworkWhole = ".agents/directives/framework.md";
        const string frameworkRegion = ".agents/directives/framework-region.md";
        const string extensionWhole = ".agents/directives/extension.md";
        const string extensionRegion = ".agents/directives/extension-region.md";
        const string libraryPath = ".agents/directives/library.md";
        const string libraryRoot = ".agents/directives/libraries";
        const string libraryChild = ".agents/directives/libraries/catalogue/guide.md";
        const string payloadOwned = ".agents/directives/payload-owned.md";

        var ownership = CompleteOwnership(
            framework: new FrameworkOwnership(
                new OwnedSource("framework", "1"),
                [frameworkWhole],
                [new OwnedRegion(frameworkRegion, "entries")]),
            extensions:
            [
                new ExtensionOwnership(
                    "extension",
                    "1",
                    "packages/extension",
                    [],
                    [extensionWhole],
                    [new OwnedRegion(extensionRegion, "entries")]),
            ],
            libraries:
            [
                new LibraryOwnership(
                    "library",
                    ".agents/libraries/source",
                    libraryRoot,
                    [libraryPath]),
            ]);
        var sources = new[]
        {
            Source(frameworkWhole),
            Source(frameworkRegion),
            Source(extensionWhole),
            Source(extensionRegion),
            Source(libraryPath),
            Source(libraryChild),
            Source(payloadOwned),
        };

        var plan = Plan(
            sources,
            [Source(".agents/directives/_directives.md"), Source(payloadOwned)],
            ownership);

        Assert.Null(plan.Cause);
        Assert.Equal([frameworkRegion], plan.EligibleSourcePaths);
        Assert.Empty(plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A whole-file Framework entrypoint remains payload-authoritative instead of being reused")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void WholeOwnedFrameworkEntrypointIsNotReused()
    {
        const string rootPath = ".agents/directives/_directives.md";
        var ownership = CompleteOwnership(
            framework: new FrameworkOwnership(
                new OwnedSource("framework", "1"),
                [rootPath],
                []));

        var plan = Plan([Source(rootPath)], [Source(rootPath)], ownership);

        Assert.Null(plan.Cause);
        Assert.Empty(plan.ReusedEntrypoints);
        Assert.Empty(plan.EligibleSourcePaths);
        Assert.Empty(plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A Framework Entries-region receipt does not claim its authored host")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void FrameworkEntriesRegionHostCanBeReused()
    {
        const string payloadPath = ".agents/guidance/_guidance.md";
        const string existingHost = ".agents/guidance/index.md";
        var ownership = CompleteOwnership(
            framework: new FrameworkOwnership(
                new OwnedSource("framework", "1"),
                [],
                [new OwnedRegion(existingHost, "entries")]));

        var plan = Plan([Source(existingHost)], [Source(payloadPath)], ownership);

        Assert.Null(plan.Cause);
        Assert.Equal(existingHost, plan.ReusedEntrypoints[payloadPath]);
        Assert.Empty(plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A foreign-owned region host blocks payload reuse")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void ExtensionRegionHostIsNotReused()
    {
        const string payloadPath = ".agents/guidance/_guidance.md";
        const string existingHost = ".agents/guidance/index.md";
        var ownership = CompleteOwnership(
            extensions:
            [
                new ExtensionOwnership(
                    "foreign-extension",
                    "1",
                    "packages/foreign",
                    [],
                    [],
                    [new OwnedRegion(existingHost, "entries")]),
            ]);

        var plan = Plan([Source(existingHost)], [Source(payloadPath)], ownership);

        AssertBlocked(plan, "Extension-owned host");
        Assert.Contains(existingHost, Assert.IsType<string>(plan.Cause), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Invalid or unavailable ownership blocks adoption when user sources need routing")]
    [InlineData((int)WorkspaceOwnershipReadState.Invalid)]
    [InlineData((int)WorkspaceOwnershipReadState.Unavailable)]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void UnknownOwnershipBlocksNeededAdoption(int stateValue)
    {
        var state = (WorkspaceOwnershipReadState)stateValue;
        var ownership = new WorkspaceOwnershipRead(
            state,
            WorkspaceOwnershipDocument.Empty,
            LockPath,
            Snapshot: null,
            Cause: "ownership could not be established");
        var plan = WorkspaceAdoptionTopologyPlanner.Plan(
            [Source(".agents/directives/guide.md")],
            [Source(".agents/directives/_directives.md")],
            [],
            ownership);

        AssertBlocked(plan, "Workspace ownership is");
        Assert.Contains(
            "cannot safely adopt",
            Assert.IsType<string>(plan.Cause),
            StringComparison.Ordinal);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Absent ownership is known empty and deterministic output is ordinal")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void AbsentOwnershipAllowsDeterministicOrdinalPlanning()
    {
        var sources = new[]
        {
            Source(".agents/directives/z-last.md"),
            Source(".agents/directives/nested/beta.md"),
            Source(".agents/directives/a-first.md"),
        };
        var payload = new[] { Source(".agents/directives/_directives.md") };

        var first = Plan(sources, payload);
        var second = Plan(sources.Reverse().ToArray(), payload.Reverse().ToArray());

        Assert.Null(first.Cause);
        Assert.Equal(first.EligibleSourcePaths, second.EligibleSourcePaths);
        Assert.Equal(first.CreatedEntrypointPaths, second.CreatedEntrypointPaths);
        Assert.Equal(
            [
                ".agents/directives/a-first.md",
                ".agents/directives/nested/beta.md",
                ".agents/directives/z-last.md",
            ],
            first.EligibleSourcePaths);
        Assert.Equal(
            [".agents/directives/nested/_nested.md"],
            first.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Ownership path comparisons use portable canonical identities")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void PortableOwnershipClaimExcludesSourceRegardlessOfCase()
    {
        const string existingPath = ".agents/directives/local.md";
        var ownership = CompleteOwnership(
            framework: new FrameworkOwnership(
                new OwnedSource("framework", "1"),
                [".AGENTS/DIRECTIVES/LOCAL.MD"],
                []));

        var plan = Plan(
            [Source(existingPath)],
            [Source(".agents/directives/_directives.md")],
            ownership);

        Assert.Null(plan.Cause);
        Assert.DoesNotContain(existingPath, plan.EligibleSourcePaths);
        Assert.Empty(plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Nested Skills create grouping and resource routes without routing inside a Skill root")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void NestedSkillsKeepNativeBoundariesAndGroupRoutes()
    {
        var sources = new[]
        {
            Source(".agents/skills/tools/git/SKILL.md"),
            Source(".agents/skills/tools/git/references/usage.md"),
            Source(".agents/skills/tools/git/references/nested/howto.md"),
            Source(".agents/skills/ops/SKILL.md"),
            Source(".agents/skills/ops/references/readme.md"),
        };

        var plan = Plan(sources, [Source(".agents/skills/_skills.md")]);

        Assert.Null(plan.Cause);
        Assert.Equal(
            [
                ".agents/skills/ops/references/_references.md",
                ".agents/skills/tools/_tools.md",
                ".agents/skills/tools/git/references/_references.md",
                ".agents/skills/tools/git/references/nested/_nested.md",
            ],
            plan.CreatedEntrypointPaths);
        Assert.DoesNotContain(".agents/skills/tools/git/_git.md", plan.CreatedEntrypointPaths);
        Assert.DoesNotContain(".agents/skills/ops/_ops.md", plan.CreatedEntrypointPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace adoption returns only logical bases when a source has an overwrite companion")]
    [Trait("Feature", "workspace-adoption"), Trait("Evidence", "Unit")]
    public void OverwriteCompanionsAreNotSeparateEligibleSources()
    {
        var baseSource = Source(".agents/directives/local.md", withOverwrite: true);

        var plan = Plan([baseSource], [Source(".agents/directives/_directives.md")]);

        Assert.Null(plan.Cause);
        Assert.Equal([".agents/directives/local.md"], plan.EligibleSourcePaths);
        Assert.DoesNotContain(".agents/directives/local.overwrite.md", plan.EligibleSourcePaths);
    }

    private static readonly string LockPath = Path.GetFullPath(Path.Combine(
        Path.GetTempPath(),
        "workspace-adoption-topology-tests",
        ".agents",
        "open-forge.lock.json"));

    private static WorkspaceAdoptionTopologyPlan Plan(
        IReadOnlyList<SourceLogicalSource> existingSources,
        IReadOnlyList<SourceLogicalSource> payloadSources,
        WorkspaceOwnershipRead? ownership = null)
        => WorkspaceAdoptionTopologyPlanner.Plan(
            existingSources,
            payloadSources,
            [],
            ownership ?? WorkspaceOwnershipRead.Absent(LockPath));

    private static WorkspaceOwnershipRead CompleteOwnership(
        FrameworkOwnership? framework = null,
        IEnumerable<ExtensionOwnership>? extensions = null,
        IEnumerable<LibraryOwnership>? libraries = null)
    {
        var document = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDocument.Empty.SchemaVersion,
            framework,
            extensions?.ToImmutableArray() ?? ImmutableArray<ExtensionOwnership>.Empty,
            libraries?.ToImmutableArray() ?? ImmutableArray<LibraryOwnership>.Empty);
        return new WorkspaceOwnershipRead(
            WorkspaceOwnershipReadState.Complete,
            document,
            LockPath,
            Snapshot: null,
            Cause: null);
    }

    private static SourceLogicalSource Source(string canonicalPath, bool withOverwrite = false)
    {
        if (!SourceFormClassifier.TryClassify(canonicalPath, out var form))
        {
            throw new ArgumentException("The test source must be a recognized Markdown source.", nameof(canonicalPath));
        }

        var physicalPath = PhysicalPath(canonicalPath);
        var baseLayer = new SourceLayer(
            canonicalPath,
            physicalPath,
            form,
            SourceLayerKind.Base);
        var overwrite = withOverwrite
            ? new SourceLayer(
                SourceOverwritePath.ReadAdjacentPath(canonicalPath),
                SourceOverwritePath.ReadAdjacentPath(physicalPath),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite)
            : null;
        return new SourceLogicalSource(
            new SourceLogicalIdentity($"test:{canonicalPath}", canonicalPath),
            baseLayer,
            overwrite);
    }

    private static string PhysicalPath(string canonicalPath)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "workspace-adoption-topology-tests",
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));

    private static void AssertBlocked(WorkspaceAdoptionTopologyPlan plan, string causeFragment)
    {
        var cause = Assert.IsType<string>(plan.Cause);
        Assert.Contains(causeFragment, cause, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(plan.ReusedEntrypoints);
        Assert.Empty(plan.EligibleSourcePaths);
        Assert.Empty(plan.CreatedEntrypointPaths);
    }
}
