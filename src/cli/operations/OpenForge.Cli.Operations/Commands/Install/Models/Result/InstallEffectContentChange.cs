namespace OpenForge.Cli.Core.Commands.Install.Models.Result;

// .agents/memory/crystallized/documents/cli/contracts/install/interface.md,
// Human Output, distinguishes conversions and preserved-content edits from replacement.
internal enum InstallEffectContentChange
{
    WholeFile,
    PreservedContent,
    FrontmatterConversion,
    GitIgnoreRulesAdded,
}
