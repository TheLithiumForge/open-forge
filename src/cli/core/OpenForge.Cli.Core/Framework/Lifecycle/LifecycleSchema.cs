using OpenForge.Cli.Core.Framework.Documents.Markdown;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal static class LifecycleSchema
{
    internal const int Version = 1;
    internal const string DirectoryName = ".agents";
    internal const string FileName = "open-forge.lifecycle.json";
    internal const string RelativePath = ".agents/open-forge.lifecycle.json";
    internal const string FrameworkProperty = "framework";
    internal const string ExtensionsProperty = "extensions";
    internal const string FingerprintPolicy = MarkdownFingerprintPolicy.Name;
    internal const string CompleteCoverage = "complete";
    internal const string SemanticFingerprintKind = "semantic";
    internal const string ExactBytesFingerprintKind = "exact-bytes";
}
