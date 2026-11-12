using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Tool.Status.Models;

internal sealed record ToolStatusSymbols(Command Command, Argument<string?> Id);
