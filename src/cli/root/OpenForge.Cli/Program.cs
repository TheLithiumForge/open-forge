using System.Text;
using OpenForge.Cli.Hosting;

using var cancellation = new CancellationTokenSource();
Console.InputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
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
