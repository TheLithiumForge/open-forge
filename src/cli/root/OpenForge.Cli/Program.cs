using OpenForge.Cli.Hosting;

using var cancellation = new CancellationTokenSource();
ConsoleCancelEventHandler handler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};
Console.CancelKeyPress += handler;
try
{
    return await CliHost.RunAsync(args, cancellation.Token).ConfigureAwait(false);
}
finally
{
    Console.CancelKeyPress -= handler;
}
