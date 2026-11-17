using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Launchers;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Launchers;

[Trait("Feature", "tool-launchers"), Trait("Evidence", "Unit")]
public sealed class ToolLauncherRenderingTests
{
    [Fact(DisplayName = "Launcher kinds and target kinds cover declared members and reject undefined values")]
    public void CoversLauncherKinds()
    {
        ToolLauncherTargetKind[] targets = [ToolLauncherTargetKind.RuntimeEntry, ToolLauncherTargetKind.ReleaseLauncher, ToolLauncherTargetKind.NativeBinary];
        Assert.Equal(targets, Enum.GetValues<ToolLauncherTargetKind>());
        ToolLauncherKind[] kinds = [ToolLauncherKind.WindowsCmd, ToolLauncherKind.PosixSh, ToolLauncherKind.NativeExecutable];
        Assert.Equal(kinds, Enum.GetValues<ToolLauncherKind>());
        var seed = new ToolLauncherSpec
        {
            ToolId = "sample",
            Kind = ToolLauncherKind.PosixSh,
            TargetKind = ToolLauncherTargetKind.RuntimeEntry,
            LauncherPath = System.IO.Path.Combine(AppContext.BaseDirectory, "sample"),
            EntryPath = System.IO.Path.Combine(AppContext.BaseDirectory, "entry"),
            RuntimePath = System.IO.Path.Combine(AppContext.BaseDirectory, "runtime"),
            Arguments = [],
        };
        foreach (var target in targets) Assert.NotEmpty(ToolLauncherWriter.Render(seed with { TargetKind = target }));
        Assert.NotEmpty(ToolLauncherWriter.Render(seed with { Kind = ToolLauncherKind.WindowsCmd }));
        Assert.Throws<ArgumentException>(() => ToolLauncherWriter.Render(seed with { Kind = ToolLauncherKind.NativeExecutable, TargetKind = ToolLauncherTargetKind.NativeBinary }));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolLauncherWriter.Render(seed with { TargetKind = (ToolLauncherTargetKind)int.MaxValue }));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolLauncherWriter.Render(seed with { Kind = (ToolLauncherKind)int.MaxValue }));
    }
}
