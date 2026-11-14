using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-config"), Trait("Evidence", "Unit")]
public sealed class ToolProfileDigestTests
{
    [Fact(DisplayName = "Profile digests sort authored object keys but keep array order")]
    public void JsonOrder()
    {
        var first = Profile(Json("{\"z\":[1,2],\"a\":true}"));
        var equivalent = Profile(Json("{\"a\":true,\"z\":[1,2]}"));
        var reordered = Profile(Json("{\"a\":true,\"z\":[2,1]}"));
        Assert.Equal(Digest(first), Digest(equivalent));
        Assert.NotEqual(Digest(first), Digest(reordered));
    }

    [Fact(DisplayName = "Profile identity range minimums and native command vectors affect the digest")]
    public void ProfileInputs()
    {
        var profile = Profile(Json("true"));
        var original = Digest(profile);
        Assert.NotEqual(original, Digest(profile with { Id = "another-profile" }));
        Assert.NotEqual(original, Digest(profile with { Range = new(2, 4) }));
        Assert.NotEqual(original, Digest(profile with { MinimumOpenForgeVersion = "9.8.7" }));
        Assert.NotEqual(original, Digest(profile with { MinimumToolVersion = "2.3.5" }));
        Assert.NotEqual(original, Digest(profile with { Setup = profile.Setup with { Arguments = ["other"] } }));
        Assert.NotEqual(original, Digest(profile with { Uninstall = profile.Uninstall with { Arguments = ["other"] } }));
        Assert.Equal(original, Digest(profile with { TestedVersions = ["2.3.9"] }));
    }

    [Fact(DisplayName = "Native declared effects and authored environment overlays affect the digest")]
    public void NativeEffects()
    {
        var profile = Profile(Json("true"));
        var effect = new ToolDeclaredEffect { Id = "effect", Kind = ToolDeclaredEffectKind.Telemetry, Scope = ToolEffectScope.Account, Description = "synthetic effect" };
        Assert.NotEqual(Digest(profile), Digest(profile with { Setup = profile.Setup with { Effects = [effect] } }));
        var first = profile with { Setup = profile.Setup with { Environment = new Dictionary<string, string?> { ["Z"] = null, ["A"] = "1" } } };
        var same = profile with { Setup = profile.Setup with { Environment = new Dictionary<string, string?> { ["A"] = "1", ["Z"] = null } } };
        Assert.Equal(Digest(first), Digest(same));
    }

    [Fact(DisplayName = "Managed template content affects the digest while resource names alone do not")]
    public void TemplateContent()
    {
        var change = new ToolManagedFileChange
        {
            Id = "file",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.AbsentFile,
            Description = "synthetic file",
            PathTemplate = "synthetic.txt",
            TemplateResource = "first-resource"
        };
        var profile = Profile(change);
        Assert.NotEqual(ToolProfileDigest.Compute(profile, _ => [1]), ToolProfileDigest.Compute(profile, _ => [2]));
        Assert.Equal(Digest(profile), Digest(Profile(change with { TemplateResource = "another-resource" })));
    }

    [Fact(DisplayName = "Native read apply manual vectors and desired values are part of the recipe digest")]
    public void NativeRecipe()
    {
        var change = new ToolNativeConfigChange
        {
            Id = "native",
            Scope = ToolEffectScope.Account,
            InitialState = ToolInitialConfigState.ReadableNativeValue,
            Description = "synthetic native setting",
            AllowListId = "synthetic-setting",
            ApplyArguments = ["config", "off"],
            ReadArguments = ["status"],
            ReadEnvironment = new Dictionary<string, string?> { ["SYNTHETIC_OVERRIDE"] = null },
            ValueParserId = "synthetic-parser",
            DesiredValue = "off",
            ManualRestoreArgumentsTemplate = ["config", "<previous>"]
        };
        var digest = Digest(Profile(change));
        Assert.NotEqual(digest, Digest(Profile(change with { ReadArguments = ["other"] })));
        Assert.NotEqual(digest, Digest(Profile(change with { ApplyArguments = ["other"] })));
        Assert.NotEqual(digest, Digest(Profile(change with { DesiredValue = "on" })));
        Assert.NotEqual(digest, Digest(Profile(change with { ManualRestoreArgumentsTemplate = ["other"] })));
    }

    [Fact(DisplayName = "Profile recipe order is preserved in the digest")]
    public void RecipeOrder()
    {
        var first = Json("true");
        var second = Json("false") with { Id = "second" };
        Assert.NotEqual(Digest(Profile(first, second)), Digest(Profile(second, first)));
    }

    [Fact(DisplayName = "Native home policy inventory declarations process bounds and output sets affect the digest")]
    public void PlanningProjection()
    {
        var profile = Profile(Json("true"));
        var digest = Digest(profile);
        Assert.NotEqual(digest, Digest(profile with { NativeHomePolicy = ToolNativeHomePolicy.HomeThenUserProfile }));
        Assert.NotEqual(digest, Digest(profile with { Setup = profile.Setup with { MaximumOutputBytes = profile.Setup.MaximumOutputBytes + 1 } }));
        var inventory = new ToolNativePlanSpec { Arguments = ["inventory"], Parser = ToolNativePlanParser.CbmInstallPlanV1, Timeout = TimeSpan.FromSeconds(5), MaximumOutputBytes = 100 };
        Assert.NotEqual(digest, Digest(profile with { Setup = profile.Setup with { NativePlan = inventory } }));
        var output = new ToolNativeOutputSpec { PathTemplate = "${workspace}/synthetic", Scope = ToolEffectScope.Workspace, Match = ToolNativeOutputMatch.Exact, Purpose = ToolNativeOutputPurpose.Backup };
        Assert.NotEqual(digest, Digest(profile with { Setup = profile.Setup with { PossibleOutputs = [output] } }));
    }

    [Fact(DisplayName = "Native stored-reader declarations and effective-ignore conditions affect the digest")]
    public void RecipeProjection()
    {
        var block = new ToolMarkedBlockChange
        {
            Id = "block",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.UniqueMarkedBlock,
            Description = "synthetic block",
            PathTemplate = ".gitignore",
            StartMarker = "start",
            EndMarker = "end",
            ContentTemplate = "${mainRoot}"
        };
        Assert.NotEqual(Digest(Profile(block)), Digest(Profile(block with { WhenNotIgnored = new("generated/") })));
        var native = new ToolNativeConfigChange
        {
            Id = "native",
            Scope = ToolEffectScope.Account,
            InitialState = ToolInitialConfigState.ReadableNativeValue,
            Description = "synthetic native",
            AllowListId = "synthetic-native",
            ApplyArguments = ["off"],
            DesiredValue = "off",
            ManualRestoreArgumentsTemplate = ["${previous}"]
        };
        var stored = new ToolNativeStoredValueSpec
        {
            PathTemplate = "${nativeHome}/setting.json",
            Parser = ToolNativeStoredValueParser.CodeGraphTelemetryV1,
            MissingValue = "on",
            MaximumBytes = 256
        };
        Assert.NotEqual(Digest(Profile(native)), Digest(Profile(native with { StoredValue = stored })));
        Assert.NotEqual(Digest(Profile(native with { StoredValue = stored })), Digest(Profile(native with { StoredValue = stored with { MaximumBytes = 512 } })));
    }

    private static string Digest(ToolVersionProfile profile) => ToolProfileDigest.Compute(profile, _ => Encoding.UTF8.GetBytes("synthetic bytes"));
    private static ToolVersionProfile Profile(params ToolConfigChange[] changes) => new()
    {
        Id = "synthetic-profile",
        NativeHomePolicy = ToolNativeHomePolicy.Node,
        Range = new(2, 3),
        TestedVersions = ["2.3.4"],
        MinimumToolVersion = "2.3.4",
        MinimumOpenForgeVersion = "1.0.0",
        Setup = Setup("install"),
        Uninstall = Setup("remove"),
        Changes = changes,
    };
    private static ToolSetupSpec Setup(string argument) => new()
    { Arguments = [argument], Environment = new Dictionary<string, string?>(), Effects = [], PossibleOutputs = [], MaximumOutputBytes = 4096, Timeout = TimeSpan.FromSeconds(20) };
    private static ToolJsonValueChange Json(string value)
    {
        using var document = JsonDocument.Parse(value);
        return new()
        {
            Id = "json",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.AbsentMember,
            Description = "synthetic JSON",
            PathTemplate = "synthetic.json",
            Pointer = "/member",
            Operation = ToolJsonValueOperation.SetIfAbsent,
            Value = document.RootElement.Clone()
        };
    }
}
