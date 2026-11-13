using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Models.Parsing;

internal sealed record ToolRunSymbols(Command Command, Argument<string> Id);
