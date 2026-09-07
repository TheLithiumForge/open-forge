using OpenForge.Cli.Core.Commands.Repair.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal enum RepairPlanningActionKind
{
    Pending,
    Effect,
    NoOp,
    Blocked,
}

internal sealed class RepairPlanningAction
{
    private RepairPlanningAction(
        RepairPlanningActionKind kind,
        RepairableReferenceInput? input,
        RepairEffect? effect,
        RepairNoOp? noOp)
    {
        Kind = kind;
        Input = input;
        Effect = effect;
        NoOp = noOp;
    }

    internal RepairPlanningActionKind Kind { get; private set; }

    internal RepairableReferenceInput? Input { get; }

    internal RepairEffect? Effect { get; private set; }

    internal RepairNoOp? NoOp { get; }

    internal static RepairPlanningAction PendingFor(RepairableReferenceInput input)
        => new(RepairPlanningActionKind.Pending, input, effect: null, noOp: null);

    internal static RepairPlanningAction ForNoOp(RepairNoOp noOp)
        => new(RepairPlanningActionKind.NoOp, input: null, effect: null, noOp);

    internal static RepairPlanningAction Blocked()
        => new(RepairPlanningActionKind.Blocked, input: null, effect: null, noOp: null);

    internal void Resolve(RepairEffect effect)
    {
        Effect = effect;
        Kind = RepairPlanningActionKind.Effect;
    }

    internal void Block()
        => Kind = RepairPlanningActionKind.Blocked;
}
