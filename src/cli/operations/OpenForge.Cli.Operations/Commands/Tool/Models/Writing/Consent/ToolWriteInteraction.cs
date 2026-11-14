using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;

internal sealed record ToolWriteShellFileView(string Path, bool Fish);
internal sealed record ToolWritePathAnswer(bool AddForMe, IReadOnlyList<ToolWriteShellFileView> ShellFiles);
internal sealed record ToolWritePathQuestion(string BinPath, IReadOnlyList<ToolWriteShellFileView> Files, bool UsesShellFiles = true);

internal sealed record ToolWriteInteraction
{
    public required bool CanPrompt { get; init; }
    public required CliPlanConfirmation<ToolWriteResult, ToolWriteConsentQuestion> ConfirmPlan { get; init; }
    public required CliPrompt<ToolWriteConsentQuestion, bool> ConfirmOptional { get; init; }
    public required CliPrompt<ToolWriteManagerQuestion, int> ChooseManager { get; init; }
    public required CliPrompt<ToolWritePathQuestion, ToolWritePathAnswer> ChoosePath { get; init; }
}
