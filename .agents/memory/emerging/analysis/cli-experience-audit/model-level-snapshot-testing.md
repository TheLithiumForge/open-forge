---
open-forge:
  description: Where full rendered-command snapshots belong, why the integration layer is the right home, the AOT constraint that decides the tooling, and what the snapshot mechanism actually needs
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Testing, Snapshot, Presentation, Pipeline]
---

# Full-Command Snapshot Testing

## Conclusion

The idea is right and the pipeline already supports it end to end. Three
answers:

- **Layer: integration.** The whole pipeline — real workspace, real operation,
  real model, real renderer — already runs in-process there today. Only the
  argument parser and the process boundary are excluded, and neither is worth
  the cost of a process spawn.
- **Tooling: finish your own, and the reason is AOT.** The integration suite is
  published with `PublishAot` and executed as a **native binary** in a suite
  named `native-integration`. Reflection-based snapshot libraries (Verify,
  Snapshooter) are the wrong shape for that. But a rendered command is already a
  `string`, so no serializer is needed at all — the AOT-safe mechanism is also
  the simplest one.
- **Sequencing: write the invariants now, the snapshots after G4.** Snapshots
  taken against output you intend to replace become the thing that blocks
  replacing it — which has already happened twice in this repository.

## The pipeline already runs in-process

`StatusIntegrationWorkspace.CreateInstalledAsync` creates a real temporary
workspace and executes the real operation directly:

```csharp
var result = await InstallOperationFactory.Create(
        new CliInteractiveSession(input, prompt, canPrompt: false),
        workspace.LockStoreRoot)
    .ExecuteAsync(new InstallRequest(workspace.Workspace, InstallMode.Apply, …), ct);
```

And integration tests already reach the renderers:

```csharp
ContextHumanRenderer.Render(CliPresentationStage.Create(…));   // ContextOperationFindingTests.cs:134
ExtensionListHumanRenderer.Render                              // ExtensionListApplicationIntegrationTests.cs:85
FindCompactRenderer.Render(result, CliHumanStyle.Plain);       // FindApplicationIntegrationTests.cs:377
```

So **real filesystem → real operation → typed result → rendered string** is
already a supported path. Nobody has ever snapshotted the string; the renderer
calls exist to make scattered `Assert.Contains` checks.

That is the whole gap. Not a missing capability — a missing habit.

### What this layer does and does not cover

Excluded: argument parsing, exit-code mapping, stdout/stderr routing, and
process cancellation. Those stay in end-to-end, where they belong and are
already well covered.

Included: everything a user reads. Which means a full-command snapshot here
catches the presentation defects _and_, because the model is produced by the
real operation against a real workspace, a large share of the pipeline defects
too — unlike the hand-built-model approach, which would catch none of them.

That is the important correction: **because it runs the real operation, it is
strictly more powerful than snapshotting a hand-constructed model.** Of the
eight ship-blockers in this audit, a hand-built-model test catches zero; a
real-workspace integration snapshot catches most, because a wrong model produces
wrong text.

## The AOT constraint decides the tooling

`OpenForge.Cli.IntegrationTests.csproj`:

```xml
<PublishAot>true</PublishAot>
<DefineConstants>$(DefineConstants);XUNIT_AOT</DefineConstants>
```

and `scripts/delivery/layout.ts:33`:

```ts
{ name: "native-integration", executable: `${nativeDirectory(rid)}/integration/…${suffix}` }
```

The integration assembly is run **twice** — once managed, once as a published
native executable. Anything added to that project must survive trimming and
Native AOT.

That rules out the usual libraries: Verify and Snapshooter both serialize
_objects_ through reflection-based writers. They would either trim-warn or fail
in `native-integration`.

**But none of that machinery is needed here.** The thing being snapshotted is
already a `string`. The entire mechanism is:

1. resolve the snapshot file path,
2. read it if it exists,
3. compare ordinal,
4. on mismatch, fail with a diff — or overwrite when updating.

No serializer, no reflection, no source generation. That is trivially AOT-safe,
and it is a strong argument for finishing your own tool rather than adopting
one: **you need roughly 200 lines, and the libraries are large because they
solve the object-serialization problem you do not have.**

### The two mechanics that actually matter

**Locating the snapshot file under a native runner.** The native executable runs
from `artifacts/publish/<rid>/integration/`, not the source tree, so a relative
path will not find `__snapshots__/`. `[CallerFilePath]` is resolved at compile
time, is AOT-safe, and gives the test's own source path — which is how modern
C# snapshot libraries solve this. Anchor the snapshot directory to that:

```csharp
internal static void MatchSnapshot(
    string actual,
    [CallerFilePath] string sourceFile = "",
    [CallerMemberName] string testName = "")
```

Embedding snapshots as resources is the other option, but it makes them
unupdatable, which defeats the point.

**Update via environment variable, not a CLI flag.** Argument passing differs
between `dotnet test`, the Microsoft.Testing.Platform runner, and a bare native
executable. `OPENFORGE_SNAPSHOT_UPDATE=1` works identically in all three, and
maps cleanly to one npm script:

```
npm run test:snapshots:update
```

## Normalization is the part that decides whether it survives

More important than everything above. Without scrubbing, this suite is flaky on
this repository _today_, for reasons the audit already documented:

| Volatile value                    | Evidence from this audit                                          |
| --------------------------------- | ----------------------------------------------------------------- |
| Absolute workspace paths          | printed in every command's header                                 |
| Path separators and `\\` escaping | `route list` prints `<workspace>\\…`, `status` prints single |
| Line endings                      | CRLF blocks the workspace entirely; it will also churn snapshots  |
| Version string                    | `0.0.0-dev.sha-62b0e23e…` appears in `--version` and lifecycle    |
| SHA-256 fingerprints              | `route create` prints them as `Before:` / `Expected:`             |
| Byte and token counts             | vary with payload; keep real only where asserted                  |
| Non-UTF-8 bytes                   | `route inspect` emits `0xFA` as a separator                       |

Scrub the first five to stable placeholders. **Do not scrub the last two** —
a snapshot that hides a `0xFA` byte or a wrong count is worse than no snapshot.
That distinction is the design decision in the tool.

## Why the current tests are brittle, specifically

Naming the mechanisms, since the instinct that they are bad is correct and worth
grounding.

- **Substring assertions cannot fail on noise.** `Assert.Contains("Total
available context", output)` passes whether the output is 4 lines or 104. The
  suite has **211 human-text assertions** across 2,811 tests, nearly all of this
  shape.
- **Fragment snapshots freeze bugs.** `DoctorHumanSnapshots` is a 30-line
  `const string` containing the `Observed:` / `Read from:` detachment defect as
  its expected value.
- **Verbatim strings pin bad copy.** `PublishedDoctorProcessTests` asserts
  _"Typed Extension bridge-registration role and observed-state authority is
  unavailable; no target role was inferred."_ Rewriting that sentence is a test
  failure.
- **`const string` snapshots cannot be regenerated.** No update command exists,
  so they are edited by hand, which is why they stayed small enough to miss the
  problem.
- **Tests assert model properties, not outcomes.** _"retains the admitted failure
  and its exact direct cause"_, _"maps every non-contained candidate state"_.
  They prove the model is self-consistent. Nothing proves the tool is usable.

A file-based full-command snapshot fixes four of those five by construction: the
whole output is the assertion, it regenerates, it is reviewable as a diff, and it
is too large to hand-maintain into irrelevance.

The fifth — asserting outcomes rather than properties — is a naming discipline,
not a mechanism. Snapshot files should be named for situations:
`doctor/healthy-workspace.txt`, `install/into-existing-skill-workspace.txt`,
`extension-install/after-route-added.txt`.

## The sequencing risk

Two artifacts in this repository already demonstrate it: `DoctorHumanSnapshots`
freezes a rendering bug, and the e2e suite pins unreadable copy verbatim.

Writing 300 full-command snapshots against today's output would multiply that by
two orders of magnitude and make the G4 rewrite substantially more expensive —
every deleted noise line becomes a snapshot diff to review and approve.

**Order:**

1. **Now** — cross-command invariants, which need no snapshots and express the
   contract G4 will establish rather than current behaviour:
   `lines(brief) ≤ lines(standard) ≤ lines(verbose)`; no output exceeds its
   `outputLines` threshold; errors before warnings before informational; a
   zero-finding result never contains `findings:`; output is valid UTF-8 with no
   `\uXXXX`, `\\` or literal `\n`; rendering is idempotent. Several fail
   immediately — that is the point.
2. **Now** — finish the snapshot tool and prove it on two or three commands. The
   tool is not what freezes anything; the breadth of coverage is.
3. **After G4's selection layer** — full coverage, one snapshot per command per
   status per detail level.
4. **Delete** `DoctorHumanSnapshots` and the verbatim e2e strings as part of G4,
   rather than migrating them.

## How this changes the scenarios plan

Materially, and for the better. Most of what
[test-strategy-and-scenarios.md](test-strategy-and-scenarios.md) proposed to
assert through transcript scenarios is better served here: same real workspace,
same real operation, same rendered output, but **no process spawn**. A real
`doctor` run on this repository takes **7.4 seconds**; an in-process render of
the same state is a fraction of that.

Revised division:

| Layer                                    | Owns                                                                                                                                                        |
| ---------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Unit**                                 | fragment rendering, vocabulary closure, planner decisions                                                                                                   |
| **Integration + full-command snapshots** | seeded workspace → real operation → rendered output, per command per status per detail level. The bulk of the coverage.                                     |
| **End-to-end**                           | argument parsing, exit codes, stream routing, cancellation, the relocated executable, write-freedom                                                         |
| **Scenarios**                            | multi-command journeys where the _sequence_ is the subject — `install` → `route init` → `extension install`. Far fewer than the twelve originally proposed. |

The seeds from the scenarios plan are still needed — they become the integration
fixtures. The journeys shrink to the handful where cross-command state is the
thing under test, which is exactly where the audit's ship-blockers live.

The coverage-matrix meta-test still applies, and now targets the snapshot set:
every `(command, status, detail)` triple must have a snapshot file, and the build
fails when one is missing.
