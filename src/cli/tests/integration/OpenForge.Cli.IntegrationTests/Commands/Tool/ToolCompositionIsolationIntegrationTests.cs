using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

public sealed class ToolCompositionIsolationIntegrationTests
{
    public static TheoryData<string, bool> Cases
    {
        get
        {
            var result = new TheoryData<string, bool>();
            foreach (var root in new[] { false, true })
                foreach (var command in new[] { "--help", "tool", "tool --help", "tool list --help", "tool inspect --help", "tool status --help", "tool install --help", "tool update --help", "tool remove --help", "--version", "tool inspect unknown-tool", "tool status unknown-tool", "tool install unknown-tool --yes", "tool install alpha@1.x --yes", "tool install --channel missing", "tool install <known> --channel missing", "help" })
                    result.Add(command, root);
            return result;
        }
    }

    [Theory(DisplayName = "Inert composed invocations never resolve invalid homes or call any Tool host"), MemberData(nameof(Cases)), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task InertInvocations(string command, bool root)
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        var before = fixture.Snapshot();
        var known = root ? CodeGraphDefinitions.Id : ToolCompositionFixture.FirstId;
        var arguments = command.Replace("<known>", known, StringComparison.Ordinal).Split(' ');
        var run = await fixture.RunAsync(arguments, fullRoot: root);
        Assert.True(run.ExitCode is 0 or 4);
        Assert.Equal(before, fixture.Snapshot());
        Assert.Equal(0, fixture.ResolveCalls);
        Assert.Equal(0, fixture.PathObservations);
        Assert.Equal(0, fixture.PathEffects);
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Empty(fixture.Events);
        Assert.False(Directory.Exists(fixture.Home));
    }
    [Theory(DisplayName = "Unrelated read operations never construct the Tool graph"), InlineData("find --tag Synthetic"), InlineData("context"), InlineData("route list root"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task UnrelatedOperations(string command)
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(command.Split(' '), fullRoot: true);
        Assert.True(run.ExitCode is 0 or 3 or 4 or 5);
        Assert.Equal(before, fixture.Snapshot());
        Assert.Equal(0, fixture.ResolveCalls);
        Assert.Equal(0, fixture.PathObservations);
        Assert.False(Directory.Exists(fixture.Home));
    }

}
