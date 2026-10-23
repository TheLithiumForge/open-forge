using OpenForge.Cli.Core.Commands.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Remove.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Workspace.Shared.Runtime;

public sealed partial class WorkspaceRuntimeReservationIntegrationTests
{
    [Theory(DisplayName = "Remove selection and inventories reject direct and linked runtime targets")]
    [InlineData(".open-forge-state"), InlineData("alias")]
    public async Task RemoveRejectsRuntime(string root)
    {
        using var files = TemporaryWorkspace.Create("runtime-remove");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        Assert.True(files.TryCreateDirectorySymbolicLink("alias", files.Combine(".open-forge-state"), out var cause), cause);
        var workspace = Workspace(files);
        foreach (var target in new[] { root, $"{root}/recovery/v1/history.md", $"{root}/missing/file.md" })
        {
            var selected = await RemoveTargetResolver.ResolveAsync(workspace, RemoveSelection.Selected(RemoveKind.Path, target), Token);
            Assert.NotNull(selected.Cause);
        }
        var inventory = new RemovePathInventory(new PhysicalPathResolver());
        Assert.IsType<RemovePathDirectoryInventory.Unavailable>(await inventory.InventoryDirectoryAsync(workspace, files.Combine(root), null, Token));
        Assert.IsType<RemovePathFileCapture.Unavailable>(await inventory.CaptureFileAsync(workspace, files.Combine($"{root}/recovery/v1/history.md"), Token));
        Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(RuntimeFile)));
    }

    [Theory(DisplayName = "Route Create and Init reject new targets below a runtime alias")]
    [InlineData(false), InlineData(true)]
    public async Task RouteCreationRejectsRuntimeAlias(bool existing)
    {
        using var files = TemporaryWorkspace.Create("runtime-create");
        SeedWorkspace(files);
        files.CreateDirectory(".open-forge-state");
        if (existing) files.CreateFile(".open-forge-state/new.md", new byte[] { 255 });
        Assert.True(files.TryCreateDirectorySymbolicLink(".agents/alias", files.Combine(".open-forge-state"), out var cause), cause);
        var workspace = Workspace(files);
        var create = await new RouteCreateTargetInspector().InspectAsync(new RouteCreateRequest(workspace, ".agents/alias/new.md",
            new RouteCreateMetadataInput(description: "New", tags: ["Docs"], responsibility: null), templateReference: null, RouteCreateMode.DryRun), Token);
        Assert.Equal(RouteCreateFindingCode.TargetUnsafe, create.Boundary?.Finding.Code);
        var init = new RouteInitTargetPlanner().Resolve(new RouteInitRequest(workspace, ".agents/alias/new/_new.md",
            RouteInitScaffold.Generic, RouteInitMode.DryRun, RouteInitMetadataInput.None));
        Assert.Equal(RouteInitTargetResolutionState.Invalid, init.State);
        Assert.False(File.Exists(files.Combine(".open-forge-state/new/_new.md")));
    }

    [Theory(DisplayName = "Route selectors refuse runtime aliases without changing their contents")]
    [InlineData("move-source"), InlineData("move-destination"), InlineData("remove")]
    public async Task RouteSelectorsRejectRuntimeAliases(string operation)
    {
        using var files = TemporaryWorkspace.Create("runtime-route-selectors");
        SeedWorkspace(files);
        files.CreateFile(RuntimeFile, "retained history");
        Assert.True(files.TryCreateDirectorySymbolicLink(".agents/alias", files.Combine(".open-forge-state"), out var cause), cause);
        var arguments = operation switch
        {
            "move-source" => new[] { "route", "move", ".agents/alias/recovery/v1/history.md", ".agents/root/moved.md", "--dry-run" },
            "move-destination" => ["route", "move", ".agents/root/leaf.md", ".agents/alias/new.md", "--dry-run"],
            "remove" => ["route", "remove", ".agents/alias/recovery/v1/history.md", "--dry-run"],
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        var result = await CliHostCapture.RunAsync(arguments, files.Path);
        Assert.True(result.ExitCode != 0, result.Output + result.Error);
        Assert.Equal("retained history", File.ReadAllText(files.Combine(RuntimeFile)));
        Assert.False(File.Exists(files.Combine(".open-forge-state/new.md")));
        Assert.False(File.Exists(files.Combine(".agents/root/moved.md")));
    }

    [Fact(DisplayName = "Index ignores runtime content and aliases when no ignore file exists")]
    public async Task IndexIgnoresRuntimeContent()
    {
        using var files = TemporaryWorkspace.Create("runtime-index");
        SeedWorkspace(files);
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        Assert.True(files.TryCreateDirectorySymbolicLink(".agents/root/runtime", files.Combine(".open-forge-state"), out var cause), cause);
        var result = await CliHostCapture.RunAsync(["index", "--dry-run", "--format", "json"], files.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.DoesNotContain("history.md", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain(".agents/root/runtime", result.Output, StringComparison.Ordinal);
        Assert.False(File.Exists(files.Combine(".open-forge-state/.gitignore")));
        Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(RuntimeFile)));
    }

    [Fact(DisplayName = "Removing a Framework workspace file preserves runtime recovery bytes")]
    public async Task FrameworkFileRemovalPreservesRuntimeRecovery()
    {
        using var files = TemporaryWorkspace.Create("runtime-removal-preservation");
        SeedWorkspace(files);
        files.CreateFile(".agents/root/scratch.bin", "remove me");
        files.CreateFile(RuntimeFile, "retained history");
        var result = await CliHostCapture.RunAsync(["remove", ".agents/root/scratch.bin", "--kind", "path", "--automatic"], files.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.False(File.Exists(files.Combine(".agents/root/scratch.bin")));
        Assert.Equal("retained history", File.ReadAllText(files.Combine(RuntimeFile)));
    }

    private static void SeedWorkspace(TemporaryWorkspace files)
    {
        files.CreateFile(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Root](root/_root.md) - #Docs\n");
        files.CreateFile(".agents/open-forge.json", "{\"schemaVersion\":2}\n");
        files.CreateFile(".agents/open-forge.lock.json", WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
        {
            Framework = new(new("framework", null), [], []),
        }));
        files.CreateFile(".agents/root/_root.md", "---\nopen-forge:\n  description: Root\n  tags: [Docs]\n---\n# Root\n\n## Entries\n\n- [Leaf](leaf.md) - #Docs\n");
        files.CreateFile(".agents/root/leaf.md", "---\nopen-forge:\n  description: Leaf\n  tags: [Docs]\n---\n# Leaf\n");
    }
}
