using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Shared.Content;

public sealed class FrameworkManagedHostIdentityTests
{
    [Theory(DisplayName = "Managed identity preserves Unicode character offsets and exact UTF-8 host bytes")]
    [InlineData("# Open Forge\r\n\r\nCafé 🛠\r\n\r\n**End of Open Forge managed section.**\r\n")]
    [InlineData("<!-- open-forge:start -->\r\n# Open Forge\r\n\r\nCafé 🛠\r\n<!-- open-forge:end -->\r\n")]
    [Trait("Feature", "framework-distribution"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BytesAndOffsetsArePreserved(string host)
    {
        const string prefix = "\uFEFFPréface 🛠\r\n\r\n";
        const string suffix = "\r\n## Authored\r\nKeep me.\r\n";
        var source = $"{prefix}{host}{suffix}";
        var identity = new FrameworkContentIdentity();

        foreach (var result in new[] { identity.ReadManagedBlock(source), identity.ReadManagedBlock(Encoding.UTF8.GetBytes(source)) })
        {
            Assert.Equal(FrameworkManagedBlockState.Present, result.State);
            Assert.Equal(prefix.Length, result.Start);
            Assert.Equal(prefix.Length + host.Length, result.EndExclusive);
            Assert.Equal(Encoding.UTF8.GetBytes(host), result.ExistingBlockBytes);
            Assert.Equal(prefix, source[..result.Start!.Value]);
            Assert.Equal(suffix, source[result.EndExclusive!.Value..]);
        }
    }

    [Fact(DisplayName = "Invalid UTF-8 blocks managed host recognition")]
    [Trait("Feature", "framework-distribution"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void InvalidUtf8IsBlocked()
    {
        var result = new FrameworkContentIdentity().ReadManagedBlock(new byte[] { 0xFF, 0xFE });

        Assert.Equal(FrameworkManagedBlockState.Blocked, result.State);
        Assert.NotNull(result.Cause);
        Assert.Null(result.ExistingBlockBytes);
    }

    [Theory(DisplayName = "Managed identity maps absent and invalid parsed facts without owning syntax")]
    [InlineData("Ordinary authored text", false)]
    [InlineData("# Open Forge", true)]
    [InlineData("<!-- open-forge:start -->", true)]
    [Trait("Feature", "framework-distribution"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void ParsedStatesAreMapped(string source, bool blocked)
    {
        var result = new FrameworkContentIdentity().ReadManagedBlock(source);

        Assert.Equal(blocked ? FrameworkManagedBlockState.Blocked : FrameworkManagedBlockState.Absent, result.State);
        Assert.Null(result.Start);
        Assert.Null(result.EndExclusive);
        Assert.Null(result.ExistingBlockBytes);
    }
}
