namespace OpenForge.Cli.Core.Commands.Install.Models.Result;

internal sealed record InstallInvalidRoute(string Value, IReadOnlyList<string> BuiltInIds);
