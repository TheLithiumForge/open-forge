using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

internal static class LibraryReadPresentationAssertions
{
    internal static void Members(JsonElement value, params string[] expected)
        => Assert.Equal(expected, value.EnumerateObject().Select(property => property.Name));

    internal static void Envelope(JsonElement root, string command, string status)
    {
        Members(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(command, root.GetProperty("command").GetString());
        Assert.Equal(status, root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Object, root.GetProperty("result").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        if (root.GetProperty("workspace").ValueKind != JsonValueKind.Null)
        {
            Members(root.GetProperty("workspace"), "path", "selectedBy");
            Assert.Equal(LibraryReadInputs.Workspace.LexicalRoot, root.GetProperty("workspace").GetProperty("path").GetString());
            Assert.Equal("explicit-workspace", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        }
    }

    internal static async Task StreamsAsync<TResult>(TResult result, CliRendererSet<TResult> renderers, string status, int exit)
        where TResult : ICliCommandResult
    {
        // Result injection closes the unexpected-event boundary without inventing an unsafe OS failure.
        var calls = 0;
        var pipeline = new CliCommandPipeline<int, TResult>((_, _) =>
        {
            calls++;
            return ValueTask.FromResult(result);
        }, renderers);
        foreach (var format in new[] { CliOutputFormat.Human, CliOutputFormat.Json })
        {
            using var output = new StringWriter(CultureInfo.InvariantCulture);
            using var error = new StringWriter(CultureInfo.InvariantCulture);
            var completion = await pipeline.ExecuteAsync(
                0,
                new CliPresentation(format, CliView.Expanded, CliVerbosity.Normal),
                new CliOutputWriters(output, error),
                TestContext.Current.CancellationToken);
            Assert.Equal(exit, completion.ExitCode);
            Assert.Equal(result.Status, completion.Status);
            var stdout = format == CliOutputFormat.Json || status is "complete" or "attention" or "incomplete";
            Assert.Equal(stdout ? CliOutputTarget.StandardOutput : CliOutputTarget.StandardError, completion.PrimaryOutputTarget);
            Assert.Empty(stdout ? error.ToString() : output.ToString());
            var primary = stdout ? output.ToString() : error.ToString();
            Assert.NotEmpty(primary);
            if (format == CliOutputFormat.Json)
            {
                using var json = JsonDocument.Parse(primary);
                Envelope(json.RootElement, result.Command, status);
            }
            else
            {
                Assert.Contains(status, primary, StringComparison.Ordinal);
            }
        }

        Assert.Equal(2, calls);
    }
}
