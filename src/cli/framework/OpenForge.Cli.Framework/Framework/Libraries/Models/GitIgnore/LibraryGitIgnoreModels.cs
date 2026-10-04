using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.GitIgnore;

internal sealed record LibraryGitIgnoreSectionSpan(int Start, int EndExclusive, int BodyStart, int BodyEnd, string LineEnding);

internal sealed record LibraryGitIgnorePlan(PlannedFileChange Change, FileStateSnapshot Before, ImmutableArray<string> Paths);

internal enum LibraryGitIgnoreReadState
{
    Complete,
    Blocked,
    Unavailable,
}

internal sealed record LibraryGitIgnoreRead(LibraryGitIgnoreReadState State, LibraryGitIgnorePlan? Plan, string? Cause);

internal sealed record LibraryGitIgnoreRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required LibraryRegistrationSet? Current { get; init; }
    public required LibraryRegistrationSet? Intended { get; init; }
    public required bool IsExcluded { get; init; }
    public required bool IsRemoval { get; init; }
}
