namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;

internal enum FrameworkMetadataField
{
    Description,
    Responsibility,
    Tags,

    // The native Skill name. Only SKILL.md completion reports it, and no Open Forge metadata member reads as it.
    Name,
}
