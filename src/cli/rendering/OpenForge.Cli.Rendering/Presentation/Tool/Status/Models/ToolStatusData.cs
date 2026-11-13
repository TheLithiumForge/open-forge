using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;

namespace OpenForge.Cli.Core.Presentation.Tool.Status.Models;

internal sealed record ToolStatusData(ToolStatusRow[] Tools, ToolCoverage[] Coverage);
