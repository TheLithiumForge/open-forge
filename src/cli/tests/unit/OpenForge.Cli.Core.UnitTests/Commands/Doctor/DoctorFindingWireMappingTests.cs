using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorFindingWireMappingTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Doctor maps every finite finding kind to its exact wire name and rejects undefined values"), Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void FindingKindsMapToExactWireNamesAndUndefinedValuesFailClosed()
    {
        var expected = Enum.GetValues<DoctorFindingKind>();
        Assert.Equal(expected.Length, expected.Select(DoctorDefinitions.ReadFindingKind).Distinct(StringComparer.Ordinal).Count());
        foreach (var kind in expected)
        {
            Assert.Equal(DoctorDefinitions.ReadFindingKind(kind), DoctorWording.MachineCode(kind));
            Assert.DoesNotContain("candidate", DoctorWording.MachineCode(kind), StringComparison.Ordinal);
        }

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DoctorDefinitions.ReadFindingKind((DoctorFindingKind)int.MaxValue));
    }
}
