---
open-forge:
  description: Remove redundant option-delimiter restrictions and use native parser value forms consistently
  tags: [Memory, Working, Contextual, CLI, Task, Parsing, Refactoring]
---

# Native CLI Option Delimiters

## Frozen Stage

Task 27 continuation, COMPLETE phase 3/3, milestone 3/3, from local develop
`b4a06740`. The preceding catalogue, fixture and helper stage is squash-integrated
with exact tree `64e88bdee3337295ae5712dbff78422c3f707d0b`. Root works directly
in `/tmp/open-forge-cli-refactor-sequential`, branch `codex/cli-native-delimiters`.
The [follow-up plan](cli-dogfood-follow-up-plan.md) defines the sequential horizon.

The user requested ordinary library parsing for `--tag`, accepted the audit and
continued work. The accepted delta is native spaced, equals and colon value
forms for Find and Route tags, and Route List depth. Remove the obsolete raw
delimiter guard and its global aggregation/plumbing. Keep typed parser arity,
occurrence counts, tag order, validation, command selection, terminal boundaries,
domain behavior, JSON schemas and effects unchanged. Missing/empty values remain
invalid through existing typed validation. Their command-local invalid result
replaces the removed guard's premature shell diagnostic where applicable.

The Route Update attached-empty responsibility recognizer remains the separately
accepted `CLI-EDGE-016` exception. It addresses information erased by the pinned
parser. This stage does not change that grammar or implement the audit's other
YAML/Markdown proposals.

## Applicability And Evidence

This changes public syntax and shared Shell composition, so the complete managed
and supported Linux native gate apply. The realistic risk is accepting or binding
an unintended request before a file-mutating command. Existing request validation,
workspace containment, ownership, leases, preflight, verification and recovery
remain frozen. Git reverses source changes, not user file effects; tests own their
workspaces and use read-only parsing or dry-run/public Find evidence. No additional
concurrency or malicious-process guarantee is selected.

Pinned System.CommandLine 2.0.11 already provides all three forms, proven by the
audit's direct library probe. No dependency, custom parser or exceptional
machinery is needed. Reuse the existing command tree and typed binding facts.
Update the Architecture, affected Interfaces and edge-case record to retire the
old equals-only exception before depending on the new accepted syntax.

Freeze new composed-root parser and public Find evidence in a separate Red
commit. Existing semantic, malformed-input, terminal and effect assertions stay
fixed. Adapt only retired delimiter-policy structural tests and explicit
equals-only expectations; record each changed boundary. Then remove the guard,
qualify affected commands and the complete managed/native boundary, inspect the
final diff, update records and squash into local develop. No remote operation.

Beginning evidence is the exact preceding production/test tree: 3,231 Unit,
1,724 Integration in managed/native modes, 112 public tests in all three
execution modes, zero failures/skips. Task 30 holds its receipts and limits.

Frozen Red: 21 composed-root cases have seven passes and fourteen failures,
all at the redundant delimiter guard. The parser itself reports no errors.
The five selected public Find cases have three passes and two intended failures
for space/colon tags. Release build has zero warnings/errors. The initial test
build caught a nullable assertion argument; it was corrected before recording
behavioral Red. Receipts are `artifacts/task27-native-delimiters/red*.json` and
the adjacent logs. Existing command behavior assertions are otherwise unchanged.

Red is frozen at `9399da85`. Production removes the guard, policy model,
obsolete invalid-input category, four policy registrations and the Shell/root
aggregation plumbing. The materially changed Route List symbol model now lives
in its existing `Models/Binding` scope. Values, occurrence rules and operation
implementations are unchanged.

Evidence adaptations are limited to removed policy-shape assertions, equivalent
space/colon expectations in Route List and Init, and constructor arguments in
Shell/command test composition. Route List's bare depth remains invalid and now
uses its existing command-local JSON invalid result instead of the removed
shell-only delimiter diagnostic. Its unchanged workspace assertion remains.
The new Red cases and existing mutation, metadata, malformed-input, ordering,
terminal and attached-empty responsibility assertions stay frozen. One missed
constructor argument in a mechanical test-composition edit was corrected after
the first implementation build; no product behavior changed to accommodate it.

The first full managed run passes 3,232 Unit and 1,745 Integration tests,
including all frozen Red cases. Public evidence is 111 passes and three old
Route List equals-only expectations. Its existing four-case process table is
adapted to the accepted forms and typed missing-value JSON, preserving its
exit, requested-depth and unchanged-workspace assertions. New public Find Red
cases already pass. This is the same intentional policy delta recorded above;
there is no additional production correction.

After that process-table adaptation, the 27 selected public parser/Find cases
pass with zero failures/skips (`public-green.json`). Frozen new Red files are
byte-identical to `9399da85`. Required changed-C# whitespace/style checks pass;
the final public-table change follows the existing formatting and will be
checked before integration. The canonical native gate will also rerun all
managed suites against the final candidate. No full-gate success is claimed yet.

## Qualified Closeout

Candidate `5bef9a241432766d0468c1ac4ff985a6050fef7a` passes the canonical
managed/native gate: 3,232 Unit, 1,745 Integration in managed and native modes,
and 114 public tests in all three execution modes, zero failures or skips.
The delivery manifest is tested and the CLI SHA-256 is
`d73d14395c765bcd06ffb0c1a3407753e9466c7c57832bebb713a44b713bacf2`.
Receipts: `artifacts/task27-native-delimiters/native-{build,tests}.log`,
`native-qualification.json`, and delivery reports `reports-TjYBKp`.

Nineteen real native commands verify all three forms for Find, Route List,
Create, Init and Update, plus missing-value failures and unchanged workspaces
for read-only/dry-run requests. Required C# whitespace/style checks pass,
including the final public expectation adaptation; 17 changed Markdown files
have no unresolved local links. The new Red files remain byte-identical.
The installed global CLI is refreshed to this exact candidate.

Final review confirms the removed guard is no longer referenced, command
composition has no remaining delimiter-policy collection, native typed binding
owns the accepted forms, and the independent responsibility recognizer remains.
The initial old-expectation failures above are superseded by the final six-suite
gate. No mutation implementation or dependency changed. Local squash integration
is authorized and this completion record accompanies it; no remote effects.

Fresh Doctor journeys capture the next stage's defects in the audit. Their
expected non-success outcomes are diagnostic evidence, not a delimiter failure.

## Durable Qualification Summary

Generated logs and machine reports mentioned above are disposable. This tracked
summary, committed regression sources and ordinary build scripts retain the
required result and reproduction path. Exploratory task scripts are not build
inputs or a substitute for committed regressions.

- Qualified source: `5bef9a241432766d0468c1ac4ff985a6050fef7a`; local squash: `6b6f054b`.
- Native target: `linux-x64`; version: `0.0.0-dev.sha-5bef9a241432766d0468c1ac4ff985a6050fef7a`.
- Native CLI SHA-256: `d73d14395c765bcd06ffb0c1a3407753e9466c7c57832bebb713a44b713bacf2`.
- Gate result: 3,232 Unit; 1,745 Integration in both modes; 114 public in all three modes; zero failures/skips.
- Toolchain: .NET SDK 10.0.111, Node 24.19.0, npm 11.17.0, Linux x64.
- Reproduce from that source commit with repository dependencies restored:
  `npm run build:native -- --sha`, then `npm run test:built`.
  These tracked scripts generate fresh outputs and validate all six suites.
- Required regression sources: `src/cli/tests/`; build/test orchestration:
  `scripts/delivery/`. No required helper exists only in `artifacts/`.

These are recorded past results. Deleting outputs discards raw receipts and
binaries; rerun the commands before claiming fresh execution evidence.
