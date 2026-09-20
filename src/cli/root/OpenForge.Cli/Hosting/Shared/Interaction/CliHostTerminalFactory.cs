using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Hosting.Shared.Interaction;

internal static class CliHostTerminalFactory
{
    internal static CliTerminal Create(
        TextReader standardInput,
        TextWriter promptOutput,
        bool standardInputRedirected,
        bool promptOutputRedirected,
        string? terminalName,
        bool isWindows)
    {
        ArgumentNullException.ThrowIfNull(standardInput);
        ArgumentNullException.ThrowIfNull(promptOutput);

        var canPrompt = !standardInputRedirected && !promptOutputRedirected;
        var supportsKeyReads = canPrompt && SupportsKeyReads();
        var capabilities = ResolveCapabilities(
            standardInputRedirected,
            promptOutputRedirected,
            terminalName,
            isWindows,
            supportsKeyReads);
        var adapter = new CliHostTerminalAdapter(
            standardInput,
            promptOutput,
            () => Console.KeyAvailable,
            () => Console.ReadKey(intercept: true));
        return new CliTerminal(capabilities, adapter.WriteAsync, adapter.ReadLineAsync, adapter.ReadKeyAsync);
    }

    internal static CliTerminalCapabilities ResolveCapabilities(
        bool standardInputRedirected,
        bool promptOutputRedirected,
        string? terminalName,
        bool isWindows,
        bool supportsKeyReads)
    {
        var canPrompt = !standardInputRedirected && !promptOutputRedirected;
        var canReadKeys = canPrompt && !string.Equals(terminalName, "dumb", StringComparison.OrdinalIgnoreCase)
            && supportsKeyReads;
        var canRedraw = canReadKeys && SupportsAnsiRedraw(terminalName, isWindows);
        return new CliTerminalCapabilities(canPrompt, canReadKeys, canRedraw);
    }

    private static bool SupportsKeyReads()
    {
        try
        {
            _ = Console.KeyAvailable;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    private static bool SupportsAnsiRedraw(string? terminalName, bool isWindows)
    {
        if (isWindows || string.IsNullOrWhiteSpace(terminalName))
            return false;

        return terminalName.ToLowerInvariant() switch
        {
            "ansi" or "linux" or "screen" or "screen-256color" or "tmux" or "tmux-256color"
                or "vt100" or "xterm" or "xterm-256color" or "rxvt" or "rxvt-unicode" => true,
            _ => false,
        };
    }
}
