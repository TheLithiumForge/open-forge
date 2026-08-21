namespace OpenForge.Cli.Core.Shell.Definitions;

internal enum CliTerminalMode
{
    None,
    Help,
    Version,
}

internal static class CliTerminalPolicy
{
    internal static CliTerminalMode Resolve(bool help, bool version)
    {
        if (help && version)
        {
            throw new ArgumentException("--help and --version are mutually exclusive.");
        }

        if (help)
        {
            return CliTerminalMode.Help;
        }

        return version ? CliTerminalMode.Version : CliTerminalMode.None;
    }

    internal static void Validate(CliTerminalMode mode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "The terminal mode is not defined.");
        }
    }
}
