using System.Text;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.RoutedMaintenance.Metadata;

[Trait("Feature", "metadata-completion"), Trait("Evidence", "Unit")]
public sealed class RoutedMetadataCompletionTests
{
    [Fact(DisplayName = "Boolean reference membership allocates only its stateless protection check")]
    public void MembershipDoesNotConstructMaintenancePolicy()
    {
        const int WarmupCheckCount = 100;
        const int ShortCheckCount = 1000;
        const int LongCheckCount = ShortCheckCount * 4;
        const int AdditionalCheckCount = LongCheckCount - ShortCheckCount;
        var policy = new RoutedMetadataCompletionPolicy();
        var path = Facts().CanonicalPath;
        var libraryContext = Context() with
        {
            Ownership = WorkspaceOwnershipDocument.Empty with
            {
                Libraries = [new("library", Path.GetFullPath("source"), DefaultWorkspaceLayout.CategoryDirectory(WorkspaceCategoryNames.Guidance), [path])],
            },
        };
        foreach (var (context, expected) in new[]
        {
            (Context(), RoutedMetadataEligibility.Eligible),
            (libraryContext, RoutedMetadataEligibility.LibraryProjection),
        })
        {
            Assert.Equal(expected, policy.Protection(path, context));
            Func<bool> direct = () => policy.Protection(path, context) is RoutedMetadataEligibility.Eligible or RoutedMetadataEligibility.LibraryProjection;
            Func<bool> membership = () => ReferenceProtectionReader.Allows(path, context);
            for (var index = 0; index < WarmupCheckCount; index++)
            {
                Assert.True(direct());
                Assert.True(membership());
            }

            var directShort = Allocated(direct, ShortCheckCount);
            var membershipShort = Allocated(membership, ShortCheckCount);
            var directGrowth = Allocated(direct, LongCheckCount) - directShort;
            var membershipGrowth = Allocated(membership, LongCheckCount) - membershipShort;

            // Batch subtraction removes fixed overhead and whole-byte amortization ignores fractional measurement variation.
            // Rebuilding a policy or catalogue per check adds heap objects, exceeding this rounding precision.
            var directBytesPerCheck = (directGrowth + AdditionalCheckCount - 1) / AdditionalCheckCount;
            var membershipBytesPerCheck = membershipGrowth / AdditionalCheckCount;
            Assert.True(membershipBytesPerCheck <= directBytesPerCheck,
                $"Membership allocation growth was {membershipGrowth} bytes for {AdditionalCheckCount} additional checks, compared with {directGrowth} bytes for direct protection.");
        }

        static long Allocated(Func<bool> check, int count)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            var allowed = 0;
            for (var index = 0; index < count; index++) if (check()) allowed++;
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.Equal(count, allowed);
            return allocated;
        }
    }

    [Fact(DisplayName = "Reference membership protection does not rebuild the ownership catalogue for each check")]
    public void ProtectionAllocationsDoNotGrowWithOwnershipCatalogue()
    {
        var policy = new RoutedMetadataCompletionPolicy();
        var path = Facts().CanonicalPath;
        var small = WithPaths(8);
        var large = WithPaths(1000);
        Assert.Equal(RoutedMetadataEligibility.ManagedContent, policy.Protection(path, large));

        var smallAllocation = Allocated(small);
        var largeAllocation = Allocated(large);
        Assert.True(largeAllocation <= smallAllocation + 8192,
            $"Membership allocated {smallAllocation} bytes for 9 paths and {largeAllocation} bytes for 1001 paths.");

        RoutedMetadataPolicyContext WithPaths(int count)
        {
            var ownership = Owned(framework: true);
            var framework = ownership.Framework ?? throw new InvalidOperationException();
            return Context() with
            {
                Ownership = ownership with
                {
                    Framework = framework with
                    {
                        Paths = Enumerable.Range(0, count).Select(index => $".agents/guidance/owned-{index}.md")
                            .Append(path).ToImmutableArray(),
                    },
                },
            };
        }

        long Allocated(RoutedMetadataPolicyContext context)
        {
            for (var index = 0; index < 8; index++) _ = ReferenceProtectionReader.Allows(path, context);
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 100; index++) _ = ReferenceProtectionReader.Allows(path, context);
            return GC.GetAllocatedBytesForCurrentThread() - before;
        }
    }

    [Theory(DisplayName = "Ordinary completion uses every exact recognized category and Memory state default")]
    [InlineData("directives", "", "LoadNow,Directive")]
    [InlineData("guidance", "", "Guidance")]
    [InlineData("maps", "", "Map")]
    [InlineData("patterns", "", "Pattern")]
    [InlineData("templates", "", "Template")]
    [InlineData("skills", "", "Skill")]
    [InlineData("extensions", "", "Extension")]
    [InlineData("memory", "", "Memory")]
    [InlineData("memory", "working", "Memory,Working,Contextual")]
    [InlineData("memory", "emerging", "Memory,Emerging,Contextual")]
    [InlineData("memory", "crystallized", "Memory,Crystallized")]
    [InlineData("memory", "archived", "Memory,Archived,Contextual,Historical")]
    [InlineData("custom", "guidance", "Workspace")]
    public void DerivesEveryCategory(string root, string state, string expected)
    {
        var facts = Facts(root, state);
        var result = new RoutedMetadataCompletionPolicy().Complete(Input(facts), Context());
        Assert.Equal(RoutedMetadataEligibility.Eligible, result.Eligibility);
        Assert.Equal(expected.Split(','), result.Completion.Tags);
    }

    [Theory(DisplayName = "Ordinary user-owned compatibility and canonical entrypoints are eligible")]
    [InlineData("_guidance.md")]
    [InlineData("index.md")]
    [InlineData("_index.md")]
    [InlineData("references.md")]
    [InlineData("_references.md")]
    public void IncludesEntrypoints(string name)
    {
        var result = new RoutedMetadataCompletionPolicy().Complete(Input(Facts() with { CanonicalPath = $".agents/guidance/{name}" }), Context());
        Assert.Equal(RoutedMetadataEligibility.Eligible, result.Eligibility);
        Assert.Equal(FrameworkMetadataCompletionState.Completed, result.Completion.State);
    }

    [Theory(DisplayName = "Completion keeps every excluded ownership selection and safety boundary")]
    [InlineData("selection", "NotSelected")]
    [InlineData("detached", "OutsideRoot")]
    [InlineData("loader", "UnsupportedForm")]
    [InlineData("host", "UnsupportedForm")]
    [InlineData("settings", "UnsupportedForm")]
    [InlineData("lock", "UnsupportedForm")]
    [InlineData("resource", "UnsupportedForm")]
    [InlineData("overwrite", "UnsupportedForm")]
    [InlineData("unreadable", "Unreadable")]
    [InlineData("link", "UnsafeLeaf")]
    [InlineData("framework", "ManagedContent")]
    [InlineData("extension", "ManagedContent")]
    [InlineData("generated", "GeneratedContent")]
    [InlineData("region", "GeneratedContent")]
    [InlineData("removed-file", "SettingsExcluded")]
    [InlineData("removed-directory", "SettingsExcluded")]
    [InlineData("removed-category", "SettingsExcluded")]
    [InlineData("private", "PrivateContent")]
    [InlineData("library", "LibraryProjection")]
    public void ExcludesEveryBoundary(string scenario, string expected)
    {
        var facts = Facts();
        var context = Context();
        switch (scenario)
        {
            case "selection": facts = facts with { IsSelected = false }; break;
            case "detached": facts = facts with { LoaderRootEntrypointPath = null }; break;
            case "loader": facts = facts with { CanonicalPath = ".agents/loader.md" }; break;
            case "host": facts = facts with { CanonicalPath = "AGENTS.md" }; break;
            case "settings": facts = facts with { CanonicalPath = ".agents/open-forge.json" }; break;
            case "lock": facts = facts with { CanonicalPath = ".agents/open-forge.lock.json" }; break;
            case "resource": facts = facts with { CanonicalPath = ".agents/guidance/image.png" }; break;
            case "overwrite": facts = facts with { CanonicalPath = ".agents/guidance/note.overwrite.md" }; break;
            case "unreadable": facts = facts with { IsReadable = false }; break;
            case "link": facts = facts with { IsSafeOrdinaryLeaf = false }; break;
            case "framework": context = context with { Ownership = Owned(framework: true) }; break;
            case "extension": context = context with { Ownership = Owned(framework: false) }; break;
            case "generated": facts = facts with { IsGeneratedFile = true }; break;
            case "region": facts = facts with { HasManagedAuthoredRegions = true }; break;
            case "removed-file": context = context with { Settings = WorkspaceSettingsDocument.Empty with { RemovedFiles = [facts.CanonicalPath] } }; break;
            case "removed-directory": context = context with { Settings = WorkspaceSettingsDocument.Empty with { RemovedDirectories = [".agents/guidance"] } }; break;
            case "removed-category": context = context with { Settings = WorkspaceSettingsDocument.Empty with { RemovedCategories = ["guidance"] } }; break;
            case "private": context = SharedContext(); break;
            case "library":
                context = context with { Ownership = WorkspaceOwnershipDocument.Empty with { Libraries = [new("library", Path.GetFullPath("source"), ".agents/guidance", [facts.CanonicalPath])] } }; break;
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var input = Input(facts);
        var result = new RoutedMetadataCompletionPolicy().Complete(input, context);
        Assert.Equal(Enum.Parse<RoutedMetadataEligibility>(expected), result.Eligibility);
        Assert.Equal(FrameworkMetadataCompletionState.NoOp, result.Completion.State);
        Assert.Equal(input.Bytes, result.Completion.Bytes);
    }

    [Fact(DisplayName = "A user-owned SKILL.md without frontmatter gets its native name and description")]
    public void CompletesNativeSkillMetadata()
    {
        var facts = Facts("skills") with { CanonicalPath = ".agents/skills/release-notes/SKILL.md" };
        var input = Input(facts) with { Bytes = [.. Encoding.UTF8.GetBytes("# Release notes\n\nWrite them.\n")] };

        var result = new RoutedMetadataCompletionPolicy().Complete(input, Context());

        Assert.Equal(RoutedMetadataEligibility.Eligible, result.Eligibility);
        Assert.Equal(FrameworkMetadataCompletionState.Completed, result.Completion.State);
        Assert.Equal([FrameworkMetadataField.Name, FrameworkMetadataField.Description], result.Completion.Fields);
        Assert.Equal("Release notes", result.Completion.Description);
        Assert.Null(result.Completion.Tags);
        Assert.Equal(
            "---\nname: \"release-notes\"\ndescription: \"Release notes\"\n---\n\n# Release notes\n\nWrite them.\n",
            Encoding.UTF8.GetString(result.Completion.Bytes.AsSpan()));
    }

    [Fact(DisplayName = "A SKILL.md keeps its native name and gains only the missing description")]
    public void KeepsSuppliedNativeSkillName()
    {
        var facts = Facts("skills") with { CanonicalPath = ".agents/skills/release-notes/SKILL.md" };
        var input = Input(facts) with { Bytes = [.. Encoding.UTF8.GetBytes("---\nname: notes\n---\n\n# Release notes\n")] };

        var result = new RoutedMetadataCompletionPolicy().Complete(input, Context());

        Assert.Equal(FrameworkMetadataCompletionState.Completed, result.Completion.State);
        Assert.Equal([FrameworkMetadataField.Description], result.Completion.Fields);
        Assert.Equal(
            "---\nname: notes\ndescription: \"Release notes\"\n---\n\n# Release notes\n",
            Encoding.UTF8.GetString(result.Completion.Bytes.AsSpan()));
    }

    [Fact(DisplayName = "A complete SKILL.md stays byte-identical")]
    public void LeavesCompleteNativeSkillUnchanged()
    {
        var facts = Facts("skills") with { CanonicalPath = ".agents/skills/release-notes/SKILL.md" };
        var input = Input(facts) with { Bytes = [.. Encoding.UTF8.GetBytes("---\nname: release-notes\ndescription: Write notes\n---\n\n# Notes\n")] };

        var result = new RoutedMetadataCompletionPolicy().Complete(input, Context());

        Assert.Equal(FrameworkMetadataCompletionState.NoOp, result.Completion.State);
        Assert.Equal(input.Bytes, result.Completion.Bytes);
    }

    [Fact(DisplayName = "An unclosed SKILL.md frontmatter stays a malformed Index fact")]
    public void ReportsMalformedNativeSkill()
    {
        var facts = Facts("skills") with { CanonicalPath = ".agents/skills/release-notes/SKILL.md" };
        var input = Input(facts) with { Bytes = [.. Encoding.UTF8.GetBytes("---\nname: release-notes\n# Notes\n")] };

        var result = new RoutedMetadataCompletionPolicy().Complete(input, Context());

        Assert.Equal(FrameworkMetadataCompletionState.Malformed, result.Completion.State);
        Assert.Equal("index.metadata-skipped", result.FactCode);
        Assert.Equal(input.Bytes, result.Completion.Bytes);
    }

    [Fact(DisplayName = "Markdown inside a user-owned Skill package gets ordinary metadata with Skill tags")]
    public void CompletesSkillPackageMarkdown()
    {
        var facts = Facts("skills") with { CanonicalPath = ".agents/skills/release-notes/references/style.md" };

        var result = new RoutedMetadataCompletionPolicy().Complete(Input(facts), Context());

        Assert.Equal(RoutedMetadataEligibility.Eligible, result.Eligibility);
        Assert.Equal(FrameworkMetadataCompletionState.Completed, result.Completion.State);
        Assert.Equal(["Skill"], result.Completion.Tags);
    }

    [Fact(DisplayName = "A managed SKILL.md stays with its managing Update")]
    public void KeepsManagedSkillProtected()
    {
        var facts = Facts("skills") with { CanonicalPath = ".agents/skills/open-forge-cli/SKILL.md" };
        var context = Context() with
        {
            Ownership = WorkspaceOwnershipDocument.Empty with { Framework = new(new("framework", null), [facts.CanonicalPath], []) },
        };

        var result = new RoutedMetadataCompletionPolicy().Complete(Input(facts), context);

        Assert.Equal(RoutedMetadataEligibility.ManagedContent, result.Eligibility);
        Assert.Equal(FrameworkMetadataCompletionState.NoOp, result.Completion.State);
    }

    [Fact(DisplayName = "The sharing evaluator keeps the shared user-owned entrypoint eligible")]
    public void RetainsSharedEntrypoint()
    {
        var facts = Facts() with { CanonicalPath = ".agents/guidance/_guidance.md" };
        Assert.Equal(RoutedMetadataEligibility.Eligible, new RoutedMetadataCompletionPolicy().Evaluate(facts, SharedContext()));
    }

    [Fact(DisplayName = "Directive entrypoint completion keeps the route on demand")]
    public void DirectiveEntrypointIsOnDemand()
        => Assert.Equal(["Directive"], RoutedMetadataDefaults.Tags(new OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceContentRoots(DefaultWorkspaceLayout), Facts("directives") with { CanonicalPath = ".agents/directives/team/_team.md" }));

    [Fact(DisplayName = "Metadata defaults reject an undefined category")]
    public void RejectsUndefinedCategory()
        => Assert.Throws<ArgumentOutOfRangeException>(() => RoutedMetadataDefaults.Tags((RoutedMetadataCategory)int.MaxValue));

    [Fact(DisplayName = "Unclaimed ordinary notes beside Library projections remain eligible")]
    public void IncludesLibraryNeighbor()
    {
        var context = Context() with
        {
            Ownership = WorkspaceOwnershipDocument.Empty with
            {
                Libraries = [new("library", Path.GetFullPath("source"), ".agents/guidance", [".agents/guidance/projected.md"])],
            },
        };
        Assert.Equal(RoutedMetadataEligibility.Eligible, new RoutedMetadataCompletionPolicy().Evaluate(Facts(), context));
    }

    [Fact(DisplayName = "A familiar state directory without a recognized state entrypoint does not add state tags")]
    public void DoesNotInferUnestablishedState()
    {
        var facts = Facts("memory", "working") with { RouteEntrypointPaths = [".agents/memory/_memory.md"] };
        var result = new RoutedMetadataCompletionPolicy().Complete(Input(facts), Context());
        Assert.Equal(["Memory"], result.Completion.Tags);
    }

    [Theory(DisplayName = "Completion projects malformed and unsafe outcomes to existing Index fact codes")]
    [InlineData("---\ntags: []\n---\n", "index.metadata-skipped")]
    [InlineData("---\n? description\n: Keep\n---\n", "index.metadata-unsafe")]
    public void ProjectsFactCode(string source, string code)
    {
        var result = new RoutedMetadataCompletionPolicy().Complete(Input(Facts()) with { Bytes = [.. Encoding.UTF8.GetBytes(source)] }, Context());
        Assert.Equal(code, result.FactCode);
    }

    private static RoutedMetadataCompletionInput Input(RoutedMetadataFileFacts facts)
        => new() { File = facts, Bytes = [.. Encoding.UTF8.GetBytes("# Note\nKeep body\n")] };

    private static RoutedMetadataPolicyContext Context()
        => new() { Layout = DefaultWorkspaceLayout, Settings = WorkspaceSettingsDocument.Empty, Ownership = WorkspaceOwnershipDocument.Empty };

    private static RoutedMetadataFileFacts Facts(string root = "guidance", string state = "")
    {
        var rootPath = $".agents/{root}/_{root}.md";
        var directory = $".agents/{root}/team";
        if (state.Length > 0) directory += $"/{state}/deep";
        return new()
        {
            CanonicalPath = $"{directory}/note.md",
            LoaderRootEntrypointPath = rootPath,
            RouteEntrypointPaths = state.Length == 0 ? [rootPath] : [rootPath, $".agents/{root}/team/{state}/_{state}.md"],
            IsSelected = true,
            IsReadable = true,
            IsSafeOrdinaryLeaf = true,
            IsGeneratedFile = false,
            HasManagedAuthoredRegions = false,
        };
    }

    private static WorkspaceOwnershipDocument Owned(bool framework)
    {
        var path = Facts().CanonicalPath;
        if (framework) return WorkspaceOwnershipDocument.Empty with { Framework = new(new("framework", null), [path], []) };
        return WorkspaceOwnershipDocument.Empty with { Extensions = [new("extension", null, null, [], [path], [])] };
    }

    private static RoutedMetadataPolicyContext SharedContext()
        => Context() with
        {
            Ownership = WorkspaceOwnershipDocument.Empty with
            {
                Framework = new(new("framework", null), [], []) { GitIgnoredRoutes = [new(".agents/guidance", ".agents/guidance/_guidance.md")] },
            },
        };

    private static readonly WorkspaceLayout DefaultWorkspaceLayout = WorkspaceLayout.Create(
            new CliWorkspace(
                Path.GetFullPath(nameof(RoutedMetadataCompletionTests)),
                Path.GetFullPath(nameof(RoutedMetadataCompletionTests)),
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            WorkspaceLayout.DefaultMainRoot);
}
