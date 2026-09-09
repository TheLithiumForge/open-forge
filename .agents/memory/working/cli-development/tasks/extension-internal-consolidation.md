---
open-forge:
  description: Track accepted Extension manifest consolidation and exact managed/native behavior preservation
  tags: [Memory, Working, CLI, Task, Extension, Refactoring, Testing, Contextual, Complete]
---

# Task 26: Extension Internal Consolidation

## Task State

- State: Complete; final candidate accepted for exact local integration after
  Tasks 24 and 25.
- Permanent mapping: Task 26 “Extension Internal Consolidation” in the
  [project control ledger](../project-control.md).
- Prior input: read-only preparation tip
  `8a153f23dabb05019eae2c15fae51331b9e88335`, tree
  `bafd3d1e3173ad9342bd25a6086330dcfbe5ee16`.
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Phase and milestone: phase 4/4, milestone 6/6.

## Candidate Boundary

This task owns only the later internal consolidation of Extension Create,
Inspect, Install, List, Update, and Remove after all command and destination
behavior is final. It may introduce one neutral shared Extension foundation and
move detailed public cases to Unit or Integration evidence while retaining
exactly three simple public EndToEnd journeys per command.

The work is differential-locked: public syntax, help, text and JSON bytes,
result shapes, package and lifecycle schemas, ownership, recovery, and command
semantics do not change. Known source-reader and Update behavior candidates
belong to Task 24 review rather than this refactor.

## Activation Conditions

Activation requires completed command work, a fresh accepted post-Task-25
baseline, exact before/after behavior oracles, a shared-fixture ownership plan,
and an Overseer-frozen execution plan under the user’s existing authorization.
Shared foundation, composition, artifacts, and final
convergence remain serialized; command-private evidence and leaf adaptations
may run in parallel after those boundaries freeze.

No behavior change, new command, compatibility machinery, runtime registry,
JavaScript path, remote action, destructive external action, or publication is
authorized.

## Task 24 Acceptance Input

Task 24 completed the source-enumeration correction, current `content/` naming,
consumer permission behavior and eighteen public Extension journeys. Its fresh
review also aligned external Inspect identities and closed selected destination
ancestry/control-file gaps. These are accepted behavior, not remaining refactor
work. Retain their lower-tier regressions and public count. Re-evaluate the
older source-universe/manifest/graph preparation against the final Task 25
baseline before freezing any shared foundation; do not repeat completed work.

## Frozen Execution Capsule

- Outcome: remove duplicate manifest decoding and file-read failure translation,
  and give the package manifest filename one shared owner. Preserve all six
  Extension commands and accepted Library behavior.
- Profile: bounded sequential refactor with characterization before production
  mutation and one fresh review. This local developer tool reads untrusted
  package data and modifies consumer files through existing guarded operations.
  Misclassification could change admission or reporting; Git, retained recovery
  evidence and manual repair remain the existing recovery boundary. Ordinary
  malformed input, interruption and filesystem failures apply; no malicious
  same-user guarantee is added.
- Authority: existing user authorization for the remaining refactor; accepted
  Task 25 feature `564545cb`, integrated `3b4aba9d`, exact tree `7bdbe449`.
  Execution base is `02e929fd3c79750e7d54253b516a2a27173610ce`, whose later
  coordination prose leaves every accepted executable source byte unchanged.
  Branch: `codex/extension-internal-consolidation`.
- Ownership: root owns this capsule, acceptance and control prose. One continuous
  Astra/high implementation specialist owns the bounded source/test slice and
  its verification artifacts. No concurrent runtime or shared-source mutation.
- Standard capability: retain pinned .NET 10 BCL byte reads, strict UTF-8, current
  duplicate-property validator and source-generated JSON context. Ordinary
  existing methods already provide the required decoding and failure semantics.
  Exceptional machinery: none. No dependency, project, runtime, parser, schema,
  registry, DI, reflection, compatibility reader, JS implementation or bridge.
- Placement and algorithm: move the existing `ExtensionManifestReader` and
  `ExtensionManifestFileReader` together to
  `Framework/Extensions/Shared/Manifest/` with matching namespaces and direct
  caller imports. Delete the unused two-argument file reader and `ReadManifestOnly`.
  The former has no production callers; the latter is reached only through
  that dead method and one accepted test helper. Adapt that helper to the live
  full `Read(bytes, manifest path, [])` API. Preserve the full methods' ingress
  validation, catch ordering, messages and package formation. Keep the existing manifest conflict exception with its
  owning parser unless a concrete consumer needs separate placement.
- Shared filename: add `ManifestFileName` to existing `ExtensionPackageLayout`.
  Replace matching Framework source/embedded readers and Create production uses;
  remove the Create-owned duplicate without aliases. Preserve independent test
  literals and embedded JSON inventory bytes. Layout remains the accepted
  package component contract at its current owning root.
- Direct neighborhood: the two readers, layout, `ExtensionSourceReader`, embedded
  catalogue reader, Create definitions/planning/application consumers and their
  existing Unit/Integration callers. New focused tests mirror the two-reader
  `Shared/Manifest` scope. No broad model migration. Serialized models and JSON
  context keep exact namespaces because decoder errors can reach public causes.
- Rejected consolidation: whole-universe dependency validation, Install closure
  and Inspect closure have distinct ordering, missing-node and failure policies.
  Install's parent-catalogue fallback has no second identical consumer. Preserve
  these algorithms; no generalized graph or source-policy engine.
- Evidence ownership: pure manifest facts and Open Forge validation at Unit;
  real file outcome classification, cancellation, manifest-path ingress and source
  readers at Integration. Use current test-support workspace primitives and
  local fixture lifetime; no shared universal fixture or general cleanup task.
  Keep exactly eighteen simple public journeys, three per Extension command,
  and fifteen Library journeys. Do not move unrelated tests.
- Before/after oracles: capture exit/stdout/stderr bytes for six help commands
  plus deterministic local Create preview, source List/Inspect and mutation
  previews covering valid and malformed packages. Use the same workspace/source
  paths and equivalent initial files before and after; compare raw bytes without
  normalization. Add or retain direct assertions for valid manifest facts, owned
  invalid/dependency classification, the live file API and cancelled/missing
  files. Verify semantic results independently; do not merely compare two new
  implementations that could agree on the same defect. Platform decoder message
  text may be captured as same-runtime differential evidence but must not become
  a new Open Forge-owned dependency contract.
- Baseline prerequisite: before production changes, freeze source and existing
  same-worktree native executable hashes, run the selected characterization
  tests against accepted production, and seal the public byte captures. The
  unchanged Task 25 complete 2820/1559/188 managed and 1559/188/188 native results
  provide the exact predecessor gate. New characterization must pass before
  refactoring; this behavior-preserving task does not invent failing Red.
- Final gates: shared manifest ingress and failure translation are a material
  shared-capability trigger. Run complete managed and supported linux-x64 native
  gates once on the frozen corrected candidate, plus raw differential comparison.
  Focused characterization and direct consumer tests precede that freeze. Apply
  informational formatting, static/protected/callable/namespace/prohibited-pattern
  and line-length checks. Account for every addition and moved path before stage.
  A later prose-only update does not invalidate executable qualification.
- Budgets: architecture/council zero; one fresh holistic review T26-R1 for
  exception translation, constant ownership, source/embedded/Create consumers
  and evidence fidelity; one grouped correction T26-C1 if required. Consumed:
  T26-R1, the fresh review of the immutable M4 candidate. Correction remains unused.
- Stop conditions: a changed observable result, required policy merger, schema
  change, exceptional machinery or external effect. Return concrete evidence to
  root before expanding scope. Ordinary implementation choices remain delegated.
- Current boundary: all M6 executable and differential gates passed. Root owns
  final prose/source freeze and exact local integration, then Task 10 activation.

| Phase | Boundary                           | Completed milestones at boundary                        |
| ----- | ---------------------------------- | ------------------------------------------------------- |
| 1/4   | Preflight and characterization     | M1 scope; M2 before-change oracles                      |
| 2/4   | Consolidation and focused evidence | M3 implementation; M4 focused/differential verification |
| 3/4   | Review and full gates              | M5 fresh review and any grouped correction              |
| 4/4   | Acceptance                         | M6 full gates, freeze and exact local integration       |

Root personally reread the complete C# directive trio before this freeze.
Fingerprints: `_csharp.md`
`31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`;
`design.md` `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`;
`style.md` `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
Every author and reviewer independently reads the current complete files.

### Preflight Call-Graph Correction

Root's subsequent complete invocation search, independently confirmed by the
implementer, showed that the proposed convenience delegation would retain two
dead internal entrypoints. `ExtensionSourceReader` is the only file-reader
production consumer and calls its four-argument method. The two-argument method
has no production callers; it is the only production reference to
`ReadManifestOnly`. `ExtensionPackageContractTests` is the latter's sole existing
test consumer. The bounded capsule now removes both methods and adapts that
helper to the live full parser instead of preserving wrappers for tests.

This is a simplification within accepted behavior, with no public contract or
review-budget expansion. Initial two-entrypoint characterization drafts and
their run remain under `artifacts/task26-m2/`; a warning-free build and 54 Unit
passes do not qualify the failed Integration assertion or the revised M2.
Remove synthetic null injection rather than introducing suppression or a probe
bridge for a trusted internal non-nullable parameter. Qualify the narrowed
characterization and seal raw public bytes before production changes.

## M2 Baseline Characterization

M2 is accepted. The narrowed live-API suite passed 54 Unit and 37 Integration
cases against unchanged accepted production, with zero failures/skips and
warning/skip failures enabled. Fifty native and fifty managed public scenarios
seal exact exits, stdout and stderr: 300 raw output files plus expected semantic
status/stream assertions and unchanged same-path fixture inventories. The seal
is `artifacts/task26-m2/m2-seal.json`, SHA-256
`0ddedab4537cf497b0cd091828b9849bda6f03eeba6fe677826073395f273dd7`.
The preserved native executable hash is
`c5d008992364ecd0bc22560c976616f7cdbe6f8313ef82919ec9ea46e80f4af1`.

The baseline accounts for the six root prose changes at `8402c553` and two new
characterization files. Executable production bytes are unchanged. Initial
rejected attempts remain separately retained: unowned raw fixture writes caused
cleanup failures, and assumed Inspect statuses did not match accepted command
policy. Corrected fixtures use their owned file helper; public assertions now
distinguish incomplete package-invalid inspection from blocked dependency
conflict. These setup/oracle corrections establish the actual baseline, not
new product behavior. M3 may now remove the dead APIs and relocate the live
reader cluster under the amended capsule.

## M3 And M4 Focused Candidate

The implementation removes both confirmed dead manifest entrypoints, moves
the two live readers to `Shared/Manifest`, and centralizes `extension.json`
in the existing package-layout owner. Nine production owners and four test
files change; two moves give fifteen physical delta paths. Four additions are
accounted: two moved readers and two focused characterization files. No new
production type, serialized-model/context change, package inventory edit, graph
policy or source-fallback change is introduced. Informational formatting also
selects the equivalent eager collection expression at package construction.

Focused evidence passed 262 Unit, 247 Integration and 33 public journeys, with
zero failures/skips. The build has zero warnings/errors and informational
formatting reports no diagnostics. All eighteen Extension and fifteen Library
public journeys remain, exactly three per command. Fifty managed scenarios
match all 150 raw baseline exit/stdout/stderr files byte-for-byte. Source and
runtime hashes stayed fixed throughout verification. Native differential remains
for the final root-owned publish. The expected complete counts are 2837 Unit,
1570 Integration and 188 public cases.

Canonical completion is `artifacts/task26-focused/completion.json`, SHA-256
`07e2e388a6100aac663c3b39ea29bf811597ffcec642b188ea10cd62d4655348`;
`source-paths.json` hashes every changed/new C# file and identifies the moves.
Root independently matched every hash and all four untracked additions before
staging. Final immutable source accounting is under `artifacts/task26-m6/`.
Earlier fixture failures, a scanner false positive on unchanged prose, and an
unsupported public-discovery wildcard remain separate failed attempts. The
corrected discovery used eleven explicit classes and preserved the existing
source/runtime freeze without repeating completed lower-tier runs.

The implementation owner `/root/extension_consolidation_impl` assumed
`.apm/agents/brilliant-implementer.agent.md`, explicitly invoked as
`gpt-6-astra` / `high`. It personally read the complete C# trio and reported
the same fingerprints as root. Ownership has returned; no child runtime writer
remains. Root commissions the single T26-R1 fresh review and runs full gates
on the frozen source. This is focused acceptance, not completed M6.

## M5 Review And Full Managed Qualification

T26-R1 passed with no material findings on immutable `9fbd3ed1`, tree
`29376bf0e785d8a2867e4b9a55879395bc226338`. The fresh reviewer independently
reconciled all 22 physical accounting paths, four additions, both production
freezes, characterization and focused test identities, seal hashes and all
150 managed byte comparisons. It confirmed live reader validation, exception
ordering/messages, collection ownership, filename consumers, placement and
protected schemas/policies. Root accepts the result; no correction cycle is
needed. `artifacts/task26-m6/review.json` owns the compact review receipt.

Reviewer `/root/extension_consolidation_review` assumed
`.apm/agents/reviewer.agent.md`, explicitly invoked as `gpt-6-astra` / `high`.
It personally read the complete C# trio before review and reported the same
fingerprints recorded above. It performed no runtime or source mutation.

Full managed evidence on `9fbd3ed1` passed 2,837 Unit, 1,570 Integration and
188 public cases, zero failures/skips. Every discovered method and executed row
was reconciled, including deferred theory expansion. Informational formatting
reports zero diagnostics; Release build has zero warnings/errors. Source and
runtime hashes stayed fixed. Exactly three public journeys remain for each of
five Library and six Extension commands. These receipts are retained under
`artifacts/task26-m6/`. Subsequent control prose is a documented overlay only;
native publication/execution and raw native comparison remain before M6.

## M6 Final Acceptance

Task 26 is accepted at phase 4/4, milestone 6/6. Final managed evidence uses
`9fbd3ed178ecc2923436cbd9bf15104534300516`; native evidence uses
`cd4e64f3db931f32076c87767e7d8b5d79b8356f`, tree
`f4c7c339c7f18300d838dd46adb12bdecb71a32d`. The seven-file overlay between
them is control/observation prose only; every other source byte and mode matches.

| Gate                                     | Passed | Failed / skipped |
| ---------------------------------------- | -----: | ---------------- |
| Complete managed Unit                    |   2837 | 0 / 0            |
| Complete managed Integration             |   1570 | 0 / 0            |
| Complete managed public                  |    188 | 0 / 0            |
| linux-x64 Native AOT Integration         |   1570 | 0 / 0            |
| linux-x64 Native AOT public              |    188 | 0 / 0            |
| Managed public runner against native CLI |    188 | 0 / 0            |

All 6,541 executions passed. Every discovered method and executed row is
qualified; exactly three public journeys remain for each of five Library and
six Extension commands. All three native publications succeeded. The root
version/help smokes and ELF x86-64 checks passed. The pinned SDK is `10.0.111`.
Builds and informational formatting remain warning/error/diagnostic-free.

Fifty native scenarios also matched all 150 preserved exit/stdout/stderr files
byte-for-byte. Together with the managed comparison, all 300 raw files match
across 100 runtime-specific scenarios. Fixture file inventories are unchanged.
Final source and runtime manifests remain exact. No public syntax, output,
result or package/lifecycle schema changed. No compatibility wrapper remains.

Canonical qualification is `artifacts/task26-m6/runtime-acceptance.json`;
`acceptance.json` binds the final immutable candidate and complete physical-path
accounting, including both sides of moves and every formerly untracked addition.
T26-R1 passed independently without findings. Static, namespace, callable,
prohibited-pattern, 200-column and protected-path checks pass; all 24 unrelated
dirty-file bytes/modes remain preserved. Earlier failed fixture/oracle/scanner/
discovery attempts are retained separately and do not qualify acceptance.

The result removes dead internal entrypoints and duplicated filename authority
without changing the live reader policy. Global installation was not refreshed;
no external publication occurred. Task 10 now takes the whole accepted CLI
surface for strategic audit before any separately accepted Task 21 correction.
