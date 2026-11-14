namespace OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

// Tool backup and coverage probes address their workspace independently of inherited Git target overrides.
internal static class ToolGitEnvironment
{
    internal const string Executable = "git";
    internal static readonly IReadOnlyDictionary<string, string?> Probe = new Dictionary<string, string?>
    {
        ["GIT_OPTIONAL_LOCKS"] = "0",
        ["GIT_DIR"] = null,
        ["GIT_WORK_TREE"] = null,
        ["GIT_COMMON_DIR"] = null,
        ["GIT_INDEX_FILE"] = null,
        ["GIT_CONFIG_COUNT"] = "0",
        ["GIT_CONFIG_PARAMETERS"] = null,
    };
}
