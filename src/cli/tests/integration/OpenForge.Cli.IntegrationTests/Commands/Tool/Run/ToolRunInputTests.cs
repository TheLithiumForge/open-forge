using System.IO.Pipes;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Run;

[Trait("Feature", "tool-run"), Trait("Evidence", "Integration")]
public sealed class ToolRunInputTests
{
    private const string InputFile = "parent-input.bin";
    private const int InputLength = 1024 * 1024;

    [Fact(DisplayName = "MCP input keeps reading parent EOF after the child closes its input pipe")]
    public async Task DrainsAfterChildClosesInput()
    {
        using var temporary = TemporaryWorkspace.Create("tool-run-input");
        await using var source = File.OpenRead(temporary.CreateFile(InputFile, new byte[InputLength]));
        using var destination = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.None);
        destination.DisposeLocalCopyOfClientHandle();
        // There is no reader. A real OS write fails, so the relay must drain the parent input separately.
        using var signals = new ToolDispatchSignals();
        await ToolDispatchInput.RelayAsync(source, destination, signals, TestContext.Current.CancellationToken);
        Assert.Equal(InputLength, source.Position);
    }
}
