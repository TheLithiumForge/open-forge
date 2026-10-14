using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.RoutedMaintenance.Metadata;

[Trait("Feature", "metadata-completion"), Trait("Evidence", "Unit")]
public sealed class RoutedMetadataCompletionTests
{
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
    [InlineData("native", "NativePackage")]
    [InlineData("native-resource", "NativePackage")]
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
            case "native": facts = facts with { CanonicalPath = ".agents/skills/native/SKILL.md" }; break;
            case "native-resource":
                facts = Facts("skills") with { CanonicalPath = ".agents/skills/native/docs/note.md" };
                context = context with { NativeSkillPaths = [".agents/skills/native/SKILL.md"] }; break;
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

    [Fact(DisplayName = "The sharing evaluator keeps the shared user-owned entrypoint eligible")]
    public void RetainsSharedEntrypoint()
    {
        var facts = Facts() with { CanonicalPath = ".agents/guidance/_guidance.md" };
        Assert.Equal(RoutedMetadataEligibility.Eligible, new RoutedMetadataCompletionPolicy().Evaluate(facts, SharedContext()));
    }

    [Fact(DisplayName = "Directive entrypoint completion keeps the route on demand")]
    public void DirectiveEntrypointIsOnDemand()
        => Assert.Equal(["Directive"], RoutedMetadataDefaults.Tags(Facts("directives") with { CanonicalPath = ".agents/directives/team/_team.md" }));

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
        => new() { Settings = WorkspaceSettingsDocument.Empty, Ownership = WorkspaceOwnershipDocument.Empty, NativeSkillPaths = [] };

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
}
