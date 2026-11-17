using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups.Models;

internal sealed record ToolPreparedBackup(CliWorkspace Scope, ToolBackupResult Result, FileStateSnapshot Original, FileStateSnapshot? Sibling);
