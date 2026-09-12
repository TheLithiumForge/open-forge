using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal sealed record DoctorResult : ICliCommandResult
{
    public string Command => DoctorDefinitions.CommandIdentity;

    public required CliSemanticStatus Status { get; init; }

    public required CliWorkspace? Workspace { get; init; }

    public required CliNextAction? Next { get; init; }

    internal required DoctorDiagnosis Diagnosis { get; init; }
}
