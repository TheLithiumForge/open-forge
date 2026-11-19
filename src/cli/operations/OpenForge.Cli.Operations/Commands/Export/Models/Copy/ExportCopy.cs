using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Export.Models.Copy;

/// <summary>
/// The project's files as canonical workspace-relative paths: Git's list, tracked plus untracked and not ignored,
/// or the .gitignore-aware walk outside Git. Null when the files cannot be listed.
/// </summary>
internal delegate ValueTask<IReadOnlyList<string>?> ExportFileList(string root, CancellationToken token);

/// <summary>One file in the copy. Null bytes copy the project file byte for byte.</summary>
internal sealed record ExportFile(string Source, string Target, ImmutableArray<byte>? Bytes);

/// <summary>Everything export writes, and the plan entries that change the copy.</summary>
internal sealed record ExportCopy(ImmutableArray<ExportFile> Files, ImmutableArray<StripPlanEntry> Entries, ImmutableArray<string> Linked);
