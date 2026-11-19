namespace OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;

internal static class SourceCommentSyntax
{
    private static readonly string[] SlashLines = ["//"];
    private static readonly string[] HashLines = ["#"];
    private static readonly string[] DashLines = ["--"];
    private static readonly string[] ApostropheLines = ["'"];
    private static readonly string[] PercentLines = ["%"];
    private static readonly string[] SemicolonLines = [";"];
    private static readonly string[] IniLines = [";", "#"];
    private static readonly string[] PhpLines = ["//", "#"];
    private static readonly string[] CommonLines = ["//", "#", "--", ";", "'", "%"];

    internal static (IReadOnlyList<string> Lines, bool SlashBlocks, bool HtmlBlocks) Read(string path)
        => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".cs" or ".csx" or ".js" or ".jsx" or ".mjs" or ".cjs" or ".ts" or ".tsx"
                or ".c" or ".h" or ".cpp" or ".hpp" or ".cc" or ".java" or ".kt" or ".kts"
                or ".go" or ".rs" or ".swift" or ".scala" or ".dart" => (SlashLines, true, false),
            ".py" or ".pyw" or ".rb" or ".sh" or ".bash" or ".zsh" or ".ps1" or ".psm1"
                or ".yaml" or ".yml" or ".toml" or ".r" or ".pl" => (HashLines, false, false),
            ".sql" => (DashLines, true, false),
            ".lua" or ".hs" or ".adb" or ".ads" => (DashLines, false, false),
            ".vb" or ".vbs" or ".bas" => (ApostropheLines, false, false),
            ".tex" or ".sty" or ".cls" or ".erl" or ".hrl" or ".m" => (PercentLines, false, false),
            ".lisp" or ".cl" or ".clj" or ".scm" or ".asm" => (SemicolonLines, false, false),
            ".ini" or ".cfg" => (IniLines, false, false),
            ".php" => (PhpLines, true, true),
            ".css" or ".scss" => (SlashLines, true, false),
            ".html" or ".htm" or ".xml" or ".svg" or ".md" => ([], false, true),
            _ => (CommonLines, true, true),
        };
}
