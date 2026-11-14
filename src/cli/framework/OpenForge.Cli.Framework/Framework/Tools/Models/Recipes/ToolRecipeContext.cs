namespace OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

internal sealed record ToolRecipeContext
{
    public required string WorkspacePath { get; init; }
    public required string HomePath { get; init; }
    public required string BinPath { get; init; }
    public required string NativeHomePath { get; init; }
    public required string NativeInstallDirectory { get; init; }
    public required string MainRoot { get; init; }
    public required IReadOnlyDictionary<string, string> ClientRoots { get; init; }
    public IReadOnlyDictionary<string, string>? NativeEnvironment { get; init; }
    public IReadOnlyList<string> AdditionalAllowInstallPaths { get; init; } = [];
}

internal static class ToolRecipeDefinitions
{
    internal const string DollarEscapeToken = "$$";
    internal const string WorkspaceToken = "${workspace}";
    internal const string HomeToken = "${openForgeHome}";
    internal const string BinToken = "${bin}";
    internal const string NativeHomeToken = "${nativeHome}";
    internal const string NativeInstallDirectoryToken = "${nativeInstallDirectory}";
    internal const string MainRootToken = "${mainRoot}";
    internal const string ClientTokenPrefix = "${client:";
    internal const string PreviousValueToken = "${previous}";
}
