using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Recovery.Serialization;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Unit")]
public sealed class LibraryRecoveryManifestTests
{
    private const string Missing = """{"kind":"missing","length":null,"sha256":null,"linkKind":null,"rawRelativeTarget":null}""";
    private const string Link = """{"kind":"relative-file-link","length":null,"sha256":null,"linkKind":"relative-file-symbolic-link","rawRelativeTarget":"../shared/.agents/a.md"}""";
    private const string Ordinary = """{"kind":"ordinary-file","length":0,"sha256":"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855","linkKind":null,"rawRelativeTarget":null}""";

    [Theory(DisplayName = "Library schema-v1 recovery admits exact workspace attribution and payload-free typed link or record-create states")]
    [InlineData("attach", "ordinary-create"), InlineData("attach", "relative-file-link-create")]
    [InlineData("sync", "relative-file-link-create"), InlineData("sync", "relative-file-link-delete"), InlineData("detach", "relative-file-link-delete")]
    public void ReadsIndependentManifest(string operation, string kind)
    {
        var json = Manifest(operation, kind);

        var result = RecoveryBundleManifestCodec.Decode(Encoding.UTF8.GetBytes(json));

        Assert.Equal(RecoveryBundleManifestState.Valid, result.State);
        var attribution = Assert.IsType<RecoveryBundleAttribution>(result.Attribution);
        Assert.Equal(RecoveryBundleProducer.Library, attribution.Producer);
        Assert.Equal(operation, attribution.Operation.ToString().ToLowerInvariant());
        Assert.Equal(RecoveryBundleSubjectKind.Workspace, attribution.Subject.Kind);
        var entry = Assert.Single(result.Entries);
        Assert.Null(entry.PriorPayload);
        if (kind == "ordinary-create")
        {
            Assert.Equal(RecoveryEntryStateKind.Missing, entry.Prior.Kind);
            Assert.Equal(RecoveryEntryStateKind.OrdinaryFile, entry.Intended.Kind);
        }
        else
        {
            var link = kind == "relative-file-link-delete" ? entry.Prior : entry.Intended;
            Assert.Equal("../shared/.agents/a.md", Assert.IsType<RelativeFileLinkIdentity>(link.RelativeFileLink).RawRelativeTarget);
            Assert.Null(link.OrdinaryFile);
        }
    }

    public static TheoryData<string> MalformedManifests
    {
        get
        {
            var original = Manifest("attach", "relative-file-link-create");
            TheoryData<string> rows = [];
            foreach (var replacement in new[] { "\"producer\":\"unknown\"", "\"producer\":null", "\"producer\":1", "\"producer\":\"library\",\"producer\":\"library\"" })
            {
                rows.Add(original.Replace("\"producer\":\"library\"", replacement, StringComparison.Ordinal));
            }
            foreach (var operation in new[] { "install", "update", "create", "repair", "future" })
            {
                rows.Add(original.Replace("\"operation\":\"attach\"", $"\"operation\":\"{operation}\"", StringComparison.Ordinal));
            }
            rows.Add(original.Replace("\"producer\":\"library\",", "", StringComparison.Ordinal));
            rows.Add(original.Replace("\"kind\":\"workspace\"", "\"kind\":\"library\"", StringComparison.Ordinal));
            rows.Add(original.Replace("\"priorPayload\":null", "\"priorPayload\":\"payload/000000.bin\"", StringComparison.Ordinal));
            rows.Add(original.Replace("\"linkKind\":\"relative-file-symbolic-link\"", "\"linkKind\":\"directory-symbolic-link\"", StringComparison.Ordinal));
            rows.Add(original.Replace("../shared/.agents/a.md", "/outside/a.md", StringComparison.Ordinal));
            rows.Add(original.Replace("../shared/.agents/a.md", "..\\\\shared\\\\a.md", StringComparison.Ordinal));
            rows.Add(original.Replace($"\"intended\":{Link}", $"\"intended\":{Missing}", StringComparison.Ordinal));
            rows.Add(original.Replace($"\"prior\":{Missing}", $"\"prior\":{Link}", StringComparison.Ordinal));
            rows.Add(original.Replace("\"length\":null", "\"length\":1", StringComparison.Ordinal));
            rows.Add(original.Replace("\"sha256\":null", $"\"sha256\":\"{new string('a', 64)}\"", StringComparison.Ordinal));
            rows.Add(original.Replace("\"logicalPath\":\".agents/a.md\"", "\"logicalPath\":\"../source/a.md\"", StringComparison.Ordinal));
            rows.Add(original.Replace("\"ordinal\":0", "\"ordinal\":1", StringComparison.Ordinal));
            rows.Add(original.Replace("\"ordinal\":0", "\"ordinal\":0,\"extra\":true", StringComparison.Ordinal));
            rows.Add(original.Replace("\"rawRelativeTarget\":\"../shared/.agents/a.md\"", "\"rawRelativeTarget\":null", StringComparison.Ordinal));
            rows.Add(original.Replace("\"priorPayload\":null", "\"priorPayload\":null,\"priorPayload\":null", StringComparison.Ordinal));
            rows.Add(original.Replace("\"schemaVersion\":1", "\"schemaVersion\":1,\"schemaVersion\":1", StringComparison.Ordinal));
            return rows;
        }
    }

    [Theory(DisplayName = "Library recovery manifest rejects malformed provenance typed states source payloads and noncanonical identity"), MemberData(nameof(MalformedManifests))]
    public void RejectsMalformedManifest(string json)
    {
        var result = RecoveryBundleManifestCodec.Decode(Encoding.UTF8.GetBytes(json));

        Assert.Equal(RecoveryBundleManifestState.Malformed, result.State);
        Assert.Null(result.Attribution);
        Assert.Empty(result.Entries);
    }

    private static string Manifest(string operation, string kind)
    {
        var workspace = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-library-manifest"));
        var identityPath = OperatingSystem.IsWindows() ? workspace.ToUpperInvariant() : workspace;
        var key = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identityPath)));
        var prior = kind == "relative-file-link-delete" ? Link : Missing;
        var intended = kind switch
        {
            "ordinary-create" => Ordinary,
            "relative-file-link-create" => Link,
            "relative-file-link-delete" => Missing,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        return $$$"""
            {
              "schemaVersion":1,
              "command":"library {{{operation}}}",
              "operationId":"123456781234123412341234567890ab",
              "workspacePath":"{{{JsonEncodedText.Encode(workspace)}}}",
              "workspaceKey":"{{{key}}}",
              "attribution":{"producer":"library","operation":"{{{operation}}}","subject":{"kind":"workspace","identity":"{{{key}}}"}},
              "entries":[{"ordinal":0,"logicalPath":".agents/a.md","kind":"{{{kind}}}","prior":{{{prior}}},"intended":{{{intended}}},"priorPayload":null}]
            }
            """;
    }
}
