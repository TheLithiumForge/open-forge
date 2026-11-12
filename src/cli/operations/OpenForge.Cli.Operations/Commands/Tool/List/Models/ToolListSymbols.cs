using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Tool.List.Models;

internal sealed record ToolListSymbols(Command Command, Option<string?> Source, Option<string?> Role);
