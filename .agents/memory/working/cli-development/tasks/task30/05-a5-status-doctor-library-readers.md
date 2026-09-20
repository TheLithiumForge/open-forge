---
open-forge:
  description: Task 30 G1 step 4 slice A5 execution plan for flipping Status, Doctor, and Library readers to the lock and redefining target state
  tags: [Memory, Working, CLI, Task, Plan, Contextual, Active]
---

# A5 — Status, Doctor, and Library readers flip

> Read [00 — Slice conventions](00-conventions.md) first: verification
> commands, pass conditions, the recurring composition pattern, and the rules
> every slice shares. This plan does not repeat them.

## Goal

`status`, `doctor`, and the Library readers take ownership from the lock. Target
`state` stops meaning "matches what we wrote" and starts meaning "matches what
this release would install". `doctor` stops reporting a missing lifecycle
document on a Framework source checkout.

**Behaviour-changing slice.** Output changes; contracts change with it.

## Depends on / Blocks

- Depends on: A1, A2, A3.
- Blocks: A6.

## References

Authorizing decisions: refinements 10 and 11 in [phase-4a-g1](phase-4a-g1.md) —
the comparison is current against intended, normalized semantically.

Current-state baseline, measured 2026-09-12 on this repository, a Framework
source checkout with no lifecycle document:

```
ERROR framework.install-incomplete   "The lifecycle document is missing."
                                     Resolution: repair is blocked
INFO  extension.lifecycle-document-missing
```

Both must be gone after this slice.

**The seam is the operational contributors, not the aggregators.** Status and
Doctor both consume the same three contributors, and the Status aggregators are
pure functions over the views those contributors produce. Flip the contributors
and both commands follow; the aggregators need no change.

- `Framework/Lifecycle/Operational/FrameworkLifecycleOperationalContributor.cs`
- `Framework/Extensions/Operational/ExtensionLifecycleOperationalContributor.cs`
- `Framework/Libraries/Operational/LibraryOperationalContributor.cs`

Touch an aggregator or a Doctor inspector only where it names a lifecycle
document directly, rather than reading a view:

- `Commands/Doctor/Shared/Domains/FrameworkLifecycleStateDoctorInspector.cs` —
  emits the missing-document findings that this slice deletes.
- `Commands/Doctor/Shared/Domains/FrameworkManagedTargetDoctorInspector.cs:146`
  and `ExtensionManagedTargetDoctorInspector.cs:88` — build
  `DoctorComparisonEvidence` from a baseline that no longer exists.

Existing snapshots to diff against:
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/__snapshots__/Commands/Status/Shared/Rendering/StatusLifecycleOutputSnapshotTests/`

## Decision fixed for this slice

`state` **keeps its existing words** — `current`, `changed`, `missing`,
`unavailable`, `blocked` — with `current` now meaning "matches what this release
would install". Keeping the words follows refinement 12's precedent for the
inspect relation, and `current` already reads as "up to date with the release".
Do not invent new words.

## Preconditions

### Execution capsule

- Baseline: A4 commit `70b4a2ff`, followed by the reviewed A5 snapshot
  `fa6019ca` and prerequisite corrections `13239822`, isolated on
  `task30-a4-extension-readers`.
  The source checkout remains untouched. Direct sequential execution continues
  under the maintainer's authorization through accepted pre-G4 work.
- Consequence: Status and Doctor remain read-only. Library ownership readers
  also feed real link mutations, whose existing leases, no-follow observations,
  destination policy, expected-state checks, and recovery remain mandatory.
  Stable mappings and cooperating processes define the supported boundary.
- Reuse: the accepted ownership reader/codec, typed Library identities and
  mappings, operational contributors, embedded/current package payloads,
  Markdown-semantic content comparison, and existing rendering/snapshot tools.
  The pinned BCL and libraries provide these capabilities. Exceptional machinery:
  none. No schema, parser, dependency, or platform expansion is selected.
- Evidence: reviewed pre-change output snapshots first; direct Integration
  assertions for lock/legacy disagreement, missing and unreadable lock behavior,
  current-against-intended comparison, and source-checkout Doctor observations.
  Run the four prescribed commands and supported host Native AOT gate because
  ownership, shared composition, and public output materially change.
- Preparation boundary: the initial test-only Doctor harness was drafted under
  A4's carried execution profile before this capsule was refreshed. No snapshot
  had been captured and no production behavior changed. This capsule now owns
  A5's evidence and scope decisions before capture and implementation.

- [x] A1, A2 and A3 committed; three suites green. A4 additionally passed all
      prescribed managed and supported native checks before this slice.
- [x] A `doctor` output snapshot baseline exists. If absent, capture, review, and
      commit it before changing behaviour.

## Steps

1. [x] Capture the `doctor` snapshot baseline if absent. Commit separately.
       Verify: unit suite green.

2. [x] Compute target state as current-against-intended: hash the file on disk and
       the payload asset with the Markdown-semantic policy, compare. Missing file
       stays `missing`; unreadable stays `unavailable`; boundary failure stays
       `blocked`. Verify: `npm run build`, unit suite green.

3. [x] Flip `StatusLifecycleAggregator` and `StatusLibraryAggregator` to the lock.
       Verify: integration suite green.

4. [x] Flip the four Doctor inspectors. Delete the `framework.install-incomplete`
       error and `extension.lifecycle-document-missing` information that fire only
       because a lifecycle document is absent. A missing lock is rebuilt best-effort
       and reported, never an error. Verify: integration suite green.

5. [x] Flip Library readers to the lock. Verify: integration suite green.

6. [x] Run `doctor` against this repository. It must report neither finding from the
       baseline above. Record the new output in the Divergences section as evidence.

7. [x] Update contracts in the same commit as the behaviour:
       `contracts/status/interface.md` and `behavior.md`,
       `contracts/doctor/interface.md` and `behavior.md`, and the `library/*`
       contracts that name `.agents/open-forge.libraries.json`.
       Verify: `npx prettier --check` on the changed files.

8. [x] Review both snapshot diffs. Expect the `state` semantics change and the two
       removed doctor findings, nothing else.

## Expected result

`status` reports whether each managed file matches what this release would
install. `doctor` on a Framework source checkout reports no lifecycle error.
Neither command refuses because of lock state.

## Acceptance

- [x] Snapshot baselines captured before the change; both diffs reviewed.
- [x] `doctor` on this repository emits neither baseline finding.
- [x] No command refuses for a missing, stale, or unreadable lock — assert a
      command succeeds with the lock deleted.
- [x] Status, Doctor, and Library contracts included with the behavior in this increment.
- [x] Three suites green.

## Preparation evidence

Commit `fa6019ca` captured sixteen complete outputs before A5 behavior changed:
eight Doctor missing-state/changed-target cases and four each for Library List
and Inspect. The nineteen-file commit contains the harnesses and snapshots only.
Reviewed every human and JSON output, including the old record path and Doctor
comparison evidence. The existing Status snapshots remain the Status baseline.

A fresh offline-capable build completed with zero warnings and errors. The full
Unit suite passed: 3497 passed, 0 failed, 0 skipped, with snapshot updates disabled.
The focused whitespace verification of the three new harnesses also passed.
Artifact logs: `a5-snapshot-build.log`, `a5-snapshot-unit-full.log`, and
`a5-snapshot-format-check.log`. The preparation does not claim a new Integration,
EndToEnd, or native qualification; A4 supplies that beginning baseline.

## Prerequisite review verification

R5's real-command regression first failed for both non-root Library cases
(5 passed, 2 failed). After using the existing typed destination mapping, all
9 cases passed, including two uninterpretable mappings that preserve every file
and return a complete ownership observation. R6's four real writer regressions
all failed before the correction and all passed afterwards.

After the final source and test corrections, the prescribed managed checks were:

| Check                            | Result                                           | Artifact                                 |
| -------------------------------- | ------------------------------------------------ | ---------------------------------------- |
| Rebuild with `--no-restore`      | 0 warnings, 0 errors                             | `a5-prerequisite-final-build.log`        |
| Unit                             | 3497 passed, 0 failed, 0 skipped                 | `a5-prerequisite-final-unit.log`         |
| Integration                      | 1825 passed, 0 failed, 17 skipped                | `a5-prerequisite-final-integration.log`  |
| EndToEnd against the managed CLI | 124 passed, 0 failed, 0 skipped                  | `a5-prerequisite-final-e2e.log`          |
| `npm run check:dotnet`           | Exactly the five known whitespace errors; exit 1 | `a5-prerequisite-final-dotnet-check.log` |

The format command's chained analyzer did not run after the expected whitespace
failure. All snapshot comparisons ran with updates disabled. The reviewed A4
Remove and A5 preparation snapshots are unchanged: these fixes add no fields or
presentation changes. The lock schema example now shows the root managed region,
and its Library path description matches the existing source-relative mapping.
The supported Windows native build also completed with no warnings or errors.
Native Integration passed 1825 tests with 0 failures and exactly 17 skips
(`a5-prerequisite-native-integration.log`); native EndToEnd passed 124 with
0 failures and 0 skips (`a5-prerequisite-native-e2e.log`). The copied managed
Unit closure from that build passed 3497 with 0 failures and 0 skips
(`a5-prerequisite-native-build-unit.log`). The prescribed EndToEnd executable
rebuilt to target the native CLI also passed 124 with 0 failures and 0 skips
(`a5-prerequisite-managed-native-e2e.log`).

The build manifest and its complete artifact hashes were verified against the
unchanged source before staging. Its parent is
`fa6019caa3cf91ddd6e4705b0b0cbc70109c9cdf`, with source-change hash
`107e1e6dd04650e7a4158ce01bb86a42787cf52e4ad04b5e199ea45aa30716c3`.
Exact native executable hashes:

| Artifact                                                                   | SHA-256                                                            |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe`                   | `2fc04e2ae55e71516cce89dec550dd0074c753c4f973b2fcc9c41401fe853f3b` |
| `artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe` | `a5843321e4fb232b859c60dc87680d75909778faf6fb1e1d1ac8a4a6e6e17101` |
| `artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe`     | `4dff8b6384744d617b44cbb864fe9291958c5202fde3997ff01c3b0b9f3dea9d` |

The manifest remains `tested: false`: direct convention gates were used, without
asserting package or release qualification through the delivery driver's fixed
count/skip thresholds. No delivery scripts were changed. A5's operational reader
implementation remains next after these prerequisite corrections.

## Divergences observed

- The final consumer inventory found that Library List/Inspect binding failures
  still named the old record before reaching the operational reader. Their
  contracts already name the ownership lock, and the committed before snapshots
  already cover that path rename. Repoint the two binders as part of A5 and add
  direct assertions for parser, semantic-ID, missing-workspace and unsafe-workspace
  results. No new status, finding, wire member, or snapshot scenario is needed.
  Final qualification is repeated after this correction; the earlier run below
  is retained as intermediate evidence, not the final executable identity.

- Existing Library evidence exposed R7 in A1's shared lock reader: checking the
  lock leaf alone allowed a linked `.agents` parent pointing inside the workspace.
  Library's previous reader rejected that ancestry. Check the direct parent with
  the existing no-follow observer before reading and again after reading. An
  unsafe parent produces an informational unavailable ownership observation and
  no claims, preserving the existing Library boundary without a new path walker.

- Refinement 11 requires the existing Markdown-semantic policy, but its rationale
  also describes trailing whitespace and final-newline differences as normalized.
  The immutable `open-forge-markdown-v1` Inspect contract and frozen test vectors
  explicitly preserve both; only line endings and eligible generated content
  normalize. Preserve that existing policy, as the binding decision requires,
  and assert CRLF equivalence separately from authored final-newline changes.
  No new fingerprint version or normalization rule is introduced.

- The operational target models still require internal baseline/fingerprint-kind
  members even though earlier Task 30 work removed them from rendering. Replace
  those members with the current intended fingerprint where comparison evidence
  needs it. This avoids manufacturing a legacy baseline from the new payload.
  Keep generated Entries currentness tied to the existing current-topology
  observation; a release payload does not describe a workspace's generated links.

- The full prerequisite Integration run exposed one A1 assertion equating every
  physical file-creation effect with whole-file ownership, including fresh root
  managed hosts. A new host is still managed only inside its `open-forge` block.
  Narrow that assertion to whole-file payload receipts; the four new writer
  regressions assert the managed-host region receipts and authored preservation.
- Continued review also found a tautological A1 Route Init assertion comparing
  the post-repeat workspace snapshot with itself. Capture it before the repeat
  and compare afterwards. The existing lock-byte stability assertion remains.

- Continued review found R6 in A1's Framework writers. Install omitted root
  managed blocks and selected whole-file ownership from changed effects alone;
  Update and Route Init instead copied legacy region-null targets, classifying
  `AGENTS.md` and `CLAUDE.md` as whole-file ownership. The accepted path/region
  distinction requires those hosts' existing `open-forge` marker blocks to be
  region receipts. Correct these writer projections before flipping readers,
  retaining previous verified claims when a command leaves their bytes unchanged.
  Use `open-forge` for the region already named by the existing markers; no new
  marker syntax or serialized field is introduced. Cover initial Install,
  re-establishment, Update, and Route Init with real operations. This prerequisite
  repair uses the same leases, recovery, verification and ownership store as A1;
  the full managed and native composition gates remain selected.

- Continued review found R5: A3 serializes Library `paths` as source-relative
  eligible paths beneath `destinationRoot`, consistent with its existing mapping
  model and the lock example. A4's Remove boundary instead compared those raw
  strings as workspace paths, missing a conflict for a non-root destination.
  Extend the existing real-command regression to non-root destinations, then
  apply the existing typed Library mapping before the portable ownership check.
  The schema's inherited workspace-relative description is too broad for Library;
  clarify its Library-specific description without changing the serialized shape.
  This is an accidental ownership-conflict defect within the accepted boundary,
  not new cleanup behavior or a stronger filesystem guarantee.

- Step 4 asks read-only Doctor to rebuild a missing lock. The accepted receipt
  model says ownership cannot be reconstructed from matching files or payload
  declarations, and the command contracts prohibit writes. Read-only readers
  therefore report unavailable ownership without creating claims or writing the
  lock. A subsequent verified mutation remains responsible for best-effort
  publication. This follows both the read-only contract and refinement 7.

- The numbered Steps tell the executor to change the Status aggregators, while
  References explicitly locate the shared seam in the operational contributors.
  Follow the binding References: switch ownership at those contributors and
  change direct document/baseline consumers only where their facts require it.
- Doctor had inline fragment checks but no complete Imprint baseline for the
  affected lifecycle output. Capture missing-state and changed-target scenarios
  separately in both views and formats, before any reader change. Library List
  and Inspect also expose the old record path, so their complete output needs
  a reviewed pre-change baseline even though Step 1 names only Doctor.
- The repository-source Doctor baseline was captured with the A4 native CLI.
  It emitted both `framework.install-incomplete` and
  `extension.lifecycle-document-missing`; the full raw output is a disposable
  local artifact. This is the before-state for the required after check.
- Preparation reused A4's execution profile while drafting the initial snapshot
  harness; the A5 capsule was recorded after that draft and before capture or
  production changes. The preparation remains test-only and independently
  committed as required.

### Reader composition discoveries

- The plan located changes in the contributors and permitted direct legacy-field
  consumers to follow. The absence-proof adapter and Doctor ownership inspectors
  also consumed legacy reads directly. They now take lock observations; actual
  footprint evidence still decides proven absence, while unavailable lock claims
  do not create installation errors.
- Library selection and the transitional legacy publication shared one snapshot.
  A lock snapshot cannot be passed to the old Library codec without overwriting
  the ownership document. A5 separates the optional legacy publication snapshot
  from the lock-derived catalogue. A6 will delete that remaining publication;
  no legacy content supplies Library selection or ownership.
- Existing currentness models required stored fingerprint fields even though the
  public serializers had already removed them. The internal observations now
  carry a nullable intended fingerprint computed in this run. Generated Entries
  use the existing current route projection, so their changed finding uses state
  evidence instead of inventing a payload comparison hash.

- Exact payload-path lookup cannot compare supported scoped Framework targets:
  Route Init places a canonical payload entrypoint below scope slugs. Its existing
  canonical-topology alignment is promoted to Distribution's source helpers and
  used by both Route Init and the operational target reader. This preserves the
  accepted scope grammar without reading source identity from legacy state or
  adding a second alignment algorithm. Ambiguous or unavailable alignment yields
  unavailable comparison, never an invented source identity.

### Additional divergences resolved

- Step 8 expects only changed semantics and removal of two Doctor findings.
  The accepted lock policy also requires explicit ownership observations. Their
  severity/count/status/source-availability consequences appear in the reviewed
  snapshots; Library's visible record path must name the lock actually read.
  These are required truthful projections of the accepted reader behavior, not
  a new presentation taxonomy or a G4 renderer change.
- The shared Library technical design also described legacy selection. Update
  its reader/publication distinction alongside the command contracts so it does
  not contradict their new current behavior. A6 still owns deletion of legacy
  writers, codecs, and remaining references.
- The plan's rollback text assumed both records always remain current. Optional
  publication cannot promise that. Record the actual rollback limitation rather
  than claiming a successful dual write when one publication was skipped.

- Published Library process fixtures still wrote only legacy registrations. After
  the reader switch, ten process assertions failed from those empty catalogues
  and Doctor's removed legacy coverage errors. Seed current lock ownership in
  the fixture and assert Doctor's remaining independent warnings. The rebuilt
  end-to-end suite then passed all 124 journeys without skips. Historical
  recovery-bundle bytes retain their legacy format until A6 removes that format.

## Rollback

Reverting the code restores A4 readers. Do not assume both old and new records
remain current: accepted best-effort ownership publication and optional legacy
publication may leave stale or unavailable state. Preserve workspace files and
recovery bundles; do not reconstruct old authority from matching content.

## A5 reader and snapshot evidence

The full managed Unit suite passed 3499 tests with 0 failures and 0 skips.
Integration passed 1846 with 0 failures and exactly 17 platform skips. New direct
contributor evidence covers CRLF equivalence, authored edits, significant final
newlines, missing and nonordinary targets, external links, scoped Framework
alignment, root authored-content preservation, retired source assets, and old
record/version disagreement. Full Status and Doctor operations succeed or retain
only independent diagnostics when the lock is deleted or malformed; all three
ownership findings are informational and all workspace/recovery bytes remain
unchanged. Library List/Inspect select only lock claims; Sync/Detach with missing,
unreadable, or invalid locks select nothing in both dry-run and apply mode and
preserve an existing matching link even when legacy registrations are present.

The full reader migration initially exposed stale legacy fixtures, a finite
Status finding-map ordering defect, and a complete-no-op execution path that
still required a selected Library. Those were corrected before qualification.
Recovery fixtures now distinguish current lock membership from historical legacy
bundle bytes. Exact inverse recovery, newly registered source protection before
and after preflight, and unselected-entry preservation all pass.

The reviewed snapshot diff is `artifacts/a5-reviewed-snapshot.diff`: 18 snapshots,
71 lines added and 71 removed. Doctor's missing-state scenarios replace the
Framework error and Extension legacy information with two ownership information
findings, complete status, unavailable-ownership evidence, and not-applicable
source availability. Changed-target evidence now names the current intended
fingerprint and message. Library List/Inspect and Status's Library record path
change to `.agents/open-forge.lock.json`. JSON members and ordering are unchanged.
The four A4 Remove snapshots are unchanged. Capture ran 22 snapshot tests with
`IMPRINT_UPDATE=all`, then updates were disabled for full verification.

The same source checkout's actual Doctor output is retained locally in
`artifacts/a5-doctor-after.json`. It emits neither `framework.install-incomplete`
nor `extension.lifecycle-document-missing`; it emits the three informational
ownership observations. Its exit is still 3 due to independent repository
coverage findings, not ownership absence. No repository repair was attempted.

## A5 qualification before the final binding-path correction

The final managed build (`a5-process-fixture-build.log`) completed with zero
warnings and errors. With snapshot updates disabled, the prescribed Unit
executable passed 3499 with 0 failures and 0 skips
(`a5-final-unit-rerun.log`), Integration passed 1846 with 0 failures and exactly
17 skips (`a5-final-integration.log`), and EndToEnd passed 124 with 0 failures
and 0 skips (`a5-final-e2e-rerun.log`). The only changes after the Integration
run were Unit/published-process fixtures and documentation; production and
Integration evidence were unchanged.

`npm run check:dotnet` reported exactly the five existing whitespace errors in
`ReferencesOperation.cs` and `ExtensionListApplicationIntegrationTests.cs`; exit
1 stops its chained analyzer (`a5-final-dotnet-check-rerun.log`). Changed Markdown
passed Prettier (`a5-docs-check.log`). No cancellation-flake rerun was needed.
Supported Windows Native AOT results follow below.

The native build completed with zero warnings and errors (`a5-native-build.log`).
Native Integration passed 1846 with zero failures and exactly 17 skips
(`a5-native-integration.log`); native EndToEnd passed 124 with zero failures and
zero skips (`a5-native-e2e.log`). Its copied managed Unit closure passed 3499 with
zero failures and zero skips (`a5-native-build-unit.log`). The initial copied
Unit invocation used the assembly name as the folder and did not start a test
process; rerunning from the build's actual `build/unit` directory supplied this
result. This was a command-path correction, not a failed test.

Before staging, `readBuilt` verified the source identity and every native and
managed-closure hash. The exact record is `artifacts/a5-evidence-identity.json`.
Parent: `1323982263a51772ea3e4059b37154b0c73fbe22`; change hash:
`67fc5fe6046174608925366825b77c80f4729860a60b4683df157b5c0881f130`.

| Native artifact                                                            | SHA-256                                                            |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe`                   | `754e0283c27915937a6fdf2231dcbcef8b231598bb01753ba1f628738d5ca5e2` |
| `artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe` | `57894c3f20aa9774c3331d1eea9978ec0d85a26bbb43428982baff67b0c9cbd5` |
| `artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe`     | `78345fb90cdf5479a921cd6092041889ca606444ebbf28f2fbdf6c2bfaac180e` |

The manifest remains `tested: false`: these are direct suite qualifications,
not a packaging or release qualification. No push or integration is authorized
or performed. `scripts/delivery/test-suites.ts` remains untouched.

## Final binding-path correction and qualification

The two Library read binders now name the ownership lock for invalid parser/ID
inputs and unavailable or unsafe workspace selection. Four additional direct
Unit cases cover workspace selection, and the existing input cases now assert
the record path. Their first setup incorrectly depended on an unknown token
producing a parser error; the pinned parser may bind it as the ID. Construct the
typed workspace-invalid input directly through the existing shared test input
factory instead. This changes no parser policy.

After rebuilding (`a5-accepted-build.log`, zero warnings/errors), Unit passed
3503 with zero failures and zero skips (`a5-accepted-unit-rerun.log`). Integration
passed 1846 with zero failures and exactly 17 skips
(`a5-accepted-integration.log`); only the Unit fixture construction changed
after that Integration run. The format gate still reports exactly the five
known errors (`a5-accepted-dotnet-check.log`). Further final evidence follows.

Final managed EndToEnd passed 124 with zero failures/skips
(`a5-accepted-e2e.log`). The supported Windows native rebuild completed with
zero warnings/errors (`a5-accepted-native-build.log`). Its copied Unit closure
passed 3503 with zero failures/skips (`a5-accepted-native-build-unit.log`),
native Integration passed 1846 with zero failures and exactly 17 skips
(`a5-accepted-native-integration.log`), and native EndToEnd passed 124 with
zero failures/skips (`a5-accepted-native-e2e.log`).

The final source identity and every artifact/closure hash were verified before
staging using `readBuilt`; `artifacts/a5-accepted-evidence-identity.json` holds
the complete record. Parent `1323982263a51772ea3e4059b37154b0c73fbe22`,
change hash `8ffabff604beb68d279b2a9c2b8e9e38e7b53c6e49443e150bd484e04f79f348`.

| Final native artifact                                                      | SHA-256                                                            |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe`                   | `fa5d4a53dd115fe8133d17c56ce5063caf3731bb601c3907863d6d0441e79b85` |
| `artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe` | `c92a3e64db4beb747416614a347dc1cc56b02a14c0f90132c589730b578dc1d5` |
| `artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe`     | `78345fb90cdf5479a921cd6092041889ca606444ebbf28f2fbdf6c2bfaac180e` |

The earlier identity is superseded by this final one. No snapshot changed after
its reviewed 18-file diff; the binding correction adds direct path assertions
and preserves the already captured old-to-lock rename.

The prescribed managed EndToEnd executable retargeted to the native CLI also
passed 124 with zero failures/skips (`a5-accepted-managed-native-e2e.log`). Final
`check:dotnet` again reported exactly the five known errors
(`a5-accepted-dotnet-check-final.log`); its chained analyzer did not run. All
changed Markdown passed Prettier (`a5-accepted-docs-check.log`). All A5 acceptance
items are complete. Legacy publication, remaining Extension List/Inspect and
Framework mutation comparison readers, and deletion of the old subsystem are
intentionally left to A6. G4 has not been entered.
