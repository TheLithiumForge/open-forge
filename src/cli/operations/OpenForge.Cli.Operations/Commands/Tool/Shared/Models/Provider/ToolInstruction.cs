namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;

internal sealed record ToolInstruction(string? Command, string[]? Arguments,
    Dictionary<string, string>? Environment, string? WorkingDirectory, string? Cause);
