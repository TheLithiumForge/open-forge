using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Binding;

internal sealed record ToolWriteSymbols
{
    public required ToolWriteKind Kind { get; init; }
    public required Command Command { get; init; }
    public required Argument<string?> Spec { get; init; }
    public Option<string?>? Channel { get; init; }
    public required Option<bool> Yes { get; init; }
    public required Option<bool> DryRun { get; init; }
    public required Option<string[]> AllowPath { get; init; }
}
