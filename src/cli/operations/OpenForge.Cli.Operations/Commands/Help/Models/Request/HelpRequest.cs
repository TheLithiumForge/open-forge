namespace OpenForge.Cli.Core.Commands.Help.Models.Request;

internal sealed record HelpRequest(string? Topic, bool NamesCommand = false);
