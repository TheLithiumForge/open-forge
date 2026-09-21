using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Shared;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Actions;

internal static class DoctorIndexAction
{
    private const string OperationReason = "Index owns deterministic generated-navigation projection.";

    internal static DoctorNextAction ForPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var command = IsPortablePath(path)
            ? $"{CommandLines.Index} {CommandArgument(path)}"
            : null;
        return new DoctorNextAction
        {
            Kind = DoctorNextActionKind.AcceptedOperation,
            Operation = DoctorNextOperation.Index,
            Command = command,
            Reason = OperationReason,
        };
    }

    private static string CommandArgument(string path)
        => path.Contains(' ') ? $"\"{path}\"" : path;

    private static bool IsPortablePath(string path)
    {
        if (path[0] == '-')
        {
            return false;
        }

        foreach (var character in path)
        {
            if (character is (>= 'A' and <= 'Z')
                or (>= 'a' and <= 'z')
                or (>= '0' and <= '9')
                or '/'
                or '.'
                or '_'
                or '-'
                or ' ')
            {
                continue;
            }

            return false;
        }

        return true;
    }
}
