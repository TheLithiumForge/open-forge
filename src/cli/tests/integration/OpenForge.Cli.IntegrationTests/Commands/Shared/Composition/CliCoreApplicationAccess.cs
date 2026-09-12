using System.Runtime.CompilerServices;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;

internal static class CliCoreApplicationAccess
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_tree")]
    internal static extern ref CliCommandTree Tree(CliCoreApplication application);
}
