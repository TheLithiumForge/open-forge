using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Models.Observation;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class WorkspaceLayoutReadTests
{
    private const string CustomRoot = "docs/forge";
    private const string UnavailableCause = "The recorded boundary is unavailable.";

    [Theory(DisplayName = "Diagnostic observations retain selected layouts independently of writer admission")]
    [InlineData((int)WorkspaceLayoutReadState.Invalid, (int)WorkspaceLayoutIssueCode.UnsafeRoot)]
    [InlineData((int)WorkspaceLayoutReadState.Unavailable, (int)WorkspaceLayoutIssueCode.ObservationUnavailable)]
    [InlineData((int)WorkspaceLayoutReadState.Mismatch, (int)WorkspaceLayoutIssueCode.ConflictingLoader)]
    public void RetainsDiagnosticLayout(int stateValue, int codeValue)
    {
        var state = (WorkspaceLayoutReadState)stateValue;
        var code = (WorkspaceLayoutIssueCode)codeValue;
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, CustomRoot);
        var settings = WorkspaceSettingsRead.Absent(Path.Combine(workspace.LexicalRoot, WorkspaceSettingsDefinitions.RelativePath)) with
        {
            State = WorkspaceSettingsReadState.Complete,
            Document = OpenForge.Cli.Core.Framework.Settings.Models.Document.WorkspaceSettingsDocument.Empty with { DeclaredMainRoot = CustomRoot },
        };
        var ownership = WorkspaceOwnershipRead.Absent(Path.Combine(workspace.LexicalRoot, WorkspaceOwnershipDefinitions.RelativePath)) with
        {
            State = WorkspaceOwnershipReadState.Unavailable,
            Cause = UnavailableCause,
        };
        var issue = new WorkspaceLayoutIssue(code, CustomRoot, UnavailableCause, []);
        var expectation = OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files.FileExpectation.Missing(settings.LogicalPath);
        var read = new WorkspaceLayoutRead(state, settings, ownership, layout, [issue], [expectation]);

        Assert.Equal(state, read.State);
        Assert.Same(layout, read.Layout);
        Assert.Same(settings, read.Settings);
        Assert.Same(ownership, read.Ownership);
        Assert.Same(issue, Assert.Single(read.Issues));
        Assert.Equal(expectation, Assert.Single(read.Expectations));
        Assert.NotEqual(WorkspaceLayoutReadState.Ready, read.State);
        Assert.NotEqual(WorkspaceLayoutReadState.Uninstalled, read.State);
    }

    [Theory(DisplayName = "Invalid and unavailable settings cannot carry a fallback layout")]
    [InlineData(false), InlineData(true)]
    public void RejectsFallback(bool unavailable)
    {
        var state = unavailable ? WorkspaceSettingsReadState.Unavailable : WorkspaceSettingsReadState.Invalid;
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var settings = WorkspaceSettingsRead.Absent(Path.Combine(workspace.LexicalRoot, WorkspaceSettingsDefinitions.DirectoryName, WorkspaceSettingsDefinitions.FileName)) with { State = state };
        var ownership = WorkspaceOwnershipRead.Absent(Path.Combine(workspace.LexicalRoot, WorkspaceOwnershipDefinitions.DirectoryName, WorkspaceOwnershipDefinitions.FileName));
        Assert.Throws<ArgumentException>(() => new WorkspaceLayoutRead(
            WorkspaceLayoutReadState.Invalid, settings, ownership, WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), [], []));
        var read = new WorkspaceLayoutRead(WorkspaceLayoutReadState.Invalid, settings, ownership, null, [], []);
        Assert.Null(read.Layout);
        Assert.Same(settings, read.Settings);
        Assert.Same(ownership, read.Ownership);
    }

    [Fact(DisplayName = "Mismatch observations retain the selected layout and exact diagnostics")]
    public void RetainsMismatchFacts()
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, CustomRoot);
        var settings = WorkspaceSettingsRead.Absent(Path.Combine(workspace.LexicalRoot, WorkspaceSettingsDefinitions.DirectoryName, WorkspaceSettingsDefinitions.FileName));
        var ownership = WorkspaceOwnershipRead.Absent(Path.Combine(workspace.LexicalRoot, WorkspaceOwnershipDefinitions.DirectoryName, WorkspaceOwnershipDefinitions.FileName));
        var issue = new WorkspaceLayoutIssue(WorkspaceLayoutIssueCode.MissingLoader, layout.LoaderPath, "Missing selected loader.", [WorkspaceLayoutNames.AgentHostFileName]);
        var read = new WorkspaceLayoutRead(WorkspaceLayoutReadState.Mismatch, settings, ownership, layout, [issue], []);
        Assert.Same(layout, read.Layout);
        Assert.Same(issue, Assert.Single(read.Issues));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WorkspaceLayoutRead((WorkspaceLayoutReadState)999, settings, ownership, null, [], []));
    }
}
