using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;

public sealed class GenericRouteInitOperationIntegrationTests
{
    [Fact(DisplayName = "Generic Route Init creates a missing chain with parent-first directories and exact draft scaffolds"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task MissingAgentsChainCreatesCanonicalDraftsAndReportsResidualDirectoryEffects()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-missing-chain");

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project-alpha/documents"));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteInitFindingCode.NeedsAuthoring);
        Assert.True(Directory.Exists(workspace.Absolute(".agents")));
        Assert.True(Directory.Exists(workspace.Absolute(".agents/memory")));
        Assert.True(Directory.Exists(workspace.Absolute(".agents/memory/project-alpha")));
        Assert.True(Directory.Exists(workspace.Absolute(".agents/memory/project-alpha/documents")));
        Assert.False(workspace.Exists(".agents/loader.md"));

        Assert.Equal(
            [
                ".agents",
                ".agents/memory",
                ".agents/memory/project-alpha",
                ".agents/memory/project-alpha/documents",
            ],
            result.Effects
                .Where(effect => effect.Kind == RouteInitEffectKind.Directory)
                .Select(effect => effect.Path)
                .ToArray());
        Assert.All(
            result.Effects.Where(effect => effect.Kind == RouteInitEffectKind.Directory),
            effect =>
            {
                Assert.Equal(RouteInitEffectAction.Create, effect.Action);
                Assert.Equal(RouteInitEffectOutcome.Verified, effect.Outcome);
                Assert.Equal(RouteInitEffectResidual.None, effect.Residual);
            });

        Assert.Collection(
            result.Entrypoints,
            entrypoint => AssertCreatedDraft(
                entrypoint,
                "memory",
                ".agents/memory/_memory.md"),
            entrypoint => AssertCreatedDraft(
                entrypoint,
                "memory/project-alpha",
                ".agents/memory/project-alpha/_project-alpha.md"),
            entrypoint => AssertCreatedDraft(
                entrypoint,
                "memory/project-alpha/documents",
                ".agents/memory/project-alpha/documents/_documents.md"));

        Assert.Equal(
            ExpectedDraft(
                "documents",
                "Draft route for memory/project-alpha/documents; replace this description before relying on it for selection"),
            workspace.ReadText(".agents/memory/project-alpha/documents/_documents.md"));
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(RouteInitRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(0, await workspace.RecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Generic Route Init emits complete explicit metadata and optional responsibility for a one-segment target"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task ExplicitMetadataCreatesCompleteCanonicalScaffold()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-explicit-metadata");
        var metadata = new RouteInitMetadataInput(
            description: "A ready route",
            responsibilitySpecified: true,
            responsibility: "Owns the ready route.",
            tags: ["Docs", "Architecture"]);

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("documents", metadata: metadata));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        var entrypoint = Assert.Single(result.Entrypoints);
        Assert.Equal("documents", entrypoint.Id);
        Assert.Equal(RouteInitEntrypointOutcome.Created, entrypoint.Outcome);
        Assert.NotNull(entrypoint.Metadata);
        Assert.Equal(RouteInitDescriptionSource.Explicit, entrypoint.Metadata!.DescriptionSource);
        Assert.Equal(RouteInitResponsibilitySource.Explicit, entrypoint.Metadata.ResponsibilitySource);
        Assert.Equal(RouteInitTagsSource.Explicit, entrypoint.Metadata.TagsSource);
        Assert.Equal(["Docs", "Architecture"], entrypoint.Metadata.Tags);

        var source = workspace.ReadText(".agents/documents/_documents.md");
        Assert.Contains("description: A ready route", source, StringComparison.Ordinal);
        Assert.Contains("responsibility: Owns the ready route.", source, StringComparison.Ordinal);
        Assert.Contains("tags: [Docs, Architecture]", source, StringComparison.Ordinal);
        Assert.DoesNotContain("NeedsAuthoring", source, StringComparison.Ordinal);
        Assert.DoesNotContain("rune:", source, StringComparison.Ordinal);
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(RouteInitRecoveryState.NotCreated, result.Recovery.State);
    }

    [Fact(DisplayName = "Generic Route Init preserves a canonical parent and only replaces its bounded generated region"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task ExistingCanonicalParentPreservesAuthoredBytesAndUnrelatedFiles()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-canonical-parent");
        workspace.WriteText(
            ".agents/memory/_memory.md",
            CanonicalEntrypoint(
                "Memory",
                "Memory",
                "# memory\n\nAuthored prefix must remain.\n\n",
                "stale generated entry",
                "\nAuthored suffix must remain."));
        workspace.WriteText("unrelated.txt", "unrelated authored bytes\n");
        var beforeUnrelated = workspace.ReadText("unrelated.txt");
        var before = workspace.ReadText(".agents/memory/_memory.md");

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.True(workspace.Exists(".agents/memory/project/_project.md"));
        var after = workspace.ReadText(".agents/memory/_memory.md");
        Assert.StartsWith(
            "---\nopen-forge:\n  description: Memory\n  tags: [Memory]\n---\n# memory\n\nAuthored prefix must remain.\n\n",
            after,
            StringComparison.Ordinal);
        Assert.Contains("\nAuthored suffix must remain.", after, StringComparison.Ordinal);
        Assert.Contains("project/_project.md", after, StringComparison.Ordinal);
        Assert.DoesNotContain("stale generated entry", after, StringComparison.Ordinal);
        Assert.Equal(beforeUnrelated, workspace.ReadText("unrelated.txt"));
        Assert.Contains(
            result.Effects,
            effect => effect.Path == ".agents/memory/_memory.md"
                && effect.Kind == RouteInitEffectKind.GeneratedRegion
                && effect.Action == RouteInitEffectAction.Replace);
        Assert.Equal(RouteInitRecoveryState.Removed, result.Recovery.State);
        Assert.Equal(0, await workspace.RecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
        Assert.NotEqual(before, after);
    }

    [Theory(DisplayName = "Generic Route Init preserves every accepted compatibility entrypoint without creating a canonical sibling"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    [InlineData("index.md")]
    [InlineData("_index.md")]
    [InlineData("references.md")]
    [InlineData("_references.md")]
    public async Task CompatibilityEntrypointsRemainTheSelectedSource(string compatibilityName)
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create(
            $"route-init-generic-compatibility-{compatibilityName.Replace('.', '-')}");
        var compatibilityPath = $".agents/memory/{compatibilityName}";
        workspace.WriteText(
            compatibilityPath,
            CanonicalEntrypoint(
                "Memory compatibility",
                "Memory",
                "# memory\n\nAuthored compatibility content.\n\n",
                "stale generated entry",
                "\nCompatibility suffix."));
        var before = workspace.ReadText(compatibilityPath);

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        var existing = Assert.Single(
            result.Entrypoints.Where(entrypoint => entrypoint.Current == RouteInitEntrypointCurrent.Existing));
        Assert.Equal(RouteInitEntrypointForm.Compatibility, existing.Form);
        Assert.Equal(compatibilityPath, existing.Path);
        Assert.Equal("memory/project", Assert.Single(
            result.Entrypoints.Where(entrypoint => entrypoint.Current == RouteInitEntrypointCurrent.Missing)).Id);
        Assert.False(workspace.Exists(".agents/memory/_memory.md"));
        Assert.True(workspace.Exists(".agents/memory/project/_project.md"));
        var after = workspace.ReadText(compatibilityPath);
        Assert.StartsWith(before[..before.IndexOf("<!-- open-forge:generated-index:start -->", StringComparison.Ordinal)], after, StringComparison.Ordinal);
        Assert.Contains("\nCompatibility suffix.", after, StringComparison.Ordinal);
        Assert.Contains("project/_project.md", after, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Generic Route Init preserves an existing exact compatibility operand without creating a canonical sibling"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task ExistingExactCompatibilityOperandRemainsSelected()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-existing-exact-compatibility");
        const string compatibilityPath = ".agents/memory/index.md";
        workspace.WriteText(
            compatibilityPath,
            CanonicalEntrypoint(
                "Memory compatibility",
                "Memory",
                "# memory\n\nAuthored compatibility content.\n\n",
                "- none - No entries - #Empty",
                "\nCompatibility suffix."));

        var result = await ExecuteAsync(
            workspace,
            workspace.Request(compatibilityPath));

        Assert.Equal(compatibilityPath, result.Target.Path);
        var existing = Assert.Single(result.Entrypoints);
        Assert.Equal(compatibilityPath, existing.Path);
        Assert.Equal(RouteInitEntrypointForm.Compatibility, existing.Form);
        Assert.Equal(RouteInitEntrypointCurrent.Existing, existing.Current);
        Assert.False(workspace.Exists(".agents/memory/_memory.md"));
        Assert.True(workspace.Exists(compatibilityPath));
    }

    [Fact(DisplayName = "Generic Route Init treats Rune-only YAML as absent metadata without writes"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task RuneOnlyYamlIsMetadataIncompleteAndPreserved()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-opaque-rune");
        const string opaque = "---\nrune:\n  description: Unrelated value\n  tags: [Unrelated]\n---\n\n# memory\n\nOpaque authored bytes.\n\n";
        workspace.WriteText(
            ".agents/memory/index.md",
            opaque + "\nOpaque suffix." + OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = "stale generated entry",
                Prefix = string.Empty,
            }));
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteInitFindingCode.MetadataIncomplete, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/memory/project/_project.md"));
        Assert.Equal(RouteInitRecoveryState.NotRequired, result.Recovery.State);
    }

    [Fact(DisplayName = "Generic Route Init reads Open Forge and preserves a sibling Rune root as opaque YAML"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task OpenForgeMetadataIgnoresAndPreservesSiblingRuneYaml()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-dual-metadata");
        const string unrelated = "rune:\n  description: Unrelated value\n  tags: [Unrelated]\n";
        workspace.WriteText(
            ".agents/memory/_memory.md",
            "---\nopen-forge:\n  description: Canonical memory\n  tags: [Memory]\n"
                + unrelated
                + "---\n\n# memory\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n\n- none - No entries - #Empty\n\n<!-- open-forge:generated-index:end -->\n");

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(RouteInitFindingCode.NeedsAuthoring, Assert.Single(result.Findings).Code);
        Assert.True(workspace.Exists(".agents/memory/project/_project.md"));
        var source = workspace.ReadText(".agents/memory/_memory.md");
        Assert.Contains("open-forge:\n  description: Canonical memory\n  tags: [Memory]\n", source, StringComparison.Ordinal);
        Assert.Contains(unrelated, source, StringComparison.Ordinal);
        Assert.Contains("project/_project.md", source, StringComparison.Ordinal);
        Assert.Equal(RouteInitRecoveryState.Removed, result.Recovery.State);
    }

    [Fact(DisplayName = "Generic Route Init updates a valid Loader generated region for the first new entrypoint without creating a Loader"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task ValidLoaderReceivesFirstNewEntrypointProjection()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-loader");
        workspace.WriteText(
            ".agents/loader.md",
            Loader(
                "# Loader\n\nLoader authored prefix.\n\n",
                "stale loader entry",
                "\nLoader authored suffix."));

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.True(workspace.Exists(".agents/loader.md"));
        Assert.True(workspace.Exists(".agents/memory/_memory.md"));
        var loader = workspace.ReadText(".agents/loader.md");
        Assert.Contains("Loader authored prefix.", loader, StringComparison.Ordinal);
        Assert.Contains("memory/_memory.md", loader, StringComparison.Ordinal);
        Assert.Contains("Loader authored suffix.", loader, StringComparison.Ordinal);
        Assert.DoesNotContain("stale loader entry", loader, StringComparison.Ordinal);
        Assert.DoesNotContain(
            result.Effects,
            effect => effect.Path == ".agents/loader.md"
                && effect.Action == RouteInitEffectAction.Create);
        Assert.Contains(
            result.Effects,
            effect => effect.Path == ".agents/loader.md"
                && effect.Kind == RouteInitEffectKind.GeneratedRegion
                && effect.Action == RouteInitEffectAction.Replace);
    }

    private static async ValueTask<RouteInitResult> ExecuteAsync(
        GenericRouteInitIntegrationWorkspace workspace,
        RouteInitRequest request,
        CancellationToken? cancellationToken = null)
        => await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(request, cancellationToken ?? TestContext.Current.CancellationToken);

    private static void AssertCreatedDraft(
        RouteInitEntrypoint entrypoint,
        string id,
        string path)
    {
        Assert.Equal(id, entrypoint.Id);
        Assert.Equal(path, entrypoint.Path);
        Assert.Equal(RouteInitEntrypointForm.Canonical, entrypoint.Form);
        Assert.Equal(RouteInitEntrypointCurrent.Missing, entrypoint.Current);
        Assert.Equal(RouteInitEntrypointOwnership.User, entrypoint.Ownership);
        Assert.NotNull(entrypoint.Metadata);
        Assert.Equal(RouteInitEntrypointOutcome.Created, entrypoint.Outcome);
    }

    private static string ExpectedDraft(string slug, string description)
        => $"---\nopen-forge:\n  description: {description}\n  tags: [NeedsAuthoring]\n---\n\n"
            + $"# {slug}\n\n{description}.\n\n"
            + "## Axioms\n\n"
            + "- inherited - No local axioms; loaded ancestor axioms remain active.\n\n"
            + "## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "- none - No entries - #Empty\n"
            + "<!-- open-forge:generated-index:end -->";

    private static string CanonicalEntrypoint(
        string description,
        string tag,
        string prefix,
        string generatedEntries,
        string suffix)
        => OpenForgeDocumentSeed.Metadata(
            description,
            [tag],
            prefix + suffix
                + OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                {
                    Entries = generatedEntries,
                    Prefix = string.Empty,
                }));

    private static string Loader(
        string prefix,
        string generatedEntries,
        string suffix)
        => prefix + suffix
            + OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = generatedEntries,
                Prefix = string.Empty,
            });
}
