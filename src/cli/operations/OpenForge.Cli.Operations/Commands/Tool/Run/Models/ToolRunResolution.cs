using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Commands.Tool.Run.Shared.Reporting;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Models;

internal enum ToolRunFailureCode
{
    InvalidInput, ToolUndeclared, DeclaredToolMissing, PinMismatch, WorkspaceUnavailable,
    VerificationUnavailable, LaunchIntegrityMismatch, StartFailed, Cancelled,
}

internal sealed record ToolRunFailure(ToolRunFailureCode Code, string Id, string Cause, bool OfferInstall)
{
    public required CliWorkspace? Workspace { get; init; }
    public int ExitCode => ToolRunFailureCodes.ExitCode(Code);
}
internal abstract record ToolRunResolution;
internal sealed record ToolRunReady(ToolDispatchRequest Dispatch) : ToolRunResolution;
internal sealed record ToolRunRefused(ToolRunFailure Failure) : ToolRunResolution;

internal delegate ValueTask<ToolRunResolution> ToolResolveRun(ToolRunRequest request, CancellationToken token);
