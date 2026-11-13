using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal sealed record ToolWriterState(WorkspaceSettingsRead Settings, WorkspaceOwnershipRead Lock, WorkspaceLayout Layout);
internal sealed record ToolWriterStoreRead(ToolStoreRead Registry, string InstallKey, ToolStoredUnit? Install, bool Verified);
