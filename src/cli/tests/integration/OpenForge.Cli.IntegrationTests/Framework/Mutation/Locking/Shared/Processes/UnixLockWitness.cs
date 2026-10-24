using System.Diagnostics;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking.Shared.Processes;

internal static class UnixLockWitness
{
    internal const string Held = "held";
    internal const string Acquired = "acquired";
    internal const string Busy = "busy";
    internal const string Writable = "writable";
    internal const string Denied = "denied";
    internal const string ReadOnlyMode = "rb";
    internal const string ReadWriteMode = "r+b";

    private const string HoldScript = $"""
        import fcntl,sys
        f=open(sys.argv[1],sys.argv[2])
        fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB)
        print('{Held}',flush=True)
        sys.stdin.readline()
        """;

    private const string ProbeScript = $"""
        import fcntl,sys
        f=open(sys.argv[1],sys.argv[2])
        try:
         fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB)
         print('{Acquired}')
        except BlockingIOError:
         print('{Busy}')
        """;

    private const string CreateFifoScript = "import os,sys\nos.mkfifo(sys.argv[1])";

    private const string CreateWriteDeniedFifoScript = $"""
        import os,sys
        os.mkfifo(sys.argv[1],0o400)
        try:
         fd=os.open(sys.argv[1],os.O_RDWR|os.O_NONBLOCK)
         os.close(fd)
         print('{Writable}')
        except PermissionError:
         print('{Denied}')
        """;

    internal static Process Hold(string path, bool readOnly = false)
        => Start(HoldScript, path, readOnly ? ReadOnlyMode : ReadWriteMode);

    internal static Process Probe(string path, bool readOnly)
        => Start(ProbeScript, path, readOnly ? ReadOnlyMode : ReadWriteMode);

    internal static Process CreateFifo(string path) => Start(CreateFifoScript, path);

    internal static Process CreateWriteDeniedFifo(string path) => Start(CreateWriteDeniedFifoScript, path);

    private static Process Start(string script, params string[] arguments)
    {
        var start = new ProcessStartInfo("python3")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add(script);
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        return Process.Start(start) ?? throw new InvalidOperationException("The flock witness process did not start.");
    }
}
