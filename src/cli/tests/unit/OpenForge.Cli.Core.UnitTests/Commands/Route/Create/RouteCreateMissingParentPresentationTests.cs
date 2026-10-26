using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

[Trait("Feature", "route-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed class RouteCreateMissingParentPresentationTests
{
    private const string OutsideAnchor = SourceWorkspaceRelativePath.LiteralReferencePrefix + "docs/team/_team" + SourceFormClassifier.MarkdownExtension;
    [Theory(DisplayName = "Create missing-parent guidance parses as the projected outside anchor or unchanged primary short ID")]
    [InlineData(null), InlineData(OutsideAnchor)]
    public void NextUsesTheCommandOwnedInitOperand(string? anchor)
    {
        var formation = RouteCreateTestData.PreviewFormation() with
        {
            Workspace = null,
            Target = RouteCreateTestData.PreviewFormation().Target with { MissingParentEntrypoint = anchor },
            Effects = [],
            Findings = [RouteCreateTestData.Finding(RouteCreateFindingCode.ParentMissing)],
        };
        var report = RouteCreateReportSelector.Select(RouteCreateTestData.Result(formation), new CliSelection(CliDetail.Standard));
        var next = Assert.IsType<OpenForge.Cli.Core.Shell.Pipeline.Models.Operation.CliNextAction>(report.Next);
        var command = new Command(CliSyntaxDefinitions.ExecutableName);
        var route = RouteBinding.CreateGroup();
        command.Subcommands.Add(route);
        var symbols = RouteInitBinding.CreateSymbols(route);
        var workspace = new Option<string?>(CliSyntaxDefinitions.Workspace.Name) { Recursive = true };
        command.Options.Add(workspace);
        var parsed = command.Parse(next.Command);
        Assert.Empty(parsed.Errors);
        Assert.Same(symbols.InitCommand, parsed.CommandResult.Command);
        Assert.Equal(anchor ?? RouteCreateTestData.ParentId, parsed.GetValue(symbols.RouteTarget));
        Assert.Null(parsed.GetValue(workspace));
    }
}
