using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Selection;

public sealed class CliNextActionCompletionTests
{
    [Fact, Trait("Feature", "command-retry"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void GuidedRetryKeepsTheRequestAndPresentationWithoutAutomaticOrPreview()
    {
        const string interactive = "open-forge repair --relink .agents/source.md@3:8 old.md .agents/next.md --workspace 'other workspace' --format json --detail full";
        var replay = new CliRequestReplay("open-forge repair", "open-forge repair", interactive + " --automatic --dry-run", "debug", "automatic")
        { InteractiveCommand = interactive, WorkspaceSpecified = true };
        var report = new CliReport<string>
        {
            Command = "repair",
            Status = CliSemanticStatus.Attention,
            Data = "data",
            Headline = new("Choice needed.", CliHeadlineKind.Warnings),
            Workspace = new("other workspace", true),
            Next = new("open-forge repair", "Choose the intended target.") { UsesInteractiveRequest = true },
        };
        Assert.Equal(interactive, CliNextActionCompletion.Complete(report, replay).Next?.Command);
    }
    [Fact(DisplayName = "Selected-removal apply actions preserve the request without restoring preview mode")]
    [Trait("Feature", "command-retry"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ApplyActionPreservesRequest()
    {
        var report = new CliReport<string>
        {
            Command = "extension remove",
            Status = CliSemanticStatus.Attention,
            Data = "data",
            Headline = new("Would remove the toolkit Extension.", CliHeadlineKind.Warnings),
            Workspace = new("other workspace", true),
            Next = new("open-forge extension remove toolkit", "Apply the selected removal.") { AppliesRequest = true },
        };
        const string apply = "open-forge extension remove toolkit --workspace 'other workspace' --allow-path 'notes folder' --format json";
        var replay = new CliRequestReplay("open-forge extension remove", "open-forge extension remove toolkit",
            $"{apply} --dry-run", "debug", "automatic")
        { ApplyCommand = apply, WorkspaceSpecified = true };
        var completed = CliNextActionCompletion.Complete(report, replay);
        Assert.Equal(apply, completed.Next?.Command);
        Assert.Equal(completed.Next, CliNextActionCompletion.Complete(completed, replay).Next);
    }

    [Theory(DisplayName = "Exact-source next actions retain inspection options and workspace selection")]
    [InlineData(""), InlineData(" --for <path>")]
    [Trait("Feature", "command-retry"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ExactSourceRetainsOptions(string suffix)
    {
        var report = new CliReport<string>
        {
            Command = "route inspect",
            Status = CliSemanticStatus.Blocked,
            Data = "data",
            Headline = new("Source needs an exact path.", CliHeadlineKind.CannotStart),
            Workspace = new("other workspace", true),
            Next = new($"open-forge route inspect .agents/guide.md{suffix}", "Select the exact source."),
        };
        var replay = new CliRequestReplay("open-forge route inspect", "open-forge route inspect guide", "request", "debug", "automatic")
        { OptionArguments = "--workspace 'other workspace' --matching-files --format json --detail full", WorkspaceSpecified = true };
        var next = CliNextActionCompletion.Complete(report, replay).Next;
        Assert.Equal($"open-forge route inspect .agents/guide.md {replay.OptionArguments}{suffix}", next?.Command);
    }

    [Theory(DisplayName = "Literal retry completion preserves the request in both the final next action and finding actions")]
    [InlineData("", "--force --dry-run --format json"), InlineData(" --detail debug", "--force --dry-run --format json --detail debug")]
    [InlineData(" --automatic", "--force --dry-run --format json --automatic")]
    [Trait("Feature", "command-retry"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void LiteralRetryRetainsRequest(string suffix, string expected)
    {
        var action = new CliNextAction($"open-forge references{suffix}", "Retry the same request.");
        var report = new CliReport<string>
        {
            Command = "references",
            Status = CliSemanticStatus.Interrupted,
            Data = "data",
            Headline = new("Cancelled.", CliHeadlineKind.Cancelled),
            Workspace = new("workspace folder", true),
            Next = action,
            Findings = [new CliFinding { Code = "references.interrupted", Title = "Cancelled", Message = "Cancelled.", Severity = CliSeverity.Error,
                Subject = new(CliSubjectKind.Source, Id: "guide"), Actions = [action] }],
        };
        var replay = new CliRequestReplay("open-forge references", "open-forge references guide",
            "open-forge references guide --force --dry-run --format json", "open-forge references guide --force --dry-run --format json --detail debug",
            "open-forge references guide --force --dry-run --format json --automatic");
        var selected = CliNextActionCompletion.Complete(report, replay);
        Assert.Equal($"open-forge references guide {expected} --workspace 'workspace folder'", selected.Next?.Command);
        Assert.Equal(selected.Next, Assert.Single(selected.Findings[0].Actions));
        Assert.Equal(selected.Next, CliNextActionCompletion.Complete(selected, replay).Next);
    }
}
