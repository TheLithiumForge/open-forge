using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

// Registration point: Writers bind Services and retain permission, exclusion, backup and write control.
internal sealed class ToolEntryPlanner
{
    internal ToolEntryServices Services => new(PlanAsync, ObserveAsync);

    internal ValueTask<ToolEntryPlan> PlanAsync(ToolEntryRewriteRequest request, CancellationToken token)
        => ReadAsync(request, observe: false, token);

    internal async ValueTask<ToolEntryCheck[]> ObserveAsync(ToolEntryRewriteRequest request, CancellationToken token)
        => (await ReadAsync(request, observe: true, token).ConfigureAwait(false)).Checks;

    internal ToolEntryPlan Plan(ToolEntryRewriteRequest request, IReadOnlyDictionary<string, byte[]?> files)
        => Evaluate(ToolEntryDeclarations.Resolve(request), files, observe: false);

    internal ToolEntryCheck[] Observe(ToolEntryRewriteRequest request, IReadOnlyDictionary<string, byte[]?> files)
        => Evaluate(ToolEntryDeclarations.Resolve(request), files, observe: true).Checks;

    private async ValueTask<ToolEntryPlan> ReadAsync(ToolEntryRewriteRequest request, bool observe, CancellationToken token)
    {
        request = ToolEntryDeclarations.Resolve(request);
        var files = new Dictionary<string, byte[]?>(StringComparer.Ordinal);
        var unavailable = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in Files(request))
        {
            token.ThrowIfCancellationRequested();
            try { files[file] = await ToolRegistrationFiles.ReadAsync(request.WorkspaceRoot, file, token).ConfigureAwait(false); }
            catch (Exception exception) when (ToolRegistrationFiles.IsReadFailure(exception)) { unavailable[file] = exception.Message; }
        }
        var plan = Evaluate(request, files, observe);
        var checks = plan.Checks.Select(check => unavailable.TryGetValue(check.File, out var cause)
            ? check with { State = ToolEntryState.Unavailable, Cause = cause } : check).ToArray();
        return new(checks.Any(Failed) ? [] : plan.Edits, checks);
    }

    private static ToolEntryPlan Evaluate(ToolEntryRewriteRequest request, IReadOnlyDictionary<string, byte[]?> files, bool observe)
    {
        var edits = new List<ToolEntryEdit>();
        var checks = new List<ToolEntryCheck>();
        foreach (var file in Files(request))
        {
            var rewrites = request.Rewrites.Where(rewrite => rewrite.File == file).ToArray();
            var removals = request.DeclinedComponents.SelectMany(component => component.DeclinedEntries.Where(entry => entry.File == file)
                .Select(entry => (component.Id, Entry: entry))).ToArray();
            var subjects = rewrites.Select(rewrite => new ToolEntryCheck(rewrite.Id, rewrite.Host, file, ToolEntryState.Original, null))
                .Concat(removals.Select(removal => new ToolEntryCheck(removal.Id, removal.Entry.Host, file, ToolEntryState.Original, null))).ToArray();
            if (!files.TryGetValue(file, out var before) || before is null)
            {
                checks.AddRange(subjects.Take(rewrites.Length).Select(check => check with
                {
                    State = ToolEntryState.Missing,
                    Cause = "The declared entry file is missing.",
                }));
                checks.AddRange(subjects.Skip(rewrites.Length).Select(check => check with
                {
                    State = observe ? ToolEntryState.Declined : ToolEntryState.Missing,
                    Cause = observe ? null : "The declared entry file is missing.",
                }));
                continue;
            }
            try
            {
                using var json = rewrites.Any(rewrite => rewrite is ToolJsonCommandRewrite or ToolHookCommandRewrite) || removals.Length > 0 ? new ToolJsonEntries(before) : null;
                if (observe)
                {
                    checks.AddRange(rewrites.Select(rewrite => Check(rewrite, before, json)));
                    checks.AddRange(removals.Select(removal => new ToolEntryCheck(removal.Id, removal.Entry.Host, file,
                        json?.IsAbsent(removal.Entry) == true ? ToolEntryState.Declined : ToolEntryState.Drifted, null)));
                    continue;
                }
                var changes = new List<ToolEntrySpan>();
                foreach (var rewrite in rewrites)
                {
                    changes.AddRange(rewrite switch
                    {
                        ToolJsonCommandRewrite command when json is not null => json.Rewrite(command),
                        ToolHookCommandRewrite hook when json is not null => [json.Rewrite(hook)],
                        ToolTomlCommandRewrite toml => ToolTextEntries.Rewrite(before, toml),
                        ToolMarkedLineRewrite line => [ToolTextEntries.Rewrite(before, line)],
                        _ => throw new InvalidDataException("The entry format is unavailable."),
                    });
                }
                if (json is not null) changes.AddRange(json.Remove([.. removals.Select(removal => removal.Entry)]));
                var after = ToolRegistrationBytes.Replace(before, changes);
                var validation = Evaluate(request with
                {
                    Rewrites = rewrites,
                    DeclinedComponents = [.. request.DeclinedComponents.Select(component => component with
                { DeclinedEntries = [.. component.DeclinedEntries.Where(entry => entry.File == file)] })]
                },
                    new Dictionary<string, byte[]?> { [file] = after }, observe: true);
                if (validation.Checks.Any(check => check.State is not (ToolEntryState.Matching or ToolEntryState.Declined)))
                    throw new InvalidDataException("The planned entries did not pass replacement validation.");
                checks.AddRange(subjects);
                if (!before.AsSpan().SequenceEqual(after)) edits.Add(new(file, before, after, [.. subjects.Select(check => check.Id).Distinct(StringComparer.Ordinal)]));
            }
            catch (Exception exception) when (ToolRegistrationFiles.IsShapeFailure(exception))
            {
                checks.AddRange(subjects.Select(check => check with { State = ToolEntryState.Drifted, Cause = exception.Message }));
            }
        }
        return new(checks.Any(Failed) ? [] : [.. edits], [.. checks]);
    }

    private static ToolEntryCheck Check(ToolEntryRewrite rewrite, byte[] bytes, ToolJsonEntries? json)
    {
        var matching = rewrite switch
        {
            ToolJsonCommandRewrite command => json?.Matches(command.Pointer, command.Replacement) == true,
            ToolHookCommandRewrite hook => json?.Value(hook.Pointer) is { ValueKind: System.Text.Json.JsonValueKind.String } value && value.GetString() == hook.Replacement,
            ToolTomlCommandRewrite toml => ToolTextEntries.Matches(bytes, toml, replacement: true),
            ToolMarkedLineRewrite line => ToolTextEntries.Matches(bytes, line, replacement: true),
            _ => throw new InvalidDataException("The entry format is unavailable."),
        };
        return new(rewrite.Id, rewrite.Host, rewrite.File, matching ? ToolEntryState.Matching : ToolEntryState.Drifted,
            matching ? null : "The entry differs from its recorded replacement.");
    }

    private static bool Failed(ToolEntryCheck check) => check.State is ToolEntryState.Drifted or ToolEntryState.Missing or ToolEntryState.Unavailable;
    private static string[] Files(ToolEntryRewriteRequest request) => [.. request.Rewrites.Select(rewrite => rewrite.File)
        .Concat(request.DeclinedComponents.SelectMany(component => component.DeclinedEntries.Select(entry => entry.File))).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
}
