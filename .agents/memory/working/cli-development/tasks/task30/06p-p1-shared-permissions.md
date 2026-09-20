---
open-forge:
  description: Close Task 30 G1 shared allow-list admission and retire the remaining per-owner permission subsystem
  tags: [Memory, Working, CLI, Task, Plan, Contextual, Active]
---

# P1 — Shared Allow-list Closeout

Read [slice conventions](00-conventions.md). This closes G1 steps 3a/3b omitted
from the original numbered sequence; [A6-R8](06-a6-delete-lifecycle.md#divergences-observed)
records the discovery. It follows the verified A6 commit and precedes B1/M1.

## Goal

Every gated Extension and Library command uses only the shared
`allowInstallPaths` in `.agents/open-forge.json`, with implicit `.agents/`
admission and existing reserved-path/physical checks. The retired permission
file is neither read, honoured, written, migrated nor deleted.

## References And Accepted Meaning

- [Workspace State Files](../../../../crystallized/decisions/framework/workspace-state-files.md):
  one authored settings file, one generated lock; no legacy reader; no per-subject
  grants; ordinary paths; allow always, allow once, cancel; no settings rewrite
  except explicit grant authoring.
- [G1 decisions and recorded steps 3a/3b](phase-4a-g1.md): `--allow-path` on all six
  gated commands; it persists, never writes in dry-run; preserve unknown authored
  keys and their order, with comments allowed to disappear on an explicit write.
- The maintainer clarified during A4 that the allow list applies to all owners.
  No extension-specific destination declaration or Library source binding exists.
- Update the shared workspace-permissions Interface/Behavior and technical design,
  affected command contracts and Framework layer record with the behavior.

## Execution Capsule

Direct sequential execution; no delegation or new dependency. Reuse the pinned
BCL, concrete source-generated JSON projections, settings codec/object-model
editing, path policy, current mutation lease, expectation validation and recovery
receipts. Keep command result/prompt formation in the command layer. The settings
reader is the sole parser of settings and carries the observation needed by
mutation revalidation. Remove the old subsystem instead of adding an adapter
that fabricates its per-owner document.

The change affects persistent grants and writes/deletes user content. Exact
pre-effect observation, destination admission and recovery remain binding.
Capture reviewed before snapshots separately; use direct real-filesystem tests
for grants, revocation, no-follow boundaries and preserved authored/legacy bytes.
Run the four prescribed checks after rebuilding and the supported Windows native
gate. No release or package qualification is claimed.

## Expected Result

A grant for one destination admits every Extension or Library using that
same destination. Removing it before application prevents the planned write.
Changing a Library source never transfers, replaces or revokes an owner-specific
grant because that grant concept no longer exists. Missing or unreadable settings
withhold external permission; implicit destinations remain independent of it.
Explicit approval never destroys malformed authored settings.

`--allow-path` works on Extension Install/Update/Remove and Library
Attach/Sync/Detach. Dry-run writes nothing. Interactive always persists to
settings, once applies only to the current operation, and cancel applies nothing.
Approval does not bypass ownership, reserved destinations or physical safety.

Result permissions identify the settings file and retain truthful decision,
action and outcome facts. Required/missing destinations lose grant-subject
coordinates; Library scope descriptions lose source binding and rebinding.
Do not add a second state-file outcome, a new persisted schema, or G4 presentation
selection/status/budget policy.

## Implementation And Verification

1. Capture and commit reviewed output for nonempty permission requirements and
   approved grants before changing behavior; include the current Library rebinding
   output that is being removed. Existing A6 command snapshots cover the common
   empty/not-evaluated results.
2. Finish the four missing `--allow-path` bindings using the existing typed option
   and settings grant path. Keep native parser diagnostics and dry-run behavior.
3. Replace legacy document reads/evaluation/publication with settings observation,
   shared path admission and existing authored-key-preserving updates. Rehome only
   surviving neutral capabilities at their actual owner; delete Framework/Permissions.
4. Collapse subject-bound result/prompt coordinates and update the contracts in
   the behavior commit. Review every snapshot delta, including recovery entries.
5. Prove cross-owner reuse, source independence, revocation between planning and
   application, explicit always/once/cancel, dry-run, unknown-key preservation,
   ignored malformed legacy files, refused unsafe settings and current reserved
   controls. Preserve exact recovery-before-content ordering for interactive always grants. Explicit `--allow-path` retains its accepted separate authored-edit path.
6. Rebuild; run the four checks and supported native gate; record exact artifacts,
   divergences and anything intentionally omitted. Commit without pushing.

## Acceptance

- [x] No reader, writer or evaluator consults the retired permission file.
- [x] One shared allow list admits all six gated commands; all expose `--allow-path`.
- [x] Revalidation observes actual settings, including grant revocation.
- [x] Explicit grants preserve unknown authored keys; interactive always writes include exact prior recovery.
- [x] Implicit, reserved-path and physical-containment rules still apply.
- [x] Reviewed before snapshots are committed separately; contracts accompany behavior.
- [x] Four convention checks and supported Windows native qualification meet their gates.
- [x] G1 state is updated truthfully; no G4 work has started.

## Divergences Observed

- **Accepted work omitted from the sequence:** A6-R8 identified G1 steps 3a/3b,
  so this closeout precedes B1/M1. A6 is committed as `525f9379`.
- **Before evidence:** `1d0a7a71` captured eight nonempty permission renderings;
  `badbde7a` captured five published help outputs before those bindings/help changed.
  The latter uses the verified A6 native executable and records its hash because
  the existing unit snapshot harness did not exercise these whole published help
  surfaces. It is before evidence, not a claim that old native output verifies P1.
- **Explicit and interactive authoring:** the initial plan overgeneralized exact
  recovery to every explicit grant. Accepted G1 step 3a already implemented
  `--allow-path` as a separate authored edit before admission. That path remains;
  interactive always publication uses the existing lease and recovery bundle.
  Both use the sole settings reader/codec and preserve unknown authored keys.
- **Write refusals:** the existing flag callers discarded a refused settings write.
  All six now report their existing permission-write failure and stop content
  application. Flag writes occur after safe planning/preflight, matching the
  existing Install/Attach ordering. No second wire outcome was invented.
- **Settings observation:** prior settings reads did not carry exact raw snapshots
  for mutation revalidation. The sole reader now supplies these and refuses a
  linked `.agents` parent; grant revocation and raw changes invalidate admission.
- **Temporary publication:** the existing authoring helper used one fixed `.tmp`
  filename. It now creates an unpredictable same-directory file exclusively and
  removes it only after owning its creation, preserving unrelated user files.
- **Retired tests:** tests of the deleted per-subject persisted codec and rebinding
  rules are retired or rewritten against shared destination grants. Surviving
  observation, recovery, no-follow, projection and receipt tests move to their
  current owner. Existing Unix-only skip cases remain.
- **A6-R9, public help mismatch:** review found root Update and Extension Update
  help still described preserved modified/missing files and force-gated recovery,
  contradicting A6's verified ordinary replace/restore behavior. Captured their
  real before output in `badbde7a`; corrected those claims with this permission
  help change. No new overwrite behavior was introduced.

- **Already-covered explicit paths:** the old settings editor deduplicated only
  identical strings, rewriting authored settings for an already-admitted child.
  It now uses the existing shared allow-list evaluator, preserving exact bytes
  when a parent already grants the path. Direct codec/planner and writer evidence
  covers this; no second path grammar was introduced.
- **Recovery before a new settings file:** the neutral file applier allows
  ordinary Create without recovery. Interactive permission publication has a
  stronger contract, so both command permission operations now require a bundle
  even for Create. Four real leased tests assert absent/existing settings remain
  unchanged when recovery has not been prepared; ordinary application tests
  exercise successful publication.
- **Fixture migration failures:** the first full run exposed remaining old JSON
  object/path assertions, old prompt-answer expectations and one exact-file grant
  still seeded in the retired format. Migrated these at their current owners.
  The new once fixture passed behavior but omitted ownership-aware external
  cleanup; it now uses the existing fixture's owned cleanup in finally. Earlier
  failed temporary directories are left in place, not swept or retried.
- **Snapshot capture scope:** the eight new before views plus the existing
  command baselines cover the shared shape. A full Imprint capture rewrote line
  endings even for unchanged snapshots; normalized all snapshots back to LF.
  The reviewed semantic diff contains exactly 36 changed views: settings path,
  destination strings, removed owner/source coordinates and rebinding, settings
  wording and the recovery settings path. No decision/action/outcome changed.

## Final Verification

Implementation and contracts are committed together. Latest rebuild
`artifacts/p1-final-managed-build.log` passed with zero warnings/errors.

| Required check            | Latest result                                                                  | Evidence                              |
| ------------------------- | ------------------------------------------------------------------------------ | ------------------------------------- |
| Unit, immutable snapshots | 3349 succeeded, failed 0, skipped 0                                            | `artifacts/p1-final-managed-unit.log` |
| Integration               | 1846 succeeded, failed 0, skipped 17 (1863 total)                              | `artifacts/p1-second-integration.log` |
| EndToEnd                  | 131 succeeded, failed 0, skipped 0                                             | `artifacts/p1-final-managed-e2e.log`  |
| check:dotnet              | Exactly five inherited whitespace errors; exit 2; chained analyzer did not run | `artifacts/p1-final-check-dotnet.log` |

The first diagnostic Unit run had 41 failures, including the intentionally stale
snapshots; the first diagnostic Integration run had 18 failures and the same 17
skips. Both were corrected, rebuilt and rerun to the green results above. These
counts describe evidence and are never pass conditions. No cancellation-flake
rerun was needed. The published-help diff is reviewed; supported Windows native qualification passed.

Snapshot review: `artifacts/p1-reviewed-snapshot.diff` and
`artifacts/p1-snapshot-review-summary.txt`; 36 files, 76 additions/135 deletions.
All JSON differences were compared recursively, including the recovery entry;
all distinct human changes were reviewed. Before permission evidence is
`1d0a7a71`, published help evidence `badbde7a`. P1 is complete; G1 steps 3a/3b are closed.

The published-help diff is reviewed in `artifacts/p1-reviewed-help.diff`: the four
new flags, their wrapped syntax/option rows, one shared permission explanation,
and R9's Update authority correction. The before Markdown omits final blank
output lines around its code fences as well as normalizing line endings; its
rendered content is preserved. EndToEnd caught a stale exact help-syntax assertion
that omitted the newly accepted option; updated its expectation, with a fresh
rebuild and full rerun completed: 131 passed, failed 0, skipped 0. The initial
EndToEnd run had exactly those two stale help assertions (129 passed, 2 failed);
the six new published Library flag/dry-run cases passed in both runs.

Final read-only production review confirmed no retired permission reader/writer
or per-subject model remains in Core, and no command/Shell dependency was
introduced in the Settings or neutral Library recovery capability. The sole
settings codec remains the authoring parser. `git diff --check` is clean.
`artifacts/p1-native-build.log` records a successful Windows native build with
zero warnings/errors. No release qualification is claimed.

## Native Evidence And Closeout

| Direct native check    | Result                               | Evidence                              |
| ---------------------- | ------------------------------------ | ------------------------------------- |
| Copied Unit assembly   | 3349 succeeded, failed 0, skipped 0  | `artifacts/p1-native-unit.log`        |
| Integration executable | 1846 succeeded, failed 0, skipped 17 | `artifacts/p1-native-integration.log` |
| EndToEnd executable    | 131 succeeded, failed 0, skipped 0   | `artifacts/p1-native-e2e.log`         |

All five published native help captures equal the reviewed managed output:
`artifacts/p1-native-help-equivalence.log`. The four convention checks meet their
specified gates, including the unchanged five-error whitespace baseline; the
chained analyzer did not run. Counts describe evidence, not acceptance criteria.

`readBuilt` verified source identity and every closure before staging, recorded
in `artifacts/p1-accepted-evidence-identity.json`.
Parent: `badbde7a2968fd3a113e765b41df2e85c56c0c46`; change hash:
`ddf22482800be1f804993267bd7dd01b87e3f38880202685a85b5e0dfe0cdaa6`.

| Native artifact                                                            | SHA-256                                                            |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe`                   | `5ea0dd47d410a6e2cdb629d605db915757328debd68dd242f22004d9aaec4e1d` |
| `artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe` | `068c138235dfdb3bddc04e8e07cb81b3eba8f6625333b67511f7ed9b01afb9ec` |
| `artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe`     | `77a4c0711ae1a99f522def4924e72ecb1ae6e7e283b08deb08ca822f84509a2a` |

The manifest remains `tested: false`: these are direct suite qualifications,
not packaging or release qualification. No push or integration was performed.
The original checkout remains clean. `scripts/delivery/test-suites.ts`, shipped
payload and `.prettierignore` remain untouched by P1.

Intentionally not done: no legacy user file was read, migrated or deleted;
no second settings parser, per-owner grant adapter, new state-file outcome or
new persisted schema was introduced. Explicit `--allow-path` was not folded
into interactive recovery: accepted G1 step 3a owns its separate authored edit.
No all-command output matrix or historical harness was recreated. B1 owns the
next document migration; M1 follows it, and G4 remains unopened.

Final staging exposed one extra blank line at EOF in the new settings write
model, which unstaged `git diff --check` had not inspected while it was untracked.
Removed that blank line before closing the commit. The recorded native identity
is the qualified pre-trim source; this final edit changes no C# tokens or behavior.
The final format command returned exit 2 with the same five inherited diagnostics;
the chained analyzer did not run. The diagnostic set is the convention gate.

## Superseded by Task 30 G4

The P1 receipt above records the G1 behavior and remains historical. G4 now
stages explicit `--allow-path` grants and interactive Allow-always decisions in
the permission plan, publishing them only during confirmed application under
lease, revalidation, and recovery. Cancellation or refusal before application
leaves settings unchanged. See the shared operation contract and the workspace
permissions technical design for the current timing.
