---
open-forge:
  description: Whether integration and end-to-end are still distinct layers, what the in-process entry point already covers, and the one class of behaviour only in-process testing can reach
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Testing, Architecture, Layers]
---

# Test Layer Consolidation

## Conclusion

They are not meaningfully different. The in-process entry point already accepts
`argv`, injected writers, an injected environment and a cancellation token, and
returns the exit code — so integration can reproduce essentially everything
end-to-end asserts, without a process spawn.

End-to-end has a genuine but **small** remaining job: roughly six to ten tests,
not 106.

The more useful finding is the reverse. **There is a class of behaviour only
in-process testing can reach at all** — anything requiring a non-redirected
stdin. A spawned test process always has redirected streams, so the entire
interaction layer is invisible to end-to-end by construction. That is part of why
it is broken.

## The in-process entry point

```csharp
// CliCoreApplication.cs:32
internal async ValueTask<CliProcessCompletion> RunAsync(
    string[] arguments,
    CliProcessEnvironment environment,
    CliOutputWriters writers,
    CancellationToken cancellationToken)
```

returning:

```csharp
internal sealed record CliProcessCompletion(
    CliSemanticStatus Status,
    int ExitCode,
    CliOutputTarget PrimaryOutputTarget);
```

That single signature covers, in-process:

| End-to-end concern                | Covered by `RunAsync`?                               |
| --------------------------------- | ---------------------------------------------------- |
| Argument parsing from `argv`      | **yes** — same `_tree.Parse(arguments)`              |
| Exit code                         | **yes** — `CliProcessCompletion.ExitCode`            |
| stdout vs stderr routing          | **yes** — `CliOutputWriters` + `PrimaryOutputTarget` |
| Cancellation                      | **yes** — the token                                  |
| Environment / workspace selection | **yes** — `CliProcessEnvironment`                    |
| Full rendered human output        | **yes** — the injected writers capture it            |
| JSON projection                   | **yes** — same path                                  |

And the composition root is already constructed from tests:

```csharp
// NativeOptionDelimiterIntegrationTests.cs:27
var application = CliCompositionRoot.Create(new CliProcessIdentity("open-forge", "test"));
var parse = CliCoreApplicationAccess.Tree(application).Parse([.. arguments]);
```

So the seam is not hypothetical — one integration test already parses real
`argv` through the real composed tree. It simply stops at `Parse` instead of
continuing to `RunAsync`.

## What end-to-end still uniquely proves

Short, and worth keeping:

- **The AOT-published binary builds and runs at all.** A smoke test per command
  family; trimming and Native AOT can break reflection-adjacent code that the
  managed run never exercises.
- **Real OS signals.** `CliProcessTests` — _"Published process cancellation kills
  and drains the owned child"_ — genuinely needs a child process.
- **Real console redirection detection.** The value of
  `Console.IsInputRedirected` at process start, as opposed to the injected flag.
- **The relocated executable and its embedded payload.**
- **File-descriptor-level stream separation**, as distinct from two writer
  objects.
- **The npm wrapper and launcher parity.**

That is six concerns. The current suite spends **172 process-spawn call sites
across 106 tests** to cover them plus a large duplicated surface — 87 of 91 runs
going through `--json` to assert schema shape and exit codes, all of which
`RunAsync` returns directly.

## The class only in-process can reach

`CliCompositionInputs` is fully injectable:

```csharp
public required TextReader StandardInput { get; init; }
public required TextWriter PromptOutput { get; init; }
public required bool StandardInputRedirected { get; init; }
public required bool PromptOutputRedirected { get; init; }
```

and `CliCompositionRoot.CreateInteractiveSession` derives:

```csharp
canPrompt: !inputs.StandardInputRedirected && !inputs.PromptOutputRedirected
```

A spawned test process **always** has redirected stdin and stdout. So under
end-to-end, `CanPrompt` is permanently `false` and every interactive path is
unreachable:

- the `repair` wizard,
- the `install` confirmation prompt,
- the `extension install` package selection,
- the workspace permission prompt.

Every one of those is defective, and none of them has a single end-to-end test
that could have caught it — not because nobody wrote one, but because the
harness cannot express the precondition.

In-process, `StandardInputRedirected = false` with a `StringReader` simulates a
terminal exactly. **Interaction (G5) is testable only here.** Any plan that
leaves interaction coverage to end-to-end is planning to leave it untested.

## Proposed layering

Three layers, not four. The scenarios layer proposed earlier collapses into the
in-process layer, which is the same consolidation this document argues for.

| Layer             | Entry point                                                                           | Owns                                                                                                                                                                                |
| ----------------- | ------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Unit**          | direct type construction                                                              | fragment rendering, vocabulary closure, planner decisions, parsers                                                                                                                  |
| **In-process**    | `CliCoreApplication.RunAsync(argv, env, writers, ct)` against a seeded temp workspace | **everything a user experiences**: rendered output snapshots per command/status/detail, exit codes, stream routing, multi-command sequences, interactive flows, cross-command state |
| **Process smoke** | spawn the published binary                                                            | the six concerns above; a few dozen tests at most                                                                                                                                   |

Rename the middle layer to something that says what it is — `Journeys`,
`Behavior`, or keep `Integration` — but its defining property is _the real CLI,
from argv, without a process_.

## What consolidation buys

- **Speed.** A real `doctor` run on this repository takes **7.4 seconds** as a
  process. In-process it is a fraction of that, and there are 172 spawn sites to
  reclaim.
- **Interaction coverage becomes possible** for the first time.
- **One harness instead of three.** Today a seeded workspace is built three
  different ways: `StatusIntegrationWorkspace`, `PublishedStatusWorkspace`, and
  the proposed scenario seeds. One seed catalogue serves all of it.
- **The snapshot layer and the sequence layer stop being separate ideas.** A
  full-command snapshot and a multi-command journey are the same mechanism with
  a different number of `RunAsync` calls.

## Caveats

Two things not to lose in the merge.

**The write-freedom harness must move, not die.** The base64 before/after tree
snapshot that proves read-only commands write nothing is genuinely good and is
currently end-to-end. It applies just as well in-process and should follow the
tests it guards.

**Keep AOT execution for the in-process layer.** The integration assembly is
already published and run natively (`native-integration` in `layout.ts:33`). That
is what makes in-process testing a real substitute rather than a managed-only
approximation — it exercises the same trimming and AOT constraints the shipped
binary does. Preserve it, and keep the snapshot mechanism AOT-safe accordingly.

**One risk to name:** `RunAsync` bypasses `Program.Main` and whatever it does
before composition — console encoding setup, culture, global exception handling.
Whatever lives there is only covered by process smoke tests, so it should be kept
deliberately thin, and what remains should be listed in the smoke suite.
