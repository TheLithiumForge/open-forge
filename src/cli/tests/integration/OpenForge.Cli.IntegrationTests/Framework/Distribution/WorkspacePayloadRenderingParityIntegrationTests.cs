using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Extensions.Embedded;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.IntegrationTests.Framework.Distribution;

public sealed class WorkspacePayloadRenderingParityIntegrationTests
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly string[] SupportedFields = ["description", "responsibility", "tags", "applyTo"];

    [Fact(DisplayName = "Every shipped Framework and Extension Markdown asset renders in both forms with identical values")]
    [Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Integration"), Trait("Boundary", "Input")]
    public void EveryShippedMarkdownAssetRendersInBothForms()
    {
        var framework = EmbeddedFrameworkPayloadReader.Read();
        Assert.Equal(FrameworkPayloadReadState.Available, framework.State);
        var payload = Assert.IsType<FrameworkPayload>(framework.Payload);
        var frameworkMarkdown = payload.Assets.Where(asset => asset.Path.EndsWith(".md", StringComparison.Ordinal)).ToArray();
        Assert.NotEmpty(frameworkMarkdown);
        var frameworkChanged = 0;
        foreach (var asset in frameworkMarkdown)
        {
            if (AssertParity(asset.Path, asset.Bytes.AsMemory()))
            {
                frameworkChanged++;
            }
        }

        var extensions = EmbeddedExtensionCatalogueReader.Read();
        Assert.Equal(ExtensionSourceReadState.Complete, extensions.State);
        Assert.NotEmpty(extensions.Packages);
        var extensionMarkdown = extensions.Packages.SelectMany(package => package.Payload)
            .Where(file => file.TargetPath?.EndsWith(".md", StringComparison.Ordinal) == true).ToArray();
        Assert.NotEmpty(extensionMarkdown);
        var extensionChanged = 0;
        foreach (var asset in extensionMarkdown)
        {
            Assert.NotNull(asset.TargetPath);
            Assert.NotNull(asset.Bytes);
            if (AssertParity(asset.TargetPath, asset.Bytes.Value))
            {
                extensionChanged++;
            }
        }

        Assert.True(frameworkChanged > 0);
        Assert.True(extensionChanged > 0);
    }

    private static bool AssertParity(string path, ReadOnlyMemory<byte> source)
    {
        var scoped = WorkspacePayloadRenderer.Render(path, source, FrontmatterForm.Scoped);
        Assert.Equal(FrameworkFrontmatterTransformState.Unchanged, scoped.State);
        Assert.Null(scoped.Cause);
        Assert.Equal(source, scoped.Bytes);
        var root = WorkspacePayloadRenderer.Render(path, source, FrontmatterForm.Root);
        Assert.True(root.State != FrameworkFrontmatterTransformState.Invalid, $"{path}: {root.Cause}");
        Assert.Null(root.Cause);
        Assert.NotNull(root.Bytes);
        var original = ReadYaml(source);
        var rendered = ReadYaml(root.Bytes.Value);
        Assert.False(rendered?.Root?.TryGetMappingValue("open-forge", out _) == true, path);

        if (original?.Root?.TryGetMappingValue("open-forge", out var mapping) == true)
        {
            Assert.NotNull(mapping);
            Assert.NotNull(rendered?.Root);
            foreach (var field in SupportedFields)
            {
                mapping.TryGetMappingValue(field, out var expected);
                if (field == "applyTo" && expected is null)
                {
                    original.Root.TryGetMappingValue(field, out expected);
                }

                rendered.Root.TryGetMappingValue(field, out var actual);
                AssertSameValue(expected, actual);
            }
        }

        var originalText = StrictUtf8.GetString(source.Span);
        var renderedText = StrictUtf8.GetString(root.Bytes.Value.Span);
        var originalBoundary = new MarkdownFrontmatterParser().Parse(originalText);
        var renderedBoundary = new MarkdownFrontmatterParser().Parse(renderedText);
        Assert.NotNull(originalBoundary.BodyStart);
        Assert.NotNull(renderedBoundary.BodyStart);
        var originalBody = StrictUtf8.GetByteCount(originalText.AsSpan(0, originalBoundary.BodyStart.Value));
        var renderedBody = StrictUtf8.GetByteCount(renderedText.AsSpan(0, renderedBoundary.BodyStart.Value));
        Assert.True(source.Span[originalBody..].SequenceEqual(root.Bytes.Value.Span[renderedBody..]), path);
        return root.State == FrameworkFrontmatterTransformState.Changed;
    }

    private static YamlDocumentFacts? ReadYaml(ReadOnlyMemory<byte> bytes)
    {
        var source = StrictUtf8.GetString(bytes.Span);
        var boundary = new MarkdownFrontmatterParser().Parse(source);
        if (boundary.YamlSpan is not { } span)
        {
            return null;
        }

        var yaml = new YamlDocumentParser().Parse(source[span.Start..span.End]);
        Assert.Equal(YamlDocumentState.Complete, yaml.State);
        return yaml;
    }

    private static void AssertSameValue(YamlNode? expected, YamlNode? actual)
    {
        if (expected is null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);
        Assert.Equal(expected.Kind, actual.Kind);
        Assert.Equal(expected.Scalar?.Value, actual.Scalar?.Value);
        if (expected.Sequence is { } items)
        {
            Assert.NotNull(actual.Sequence);
            Assert.Equal(items.Count, actual.Sequence.Count);
            for (var index = 0; index < items.Count; index++)
            {
                AssertSameValue(items[index], actual.Sequence[index]);
            }
        }

        if (expected.Mapping is { } mapping)
        {
            Assert.NotNull(actual.Mapping);
            Assert.Equal(mapping.Count, actual.Mapping.Count);
            for (var index = 0; index < mapping.Count; index++)
            {
                AssertSameValue(mapping[index].Key, actual.Mapping[index].Key);
                AssertSameValue(mapping[index].Value, actual.Mapping[index].Value);
            }
        }
    }
}
