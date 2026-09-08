namespace OpenForge.Cli.Core.Commands.Library.Models.Planning;

internal enum LibraryGeneratedNavigationIssueState
{
    Incomplete,
    Blocked,
}

internal sealed record LibraryGeneratedNavigationIssue(
    LibraryGeneratedNavigationIssueState State,
    string? Path,
    string Cause);
