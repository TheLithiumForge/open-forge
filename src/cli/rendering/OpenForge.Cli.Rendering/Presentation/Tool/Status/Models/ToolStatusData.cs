using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Models.Provider;

namespace OpenForge.Cli.Core.Presentation.Tool.Status.Models;

internal sealed record ToolStatusData(ToolStatusRow[] Tools, ToolCoverage[] Coverage);
