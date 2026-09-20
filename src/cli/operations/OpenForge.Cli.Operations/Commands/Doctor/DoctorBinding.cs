using OpenForge.Cli.Core.Commands.Doctor.Models.Binding;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Doctor;

internal static class DoctorBinding
{
    internal static DoctorSymbols CreateSymbols() => DoctorSymbols.Create();

    internal static CliRequestBinding<DoctorRequest, DoctorResult> CreateRequestBinding(
        DoctorSymbols symbols,
        DoctorBindingComponents components)
    {
        var binder = new DoctorRequestBinder();
        return new CliRequestBinding<DoctorRequest, DoctorResult>
        {
            Command = symbols.DoctorCommand,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = binder.Bind,
            InvalidResultFactory = DoctorWorkspaceResultFactory.Create,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
