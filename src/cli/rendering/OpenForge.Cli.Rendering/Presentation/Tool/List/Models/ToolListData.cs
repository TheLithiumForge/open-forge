using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Models.Inventory;

namespace OpenForge.Cli.Core.Presentation.Tool.List.Models;

internal sealed record ToolListData(ToolAvailableRow[] Available, ToolInstalledRow[] Installed, ToolCatalogueFact Catalogue, ToolCoverage[] Coverage, string? RoleFilter);
