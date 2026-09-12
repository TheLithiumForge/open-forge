using System.CommandLine;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Binding;

internal sealed record DoctorSymbols(Command DoctorCommand)
{
    internal static DoctorSymbols Create()
        => new(new Command(
            DoctorDefinitions.DoctorCommand.Name,
            DoctorDefinitions.DoctorCommand.Description));
}

internal sealed class DoctorBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required DoctorOperation Operation { get; init; }

    public required CliRendererSet<DoctorResult> Renderers { get; init; }

    public CliDiagnosticRenderer<DoctorResult>? DiagnosticRenderer { get; init; }
}
