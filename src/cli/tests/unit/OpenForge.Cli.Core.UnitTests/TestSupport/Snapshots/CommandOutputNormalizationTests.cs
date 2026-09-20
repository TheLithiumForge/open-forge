using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.TestSupport.Snapshots;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Unit")]
public sealed class CommandOutputNormalizationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Cleanup snapshots normalize only the exact asserted lease operation ID member")]
    public void CleanupLeaseIdentityPreservesOtherIdentifiers()
    {
        var leaseId = Guid.Parse("abcdef01-2345-6789-abcd-ef0123456789");
        const string actual = """
            {"operationId":"abcdef0123456789abcdef0123456789"}
            {"operationId": "abcdef0123456789abcdef0123456789"}
            {"operationId":"11111111111111111111111111111111"}
            {"operationId":"abcdef0123456789abcdef01234567890"}
            {"operationId":"ABCDEF0123456789ABCDEF0123456789"}
            {"candidateId":"abcdef0123456789abcdef0123456789"}
            abcdef0123456789abcdef0123456789 3 bundles
            """;
        const string expected = """
            {"operationId":"<cleanup-lease-id>"}
            {"operationId": "<cleanup-lease-id>"}
            {"operationId":"11111111111111111111111111111111"}
            {"operationId":"abcdef0123456789abcdef01234567890"}
            {"operationId":"ABCDEF0123456789ABCDEF0123456789"}
            {"candidateId":"abcdef0123456789abcdef0123456789"}
            abcdef0123456789abcdef0123456789 3 bundles
            """;
        Assert.Equal(expected, CommandOutputNormalization.NormalizeCleanupLeaseIdentity(actual, leaseId));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Text path normalization rewrites a placeholder path and leaves other text alone")]
    public void TextPathNormalizationIsScopedToPlaceholders()
    {
        // The backslash in `literal\\marker` is content the command printed. A capture records
        // what was printed, so normalization may rewrite the separator inside a redacted path and
        // nothing else.
        Assert.Equal(
            @"<workspace>/a/b.md and literal\marker",
            CommandOutputNormalization.Normalize(
                @"C:\ws\a\b.md and literal\marker",
                @"C:\ws",
                "1.2.3-test",
                normalizeTextPaths: true));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command snapshots redact a fingerprint by its member or label, not by its shape")]
    public void FingerprintRedactionIsAnchored()
    {
        var hash = new string('a', 64);
        // The last value carries the same shape in a position no fingerprint occupies. Redacting
        // it would rewrite a real value and freeze the wrong evidence into the capture.
        var actual = $"\"sha256\": \"{hash}\"\nBefore SHA-256: {hash}\nThe {hash} route is unrelated.";

        Assert.Equal(
            $"\"sha256\": \"<sha256>\"\nBefore SHA-256: <sha256>\nThe {hash} route is unrelated.",
            CommandOutputNormalization.Normalize(actual, null, "1.2.3-test"));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "The shared delimited path replacement refuses to consume a sibling root")]
    public void DelimitedPathReplacementKeepsSiblingRoots()
    {
        const string root = @"C:\ws";
        Assert.Equal(
            @"<workspace>\a.md and C:\ws-other\a.md",
            CommandOutputNormalization.ReplaceDelimitedPath(
                @"C:\ws\a.md and C:\ws-other\a.md", root, "<workspace>"));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command snapshots preserve distinct recovery paths and nearby unrelated identities")]
    public void RecoveryCandidatesAndStoreRemainDistinct()
    {
        const string store = @"C:\owned\recovery";
        const string first = @"C:\owned\recovery\11111111.zip";
        const string second = @"C:\owned\recovery\22222222.zip";
        var actual = $"{first}\n{System.Text.Json.JsonEncodedText.Encode(second)}\n{store}\nC:\\owned\\recovery-other\\33333333.zip\n11111111 2 files";
        Assert.Equal("<recovery-bundle-1>\n<recovery-bundle-2>\n<recovery-store>\nC:\\owned\\recovery-other\\33333333.zip\n11111111 2 files",
            CommandOutputNormalization.NormalizeRecoveryPaths(actual, [first, second], store));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command snapshots normalize only the owned Extension source root and its descendants")]
    public void ExtensionSourceDoesNotConsumeAdjacentPaths()
    {
        const string source = @"C:\owned\source";
        var actual = $"{source}\n{System.Text.Json.JsonEncodedText.Encode(source + @"\toolkit\extension.json")}\nC:\\owned\\source-other\\extension.json\n2 packages";
        Assert.Equal("<extension-source>\n<extension-source>\\\\toolkit\\\\extension.json\nC:\\owned\\source-other\\extension.json\n2 packages",
            CommandOutputNormalization.Normalize(actual, null, "1.2.3-test", extensionSourcePath: source));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command snapshots replace only the explicitly observed recovery bundle path")]
    public void RecoveryPathNormalizationKeepsOtherIdentities()
    {
        var workspace = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "snapshot-workspace"));
        var recovery = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "recovery", "a782f4ca-3baf-47dc-b74a-f1e5b63e0aa0.zip"));
        var encoded = System.Text.Json.JsonEncodedText.Encode(recovery).ToString();
        var actual = $"{recovery}\n{encoded}\nReview {recovery}. Next.\n\"Review {encoded}.\"\n{recovery}.other\n{encoded}.other\n5c7275d1-378c-4941-b54e-40c093933d29\n{recovery}.";

        // The adjacent `.other` paths stay distinct evidence, but their temporary root is a
        // machine coordinate that must never reach a checked-in capture, so it collapses while
        // the identity that distinguishes them from the observed bundle survives.
        var root = Path.TrimEndingDirectorySeparator(Path.GetTempPath());
        var otherText = recovery.Replace(root, "<temp>", StringComparison.Ordinal);
        var otherEncoded = encoded.Replace(
            System.Text.Json.JsonEncodedText.Encode(root).ToString(), "<temp>", StringComparison.Ordinal);

        Assert.Equal($"<recovery-bundle>\n<recovery-bundle>\nReview <recovery-bundle>. Next.\n\"Review <recovery-bundle>.\"\n{otherText}.other\n{otherEncoded}.other\n5c7275d1-378c-4941-b54e-40c093933d29\n<recovery-bundle>.",
            CommandOutputNormalization.Normalize(actual, workspace, "1.2.3-test", recovery));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command snapshot normalization preserves counts, encoding and stable identities")]
    public void NormalizesOnlyDeclaredVolatileValues()
    {
        var workspace = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "snapshot-workspace"));
        var encoded = System.Text.Json.JsonEncodedText.Encode(workspace).ToString();
        var hash = new string('a', 64);
        var actual = $"{workspace}\r\n{encoded}\r\n1.2.3-test\r\nSHA-256: {hash}\r\n3 files 12 bytes \\u00fa  GUID 5c7275d1-378c-4941-b54e-40c093933d29\r\n";

        Assert.Equal(
            "<workspace>\n<workspace>\n<version>\nSHA-256: <sha256>\n3 files 12 bytes \\u00fa  GUID 5c7275d1-378c-4941-b54e-40c093933d29\n",
            CommandOutputNormalization.Normalize(actual, workspace, "1.2.3-test"));
    }
}
