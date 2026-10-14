namespace OpenForge.Cli.Composition.Shared.Git.Models;

internal sealed record GitProcessInvocation(string Root, string[] Arguments, byte[]? StandardInput = null);
