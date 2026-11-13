using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.Tools;

internal sealed class ToolMissingReportSeed
{
    internal CliWorkspace Workspace { get; } = new(Path.GetFullPath("tool-missing-workspace"), Path.GetFullPath("tool-missing-workspace"), CliWorkspaceSelectionMethod.CurrentDirectory);
    internal ToolRunFailure Failure { get; }
    internal string Id => Failure.Id;

    internal ToolMissingReportSeed()
        => Failure = new(ToolRunFailureCode.DeclaredToolMissing, ToolFixtureSource.Id, "fixture", true) { Workspace = Workspace };
}
