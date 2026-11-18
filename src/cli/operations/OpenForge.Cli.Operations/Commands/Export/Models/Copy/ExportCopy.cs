using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Export.Models.Copy;

/// <summary>
/// Git's list of the project's files, tracked plus untracked and not ignored, as canonical
/// workspace-relative paths, or null when the root is not a usable Git work tree.
/// </summary>
internal delegate ValueTask<IReadOnlyList<string>?> ExportGitFileList(string root, CancellationToken token);

/// <summary>One file in the copy. Null bytes copy the project file byte for byte.</summary>
internal sealed record ExportFile(string Source, string Target, ImmutableArray<byte>? Bytes);

/// <summary>Everything export writes, and the plan entries that change the copy.</summary>
internal sealed record ExportCopy(ImmutableArray<ExportFile> Files, ImmutableArray<StripPlanEntry> Entries, ImmutableArray<string> Linked);
