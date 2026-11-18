namespace OpenForge.Cli.Core.Commands.Stripping.Models.Request;

// The plain-docs Interface contract fixes this order: frontmatter or metadata, then entrypoints, then links.
internal enum StripPart
{
    Metadata,
    Frontmatter,
    Entrypoints,
    Links,
}

// Uninstall applies the shared plan in place. Export writes it to a destination.
internal enum StripCommandKind
{
    Uninstall,
    Export,
}
