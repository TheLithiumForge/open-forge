using OpenForge.Cli.Core.Shell.Definitions;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Inspect.Shared.Result;

[Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
public sealed class ExtensionInspectFingerprintBuilderTests
{
    private const string OpaquePath = ".agents/guidance/toolkit-support.bin";
    private const string OpaqueSha256 = "c4cbb7cbfda0feb8dde8cd2e8abfb0771fc2c04fe50720acc3b83971937b8ac3";
    private const string UnsupportedMarkdownPath = ".agents/toolkit.md";
    private const string UnsupportedMarkdownSha256 = "1320b5dc13aa91dbac6eabc346cb655592aef8244a8ed04b8c4b3bdd59b8af4c";

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Extension Inspect path-kind policy uses semantic Markdown only for implicit Markdown destinations"), Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    [InlineData(".agents/guidance/toolkit.md", (int)ExtensionInspectFingerprintKind.Semantic)]
    [InlineData(".agents/guidance/toolkit.markdown", (int)ExtensionInspectFingerprintKind.Semantic)]
    [InlineData(".agents/guidance/toolkit-support.bin", (int)ExtensionInspectFingerprintKind.ExactBytes)]
    [InlineData("package/toolkit.md", (int)ExtensionInspectFingerprintKind.ExactBytes)]
    public void PathKindPolicyUsesOneSharedDecision(
        string path,
        int expectedValue)
        => Assert.Equal((ExtensionInspectFingerprintKind)expectedValue, ExtensionInspectFingerprintPolicy.ReadKind(path));

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Inspect retains opaque exact-byte origins and hashes without a fallback finding"), Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void OpaquePayloadRetainsExactByteOriginsAndHash()
    {
        var bytes = OpaqueBytes();
        var findings = new List<ExtensionInspectFinding>();
        var markdownFacts = new Dictionary<string, OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownFingerprintFacts>(StringComparer.Ordinal);
        var builder = new ExtensionInspectFingerprintBuilder(new MarkdownFingerprintReader());

        var current = builder.ReadCurrentFingerprints(
            [CurrentPath(OpaquePath, bytes, OpaqueSha256)],
            findings,
            markdownFacts);
        var intended = builder.ReadIntendedFingerprints(
            [IntendedPath(OpaquePath, "toolkit-support.bin", bytes, OpaqueSha256)],
            findings,
            markdownFacts);

        var currentFingerprint = Assert.Single(current).Fingerprint!;
        var intendedFingerprint = Assert.Single(intended).Fingerprint!;
        Assert.Equal(ExtensionInspectFingerprintKind.ExactBytes, currentFingerprint.Kind);
        Assert.Equal(ExtensionInspectFingerprintKind.ExactBytes, intendedFingerprint.Kind);
        Assert.Equal("open-forge-markdown-v1", currentFingerprint.Policy);
        Assert.Equal(OpaqueSha256, currentFingerprint.Sha256);
        Assert.Equal(OpaqueSha256, intendedFingerprint.Sha256);
        Assert.Equal(ExtensionInspectFingerprintOrigin.OperationTimeCurrent, currentFingerprint.Origin);
        Assert.Equal(ExtensionInspectFingerprintOrigin.OperationTimeIntended, intendedFingerprint.Origin);
        Assert.Empty(findings);
        Assert.Empty(markdownFacts);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Inspect compares retained opaque counterparts by their exact bytes"), Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void OpaqueCounterpartsCompareAsUnchanged()
    {
        var bytes = OpaqueBytes();
        var currentFingerprint = new ExtensionInspectFingerprint
        {
            Kind = ExtensionInspectFingerprintKind.ExactBytes,
            Policy = "open-forge-markdown-v1",
            Sha256 = OpaqueSha256,
            Origin = ExtensionInspectFingerprintOrigin.OperationTimeCurrent,
        };
        var intendedFingerprint = currentFingerprint with
        {
            Origin = ExtensionInspectFingerprintOrigin.OperationTimeIntended,
        };
        var findings = new List<ExtensionInspectFinding>();
        var package = Package(OpaquePath, "toolkit-support.bin", bytes, OpaqueSha256);
        var comparison = ExtensionInspectPathComparisonBuilder.Build(
            new ExtensionInspectPathComparisonInput
            {
                ComparisonFacts = new ExtensionInspectComparisonInput
                {
                    CurrentPaths = [CurrentPath(OpaquePath, bytes, OpaqueSha256)],
                    Ownership = WorkspaceOwnershipRead.Absent(Path.GetFullPath(".agents/open-forge.lock.json")),
                    InstalledPackage = null,
                    AvailablePackage = package,
                    AvailableClosure = [package],
                    Dependencies = new ExtensionInspectDependencyClosure
                    {
                        State = ExtensionInspectDependencyState.Complete,
                        Declared = [],
                        Resolved = [],
                        Order = [],
                    },
                    Current =
                    [
                        new ExtensionInspectFingerprintFact
                        {
                            Path = OpaquePath,
                            Fingerprint = currentFingerprint,
                        },
                    ],
                    Intended =
                    [
                        new ExtensionInspectFingerprintFact
                        {
                            Path = OpaquePath,
                            Fingerprint = intendedFingerprint,
                        },
                    ],
                    Findings = findings,
                },
                Mode = ExtensionInspectComparisonMode.InstalledAndAvailable,
                InstalledClosure =
                [
                    new ExtensionOwnership(
                        "toolkit",
                        "1.0.0",
                        "catalogue",
                        ImmutableArray<string>.Empty,
                        ImmutableArray.Create(OpaquePath),
                        ImmutableArray<OwnedRegion>.Empty),
                ],
            });

        var path = Assert.Single(comparison);
        Assert.Equal(ExtensionInspectPathRelation.Unchanged, path.Relation);
        Assert.Empty(findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Inspect keeps duplicate unsupported-Markdown fallback findings fail-closed"), Trait("Feature", "extension-inspect-fingerprint"), Trait("Evidence", "Unit")]
    public void DuplicateUnsupportedMarkdownFallbackFailsClosed()
    {
        var bytes = UnsupportedMarkdownBytes();
        var findings = new List<ExtensionInspectFinding>();
        var markdownFacts = new Dictionary<string, OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownFingerprintFacts>(StringComparer.Ordinal);
        var builder = new ExtensionInspectFingerprintBuilder(new MarkdownFingerprintReader());

        builder.ReadCurrentFingerprints(
            [CurrentPath(UnsupportedMarkdownPath, bytes, UnsupportedMarkdownSha256)],
            findings,
            markdownFacts);
        builder.ReadIntendedFingerprints(
            [IntendedPath(UnsupportedMarkdownPath, "toolkit.md", bytes, UnsupportedMarkdownSha256)],
            findings,
            markdownFacts);

        Assert.Equal(2, findings.Count(finding => finding.Code == ExtensionInspectFindingCode.FingerprintFallback));
        var normalized = ExtensionInspectFindingPolicy.Normalize(findings);
        var failure = Assert.Single(normalized);
        Assert.Equal(ExtensionInspectFindingCode.OperationFailed, failure.Code);
        Assert.Equal(CliSemanticStatus.Failed, failure.Status);
    }

    private static ExtensionInspectCurrentPath CurrentPath(
        string path,
        byte[] bytes,
        string sha256)
        => new()
        {
            Path = path,
            State = ExtensionInspectCurrentPathState.Present,
            PhysicalIdentity = "owned-test-file",
            ByteLength = bytes.LongLength,
            ExactSha256 = sha256,
            Bytes = bytes,
        };

    private static ExtensionInspectDeclaredPathProjection IntendedPath(
        string path,
        string sourcePath,
        byte[] bytes,
        string sha256)
        => new()
        {
            Path = path,
            SourcePath = sourcePath,
            State = ExtensionInspectDeclaredPathState.Available,
            Owners = ["toolkit"],
            Files =
            [
                ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
                {
                    Path = sourcePath,
                    TargetPath = path,
                    State = ExtensionPackageFileReadState.Available,
                    ByteLength = bytes.LongLength,
                    Sha256 = sha256,
                    Bytes = bytes,
                }),
            ],
            HasConflict = false,
        };

    private static ExtensionPackageFact Package(
        string path,
        string sourcePath,
        byte[] bytes,
        string sha256)
        => ExtensionPackageFact.Create(
            new ExtensionPackageManifestFact
            {
                Id = "toolkit",
                Name = "Toolkit",
                Description = "Guidance tools.",
                Version = "1.0.0",
                Dependencies = [],
            },
            new ExtensionPackageContentsFact
            {
                ManifestPath = "toolkit/extension.json",
                Payload =
                [
                    ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
                    {
                        Path = sourcePath,
                        TargetPath = path,
                        State = ExtensionPackageFileReadState.Available,
                        ByteLength = bytes.LongLength,
                        Sha256 = sha256,
                        Bytes = bytes,
                    }),
                ],
            });

    private static byte[] OpaqueBytes() => [0x00, (byte)'a', 0x0D, 0x0A];

    private static byte[] UnsupportedMarkdownBytes() => [0xFF, 0x0D, 0x0A];
}
