namespace OpenForge.Cli.Core.Commands.Tool.Models.Provider;

internal sealed record ToolInstruction(string? Command, string[]? Arguments,
    Dictionary<string, string>? Environment, string? WorkingDirectory, string? Cause);
