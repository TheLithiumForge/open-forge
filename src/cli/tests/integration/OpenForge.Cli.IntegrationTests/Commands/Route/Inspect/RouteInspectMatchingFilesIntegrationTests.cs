using OpenForge.Cli.Core.Commands.Route.Inspect;
using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect;

[Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInspectMatchingFilesIntegrationTests
{
    [Theory(DisplayName = "No restrictions and all match-all conditions produce all files without enumerating")]
    [InlineData(false), InlineData(true)]
    public async Task AllFilesPreservesCompleteInspection(bool declaredMatchAll)
    {
        using var workspace = CreateWorkspace(declaredMatchAll ? ["**"] : null, declaredMatchAll ? ["src/**", "**/*"] : null);
        var calls = 0;
        var operation = RouteInspectOperationFactory.Create(null, (_, _) =>
        {
            calls++;
            throw new InvalidOperationException("An unrestricted chain must not scan.");
        });
        var result = await operation(new(workspace.Workspace, "root/item", false, ["planned.cs"], matchingFiles: true), TestContext.Current.CancellationToken);
        Assert.Equal(0, calls);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Conditions);
        Assert.Equal(RouteInspectMatchingFilesScope.AllFiles, result.MatchingFiles?.Scope);
        Assert.Null(result.MatchingFiles?.Count);
        Assert.True(result.MatchingFiles?.Complete);
    }

    [Theory(DisplayName = "Removing a match-all local condition preserves the restrictive ancestor's scan result")]
    [InlineData("**"), InlineData("**/*"), InlineData("**/**"), InlineData("*/**"), InlineData("{**,src/**}")]
    public async Task MatchAllBesideRestrictiveAncestorIsRedundant(string matchAll)
    {
        using var narrow = CreateWorkspace(["src/**"], null);
        using var combined = CreateWorkspace(["src/**"], [matchAll]);
        string[] paths = ["src/a.cs", "src/nested/b.md", "tests/c.cs"];
        foreach (var path in paths)
        {
            narrow.Write(path, "");
            combined.Write(path, "");
        }

        var calls = 0;
        var operation = RouteInspectOperationFactory.Create(null, (_, _) =>
        {
            calls++;
            return ValueTask.FromResult(RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.WorkspaceFiles, paths));
        });
        var expected = await operation(new(narrow.Workspace, "root/item", false, ["src/planned.cs"], matchingFiles: true), TestContext.Current.CancellationToken);
        var actual = await operation(new(combined.Workspace, "root/item", false, ["src/planned.cs"], matchingFiles: true), TestContext.Current.CancellationToken);
        Assert.Equal(2, calls);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.MatchingFiles?.Scope, actual.MatchingFiles?.Scope);
        Assert.Equal(expected.MatchingFiles?.Count, actual.MatchingFiles?.Count);
        Assert.Equal(expected.MatchingFiles?.Paths, actual.MatchingFiles?.Paths);
        Assert.Equal(["src/a.cs", "src/nested/b.md"], actual.MatchingFiles?.Paths);
        var applicability = Assert.IsType<RouteInspectApplicability>(actual.Applicability);
        Assert.Contains(applicability.Conditions, condition => condition.Patterns.Contains(matchAll));
    }

    [Theory(DisplayName = "Help and version with matching-files never invoke the operation or inventory")]
    [InlineData("--help"), InlineData("--version")]
    public async Task TerminalModesDoNotScan(string terminalMode)
    {
        using var workspace = CreateWorkspace(null, null);
        var route = new Command("route");
        var symbols = RouteInspectBinding.CreateSymbols(route);
        var calls = 0;
        var binding = CliReportBinding.Close(RouteInspectBinding.CreateRequestBinding(symbols, new RouteInspectBindingComponents
        {
            Help = CliHelpContent.Empty,
            Operation = (_, _) =>
            {
                calls++;
                throw new InvalidOperationException("Terminal requests cannot execute the operation.");
            },
        }), RouteInspectPresentation.Rendering);
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [new CliRootBranch(route, CliHelpContent.Empty)], [binding]);
        var application = new CliCoreApplication(new CliProcessIdentity("open-forge", "test"), tree,
            new CliWorkspaceSelector(new PhysicalPathResolver()));
        using var output = new StringWriter();
        using var error = new StringWriter();
        await application.RunAsync(["route", "inspect", "--matching-files", terminalMode],
            new CliProcessEnvironment(workspace.Path), new CliOutputWriters(output, error), TestContext.Current.CancellationToken);
        Assert.Equal(0, calls);
    }

    [Theory(DisplayName = "Matching inventory applies inherited AND and local alternatives to the same file")]
    [InlineData(false), InlineData(true)]
    public async Task MatchesOwnAndInheritedConditions(bool sameFile)
    {
        using var workspace = CreateWorkspace(["src/**"], ["**/*.cs", "**/*.ts"]);
        string[] candidates = sameFile
            ? ["src/A.cs", "src/B.ts", "src/readme.md", "tests/C.cs", "src/A.cs", "deleted.cs", "directory"]
            : ["src/readme.md", "tests/C.cs"];
        foreach (var path in candidates.Distinct().Where(path => path is not ("deleted.cs" or "directory")))
        {
            workspace.Write(path, "not read");
        }

        workspace.Write("directory/child.md", "");
        var calls = 0;
        var operation = RouteInspectOperationFactory.Create(null, (_, _) =>
        {
            calls++;
            return ValueTask.FromResult(RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.GitTrackedAndUntracked, candidates));
        });
        var result = await operation(new(workspace.Workspace, "root/item", false, matchingFiles: true), TestContext.Current.CancellationToken);

        Assert.Equal(1, calls);
        var matches = Assert.IsType<RouteInspectMatchingFiles>(result.MatchingFiles);
        Assert.True(matches.Complete);
        Assert.Equal(sameFile ? ["src/A.cs", "src/B.ts"] : Array.Empty<string>(), matches.Paths);
        Assert.Equal(sameFile ? 2 : 0, matches.Count);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Null(result.WorkingPaths);
        Assert.Equal(RouteInspectApplicabilityState.Pending, result.Applicability?.State);
    }

    [Theory(DisplayName = "Restrictive and empty inventories complete with an exact count")]
    [InlineData(0), InlineData(100), InlineData(143)]
    public async Task RestrictiveInventoryCountsAllMatches(int count)
    {
        using var workspace = CreateWorkspace(null, ["files/**"]);
        var paths = Enumerable.Range(0, count).Select(index => $"files/{index:D3}.bin").Reverse().ToArray();
        foreach (var path in paths)
        {
            workspace.Write(path, "");
        }

        var result = await Inspect(workspace, paths, ["files/planned.bin"]);
        var matches = Assert.IsType<RouteInspectMatchingFiles>(result.MatchingFiles);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(matches.Complete);
        Assert.Equal(count, matches.Count);
        Assert.Equal(paths.Order(StringComparer.Ordinal).Take(100), matches.Paths);
        Assert.Equal(count > 100, matches.Truncated);
    }

    [Fact(DisplayName = "Matching files and explicit working paths leave loading and applicability unchanged")]
    public async Task ScanDoesNotSupplyWorkingPaths()
    {
        using var workspace = CreateWorkspace(["src/**"], ["**/*.cs"]);
        workspace.Write("src/Current.cs", "");
        var baseline = await workspace.InspectAsync("root/item", TestContext.Current.CancellationToken, ["docs/planned.md"]);
        var scanned = await Inspect(workspace, ["src/Current.cs"], ["docs/planned.md"]);
        Assert.Equal(baseline.Status, scanned.Status);
        Assert.Equal(baseline.Applicability?.State, scanned.Applicability?.State);
        Assert.Equal(baseline.WorkingPaths, scanned.WorkingPaths);
        Assert.Equal(baseline.Profile?.Reading.TaskStart.State, scanned.Profile?.Reading.TaskStart.State);
        Assert.Equal(baseline.Profile?.Reading.TaskStart.Value, scanned.Profile?.Reading.TaskStart.Value);
        Assert.Equal(baseline.Profile?.Completeness, scanned.Profile?.Completeness);
        Assert.Equal(["src/Current.cs"], scanned.MatchingFiles?.Paths);
    }

    [Theory(DisplayName = "Absent matching-files and invalid sources never invoke enumeration")]
    [InlineData(false, "root/item"), InlineData(true, "missing")]
    public async Task DoesNotEnumerate(bool requested, string source)
    {
        using var workspace = CreateWorkspace(null, null);
        var operation = RouteInspectOperationFactory.Create(null, (_, _) => throw new InvalidOperationException("Must not enumerate."));
        var result = await operation(new(workspace.Workspace, source, false, matchingFiles: requested), TestContext.Current.CancellationToken);
        Assert.Equal(requested ? CliSemanticStatus.Invalid : CliSemanticStatus.Complete, result.Status);
        Assert.Equal(requested, result.MatchingFiles is not null);
    }

    [Theory(DisplayName = "Typed scan failures retain ordinary facts and map to their required status")]
    [InlineData(2, CliSemanticStatus.Incomplete)]
    [InlineData(3, CliSemanticStatus.Incomplete)]
    [InlineData(4, CliSemanticStatus.Failed)]
    [InlineData(5, CliSemanticStatus.Incomplete)]
    [InlineData(6, CliSemanticStatus.Blocked)]
    [InlineData(7, CliSemanticStatus.Interrupted)]
    public async Task MapsScanFailure(int reasonValue, int statusValue)
    {
        using var workspace = CreateWorkspace(null, ["src/**"]);
        var reason = (RouteInspectMatchingFilesReason)reasonValue;
        var operation = RouteInspectOperationFactory.Create(null, (_, _) => ValueTask.FromResult(
            RouteInspectFileEnumeration.Unavailable(RouteInspectMatchingFilesScope.GitTrackedAndUntracked, reason)));
        var result = await operation(new(workspace.Workspace, "root/item", false, ["src/planned.cs"], matchingFiles: true), TestContext.Current.CancellationToken);
        Assert.Equal((CliSemanticStatus)statusValue, result.Status);
        Assert.Equal(reason, result.MatchingFiles?.Reason);
        Assert.NotNull(result.Identity);
        Assert.Null(result.MatchingFiles?.Count);
        if (result.Status == CliSemanticStatus.Incomplete)
        {
            Assert.NotNull(result.Profile);
            Assert.Equal("open-forge route inspect --help", result.Next?.Command);
            Assert.Contains(result.Conditions, condition => condition.Code == RouteInspectConditionCode.UnavailableFact);
        }
    }

    [Fact(DisplayName = "Unsafe candidate containment discards earlier matching paths")]
    public async Task UnsafeCandidateDiscardsPartialResults()
    {
        using var workspace = CreateWorkspace(null, ["*.md"]);
        workspace.Write("inside.md", "");
        var result = await Inspect(workspace, ["inside.md", "../outside.md"]);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInspectMatchingFilesReason.UnsafePath, result.MatchingFiles?.Reason);
        Assert.Empty(Assert.IsType<RouteInspectMatchingFiles>(result.MatchingFiles).Paths);
    }

    [Fact(DisplayName = "Git administrative paths and nested repository contents are ineligible")]
    public async Task ExcludesAdministrativePathsAndSubmodules()
    {
        using var workspace = CreateWorkspace(null, ["**/*.cs"]);
        workspace.Write(".git/config", "");
        workspace.Write("module/.git", "gitdir: elsewhere");
        workspace.Write("module/file.cs", "");
        workspace.Write("ordinary.cs", "");
        var result = await Inspect(workspace, [".git/config", "module/file.cs", "ordinary.cs"]);
        Assert.Equal(["ordinary.cs"], result.MatchingFiles?.Paths);
    }

    [Fact(DisplayName = "An invalid applyTo chain does not enumerate or become an empty match set")]
    public async Task InvalidChainDoesNotScan()
    {
        using var workspace = CreateWorkspace(null, [""]);
        var calls = 0;
        var operation = RouteInspectOperationFactory.Create(null, (_, _) =>
        {
            calls++;
            return ValueTask.FromResult(RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.WorkspaceFiles, []));
        });
        var result = await operation(new(workspace.Workspace, "root/item", false, matchingFiles: true), TestContext.Current.CancellationToken);
        Assert.Equal(0, calls);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteInspectMatchingFilesReason.ConditionUnavailable, result.MatchingFiles?.Reason);
        Assert.Null(result.MatchingFiles?.Count);
    }

    [Fact(DisplayName = "An empty inventory is a complete zero even when loading remains pending")]
    public async Task EmptyConditionedInventory()
    {
        using var workspace = CreateWorkspace(["src/**"], ["**/*.cs"]);
        var result = await Inspect(workspace, []);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteInspectApplicabilityState.Pending, result.Applicability?.State);
        var matches = Assert.IsType<RouteInspectMatchingFiles>(result.MatchingFiles);
        Assert.True(matches.Complete);
        Assert.Equal(0, matches.Count);
        Assert.NotNull(matches.Note);
    }

    [Fact(DisplayName = "Overwrite matching uses the established base chain")]
    public async Task OverwriteUsesBaseChain()
    {
        using var workspace = CreateWorkspace(["src/**"], ["**/*.cs"]);
        workspace.Write(".agents/root/item.overwrite.md", "# Replacement\n");
        workspace.Write("src/Current.cs", "");
        workspace.Write("tests/Other.cs", "");
        var operation = RouteInspectOperationFactory.Create(null, (_, _) => ValueTask.FromResult(
            RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.WorkspaceFiles, ["src/Current.cs", "tests/Other.cs"])));
        var result = await operation(new(workspace.Workspace, ".agents/root/item.overwrite.md", false, matchingFiles: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(["src/Current.cs"], result.MatchingFiles?.Paths);
    }

    [Fact(DisplayName = "Detached source matching uses its established local condition chain")]
    public async Task DetachedUsesLocalChain()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader([]);
        workspace.WriteEntrypoint(new()
        {
            RelativePath = ".agents/detached/_detached.md",
            Description = "Detached",
            Tags = ["Detached"],
            ApplyTo = ["src/**"],
        });
        workspace.Write("src/current.cs", "");
        workspace.Write("docs/other.md", "");
        var operation = RouteInspectOperationFactory.Create(null, (_, _) => ValueTask.FromResult(
            RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.WorkspaceFiles, ["src/current.cs", "docs/other.md"])));
        var result = await operation(new(workspace.Workspace, ".agents/detached/_detached.md", false, matchingFiles: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(["src/current.cs"], result.MatchingFiles?.Paths);
    }

    [Fact(DisplayName = "Caller cancellation during enumeration returns the cancelled matching-files result")]
    public async Task CancellationDuringEnumeration()
    {
        using var workspace = CreateWorkspace(null, ["src/**"]);
        using var cancellation = new CancellationTokenSource();
        var operation = RouteInspectOperationFactory.Create(null, (_, token) =>
        {
            cancellation.Cancel();
            token.ThrowIfCancellationRequested();
            throw new InvalidOperationException();
        });
        var result = await operation(new(workspace.Workspace, "root/item", false, matchingFiles: true), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(RouteInspectMatchingFilesReason.Cancelled, result.MatchingFiles?.Reason);
    }

    private static RouteInspectProfileIntegrationWorkspace CreateWorkspace(string[]? inherited, string[]? local)
    {
        var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader([RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow")]);
        workspace.WriteEntrypoint(new()
        {
            RelativePath = ".agents/root/_root.md",
            Description = "Root",
            Tags = ["Root"],
            ApplyTo = inherited,
            Entries = [RouteInspectProfileIntegrationWorkspace.Entry("Item", "item.md", "LoadNow")],
        });
        workspace.WriteRoutedMarkdown(".agents/root/item.md", "Item", ["LoadNow"], "# Item\n", local);
        return workspace;
    }

    private static ValueTask<RouteInspectResult> Inspect(
        RouteInspectProfileIntegrationWorkspace workspace,
        string[] paths,
        string[]? workingPaths = null)
    {
        var operation = RouteInspectOperationFactory.Create(null, (_, _) => ValueTask.FromResult(
            RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.WorkspaceFiles, paths)));
        return operation(new(workspace.Workspace, "root/item", false, workingPaths, matchingFiles: true), TestContext.Current.CancellationToken);
    }
}
