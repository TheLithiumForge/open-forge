using OpenForge.Cli.Core.Commands.Doctor.Models.Result;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Observation;

internal sealed record DoctorDiagnosisRead(
    DoctorObservation Observation,
    DoctorResult Result);
