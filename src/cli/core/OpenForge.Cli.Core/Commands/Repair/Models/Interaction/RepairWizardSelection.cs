using OpenForge.Cli.Core.Commands.Repair.Models.Request;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Interaction;

internal sealed record RepairWizardSelection(
    bool Cancelled,
    IReadOnlyList<RepairRelinkRequest> Relinks);
