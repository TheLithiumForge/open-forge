using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Planning;

internal sealed record RepairPlanningSelection(
    RepairSelection Selection,
    IReadOnlyList<RepairPlanningSelectionEntry> Selected,
    IReadOnlyList<RepairConflict> Conflicts);

internal sealed record RepairPlanningSelectionEntry(
    RepairableReferenceInput Input,
    RepairSelectedProposal Selected);
