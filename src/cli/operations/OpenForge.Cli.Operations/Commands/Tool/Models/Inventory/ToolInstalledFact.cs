using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Inventory;

internal sealed record ToolInstalledFact(ToolInstalledRow? Record, ToolObservation Observation);
