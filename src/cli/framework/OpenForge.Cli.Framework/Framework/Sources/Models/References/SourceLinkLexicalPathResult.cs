namespace OpenForge.Cli.Core.Framework.Sources.Models.References;

internal enum SourceLinkLexicalPathState
{
    Complete,
    SourceDirectoryMissing,
    Malformed,
    OutsideWorkspace,
}

internal sealed class SourceLinkLexicalPathResult
{
    private SourceLinkLexicalPathResult(SourceLinkLexicalPathState state)
    {
        State = state;
    }

    private SourceLinkLexicalPathResult(string lexicalTarget, string canonicalPath)
    {
        State = SourceLinkLexicalPathState.Complete;
        LexicalTarget = lexicalTarget;
        CanonicalPath = canonicalPath;
    }

    internal SourceLinkLexicalPathState State { get; }

    internal string? LexicalTarget { get; }

    internal string? CanonicalPath { get; }

    internal static SourceLinkLexicalPathResult SourceDirectoryMissing { get; } = new(SourceLinkLexicalPathState.SourceDirectoryMissing);

    internal static SourceLinkLexicalPathResult Malformed { get; } = new(SourceLinkLexicalPathState.Malformed);

    internal static SourceLinkLexicalPathResult OutsideWorkspace { get; } = new(SourceLinkLexicalPathState.OutsideWorkspace);

    internal static SourceLinkLexicalPathResult Complete(string lexicalTarget, string canonicalPath)
        => new(lexicalTarget: lexicalTarget, canonicalPath: canonicalPath);
}
