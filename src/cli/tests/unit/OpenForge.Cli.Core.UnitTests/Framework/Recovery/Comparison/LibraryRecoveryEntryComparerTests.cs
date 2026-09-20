using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Recovery.Comparison;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Unit")]
public sealed class LibraryRecoveryEntryComparerTests
{
    private const string RawTarget = "../shared/.agents/a.md";

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Recovery comparison preserves exact prior intended and third ordinary object identities")]
    [InlineData("create", "missing", "Prior"), InlineData("create", "after", "Intended"), InlineData("create", "third", "Third")]
    [InlineData("replace", "before", "Prior"), InlineData("replace", "after", "Intended"), InlineData("replace", "third", "Third")]
    [InlineData("generated", "before", "Prior"), InlineData("generated", "after", "Intended"), InlineData("generated", "missing", "Third")]
    [InlineData("delete", "before", "Prior"), InlineData("delete", "missing", "Intended"), InlineData("delete", "third", "Third")]
    public void ComparesOrdinaryState(string operation, string observed, string expected)
    {
        var context = Context(operation);
        var leaf = observed == "missing" ? NoFollowLeafObservation.Missing(context.LogicalPath)
            : NoFollowLeafObservation.OrdinaryFile(context.LogicalPath);
        var identity = observed == "missing" ? null : Identity(observed);
        var content = identity is null ? null : new RecoveryOrdinaryContentObservation(context.LogicalPath, identity, failure: null);
        var input = new RecoveryEntryComparisonInput(context, leaf, content);

        var result = RecoveryEntryComparer.Compare(input);

        Assert.Same(input, result.Input);
        Assert.Equal(expected, result.State.ToString());
        var actual = Assert.IsType<RecoveryEntryState>(result.Observed);
        Assert.Equal(observed == "missing" ? RecoveryEntryStateKind.Missing : RecoveryEntryStateKind.OrdinaryFile, actual.Kind);
        Assert.Equal(identity, actual.OrdinaryFile);
        Assert.Null(actual.RelativeFileLink);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Recovery comparison uses exact raw relative link identity and safely observed absence")]
    [InlineData("link-create", "missing", "Prior"), InlineData("link-create", "exact", "Intended"), InlineData("link-create", "changed", "Third")]
    [InlineData("link-delete", "exact", "Prior"), InlineData("link-delete", "missing", "Intended"), InlineData("link-delete", "changed", "Third")]
    public void ComparesRawLinkState(string operation, string observed, string expected)
    {
        var context = Context(operation);
        var identity = observed == "missing" ? null : RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink,
            observed == "exact" ? RawTarget : "../shared/./.agents/a.md");
        var leaf = identity is null ? NoFollowLeafObservation.Missing(context.LogicalPath)
            : NoFollowLeafObservation.CreateRelativeFileLink(context.LogicalPath, identity);
        var input = new RecoveryEntryComparisonInput(context, leaf, ordinaryContent: null);

        var result = RecoveryEntryComparer.Compare(input);

        Assert.Equal(expected, result.State.ToString());
        var actual = Assert.IsType<RecoveryEntryState>(result.Observed);
        Assert.Equal(identity, actual.RelativeFileLink);
        Assert.Null(actual.OrdinaryFile);
        Assert.Null(result.Input.OrdinaryContent);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Recovery comparison blocks unsafe object kinds and preserves unavailable observation coverage")]
    [InlineData("directory", "Blocked"), InlineData("absolute-link", "Blocked"), InlineData("unknown-link", "Blocked")]
    [InlineData("reparse", "Blocked"), InlineData("special", "Blocked"), InlineData("inaccessible", "Unavailable"), InlineData("unknown", "Unavailable")]
    public void ClassifiesUnsafeAndUnavailable(string observed, string expected)
    {
        var context = Context("link-create");
        var path = context.LogicalPath;
        var failure = new FilesystemFailure(FilesystemFailureKind.AccessDenied, "unavailable observation");
        var leaf = observed switch
        {
            "directory" => NoFollowLeafObservation.Directory(path),
            "absolute-link" => NoFollowLeafObservation.CreateLink(path,
                NoFollowLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, Path.GetFullPath("source.md"), NoFollowLinkTargetForm.Absolute)),
            "unknown-link" => NoFollowLeafObservation.CreateLink(path,
                NoFollowLinkIdentity.Create(NoFollowLinkKind.Other, rawTarget: null, NoFollowLinkTargetForm.Unavailable)),
            "reparse" => NoFollowLeafObservation.Classified(path, NoFollowLeafState.ReparsePoint),
            "special" => NoFollowLeafObservation.Classified(path, NoFollowLeafState.Special),
            "inaccessible" => NoFollowLeafObservation.Classified(path, NoFollowLeafState.Inaccessible, failure),
            "unknown" => NoFollowLeafObservation.Classified(path, NoFollowLeafState.Unknown, failure),
            _ => throw new ArgumentOutOfRangeException(nameof(observed)),
        };

        var result = RecoveryEntryComparer.Compare(new RecoveryEntryComparisonInput(context, leaf, ordinaryContent: null));

        Assert.Equal(expected, result.State.ToString());
        Assert.Null(result.Observed);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Ordinary recovery content read failure remains unavailable despite ordinary leaf admission")]
    public void IndependentReadFailureIsNotMissingOrThirdIdentity()
    {
        var context = Context("create");
        var content = new RecoveryOrdinaryContentObservation(context.LogicalPath, identity: null,
            failure: new FilesystemFailure(FilesystemFailureKind.InputOutput, "read failed"));
        var input = new RecoveryEntryComparisonInput(context, NoFollowLeafObservation.OrdinaryFile(context.LogicalPath), content);

        var result = RecoveryEntryComparer.Compare(input);

        Assert.Equal(RecoveryBundleTargetComparisonState.Unavailable, result.State);
        Assert.Null(result.Observed);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Equal prior and intended recovery identities retain Prior precedence")]
    public void EqualPriorAndIntendedRetainsPriorPrecedence()
    {
        var priorIdentity = RecoveryContentIdentity.FromBytes("same"u8);
        var intendedIdentity = RecoveryContentIdentity.FromBytes("same"u8);
        var prior = RecoveryEntryState.Ordinary(priorIdentity);
        var intended = RecoveryEntryState.Ordinary(intendedIdentity);
        Assert.NotSame(priorIdentity, intendedIdentity);
        Assert.Equal(priorIdentity, intendedIdentity);
        Assert.NotSame(prior, intended);
        Assert.Equal(prior, intended);
        var workspacePath = Path.GetFullPath("comparison-workspace");
        var workspace = new CliWorkspace(
            lexicalRoot: workspacePath,
            physicalRoot: workspacePath,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var entry = RecoveryEntry.Create(
            ordinal: 0,
            logicalPath: CanonicalRelativePath.Create(".agents/a.md"),
            kind: RecoveryEntryKind.OrdinaryReplace,
            prior: prior,
            intended: intended,
            priorPayload: "payloads/00000000.bin");
        var context = new RecoveryEntryComparisonContext(workspace, entry);
        var observedIdentity = RecoveryContentIdentity.FromBytes("same"u8);
        var leaf = NoFollowLeafObservation.OrdinaryFile(context.LogicalPath);
        var content = new RecoveryOrdinaryContentObservation(context.LogicalPath, observedIdentity, failure: null);
        var input = new RecoveryEntryComparisonInput(context, leaf, content);

        var result = RecoveryEntryComparer.Compare(input);

        Assert.Equal(RecoveryBundleTargetComparisonState.Prior, result.State);
        Assert.Same(input, result.Input);
        var observed = Assert.IsType<RecoveryEntryState>(result.Observed);
        Assert.Equal(prior, observed);
        Assert.Equal(RecoveryEntryStateKind.OrdinaryFile, observed.Kind);
        Assert.Same(observedIdentity, observed.OrdinaryFile);
        Assert.Null(observed.RelativeFileLink);
        Assert.Null(result.Cause);
    }

    private static RecoveryEntryComparisonContext Context(string operation)
    {
        var workspacePath = Path.GetFullPath("comparison-workspace");
        var workspace = new CliWorkspace(workspacePath, workspacePath, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var link = RecoveryEntryState.RelativeLink(RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, RawTarget));
        var kind = operation switch
        {
            "create" => RecoveryEntryKind.OrdinaryCreate,
            "replace" => RecoveryEntryKind.OrdinaryReplace,
            "generated" => RecoveryEntryKind.OrdinaryReplaceGeneratedRegion,
            "delete" => RecoveryEntryKind.OrdinaryDelete,
            "link-create" => RecoveryEntryKind.RelativeFileLinkCreate,
            "link-delete" => RecoveryEntryKind.RelativeFileLinkDelete,
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        var prior = operation switch
        {
            "create" or "link-create" => RecoveryEntryState.Missing,
            "link-delete" => link,
            _ => RecoveryEntryState.Ordinary(Identity("before")),
        };
        var intended = operation switch
        {
            "delete" or "link-delete" => RecoveryEntryState.Missing,
            "link-create" => link,
            _ => RecoveryEntryState.Ordinary(Identity("after")),
        };
        var payload = prior.Kind == RecoveryEntryStateKind.OrdinaryFile ? "payloads/00000000.bin" : null;
        return new RecoveryEntryComparisonContext(workspace,
            RecoveryEntry.Create(ordinal: 0, logicalPath: CanonicalRelativePath.Create(".agents/a.md"), kind: kind,
                prior: prior, intended: intended, priorPayload: payload));
    }

    private static RecoveryContentIdentity Identity(string value)
        => value switch
        {
            "before" => RecoveryContentIdentity.FromBytes("before"u8),
            "after" => RecoveryContentIdentity.FromBytes("after"u8),
            "third" => RecoveryContentIdentity.FromBytes("third"u8),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
}
