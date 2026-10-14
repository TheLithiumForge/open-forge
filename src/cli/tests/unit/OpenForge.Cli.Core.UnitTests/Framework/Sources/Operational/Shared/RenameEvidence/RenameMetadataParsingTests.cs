using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class RenameMetadataParsingTests
{
    [Theory(DisplayName = "Recorded identity reads both metadata forms and normalizes declared applicability")]
    [InlineData(false), InlineData(true)]
    public void MetadataForms(bool scoped)
    {
        var declaration = "description: Specific authored rounding policy\ntags: [Guidance, Billing]\napplyTo: ['tests/**', 'src/**']\n";
        if (scoped) declaration = "open-forge:\n" + string.Concat(declaration.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => $"  {line}\n"));
        var content = RenameContentReader.Read(Encoding.UTF8.GetBytes($"---\n{declaration}---\n\n# Different title\n\nAuthored body.\n"));
        var metadata = new SourceAuthoredMetadataParser().Parse(content.Document, SourceDocumentForm.Markdown);
        Assert.Equal(SourceAuthoredMetadataState.Complete, metadata.State);
        Assert.Equal(ApplyToMetadataState.Valid, metadata.ApplyTo.State);
        var signature = RenameMetadataIdentity.Read(content.Document);
        Assert.NotNull(signature);
        Assert.Equal("Specific authored rounding policy", signature.Description);
        Assert.Equal(["Guidance", "Billing"], signature.Tags);
        Assert.Equal(["src/**", "tests/**"], signature.ApplyTo);
    }

    [Fact(DisplayName = "Invalid applicability declarations never supply a metadata identity")]
    public void InvalidApplicability()
    {
        var content = RenameContentReader.Read(Encoding.UTF8.GetBytes("---\ndescription: Specific authored identity\ntags: [Guidance]\napplyTo: [src/**]\n---\n\nBody.\n"));
        Assert.Null(RenameMetadataIdentity.Read(content.Document));
    }
}
