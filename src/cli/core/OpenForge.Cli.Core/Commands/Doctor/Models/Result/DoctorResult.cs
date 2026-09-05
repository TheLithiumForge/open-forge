using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal sealed record DoctorResult : ICliCommandResult
{
    public string Command => DoctorDefinitions.CommandIdentity;

    public required CliSemanticStatus Status { get; init; }

    public required CliWorkspace? Workspace { get; init; }

    public required CliNextAction? Next { get; init; }

    internal required DoctorDiagnosis Diagnosis { get; init; }
}
