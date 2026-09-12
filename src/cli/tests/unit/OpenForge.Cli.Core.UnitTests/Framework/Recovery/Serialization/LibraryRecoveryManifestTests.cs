using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

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

    [Theory(DisplayName = "Recovery entry kinds retain literal wire tokens and exact typed identities through serialization"), InlineData("ordinary-create"), InlineData("ordinary-replace")]
    [InlineData("ordinary-replace-generated-region"), InlineData("ordinary-delete"), InlineData("relative-file-link-create"), InlineData("relative-file-link-delete")]
    public void RoundtripsEveryEntryKindFromAdmittedTargets(string kind)
    {
        var (input, entry) = KindManifestInput(kind);

        var bytes = RecoveryBundleManifestCodec.Serialize(input, [entry]);
        using var document = JsonDocument.Parse(bytes);
        var wireEntry = Assert.Single(document.RootElement.GetProperty("entries").EnumerateArray());
        Assert.Equal(kind, wireEntry.GetProperty("kind").GetString());
        var result = RecoveryBundleManifestCodec.Decode(bytes);

        Assert.Equal(RecoveryBundleManifestState.Valid, result.State);
        Assert.Null(result.Cause);
        Assert.Equal(input.Attribution, result.Attribution);
        var decoded = Assert.Single(result.Entries);
        Assert.Equal(0, decoded.Ordinal);
        Assert.Equal(".agents/a.md", decoded.LogicalPath.Value);
        Assert.Equal(entry.Kind, decoded.Kind);
        Assert.Equal(entry.Prior, decoded.Prior);
        Assert.Equal(entry.Intended, decoded.Intended);
        var expectedPayload = kind is "ordinary-replace" or "ordinary-replace-generated-region" or "ordinary-delete"
            ? "payloads/00000000.bin"
            : null;
        Assert.Equal(expectedPayload, decoded.PriorPayload);
    }

    [Theory(DisplayName = "Recovery entry kind JSON null missing unknown and wrong-case values remain malformed"), InlineData("null"), InlineData("missing")]
    [InlineData("unknown"), InlineData("wrong-case")]
    public void RejectsAlteredEntryKindJson(string mutation)
    {
        var (input, entry) = KindManifestInput("ordinary-replace");
        var original = RecoveryBundleManifestCodec.Serialize(input, [entry]);
        Assert.Equal(RecoveryBundleManifestState.Valid, RecoveryBundleManifestCodec.Decode(original).State);
        var json = Encoding.UTF8.GetString(original);
        const string kindProperty = "\"kind\":\"ordinary-replace\",";
        Assert.Equal(1, json.Split(kindProperty, StringSplitOptions.None).Length - 1);
        var replacement = mutation switch
        {
            "null" => "\"kind\":null,",
            "missing" => string.Empty,
            "unknown" => "\"kind\":\"future\",",
            "wrong-case" => "\"kind\":\"Ordinary-Replace\",",
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        };
        var changedJson = json.Replace(kindProperty, replacement, StringComparison.Ordinal);
        var changedBytes = Encoding.UTF8.GetBytes(changedJson);
        Assert.NotEqual(json, changedJson);
        Assert.False(original.AsSpan().SequenceEqual(changedBytes));
        using var changedDocument = JsonDocument.Parse(changedBytes);
        var changedEntry = Assert.Single(changedDocument.RootElement.GetProperty("entries").EnumerateArray());
        if (mutation == "missing")
        {
            Assert.False(changedEntry.TryGetProperty("kind", out _));
        }
        else
        {
            Assert.Equal(replacement[7..^1], changedEntry.GetProperty("kind").GetRawText());
        }

        var result = RecoveryBundleManifestCodec.Decode(changedBytes);

        Assert.Equal(RecoveryBundleManifestState.Malformed, result.State);
        Assert.Empty(result.Entries);
        Assert.Null(result.Attribution);
    }

    private static (RecoveryBundleInput Input, RecoveryEntry Entry) KindManifestInput(string kind)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-kind-manifest"));
        var workspace = new CliWorkspace(lexicalRoot: root, physicalRoot: root, selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var target = KindManifestTarget(kind: kind, path: Path.Combine(root, ".agents", "a.md"));
        var (producer, operation, command) = kind switch
        {
            "ordinary-create" or "relative-file-link-create" =>
                (RecoveryBundleProducer.Library, RecoveryBundleOperation.Attach, "library attach"),
            "relative-file-link-delete" =>
                (RecoveryBundleProducer.Library, RecoveryBundleOperation.Detach, "library detach"),
            "ordinary-replace" or "ordinary-replace-generated-region" or "ordinary-delete" =>
                (RecoveryBundleProducer.Index, RecoveryBundleOperation.Index, "index"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        var input = RecoveryBundleInput.Create(
            workspace: workspace,
            command: command,
            attribution: RecoveryBundleAttribution.Create(producer, operation, workspace),
            operationId: Guid.ParseExact("123456781234123412341234567890ab", "N"),
            targets: [target]);
        Assert.Same(target, Assert.Single(input.Targets));
        Assert.Same(target, Assert.Single(input.RecoveryTargets));
        return (input, RecoveryEntry.FromTarget(input, target, ordinal: 0));
    }

    private static RecoveryBundleTarget KindManifestTarget(string kind, string path)
    {
        switch (kind)
        {
            case "ordinary-create":
                var missing = FileStateSnapshot.Missing(path);
                return RecoveryBundleTarget.CreateReversible(
                    PlannedFileChange.Create(missing.Expectation, "after"u8.ToArray()),
                    missing);

            case "ordinary-replace":
            case "ordinary-replace-generated-region":
            case "ordinary-delete":
                var before = FileStateSnapshot.File(logicalPath: path, physicalPath: path, bytes: "before"u8);
                var change = kind switch
                {
                    "ordinary-replace" => PlannedFileChange.Replace(before.Expectation, "after"u8.ToArray()),
                    "ordinary-replace-generated-region" => PlannedFileChange.ReplaceGeneratedRegion(before.Expectation, "after"u8.ToArray()),
                    "ordinary-delete" => PlannedFileChange.Delete(before.Expectation),
                    _ => throw new ArgumentOutOfRangeException(nameof(kind)),
                };
                return RecoveryBundleTarget.Create(change, before);

            case "relative-file-link-create":
            case "relative-file-link-delete":
                var destination = CanonicalRelativePath.Create(".agents/a.md");
                var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/.agents/a.md");
                if (kind == "relative-file-link-create")
                {
                    return RecoveryBundleTarget.Create(
                        RelativeFileLinkEffect.Create(destination, link),
                        NoFollowLeafObservation.Missing(path));
                }

                return RecoveryBundleTarget.Create(
                    RelativeFileLinkEffect.Delete(destination, link),
                    NoFollowLeafObservation.CreateRelativeFileLink(path, link));

            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
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
