namespace OpenForge.Cli.Core.Commands.Shared.Models.Mutation;

internal sealed record WorkspaceLockFailureInfo(WorkspaceLockFailureReason Reason, string Path);
