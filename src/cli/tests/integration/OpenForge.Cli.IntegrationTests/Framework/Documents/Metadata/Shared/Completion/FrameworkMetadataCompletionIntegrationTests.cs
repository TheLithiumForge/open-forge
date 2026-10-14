using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Documents.Metadata.Shared.Completion;

[Trait("Feature", "metadata-completion"), Trait("Evidence", "Integration")]
public sealed class FrameworkMetadataCompletionIntegrationTests
{
    [Theory(DisplayName = "Completion previews and guarded replacement preserve exact bytes in both forms and newline conventions"), Trait("Boundary", "OS")]
    [InlineData("Root", "\n", false, "plain")]
    [InlineData("Root", "\r\n", true, "plain")]
    [InlineData("Scoped", "\n", true, "plain")]
    [InlineData("Scoped", "\r\n", false, "plain")]
    [InlineData("Root", "\n", false, "root-partial")]
    [InlineData("Scoped", "\r\n", true, "root-partial")]
    [InlineData("Scoped", "\n", false, "scoped-partial")]
    [InlineData("Root", "\r\n", true, "scoped-partial")]
    [InlineData("Root", "\n", true, "root-flow")]
    [InlineData("Scoped", "\r\n", false, "root-flow")]
    [InlineData("Scoped", "\n", false, "scoped-flow")]
    [InlineData("Root", "\r\n", true, "scoped-flow")]
    [InlineData("Scoped", "\n", false, "foreign-flow")]
    [InlineData("Root", "\r\n", true, "empty-scoped-flow")]
    [InlineData("Scoped", "\n", false, "comment-only")]
    [InlineData("Root", "\r\n", true, "applicability")]
    public async Task PreviewApplyParity(string workspaceForm, string newline, bool bom, string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("metadata-completion");
        using var lockStore = WorkspaceLockTestStore.Create("metadata-completion-lock");
        var body = $"## **Deploy**  checklist{newline}Body café 💰\n\n```yaml\ndescription: body stays\n```";
        var yaml = scenario switch
        {
            "plain" => null,
            "root-partial" => "# header\ndescription: 'Authored words' # keep\ncustom: true\n",
            "scoped-partial" => "custom: true\nopen-forge: # wrapper\n    tags: [Team, Guidance] # keep\n",
            "root-flow" => "{description: 'Authored words', custom: true} # keep\n",
            "scoped-flow" => "open-forge: {tags: [Team, Guidance]} # keep\ncustom: true\n",
            "foreign-flow" => "{custom: true} # keep\n",
            "empty-scoped-flow" => "open-forge: {} # keep\n",
            "comment-only" => "# keep this header\n",
            "applicability" => "applyTo: ['src/**']\nopen-forge: {applyTo: ['src/**'], tags: [Team]}\n",
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var source = bom ? "\uFEFF" : "";
        if (yaml is not null) source += $"---{newline}{yaml.Replace("\n", newline)}---{newline}";
        source += body;
        var before = Encoding.UTF8.GetBytes(source);
        var path = temporary.CreateFile("note.md", before);
        var input = new FrameworkMetadataCompletionInput
        {
            Bytes = [.. await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken)],
            CanonicalPath = ".agents/guidance/note.md",
            Form = Enum.Parse<FrontmatterForm>(workspaceForm),
            DefaultTags = ["Guidance"],
        };
        var completer = new FrameworkMetadataCompleter();
        var preview = completer.Complete(input);
        Assert.Equal(FrameworkMetadataCompletionState.Completed, preview.State);
        Assert.Equal(before, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
        Assert.False(lockStore.InfrastructureExists);
        var applied = completer.Complete(input);
        Assert.Equal(preview.Bytes, applied.Bytes);
        await ApplyAsync(path, temporary, lockStore, before, applied);
        var after = await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
        Assert.Equal(preview.Bytes.ToArray(), after);
        Assert.True(after.AsSpan().EndsWith(Encoding.UTF8.GetBytes(body)));
        if (bom) Assert.Equal(new byte[] { 0xef, 0xbb, 0xbf }, after[..3]);
        if (yaml is not null) AssertInsertionOnly(before, after);
        Assert.Equal(FrameworkMetadataCompletionState.NoOp, completer.Complete(input with { Bytes = [.. after] }).State);
        Assert.Empty(Directory.EnumerateFiles(temporary.Path, ".open-forge-stage-*", SearchOption.AllDirectories));
    }

    [Theory(DisplayName = "Malformed or unsafe completion leaves real source bytes and mutation infrastructure untouched"), Trait("Boundary", "OS")]
    [InlineData("---\ndescription: ''\n---\nBody\n", "Malformed")]
    [InlineData("---\ntags: []\n---\nBody\n", "Malformed")]
    [InlineData("---\ndescription: [bad\n---\nBody\n", "Malformed")]
    [InlineData("---\n? description\n: Keep\n---\nBody\n", "Unsafe")]
    public async Task RejectsWithoutEffects(string source, string state)
    {
        using var temporary = TemporaryWorkspace.Create("metadata-completion-skipped");
        var path = temporary.CreateFile("note.md", source);
        var before = temporary.SnapshotHashes();
        var result = new FrameworkMetadataCompleter().Complete(new()
        {
            Bytes = [.. await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken)],
            CanonicalPath = ".agents/guidance/note.md",
            Form = FrontmatterForm.Scoped,
            DefaultTags = ["Guidance"],
        });
        Assert.Equal(Enum.Parse<FrameworkMetadataCompletionState>(state), result.State);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    private static void AssertInsertionOnly(byte[] before, byte[] after)
    {
        var prefix = 0;
        while (prefix < before.Length && before[prefix] == after[prefix]) prefix++;
        var insertedLength = after.Length - before.Length;
        Assert.True(insertedLength > 0);
        Assert.Equal(before[prefix..], after[(prefix + insertedLength)..]);
    }

    private static async Task ApplyAsync(
        string path,
        TemporaryWorkspace temporary,
        WorkspaceLockTestStore lockStore,
        byte[] before,
        FrameworkMetadataCompletionResult completion)
    {
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var change = PlannedFileChange.Replace(FileExpectation.File(path, path, FileExpectation.Hash(before)), completion.Bytes.AsSpan());
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var acquired = await lockStore.AcquireAsync(new(workspace, "index", Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var validated = await new MutationRevalidator(validator).ValidateAsync(lease, [change], TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Valid, validated.State);
        var check = Assert.Single(validated.Checks);
        var snapshot = Assert.IsType<FileStateSnapshot>(check.Actual);
        var recovery = await RecoveryBundleStore.PrepareAsync(RecoveryBundleInput.Create(
            workspace,
            lease.Request.Command,
            RecoveryBundleAttribution.Create(RecoveryBundleProducer.Index, RecoveryBundleOperation.Index, workspace),
            lease.Request.OperationId,
            [RecoveryBundleTarget.Create(change, snapshot)]), TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(recovery.Preparation);
        try
        {
            var receipt = await new FileChangeApplier(new MutationRevalidator(validator), validator)
                .ApplyAsync(lease, change, check, preparation, TestContext.Current.CancellationToken);
            Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
        }
        finally
        {
            File.Delete(preparation.BundlePath);
        }
    }
}
