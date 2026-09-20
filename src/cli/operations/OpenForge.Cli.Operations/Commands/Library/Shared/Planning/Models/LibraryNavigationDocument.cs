using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Planning.Models;

internal sealed record LibraryNavigationDocument(byte[] Bytes, MarkdownDocumentFacts Document);
