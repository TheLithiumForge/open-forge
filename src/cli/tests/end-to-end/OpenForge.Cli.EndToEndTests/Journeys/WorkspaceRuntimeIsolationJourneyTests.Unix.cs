using System.Diagnostics;
using System.Net.Sockets;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed partial class WorkspaceRuntimeIsolationJourneyTests
{
    [Theory(DisplayName = "Unix special sentinel targets refuse a published writer without changing authored files")]
    [InlineData("fifo"), InlineData("socket"), InlineData("symlink")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task UnixSpecialSentinelsRefuseWriter(string kind)
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            Assert.Skip("Unix FIFO, socket and symlink qualification requires Linux or macOS.");
            return;
        }

        using var workspace = PublishedJourneyWorkspace.Create("special");
        workspace.ExpectCoreInstall();
        var runtime = Directory.CreateDirectory(workspace.Combine(Runtime));
        File.WriteAllText(Path.Combine(runtime.FullName, ".gitignore"), "*\n");
        var sentinel = Path.Combine(runtime.FullName, "workspace.lock");
        var socketPath = workspace.Combine("socket");
        Socket? socket = null;
        try
        {
            switch (kind)
            {
                case "fifo":
                    var start = new ProcessStartInfo("mkfifo") { UseShellExecute = false, CreateNoWindow = true };
                    start.ArgumentList.Add(sentinel);
                    using (var create = Process.Start(start) ?? throw new InvalidOperationException("mkfifo did not start."))
                    {
                        try
                        {
                            await create.WaitForExitAsync(TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
                            Assert.Equal(0, create.ExitCode);
                        }
                        finally
                        {
                            if (!create.HasExited)
                            {
                                create.Kill(entireProcessTree: true);
                                await create.WaitForExitAsync(CancellationToken.None);
                            }
                        }
                    }
                    break;
                case "socket":
                    socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    var relativeSocket = Path.GetRelativePath(Environment.CurrentDirectory, socketPath);
                    var address = relativeSocket.Length < socketPath.Length ? relativeSocket : socketPath;
                    socket.Bind(new UnixDomainSocketEndPoint(address));
                    File.Move(socketPath, sentinel);
                    break;
                case "symlink":
                    workspace.WriteText("target.txt", "Preserved target\n");
                    File.CreateSymbolicLink(sentinel, workspace.Combine("target.txt"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            var before = workspace.SnapshotState();
            var refused = await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic"));
            Assert.Equal(5, refused.ExitCode);
            Assert.Equal(before, workspace.SnapshotState());
            Assert.Contains(sentinel, refused.StandardError, StringComparison.Ordinal);
            Assert.False(Directory.Exists(workspace.Combine(".agents")));
            Assert.True(File.Exists(sentinel));
        }
        finally
        {
            socket?.Dispose();
            File.Delete(socketPath);
            File.Delete(sentinel);
        }
    }
}
