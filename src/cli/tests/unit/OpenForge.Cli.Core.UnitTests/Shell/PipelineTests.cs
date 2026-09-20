using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Shell;

public sealed class PipelineTests
{
    [Fact(DisplayName = "CLI operation validates cancellation before effects"), Trait("Feature", "cli-pipeline"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public async Task OperationStageValidatesCancellationBeforeInvokingOperation()
    {
        var calls = 0;
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CliOperationStage.InvokeAsync(new CliOperationRequest<string>("request"), (request, token) =>
            {
                calls++;
                return ValueTask.FromResult(Result(CliSemanticStatus.Complete));
            }, cancellation.Token));
        Assert.Equal(0, calls);
    }

    private static TestResult Result(CliSemanticStatus status) => new("test", status, null, null);
    private sealed record TestResult(string Command, CliSemanticStatus Status, CliWorkspace? Workspace, CliNextAction? Next) : ICliCommandResult;
}
