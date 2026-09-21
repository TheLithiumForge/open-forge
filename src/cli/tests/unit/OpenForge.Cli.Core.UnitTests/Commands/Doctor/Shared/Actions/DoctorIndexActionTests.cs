using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Actions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Actions;

public sealed class DoctorIndexActionTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Doctor Index actions use the portable command spelling for supported paths")]
    [InlineData(
        ".agents/skills/use-workflow/references/_references.md",
        "open-forge index .agents/skills/use-workflow/references/_references.md")]
    [InlineData(
        ".agents/catalogue with spaces/_references.md",
        "open-forge index \".agents/catalogue with spaces/_references.md\"")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void SupportedPathsProducePortableCommand(string path, string expectedCommand)
    {
        var action = DoctorIndexAction.ForPath(path);

        Assert.Equal(DoctorNextActionKind.AcceptedOperation, action.Kind);
        Assert.Equal(DoctorNextOperation.Index, action.Operation);
        Assert.Equal(expectedCommand, action.Command);
        Assert.Equal("Index owns deterministic generated-navigation projection.", action.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Doctor Index actions retain unsupported paths for manual instructions")]
    [InlineData(".agents/owner's/_references.md")]
    [InlineData(".agents/owner\"/_references.md")]
    [InlineData(".agents/$team/_references.md")]
    [InlineData(".agents/`team`/_references.md")]
    [InlineData(".agents/100%/_references.md")]
    [InlineData(".agents/team\u0001/_references.md")]
    [InlineData("-catalogue.md")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void UnsupportedPathsHaveNoCommand(string path)
    {
        var action = DoctorIndexAction.ForPath(path);

        Assert.Equal(DoctorNextActionKind.AcceptedOperation, action.Kind);
        Assert.Equal(DoctorNextOperation.Index, action.Operation);
        Assert.Null(action.Command);
        Assert.Equal("Index owns deterministic generated-navigation projection.", action.Reason);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Doctor Index actions reject blank paths")]
    [InlineData("")]
    [InlineData(" ")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void BlankPathsAreRejected(string path)
        => Assert.Throws<ArgumentException>(() => DoctorIndexAction.ForPath(path));
}
