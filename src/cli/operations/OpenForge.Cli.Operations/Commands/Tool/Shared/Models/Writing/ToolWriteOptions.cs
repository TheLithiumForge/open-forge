using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal sealed record ToolWriteOptions
{
    public bool DryRun { get; init; }
    public bool Automatic { get; init; }
    public bool AllowInteraction { get; init; }
    public bool AllowSessionDaemon { get; init; }
    public string[] AllowPaths { get; init; } = [];
    public CliRetryOptions? RetryOptions { get; init; }
}

internal sealed record ToolComponentChoice(string Id, bool Enabled);
internal enum ToolWriteMode { Preview, Apply }
internal enum ToolWriteBackupMode { Git, Sibling }
internal enum ToolConsentDecision { NotRequired, Missing, Supplied, RecordedLocal }
internal enum ToolVerificationState { NotRun, Verified, Failed, Unavailable }
