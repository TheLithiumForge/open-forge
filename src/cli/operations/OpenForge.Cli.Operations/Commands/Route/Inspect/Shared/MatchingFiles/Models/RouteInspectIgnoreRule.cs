using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles.Models;

internal sealed record RouteInspectIgnoreRule(string BaseDirectory, ApplyToPattern Pattern, bool Include, bool DirectoryOnly);
