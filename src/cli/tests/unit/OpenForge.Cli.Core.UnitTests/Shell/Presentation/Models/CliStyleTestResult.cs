using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Shell.Presentation.Models;

internal sealed record CliStyleTestResult(CliSemanticStatus Status) : ICliCommandResult
{
    public string Command => "style-test";
    public CliWorkspace? Workspace => null;
    public CliNextAction? Next => null;
}
