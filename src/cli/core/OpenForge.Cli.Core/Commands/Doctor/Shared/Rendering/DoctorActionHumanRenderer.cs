using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorActionHumanRenderer
{
    internal static void Append(StringBuilder builder, IReadOnlyList<DoctorNextAction> actions, CliView view, string indent)
    {
        foreach (var action in actions)
        {
            builder.AppendLine($"{indent}Next: {Description(action)}");
            if (view == CliView.Expanded)
            {
                builder.AppendLine($"{indent}  {DoctorHumanRenderer.Text(action.Reason)}");
            }
        }
    }

    private static string Description(DoctorNextAction action)
    {
        var label = action.Kind switch
        {
            DoctorNextActionKind.RepairPreview => "preview the proposed repairs",
            DoctorNextActionKind.AcceptedOperation => "use the indicated command",
            DoctorNextActionKind.FutureOperation => "the required operation is not available yet",
            DoctorNextActionKind.ReviewCandidates => "review possible targets before choosing one",
            DoctorNextActionKind.ManualDecision => "review the issue and decide how to resolve it",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action.Kind, "The action kind is not defined."),
        };
        if (action.Command is { } command)
        {
            return $"{label}: {DoctorHumanRenderer.Text(command)}";
        }

        return action.Operation is { } operation
            ? $"{label} ({DoctorFindingWireVocabulary.Operation(operation)})"
            : label;
    }
}
