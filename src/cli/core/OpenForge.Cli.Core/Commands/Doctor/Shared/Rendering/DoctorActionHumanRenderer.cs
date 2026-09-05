using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorActionHumanRenderer
{
    internal static void Append(
        StringBuilder builder,
        IReadOnlyList<DoctorNextAction> actions,
        CliView view,
        string indent)
    {
        foreach (var action in actions)
        {
            builder.AppendLine($"{indent}action kind: {DoctorFindingWireVocabulary.Action(action.Kind)}");
            if (action.Operation is { } operation)
            {
                builder.AppendLine($"{indent}  operation: {DoctorFindingWireVocabulary.Operation(operation)}");
            }

            if (action.Command is { } command)
            {
                builder.AppendLine($"{indent}  command: {DoctorHumanRenderer.Text(command)}");
            }

            if (view == CliView.Expanded)
            {
                builder.AppendLine($"{indent}  reason: {DoctorHumanRenderer.Text(action.Reason)}");
            }
        }
    }
}
