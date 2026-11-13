using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Models.Resolution;

internal sealed record ToolRunWorkspaceRecords(WorkspaceSettingsRead Settings, WorkspaceOwnershipRead Ownership);
