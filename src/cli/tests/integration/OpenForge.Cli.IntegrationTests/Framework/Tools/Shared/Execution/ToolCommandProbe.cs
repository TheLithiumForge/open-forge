using System.Text.Json;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Execution;

internal static class ToolCommandProbe
{
    internal const string ChildArgument = "--open-forge-tool-command-probe";
    internal const string Witness = "tool-command-witness.json";
    internal const string StageSuffix = ".stage";
    internal const string EnvironmentName = "OPENFORGE_TOOL_FIXTURE_VALUE";
    internal const string Success = "success";
    internal const string Failure = "failure";
    internal const string Wait = "wait";
    internal const int FailureExit = 23;
    internal const string CwdKey = "cwd";
    internal const string ArgsKey = "args";
    internal const string EnvironmentKey = "environment";
    internal const string PidKey = "pid";

    internal static async Task<int> RunAsync(string[] arguments)
    {
        var stage = arguments[2] + StageSuffix;
        await using (var stream = new FileStream(stage, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
        {
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();
            writer.WriteString(CwdKey, Environment.CurrentDirectory);
            writer.WriteString(EnvironmentKey, Environment.GetEnvironmentVariable(EnvironmentName));
            writer.WriteNumber(PidKey, Environment.ProcessId);
            writer.WriteStartArray(ArgsKey);
            foreach (var argument in arguments.Skip(3)) writer.WriteStringValue(argument);
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.Flush();
        }
        File.Move(stage, arguments[2]);
        Console.Out.WriteLine("Fixture stdout");
        Console.Error.WriteLine("Fixture stderr");
        if (arguments[1] == Wait) await Task.Delay(Timeout.InfiniteTimeSpan);
        return arguments[1] == Failure ? FailureExit : 0;
    }
}
