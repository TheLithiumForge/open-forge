using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Tool.Inspect.Models;

internal sealed record ToolInspectSymbols(Command Command, Argument<string?> Id, Option<string?> Source, Option<string?> Version);
