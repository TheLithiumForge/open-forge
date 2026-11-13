using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace OpenForge.Cli.TestSupport.Tools.Run;

// A separate module lets callbacks run while the Integration module's probe initializer waits.
public static class ToolRunChild
{
    public const string EnvironmentName = "OPENFORGE_RUN_FIXTURE_VALUE";
    public const string EnvironmentValue = "verified child environment";
    public const string WitnessVariable = "OPENFORGE_RUN_FIXTURE_WITNESS";
    public const string Witness = "dispatch-witness.json";
    public const string ArgsKey = "arguments";
    public const string CwdKey = "cwd";
    public const string EnvironmentKey = "environment";
    public const string PidKey = "pid";
    public const string DescendantKey = "descendant";
    public const string ExitZero = "--fixture-exit-zero";
    public const string Hold = "--fixture-hold";
    public const string Release = "dispatch-release";
    public const int ChildExit = 23;
    public const int InterruptExit = 37;
    public static readonly byte[] ErrorBytes = [255, 0, 13, 10, 127, 1];
    public static readonly byte[] ReadyBytes = [1, 2, 3, 4];
    public static readonly byte[] InterruptBytes = [5, 6, 7, 8];

    public static int Run(string[] arguments, string executable, string[] descendantArguments)
    {
        var hold = arguments.Contains(Hold, StringComparer.Ordinal);
        using var interrupted = new ManualResetEventSlim();
        ConsoleCancelEventHandler cancel = (_, args) => { args.Cancel = true; interrupted.Set(); };
        Console.CancelKeyPress += cancel;
        using var interrupt = OperatingSystem.IsWindows() ? null
            : PosixSignalRegistration.Create(PosixSignal.SIGINT, context => { context.Cancel = true; interrupted.Set(); });
        using var quit = OperatingSystem.IsWindows() ? null
            : PosixSignalRegistration.Create(PosixSignal.SIGQUIT, context => { context.Cancel = true; interrupted.Set(); });
        using var descendant = hold ? StartDescendant(executable, descendantArguments) : null;
        try
        {
            var witness = Environment.GetEnvironmentVariable(WitnessVariable) ?? throw new InvalidOperationException("No fixture witness path.");
            using (var stream = File.Create(witness))
            {
                using var writer = new Utf8JsonWriter(stream);
                writer.WriteStartObject();
                writer.WriteString(CwdKey, Environment.CurrentDirectory);
                writer.WriteString(EnvironmentKey, Environment.GetEnvironmentVariable(EnvironmentName));
                writer.WriteNumber(PidKey, Environment.ProcessId);
                if (descendant is not null) writer.WriteNumber(DescendantKey, descendant.Id);
                writer.WriteStartArray(ArgsKey);
                foreach (var argument in arguments) writer.WriteStringValue(argument);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            if (!hold)
            {
                Console.OpenStandardError().Write(ErrorBytes);
                Console.OpenStandardInput().CopyTo(Console.OpenStandardOutput());
                return arguments.Contains(ExitZero, StringComparer.Ordinal) ? 0 : ChildExit;
            }
            Console.OpenStandardOutput().Write(ReadyBytes);
            var release = Path.Combine(Path.GetDirectoryName(witness)!, Release);
            // The test controls release. EOF alone never ends this synthetic server.
            while (!interrupted.Wait(TimeSpan.FromMilliseconds(20)))
                if (File.Exists(release)) return ChildExit;
            Console.OpenStandardOutput().Write(InterruptBytes);
            return InterruptExit;
        }
        finally
        {
            Console.CancelKeyPress -= cancel;
            if (descendant is not null && !descendant.HasExited) { descendant.Kill(entireProcessTree: true); descendant.WaitForExit(); }
        }
    }

    private static Process StartDescendant(string executable, string[] arguments)
    {
        var start = new ProcessStartInfo(executable) { UseShellExecute = false };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        return Process.Start(start) ?? throw new InvalidOperationException("The synthetic descendant didn't start.");
    }
}
