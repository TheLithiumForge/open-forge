---
open-forge:
  description: Shared execution rules, verification commands, and recurring composition patterns for every numbered slice plan
  tags: [Memory, Working, CLI, Task, Plan, Contextual, Active, KeepInMind]
---

# 00 — Slice conventions

Read once before any numbered slice. Every plan assumes these and does not repeat
them.

## Verification

Run the assemblies directly. `npm run test` stops at integration on Windows.

```
npm run build
./artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.exe --parallel collections --no-ansi --progress off --minimum-expected-tests 1
./artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.exe --parallel collections --no-ansi --progress off --minimum-expected-tests 1
./artifacts/bin/OpenForge.Cli.EndToEndTests/release/OpenForge.Cli.EndToEndTests.exe --parallel collections --no-ansi --progress off --minimum-expected-tests 1
npm run check:dotnet
```

Those three are the **managed** target. The same Integration and EndToEnd
projects also run as **Native AOT** executables through `npm run test:built`,
which selects six suites: the three managed assemblies, `native-integration`
and `native-public` (the test projects published native), and `public-native`
(the managed EndToEnd project driving the native CLI). A managed pass is not
evidence for the AOT gate. The supported Windows Native AOT gate requires
`vswhere` on `PATH` and cannot be run by a sandboxed worker; the overseer runs
it on an unsandboxed host. See the
[execution cost and runtime target decision](phase-7-8-test-architecture.md#decision--execution-cost-and-runtime-targets).

A plan step saying "unit green", "integration green", or "suites green" means
running the relevant command above.

### Pass conditions

**Counts are not pass conditions.** Slices add tests, so totals rise. Judge by:

- `failed: 0` in every suite.
- `skipped: 17` in integration, unchanged. These are Unix-only and correct on
  Windows. A different number is a real change and must be explained.
- `skipped: 0` in unit and e2e.
- `npm run check:dotnet` reports exactly **5** whitespace errors, all in
  `ReferencesOperation.cs` and `ExtensionListApplicationIntegrationTests.cs`,
  which no slice touches. More than 5 means your new files are unformatted.

Rebuild before judging a suite. A stale `artifacts/bin` runs yesterday's tests.

### Known flaky test

`Route Update cancellation after the first effect retains its receipt and stops
later effects` fails intermittently and passes on rerun. Observed before and
after unrelated slices. Rerun once; if it fails twice, it is real.

## Line numbers

Every `file.cs:123` in a plan is a **hint recording where the site was when the
plan was written**, not a guarantee. Edits move lines. Find the site by the named
symbol; if it has moved, that is not a divergence worth recording.

A named symbol that no longer exists **is** a divergence. Stop and record it.

## Recurring composition pattern

Slices A1 to A3 add a lock write to a command that already writes a state record.
Every one follows the same shape. A1 and A2 each stalled because a plan described
the outcome instead of this shape; it is written here once.

1. **Read.** Await `WorkspaceOwnershipReader.ReadAsync` wherever the command
   already awaits its existing record read.
2. **Thread.** Add `WorkspaceOwnershipRead Ownership` to the planning input
   record, beside the existing record read. **The property is called
   `Ownership` in every command**, as Install, Route Init and Extension Install
   already do. If a command already uses that name for something else, rename
   the existing one rather than inventing a second name here; Library's
   `LifecycleOwnershipReadResult Ownership` becomes `LifecycleOwnership` for
   exactly this reason.
3. **Slot.** Add a private `PlannedFileChange? OwnershipChange` to the plan,
   beside the existing record's change. **Not** a member of any public effect
   list.
4. **Fold.** Add it to every derived member that already folds in the existing
   record's change — the all-changes list, the recovery targets, the no-op test,
   the requires-recovery test. **This is what makes preflight, revalidation,
   recovery preparation and application cover it.** There is no separate wiring
   for those stages; they all read the derived members.
5. **Apply.** Handle it beside the existing record's change in the application
   operation.
6. **Exclude.** Find the single place the existing record's identity is added to
   the public result and do not add the lock's. Everything else is internal.

### Rules that apply to every lock write

- **A `Skipped` ownership result yields a null change and no finding.** The
  neighbouring lifecycle branch blocks the command when its write plan is
  blocked. Do not copy that. The lock never gates: a skipped write means the
  command proceeds and the lock goes stale, which is the accepted failure
  direction.
- **The lock is written wholesale** and does not preserve unknown members. It is
  machine-owned and regenerable. This is deliberately the opposite of the
  authored `open-forge.json`.
- **An identical intended ownership must plan `Unchanged`**, not a rewrite. If a
  command run twice rewrites the lock, that is a defect.
- **Ownership comes from the intended state the command already computes**, not
  from write receipts. Writes are deduplicated, so a path shared by two owners
  has one receipt and would be misattributed. The receipt requirement is
  satisfied structurally instead: the state record is published only after every
  effect verifies, and the lock rides the same plan.

## Output-changing slices

A4 to A7 change output. For each:

- **Capture the reviewed snapshot before the change.** A snapshot captured after
  proves nothing. If no baseline exists, capture it, review it, and commit it as
  its own commit first.
- **Update the command's contract in the same commit as the behaviour**, not in a
  later pass. See
  [why this is stated rather than left to the Evergreen axiom](../../../../emerging/ideas/evergreen-drift-signal.md).
- Review the snapshot diff afterwards. It must show only what the slice
  intended. Anything else is a divergence: record it and stop.

## House rules

- Write LF, never CRLF. Python's `write_text` converts on Windows; pass
  `newline="\\n"` or write bytes.
- Do not run Prettier over `src/open-forge/` or `src/extensions/`. Shipped
  payload, fenced in `.prettierignore`. Prettier over `.agents/` is expected.
- If a payload parity test fails, delete the stale staged copies and rebuild:
  `artifacts/bin/OpenForge.Cli.IntegrationTests/release/FrameworkPayloadSource`
  and `.../ExtensionCatalogue`. Artifacts carry future timestamps, so
  `PreserveNewest` serves stale bytes.
- Leave `scripts/delivery/test-suites.ts` alone.
- Commit messages: past tense, actionable, no `type(scope):` prefix, subject line
  only. Commit; do not push.

## Stopping

Stop and record under the slice's **Divergences observed** when a step needs a
decision the plan does not make: scope, naming, ownership, or whether something
is safe. Do not infer it. Both stalls so far were correct stops, and both were
resolved by pointing at code that already answered the question.
