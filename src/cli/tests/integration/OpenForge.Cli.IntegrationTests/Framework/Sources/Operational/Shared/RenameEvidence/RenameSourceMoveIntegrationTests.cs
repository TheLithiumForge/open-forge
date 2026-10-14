using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RenameSourceMoveIntegrationTests
{
    [Fact(DisplayName = "A broken outgoing link in a Git-proven moved source resolves from its old directory")]
    public async Task GitSourceRelativeOutgoingLink()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        const string old = ".agents/directives/csharp/design.md";
        const string current = ".agents/guidance/deeper/sub/design.md";
        const string destination = "../../patterns/software/exhaustive-csharp-enum-switch.md";
        const string target = ".agents/patterns/software/exhaustive-csharp-enum-switch.md";
        repository.Move(old, current);
        Assert.False(File.Exists(repository.PathFor(".agents/guidance/patterns/software/exhaustive-csharp-enum-switch.md")));
        var evidence = await new RenameEvidenceReader(new PhysicalPathResolver(), new GitRenameEvidenceReader(configureProcess: repository.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync)
            .ReadAsync(repository.Request(), TestContext.Current.CancellationToken);
        var result = await new RenameSourceMoveResolver(new PhysicalPathResolver())
            .ResolveAsync(new(repository.Workspace, Reference(current, destination)), evidence, TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(old, result.SourceMove.OldPath);
        Assert.Equal(target, result.OldTargetPath);
        Assert.Equal(target, result.NewTargetPath);
        Assert.Equal(File.ReadAllBytes(repository.PathFor(target)), result.TargetSnapshot.Bytes);
    }

    [Theory(DisplayName = "Source-move resolution validates a current target or that target's independently proven move")]
    [InlineData(false), InlineData(true)]
    public async Task RecordedSourceAndTargetMoves(bool moveTarget)
    {
        using var repository = new RenameEvidenceGitWorkspace();
        const string old = ".agents/guidance/billing/sub/note.md";
        const string current = ".agents/guidance/invoices/deep/sub/note.md";
        const string target = ".agents/guidance/billing/rules.md";
        const string movedTarget = ".agents/guidance/invoices/rules.md";
        repository.Write(current, "---\ndescription: Specific authored source identity\ntags: [Guidance, Billing]\n---\n\n# Source title\n\nRead [Rules](../rules.md#topic).\n");
        repository.Write(moveTarget ? movedTarget : target, "---\ndescription: Specific authored target identity\ntags: [Guidance, Billing]\n---\n\n# Rules title\n\n## Topic\n\nAuthored target content.\n");
        repository.Write(RenameMetadataEvidenceIntegrationTests.Parent,
            "# Guidance\n\n## Entries\n\n- [Specific authored source identity](billing/sub/note.md) - #Guidance #Billing\n- [Specific authored target identity](billing/rules.md) - #Guidance #Billing\n");
        var evidence = await new RenameEvidenceReader(new PhysicalPathResolver()).ReadAsync(repository.Request(old, target)
            with
        { RecordedIdentities = RenameMetadataEvidenceIntegrationTests.ReadEntries(repository) }, TestContext.Current.CancellationToken);
        var reference = Reference(current, "../rules.md#topic");
        var result = await new RenameSourceMoveResolver(new PhysicalPathResolver()).ResolveAsync(new(repository.Workspace, reference), evidence, TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(old, result.SourceMove.OldPath);
        Assert.Equal(target, result.OldTargetPath);
        Assert.Equal(moveTarget ? movedTarget : target, result.NewTargetPath);
        Assert.Equal(moveTarget, result.TargetMove is not null);
        Assert.Equal("topic", result.Fragment);
        File.Delete(repository.PathFor(moveTarget ? movedTarget : target));
        Assert.Null(await new RenameSourceMoveResolver(new PhysicalPathResolver()).ResolveAsync(new(repository.Workspace, reference), evidence, TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "A folder containing a proven moved source does not supply proof for its unrecorded child")]
    public async Task NoSourceProofNoDirectoryGuess()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        repository.Write(".agents/guidance/moved/source.md", "# Source\n\n[Missing](../target.md)\n");
        repository.Write(".agents/guidance/target.md", "# Target\n");
        var evidence = await new RenameEvidenceReader(new PhysicalPathResolver()).ReadAsync(repository.Request(), TestContext.Current.CancellationToken);
        Assert.Null(await new RenameSourceMoveResolver(new PhysicalPathResolver()).ResolveAsync(
            new(repository.Workspace, Reference(".agents/guidance/moved/source.md", "../target.md")), evidence, TestContext.Current.CancellationToken));
    }

    private static LocalReferenceObservation Reference(string path, string destination)
        => new()
        {
            SourcePath = path,
            RoutePath = path,
            Kind = LocalReferenceKind.Link,
            Destination = destination,
            Label = MarkdownLinkLabelFact.Supported("Rules"),
            Location = null,
            DestinationLocation = null,
            Facts = new SourceLinkDestinationFacts
            {
                Fragment = destination.Contains('#') ? destination.Split('#')[1] : null,
                Target = new SourceLinkTarget
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = null,
                    PhysicalPath = null,
                    Layer = null,
                    Resolution = SourceLinkTargetResolution.Missing,
                    Network = null,
                },
                Finding = null,
            },
            Fragment = LocalReferenceFragmentObservation.NotRequested(),
            Canonicalizations = [],
        };
}
