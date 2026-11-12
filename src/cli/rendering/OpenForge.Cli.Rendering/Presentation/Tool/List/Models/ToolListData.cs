using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;

namespace OpenForge.Cli.Core.Presentation.Tool.List.Models;

internal sealed record ToolListData(ToolAvailableRow[] Available, ToolInstalledRow[] Installed, ToolCatalogueFact Catalogue, ToolCoverage[] Coverage, string? RoleFilter);
