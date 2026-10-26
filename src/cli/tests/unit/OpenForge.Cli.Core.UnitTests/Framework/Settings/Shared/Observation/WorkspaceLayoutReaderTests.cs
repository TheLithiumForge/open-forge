using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Observation;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class WorkspaceLayoutReaderTests
{
    private const string CustomRoot = "docs/forge";

    [Theory(DisplayName = "A missing selected loader is reported only when its removal is not recorded")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(CustomRoot, false), InlineData(CustomRoot, true)]
    public void MissingLoaderRemoval(string root, bool removed)
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, root);
        var loader = root + "/" + WorkspaceLayoutNames.LoaderFileName;
        var settings = WorkspaceSettingsDocument.Empty with { RemovedFiles = removed ? [loader] : [] };
        Assert.Equal(!removed, WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout, settings, true, FileExpectationKind.Missing));
        Assert.False(WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout, settings, false, FileExpectationKind.Missing));
        Assert.False(WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout, settings, true, FileExpectationKind.File));
    }

    [Fact(DisplayName = "Loader removal follows selected-layout directory exclusions and ordinal paths")]
    public void DirectoryRemoval()
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, CustomRoot);
        var settings = WorkspaceSettingsDocument.Empty with { RemovedDirectories = [CustomRoot] };
        Assert.False(WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout, settings, true, FileExpectationKind.Missing));
        Assert.True(WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout,
            settings with { RemovedDirectories = [CustomRoot.ToUpperInvariant()] }, true, FileExpectationKind.Missing));
    }

    [Fact(DisplayName = "An unexpectedly missing loader alone stays ready with its issue recorded")]
    public void MissingLoaderAlone()
    {
        var missing = Issue(WorkspaceLayoutIssueCode.MissingLoader);
        var classification = WorkspaceLayoutReader.Classify(true, [missing], Ownership());

        Assert.Equal(WorkspaceLayoutReadState.Ready, classification.State);
        Assert.Equal(missing, Assert.Single(classification.Issues));
    }

    [Theory(DisplayName = "Other layout evidence blocks a missing loader without concealing either issue")]
    [InlineData((int)WorkspaceLayoutIssueCode.ConflictingLoader, (int)WorkspaceLayoutReadState.Mismatch)]
    [InlineData((int)WorkspaceLayoutIssueCode.OwnershipMismatch, (int)WorkspaceLayoutReadState.Mismatch)]
    [InlineData((int)WorkspaceLayoutIssueCode.PointerMismatch, (int)WorkspaceLayoutReadState.Mismatch)]
    [InlineData((int)WorkspaceLayoutIssueCode.UnsafeRoot, (int)WorkspaceLayoutReadState.Invalid)]
    public void MissingLoaderWithOtherLayout(int codeValue, int expectedValue)
    {
        var issues = new[] { Issue(WorkspaceLayoutIssueCode.MissingLoader), Issue((WorkspaceLayoutIssueCode)codeValue) };
        var classification = WorkspaceLayoutReader.Classify(true, issues, Ownership());

        Assert.Equal((WorkspaceLayoutReadState)expectedValue, classification.State);
        Assert.Equal(issues, classification.Issues);
    }

    [Theory(DisplayName = "Invalid and unavailable ownership are recorded without blocking an otherwise ready layout")]
    [InlineData((int)WorkspaceOwnershipReadState.Invalid), InlineData((int)WorkspaceOwnershipReadState.Unavailable)]
    public void OwnershipDoesNotBlockLayout(int stateValue)
    {
        var classification = WorkspaceLayoutReader.Classify(true, [], Ownership() with { State = (WorkspaceOwnershipReadState)stateValue });

        Assert.Equal(WorkspaceLayoutReadState.Ready, classification.State);
        var issue = Assert.Single(classification.Issues);
        Assert.Equal(WorkspaceLayoutIssueCode.ObservationUnavailable, issue.Code);
        Assert.Equal(WorkspaceOwnershipDefinitions.RelativePath, issue.Path);
    }

    [Theory(DisplayName = "A changed observation stays unavailable even when ownership cannot be read")]
    [InlineData((int)WorkspaceOwnershipReadState.Absent), InlineData((int)WorkspaceOwnershipReadState.Invalid), InlineData((int)WorkspaceOwnershipReadState.Unavailable)]
    public void ChangedObservation(int stateValue)
    {
        var changed = Issue(WorkspaceLayoutIssueCode.ObservationUnavailable);
        var classification = WorkspaceLayoutReader.Classify(true, [changed], Ownership() with { State = (WorkspaceOwnershipReadState)stateValue });

        Assert.Equal(WorkspaceLayoutReadState.Unavailable, classification.State);
        Assert.Contains(changed, classification.Issues);
    }

    [Theory(DisplayName = "Unreadable ownership supplies no installation evidence by itself")]
    [InlineData((int)WorkspaceOwnershipReadState.Absent), InlineData((int)WorkspaceOwnershipReadState.Invalid), InlineData((int)WorkspaceOwnershipReadState.Unavailable)]
    public void NoInstallationEvidence(int stateValue)
    {
        var classification = WorkspaceLayoutReader.Classify(false, [], Ownership() with { State = (WorkspaceOwnershipReadState)stateValue });

        Assert.Equal(WorkspaceLayoutReadState.Uninstalled, classification.State);
    }

    private static WorkspaceOwnershipRead Ownership()
        => WorkspaceOwnershipRead.Absent(Path.Combine(AppContext.BaseDirectory, WorkspaceOwnershipDefinitions.RelativePath));

    private static WorkspaceLayoutIssue Issue(WorkspaceLayoutIssueCode code)
        => new(code, CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName, code.ToString(), []);
}
