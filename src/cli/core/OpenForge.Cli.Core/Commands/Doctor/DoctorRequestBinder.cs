using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.Commands.Doctor;

internal sealed class DoctorRequestBinder
{
    internal CliBindResult<DoctorRequest, DoctorResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException(
                "A bound Doctor invocation requires a selected workspace.");
        return CliBindResult<DoctorRequest, DoctorResult>.Bound(new DoctorRequest(workspace));
    }
}
