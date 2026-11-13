using OpenForge.Cli.Core.Commands.Tool.Models.Writing;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Interaction;

internal sealed record ToolWriterDisclosure(string Id, string Name, string Origin, string Repository, string License, bool Telemetry);
internal sealed record ToolArtifactQuestion(ToolWriterDisclosure Disclosure, ToolReportArtifact[] Artifacts, ToolWriteReportData Review);
internal sealed record ToolProcessQuestion(ToolWriterDisclosure Disclosure, string ScopeSha256, ToolWriteReportData Review);
internal sealed record ToolComponentQuestion(ToolWriterDisclosure Disclosure, string Id, string Description, bool DefaultEnabled);
internal sealed record ToolFinalQuestion(ToolWriterDisclosure Disclosure, ToolWriteReportData Review);

internal sealed record ToolWriterInteraction(
    CliPrompt<ToolArtifactQuestion, bool> Artifacts,
    CliPrompt<ToolProcessQuestion, bool> Process,
    CliPrompt<ToolComponentQuestion, bool> Component,
    CliPrompt<CliPermissionQuestion, CliPermissionChoice> Permission,
    CliPrompt<ToolFinalQuestion, bool> Confirm)
{
    internal static readonly ToolWriterInteraction Unavailable = new(
        static (_, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Unavailable()),
        static (_, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Unavailable()),
        static (_, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Unavailable()),
        static (_, _, _) => ValueTask.FromResult(CliPromptReply<CliPermissionChoice>.Unavailable()),
        static (_, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Unavailable()));
}
