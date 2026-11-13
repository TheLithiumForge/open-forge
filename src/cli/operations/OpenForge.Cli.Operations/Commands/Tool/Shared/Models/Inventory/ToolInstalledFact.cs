using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;

internal sealed record ToolInstalledFact(ToolInstalledRow? Record, ToolObservation Observation);
