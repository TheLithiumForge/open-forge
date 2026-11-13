using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Models.Parsing;

internal sealed record ToolRunEnvironment(string CurrentDirectory, string? HostProjectDirectory, bool Interactive);
internal sealed record ToolRunInput(ToolRunRequest? Request, CliGlobalInput? Global, string[] Diagnostics);
