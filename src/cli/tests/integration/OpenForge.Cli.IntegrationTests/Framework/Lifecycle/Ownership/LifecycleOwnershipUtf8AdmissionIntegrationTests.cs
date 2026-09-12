using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Lifecycle.Ownership;

public sealed class LifecycleOwnershipUtf8AdmissionIntegrationTests
{
    [Theory(DisplayName = "Lifecycle ownership preserves full UTF-8 and JSON rejection causes"), InlineData("isolated"), InlineData("truncated"), InlineData("surrogate")]
    [InlineData("continuation"), InlineData("long-prefix"), InlineData("duplicate-property"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task InvalidOwnedDocumentsPreserveExposedCausesAndBytes(string scenario)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            using var temporary = TemporaryWorkspace.Create("lifecycle-utf8-ownership");
            var workspace = LifecycleStoreIntegrationDocuments.Workspace(temporary);
            var bytes = InvalidDocument(scenario);
            temporary.WriteBytes(LifecycleSchema.RelativePath, bytes);
            var lifecyclePath = temporary.Combine(LifecycleSchema.RelativePath);
            Assert.Equal(bytes, await File.ReadAllBytesAsync(lifecyclePath, TestContext.Current.CancellationToken));

            var result = await new LifecycleOwnershipReader(new PhysicalPathResolver())
                .ReadAsync(workspace, TestContext.Current.CancellationToken);

            Assert.Equal(LifecycleOwnershipSection.Framework, result.Framework.Section);
            Assert.Equal(LifecycleOwnershipSection.Extensions, result.Extensions.Section);
            Assert.Equal(LifecycleOwnershipReadState.Blocked, result.Framework.State);
            Assert.Equal(LifecycleOwnershipReadState.Blocked, result.Extensions.State);
            Assert.Empty(result.Claims);
            Assert.Null(result.LifecycleFileExpectation);
            var finding = Assert.Single(result.Findings);
            Assert.Equal(LifecycleOwnershipFindingCode.LifecycleInvalid, finding.Code);
            Assert.Null(finding.Section);
            Assert.Null(finding.Path);
            Assert.Equal(bytes, await File.ReadAllBytesAsync(lifecyclePath, TestContext.Current.CancellationToken));
            var expectedCause = ExpectedCause(scenario);
            Assert.Equal(expectedCause, result.Framework.Cause);
            Assert.Equal(expectedCause, result.Extensions.Cause);
            Assert.Equal(expectedCause, finding.Cause);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact(DisplayName = "Valid non-ASCII lifecycle bytes retain trusted ownership and exact file expectation"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task NonAsciiOwnedDocumentRetainsTrustedClaimsAndExactFileExpectation()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            using var temporary = TemporaryWorkspace.Create("lifecycle-utf8-ownership-valid");
            var workspace = LifecycleStoreIntegrationDocuments.Workspace(temporary);
            var envelope = LifecycleStoreIntegrationDocuments.Envelope(
                temporary,
                LifecycleStoreIntegrationDocuments.Framework(),
                LifecycleStoreIntegrationDocuments.Extensions());
            var json = Encoding.UTF8.GetString(LifecycleStoreIntegrationDocuments.Serialize(envelope))
                .Replace("1.0.0", "1.0.0-é", StringComparison.Ordinal);
            var bytes = Encoding.UTF8.GetBytes(json);
            Assert.Contains((byte)0xc3, bytes);
            Assert.Contains((byte)0xa9, bytes);
            temporary.WriteBytes(LifecycleSchema.RelativePath, bytes);
            var lifecyclePath = temporary.Combine(LifecycleSchema.RelativePath);
            Assert.Equal(bytes, await File.ReadAllBytesAsync(lifecyclePath, TestContext.Current.CancellationToken));

            var result = await new LifecycleOwnershipReader(new PhysicalPathResolver())
                .ReadAsync(workspace, TestContext.Current.CancellationToken);

            Assert.Equal(LifecycleOwnershipSection.Framework, result.Framework.Section);
            Assert.Equal(LifecycleOwnershipSection.Extensions, result.Extensions.Section);
            Assert.Equal(LifecycleOwnershipReadState.Trusted, result.Framework.State);
            Assert.Equal(LifecycleOwnershipReadState.Trusted, result.Extensions.State);
            Assert.Null(result.Framework.Cause);
            Assert.Null(result.Extensions.Cause);
            Assert.Empty(result.Findings);
            Assert.Collection(
                result.Claims,
                claim =>
                {
                    Assert.Equal(".agents/loader.md", claim.Path);
                    Assert.Equal(LifecycleOwnershipManager.Framework, claim.Manager);
                    Assert.Equal("open-forge", claim.Owner);
                },
                claim =>
                {
                    Assert.Equal(".agents/toolkit.md", claim.Path);
                    Assert.Equal(LifecycleOwnershipManager.Extension, claim.Manager);
                    Assert.Equal("toolkit", claim.Owner);
                });
            var expectation = Assert.IsType<FileExpectation>(result.LifecycleFileExpectation);
            Assert.Equal(FileExpectationKind.File, expectation.Kind);
            Assert.Equal(lifecyclePath, expectation.LogicalPath);
            Assert.Equal(lifecyclePath, expectation.PhysicalPath);
            Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(bytes)), expectation.ContentHash);
            Assert.Equal(bytes, await File.ReadAllBytesAsync(lifecyclePath, TestContext.Current.CancellationToken));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private static string ExpectedCause(string scenario)
        => scenario switch
        {
            "isolated" => "The lifecycle document is invalid: Unable to translate bytes [FF] at index 0 from specified code page to Unicode.",
            "truncated" => "The lifecycle document is invalid: Unable to translate bytes [E2][82] at index 0 from specified code page to Unicode.",
            "surrogate" => "The lifecycle document is invalid: Unable to translate bytes [ED] at index 0 from specified code page to Unicode.",
            "continuation" => "The lifecycle document is invalid: Unable to translate bytes [E2] at index 3 from specified code page to Unicode.",
            "long-prefix" => "The lifecycle document is invalid: Unable to translate bytes [FF] at index 128 from specified code page to Unicode.",
            "duplicate-property" => "The lifecycle document is invalid: The JSON property 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" +
                "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" +
                "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" +
                "xxxxxxxxxxxxxxxxxxxxxxxx' is duplicated.",
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The UTF-8 admission scenario is not defined."),
        };

    private static byte[] InvalidDocument(string scenario)
        => scenario switch
        {
            "isolated" => [0xff],
            "truncated" => [0xe2, 0x82],
            "surrogate" => [0xed, 0xa0, 0x80],
            "continuation" => [0x22, 0xc3, 0xa9, 0xe2, 0x28, 0xa1],
            "long-prefix" => [.. Encoding.UTF8.GetBytes(new string(' ', 128)), 0xff],
            "duplicate-property" => Encoding.UTF8.GetBytes($"{{\"{new string('x', 300)}\":0,\"{new string('x', 300)}\":1}}"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The UTF-8 admission scenario is not defined."),
        };
}
