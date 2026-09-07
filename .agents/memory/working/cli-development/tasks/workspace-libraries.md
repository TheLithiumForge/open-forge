---
open-forge:
  description: Execute the accepted Workspace Libraries contracts through bounded preparation, implementation, evidence, and integration
  tags: [Memory, Working, CLI, Task, Workspace, Library, Contextual, Active, Queued]
---

# Task 23: Workspace Libraries

## Task State

Task 23 “Workspace Libraries” (phase 2/5): milestone 2/8 — M2 Gray is accepted; M3 Red is current and incomplete. The task is task-locally `ACTIVE` for
its accepted contract, Gray, and Red preparation. In the project queue it
remains `QUEUED` after Task 20 “Cleanup” for semantic Green and integration.
Contract, Gray, and Red preparation may proceed in parallel with that queued
sequence; Green waits for an exact post-Task 20 integrated baseline refreeze.

- Permanent mapping: Task 23 “Workspace Libraries” in the
  [project control ledger](../project-control.md).
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Accepted group contracts at the recorded contract tip:
  `.agents/memory/crystallized/documents/cli/contracts/library/_library.md`.
- Accepted realization at the recorded contract tip:
  `.agents/memory/crystallized/documents/cli/technical-designs/workspace-libraries.md`.
- Shared boundaries: [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md), [CLI Architecture](../../../crystallized/documents/cli/architecture.md), and [CLI Implementation Directive](../../../../directives/open-forge/cli/implementation.md).
- Contract-freeze preparation baseline: `eb5829ef`, supplied for this
  disjoint contract-freeze pack. The production baseline is deferred until the
  exact post-Task 20 integrated baseline is refrozen.

No Green production owner is assigned yet. Task Mastermind Noether (GPT-6 Astra/high) owns the task; Lovelace owns the completed Gray contracts.

## Outcome And Profile

Deliver the accepted `open-forge library` surface as one complete, deterministic
Workspace Libraries capability. It registers a contained source root for one
consumer workspace, projects its complete eligible `.agents` inventory through
relative file links, and preserves source and consumer ownership boundaries.
The result includes the five public commands, strict record handling, typed
recovery, cross-command link safety, Status and Doctor integration, focused and
public evidence, and later integration after Task 20.

The streamlined Assured profile is required because this is a new public
five-command surface that combines a persistent strict record, real filesystem
links, source and destination containment, mutation and recovery safety, and a
cross-command guard for Route Update, Index, Route Move, and Route Remove. The
profile keeps Gray and Red as explicit freeze boundaries, one continuous Green
owner, one fresh whole-task review, and one grouped correction budget.

## Authority And Accepted Contract

The Crystallized Library contracts define public meaning. The Technical Design
defines the accepted neutral filesystem, mutation, recovery, and Library fact
boundaries. This task capsule defines execution ownership, sequence, evidence,
and protected surfaces; it does not broaden those sources.

The accepted public group and order are:

1. `open-forge library list`
2. `open-forge library inspect <library-id>`
3. `open-forge library attach <library-id> <source-root>`
4. `open-forge library sync <library-id>`
5. `open-forge library detach <library-id>`

The accepted final choices are:

- A Library is local filesystem composition, not a new Framework root, Loader
  federation, imported-content runtime, or Extension replacement. The consumer
  keeps one Loader and its own route chain.
- A source root is a normalized workspace-relative path to a real ordinary
  directory. It must contain a real ordinary `.agents` directory. A missing,
  linked, reparse, aliased, inaccessible, special, or externally resolving
  source boundary is invalid or blocked as defined by the command contract.
- The source root and consumer destination namespace are physically disjoint.
  Eligible ordinary files below the source `.agents` directory map to the same
  consumer-relative `.agents/...` paths through relative file symlinks. Real
  parent directories may be created only for declared destinations. Source
  bytes remain untouched and there is no copy fallback.
- The strict schema-v1 consumer record is exactly:

  ```json
  {
    "schemaVersion": 1,
    "libraries": [
      {
        "id": "team-knowledge",
        "sourceRoot": "shared/team-knowledge",
        "paths": [".agents/directives/review.md"]
      }
    ]
  }
  ```

  The root has only `schemaVersion` and `libraries`; each record has only `id`,
  `sourceRoot`, and `paths`. IDs and paths are unique and deterministically
  ordered. Expected link targets are derived from `sourceRoot` and destination
  paths, not stored in the record. Extra, missing, null, duplicate, or
  differently typed properties are malformed. The record is
  `.agents/open-forge.libraries.json` and remains separate from the lifecycle
  record.

- Library IDs use the lowercase ASCII stable-ID grammar and are management
  identities separate from automatic source IDs. A projected file keeps the
  normal destination-derived source ID. A Library ID is never a source-reference
  operand or an Extension `--source` value.
- The source inventory admits only eligible ordinary files. It excludes the
  source Loader, entrypoints, adjacent overwrite companions, lifecycle and
  Library records, other manager controls, links, junctions, reparse points,
  and special entries. An incomplete or unsafe inventory cannot authorize a
  mutation or a retirement.
- List is lightweight: it reads the strict record and bounded registered-link
  facts without a complete source inventory. Inspect forms the complete source
  inventory and compares every recorded mapping. Detail beyond these public
  journeys remains lower-tier evidence.
- Attach validates the new ID and source boundary, forms a complete inventory,
  detects every collision, and applies only the declared projection, permitted
  existing generated-region changes, and final record publication. A missing
  ordinary source `.agents` directory is invalid; a collision blocks the whole
  request.
- Sync requires a complete current inventory before it considers additions or
  retirements. It recreates missing exact links, adds current paths, and retires
  only exact registered relative links, including a dangling link whose raw
  target is still proven. An incomplete source or changed occupant blocks all
  effects; Sync never adopts an unregistered link or applies an unaffected
  subset.
- Detach is whole-library and all-or-nothing. It removes only exact registered
  relative links and publishes the record removal last. An exact dangling link
  is removable when no-follow observation, raw-target identity, and typed
  recovery proof all match. A changed occupant or unverifiable link blocks all
  effects. Source files are preserved.
- Safe, complete observed drift is `attention`; unsafe mutation drift is `blocked`. Missing IDs are `invalid`. Complete, incomplete, failed, and
  interrupted results retain their contract-defined meanings and stream/status
  rules.
- Mutations form one complete immutable plan, preflight all facts, acquire the
  normal workspace lease, revalidate under the lease, prepare typed strong
  recovery, apply monotonic declared effects, verify every effect, and publish
  the Library record last. Dry-run uses the same plan and performs no lease,
  recovery, record, link, generated-navigation, or source effect. Shared support
  does not automatically roll back or compensate.
- The neutral no-follow final-leaf guard runs before physical resolution, during
  preflight, under the lease, and immediately before each effect. Route Update,
  Index, Route Move, and Route Remove must therefore block on a Library
  projection instead of following it or deleting its source target. This guard
  does not consult Library records.
- Status consumes lightweight Library facts and Doctor consumes accepted typed
  full-inventory and mapping findings. Neither mutates, repairs, adopts a
  projection, acquires a lease, or invokes a Library command. Extension
  lifecycle ownership and existing generated-navigation boundaries remain
  explicit.
- The first release performs no Git behavior or Git diagnostics, even when a
  contained source directory is a submodule. It has no collection selection,
  path remapping, glob, dependency, copy mode, write-through mutation, external
  destination, compatibility reader, JavaScript/MJS/CJS path, or publication.

## Execution Capsule

### Phases And Milestones

The task horizon has five phases and eight milestones:

| Phase | Milestones | Accepted boundary                                                                                                                                                                                                                                        |
| ----- | ---------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| P1    | M1         | Contract and control freeze. Accepted Library contracts, exact record shape, behavior matrix, budgets, ownership, baseline, queue relation, and protected paths are recorded.                                                                            |
| P2    | M2, M3     | M2 is one Gray owner freezing callable and public shape. M3 is three disjoint Red slices: neutral guard and typed recovery; List and Inspect; mutation, cross-integration, and public evidence.                                                          |
| P3    | M4, M5, M6 | M4 establishes the neutral no-follow guard, typed recovery, record, and complete-inventory foundations. M5 implements List and Inspect. M6 implements Attach, Sync, and Detach and integrates Status, Doctor, Extension, Repair, and Cleanup boundaries. |
| P4    | M7         | Fresh whole-task review, including behavior, production structure, and test/evidence quality, followed by one grouped correction when accepted findings require it.                                                                                      |
| P5    | M8         | Full managed/public and supported `linux-x64` Native AOT acceptance, then semantic integration after Task 20 from the exact refrozen baseline.                                                                                                           |

The current boundary is P2/M3: M1 and M2 are complete, and Red remains incomplete.
No later milestone is claimed.

### Ownership And Budgets

Task Mastermind Noether (GPT-6 Astra/high) owns task-local architecture readiness, boundaries,
state, review routing, and acceptance preparation. Gray has one owner. Red has
three disjoint owners. One continuous Brilliant Implementer will own Green,
verification, and the grouped correction after the exact accepted post-Task 20
integrated baseline refreeze; that production owner is not assigned yet.

| Resource                          | Maximum | Reserved or consumed                                                                                                                |
| --------------------------------- | ------: | ----------------------------------------------------------------------------------------------------------------------------------- |
| Council                           |       0 | None                                                                                                                                |
| Gray owners                       |       1 | Continuing owner Lovelace (GPT-6 Astra/high); Gray accepted                                                                         |
| Red owners                        |       3 | Turing: neutral foundations; Hopper: List/Inspect; Hamilton: mutation/cross-integration/all fifteen public journeys; Red incomplete |
| Brilliant Implementers            |       1 | One continuous owner reserved; unassigned                                                                                           |
| Writing review                    |       1 | `T23-WR1` consumed                                                                                                                  |
| Writing correction                |       1 | `T23-WC1` consumed                                                                                                                  |
| Whole-task review                 |       1 | `T23-R1` reserved and unconsumed                                                                                                    |
| Grouped implementation correction |       1 | `T23-C1` reserved and unconsumed                                                                                                    |

`T23-WR1` and `T23-WC1` are the consumed writing review and correction IDs.
This correction pack consumes no implementation, Gray, Red, whole-task review,
or grouped implementation-correction budget.

### Writing Receipt

The `T23-WR1` findings and `T23-WC1` dispositions are accepted as follows:

- `T23-WR1-F1`: `NoFollowLeafObservation` is neutral and exhaustive. A generic
  link identity retains safely observable raw target and target form; every
  non-relative link blocks ordinary and Library link effects, while only the
  exact relative-file-link identity is accepted by effects and recovery.
- `T23-WR1-F2`: Attach classifies missing or non-ordinary mandatory source
  boundaries as `invalid`, unavailable or incomplete record/source facts as
  `incomplete`, malformed record/input as `invalid`, and unsafe, aliased,
  ambiguous, or colliding identity as `blocked`; none has effects.
- `T23-WR1-F3`: Doctor treats a safely proven missing Library record as zero
  Libraries with complete coverage and no Library finding. An unreadable
  existing record uses `library.record-unavailable` and incomplete coverage.
- `T23-WR1-F4`: Doctor attempts complete inventory for every source root named
  by a readable strict record. Those roots are its declared Library coverage;
  incomplete inventory emits `library.inventory-incomplete` and incomplete
  coverage, without unregistered-source enumeration or safe-prefix inference.
- `T23-WR1-F5`: The List journey names only an absent record or a strict empty
  record.

The Workspace Libraries lifecycle step 7 sentence fragment is corrected. Sync
retains missing-retired-destination blocking, and parent-directory residuals
remain monotonic with no automatic recovery.

### Sequence And Baseline

Task 18 “Extension Remove”, Task 19 “Repair”, and Task 20 “Cleanup” retain their
current states and completion-grace records. Task 23 remains queued after Task
20 for semantic Green and integration. Contract, Gray, and Red preparation may
proceed in parallel while those dependencies are pending. Green, full evidence,
and integration wait for the exact post-Task 20 integrated baseline to be
refrozen and accepted for this task.

Task 24 “Extensions Evolution” remains queued and inactive after Task 23 behind
its separate contract freeze; its record is not part of this capsule.

The contract-freeze pack uses the supplied task-local baseline `eb5829ef`.
Future implementation must establish and record the exact post-Task 20 source
commit and tree before Green. No production source owner, production branch,
production worktree, or production implementation is assigned by this capsule.

### Expected Production And Test Paths

These paths are forecasts, not an allowlist. A directly required neighboring
path may be added only when accepted meaning requires it and the addition is
reported.

- Production capability paths: `src/cli/core/OpenForge.Cli.Core/Framework/Filesystem/`, `src/cli/core/OpenForge.Cli.Core/Framework/Mutation/`, `src/cli/core/OpenForge.Cli.Core/Framework/Recovery/`, `src/cli/core/OpenForge.Cli.Core/Framework/Libraries/`, and the existing `src/cli/core/OpenForge.Cli.Core/Framework/GeneratedNavigation/` and `src/cli/core/OpenForge.Cli.Core/Framework/Serialization/` seams.
- Production command paths: `src/cli/core/OpenForge.Cli.Core/Commands/Library/` for List, Inspect, Attach, Sync, and Detach policy, plans, facts, results, and rendering.
- Composition and public integration paths: `src/cli/root/OpenForge.Cli/Composition/`, `src/cli/core/OpenForge.Cli.Core/Shell/`, the relevant serialization paths, and the command-owned registration/help seams required by the accepted command group.
- Unit evidence paths: `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Filesystem/`, `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Mutation/`, `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Recovery/`, `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Libraries/`, and `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Library/`.
- Integration evidence paths: `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Filesystem/`, `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Mutation/`, `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Recovery/`, `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Libraries/`, `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Library/`, and directly affected Status, Doctor, Extension, Repair, Cleanup, Route, Index, and generated-navigation tests.
- Shared fixture evidence path: `src/cli/tests/support/OpenForge.Cli.TestSupport/` for real-workspace, no-follow, link-capability, and source-preservation setup shared by the focused suites.
- Public process evidence paths: `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/` with exactly three simple public journeys per Library leaf, fifteen total, plus the existing exactly three Doctor journeys.

### Direct Integration Neighborhood

Integration must inspect the actual `CliCompositionRoot`, explicit Library
binding and command registration, shared result coordinates, source-generated
JSON, the neutral filesystem and mutation capabilities, recovery attribution,
generated-navigation projection, Status and Doctor contributor composition,
Extension ownership and lifecycle readers, Repair and Cleanup artifact
boundaries, and the Route Update, Index, Route Move, and Route Remove final-leaf
guard. It must preserve the root/Core dependency direction and the existing
project, package, dependency, Native AOT, and test-project topology.

### Protected Paths And Authorities

The following are hard boundaries. The task may not mutate or reinterpret them
unless a directly required neighboring change is already accepted by the
contract and recorded at integration:

- Source bytes and the source tree, including the source `.agents` directory and
  every source file, directory, link, reparse point, and special object.
- The consumer Loader, route entrypoints, overwrite companions, and other
  manager controls. The only planned manager-record exception is the exact
  consumer Library record `.agents/open-forge.libraries.json`; it remains a
  strict consumer record and is published last.
- The consumer lifecycle record, other lifecycle state, Extension-owned paths
  and ownership, generated regions outside their accepted bounds, and unrelated
  consumer files or local siblings.
- Arbitrary links, reparse points, aliases, and special files. No operation may
  adopt, follow, overwrite, or delete a non-exact occupant.
- JavaScript, MJS, and CJS source; package, dependency, project, solution,
  build, artifact, and Native AOT topology.
- Compatibility, migration, dependency injection, runtime registries,
  reflection-based discovery, generic mutation dispatch, and untyped operation
  catalogues.
- Remote Git state, Git operations and diagnostics, publication, deployment,
  release, credentials, and every external system.

## Behavior And Acceptance Evidence

The accepted matrix is finite and exact. Lower-tier Unit and Integration tests
may cover malformed records, path identity, aliases, link capability,
incomplete inventories, collisions, recovery races, generated-region bounds,
status precedence, streams, cancellation, and unchanged bytes. Those tests do
not create additional public journeys.

### Public journeys: `library list`

1. **Empty record, complete.** When the Library record is absent or present as
   a strict empty record, invoke List and observe `complete`, zero libraries,
   and `inventory=not-requested`.
2. **Healthy records, deterministic complete.** With strict records and current
   links, invoke List twice and observe identical ordinal records, paths, source
   IDs, observations, and `complete` status.
3. **Safe observed drift, attention.** With a strict record and a safely
   observable missing or changed registered link, invoke List and observe
   `attention`, the link finding, and `inventory=not-requested` without a source
   inventory claim.

### Public journeys: `library inspect`

1. **Healthy full inventory, complete.** With one strict record, a real
   ordinary source root, a complete eligible inventory, and matching links,
   invoke Inspect and observe deterministic `complete` facts and
   destination-derived source IDs.
2. **Required ID omitted, invalid.** Invoke Inspect without its required
   Library ID operand and observe `invalid` without a selected-library claim or
   filesystem observation.
3. **Complete additions, retirements, and drift, attention.** With a strict
   record and safely complete inventory containing additions, retirements, or
   missing or changed links, invoke Inspect and observe `attention` with exact
   comparison relations.

### Public journeys: `library attach`

1. **Dry-run parity and no effect.** Run Attach with `--dry-run` and observe the
   complete inventory, mappings, collisions, generated-region projection, and
   record plan without directory, link, record, lease, recovery, or source-byte
   effects.
2. **Successful exact apply preserves source.** Apply Attach for an eligible
   source and unoccupied destination, then observe the exact relative link and
   strict record while source bytes remain unchanged.
3. **Missing ordinary source `.agents` or collision blocks all effects.** Run
   Attach with a source root lacking a real ordinary `.agents` directory or with
   an occupied destination and observe `invalid` or `blocked` as applicable with
   no link, directory, record, generated-region, recovery, or source effect.

### Public journeys: `library sync`

1. **Unchanged complete no-op.** Run Sync with a complete inventory and matching
   registered links and observe `complete`, no link or record effect, and
   unchanged source and consumer bytes.
2. **Addition and retirement reconcile exactly.** Run Sync dry-run and then
   apply after one eligible source addition and one retirement. Observe only the
   exact link create, exact registered-link retirement, generated-region change
   where permitted, and sorted record change, with source bytes unchanged.
3. **Incomplete source or changed occupant blocks all effects.** Run Sync with
   incomplete inventory or a changed or unsafe destination and observe
   `incomplete` or `blocked` with no link, generated-region, or record effect.

### Public journeys: `library detach`

1. **Dry-run parity and no effect.** Run Detach with `--dry-run` and observe the
   exact complete deletion and record plan without lease, recovery, projection,
   record, or source effects.
2. **Exact apply preserves source and may remove an exact dangling link.** Apply
   Detach for exact registered links, including an exact dangling link, and
   observe link removal, last record publication, no-follow/raw-target/recovery
   proof, and unchanged source bytes.
3. **Changed occupant blocks all effects.** Run Detach with a changed,
   unverifiable, or unsafe occupant and observe `blocked` with every projection,
   record, recovery, and source state preserved.

These are the only fifteen Library public journeys. Preserve exactly three
Doctor EndToEnd journeys. All additional detail remains lower-tier evidence.

## Verification, Recovery, And Stop Conditions

The selected evidence ladder is focused Unit and Integration proof for each
slice, the fifteen Library public journeys and retained three Doctor journeys,
directly affected Route/Index/Extension/Status/Doctor/Repair/Cleanup
regressions, formatting and protected-path checks, then the full managed/public
and supported `linux-x64` Native AOT gate at P5/M8. The acceptance gate must
execute the published executable and prove arguments, streams, statuses, exits,
links, record bytes, source preservation, recovery, and unchanged-state cases.

Typed strong recovery covers the prior-missing Library record Create and exact
relative-link Create/Delete effects. A link identity is its raw relative target
and link kind; recovery does not resolve the target or store source bytes. A
verified exact dangling link may be recreated when the recorded identity and
no-follow state match. A changed, third, unsafe, or unavailable occupant blocks
recovery. No automatic restore, rollback, compensation, or source mutation is
claimed.

Stop and return to the Task Mastermind before changing product or architecture
meaning, adding dependencies or projects, weakening source or link safety,
following or writing source bytes, introducing Git behavior or diagnostics,
creating a collection/remap/glob/copy/write-through mode, changing lifecycle or
Extension ownership, changing compatibility or public wire shape, or requiring
remote, publication, release, or deployment authority.

## Completion And Residual Risk

Complete only when M8's full managed/public and supported `linux-x64` Native
AOT evidence passes, the fresh whole-task review and any single grouped
correction are closed, the exact post-Task 20 integration baseline is accepted,
all fifteen Library journeys and exactly three Doctor journeys remain present,
and current contracts, architecture, Plan, project control, and Checkpoint
agree. The replacement CLI remains non-shipping until its separate delivery and
release authorities accept it.

Residual risk remains capability-gated real file-link support on platforms
without equivalent evidence, cooperating-process races beyond the lease and
no-follow checks, and failures after verified monotonic effects. Such outcomes
retain exact residual and recovery evidence for explicit Repair or Cleanup;
they do not authorize automatic compensation or source changes.

### Gray authority correction — T23-GRAY-DOC-001

The Inspect Interface owns the exact three public journeys. Its second journey
requires an omitted ID operand; this capsule previously substituted an unknown
supplied ID. The parent accepted the authority correction on 2026-09-07. The
second journey above now follows the Interface; unknown supplied ID behavior
remains covered at the cheapest sufficient lower tier. The total remains fifteen
public journeys. This pre-acceptance correction consumes neither T23-R1 nor T23-C1.

### Astra restart correction receipt — 2026-09-07

The resumed task owner personally matched the sealed 102-path inventory at
`c3f01acb76c572ee486fdc24c6a2379b27459391`, with no staged paths or active
compiler/test process. The broad foundation diff is the accepted static recovery
owner migration, typed ordinary/link recovery and neutral Gray support; direct
callers and their compile/analyzer adaptations account for its breadth. No
unrelated implementation scope was added.

The owner replaced `RecoveryEntry` intended-state suppression with explicit
delete/non-delete validation branches. A fresh Integration build exposed missed
Route Move/Update static fixture callers and one deleted recovery-store helper
caller. Five additional test files were necessary to complete that migration.
The owner also removed the codec's null suppression with a truthful
`NotNullWhen(true)` contract and removed three suppressions in already touched
tests using compiler-proven state. Nine newly exposed CA1822 diagnostics in four
required caller files were corrected with static method declarations.

Grounded model observation: the continuing Astra/high owner completed these
compiler-guided corrections; one intermediate owner edit omitted a namespace
import and required a further build correction. The previous CLI help probe used
an unsupported argument placement and failed in the test driver; direct
executable help resolved the syntax. These observations support no quantitative
model cost or speed comparison. The same Astra/high Gray author independently
loaded the full C# rules, produced a finite accepted callable/representation
proposal, and remained read-only throughout the correction evidence gate.

Task phase remains 2/5, milestone 1/8. Gray acceptance, Red acceptance, Green,
and integration are not claimed by this correction receipt.

The correction gate passed before staging: nonincremental Core and Root Release
builds and fresh Unit/Integration Release builds had zero warnings/errors; exact
changed-file severity-info format passed for Core 75, Root 1, Unit 3, and
Integration 27 files. Focused recovery discovery and execution matched: Unit
20/20, Integration 10/10, zero failures/skips, with minimum thresholds 11 and 10.
The complete changed-source suppression/stale-type/static-constructor scans and
`git diff --check` passed. The 39 lines over the 200-character guideline already
exist unchanged in the base. Exact commands, logs, discovery, source inventory,
and assembly hashes are in lane-local `artifacts/task23-astra-restart/`. These
receipts prove only this bounded foundation correction, not Library behavior.

### Gray ownership correction — T23-GRAY-ARCH-001

Pre-freeze owner inspection found that three mutation plans and three application
outcomes referenced presentation-owned transport types. The five concrete
command results also retained semantic payload graphs under Presentation. The
root accepted the placement correction on 2026-09-07: retain one semantic fact
graph at its actual command Result/Planning/Application owner, reuse shared facts
only at the nearest real common owner, and keep envelope/projection/converters
presentation-owned. Both renderers consume the same concrete result. This does
not require duplicated DTO packs, a generic mapping framework, a new public
policy, or a new review/correction budget. The continuing Gray author owns this
correction before immutable acceptance.

### Immutable Gray acceptance — 2026-09-07

The task owner accepted the sole Gray author's callable and public shape over
immutable reviewed tree `d633895a50e5e8d5e5bc5c18888e2415b2f93094`, based on
parent `02df72d38e38a37b10f264b8692ad837f593797c`. All 178 C# source paths matched
the sealed manifest SHA256
`25ab9bbe115a802726d0bcdcaa607c5af08746cec44b20d76ab58af26dea9f93`, including
four relocated originals. The final acceptance-record update changes only this
task capsule. T23-GRAY-ARCH-001 is closed: one semantic fact graph has its actual
Result/Planning/Application owner, and concrete transport contexts own converters.
The owner checked source dependency direction, required command/mutation facts,
exact List/Inspect shapes and finding vocabularies, reachable enum converters,
explicit incomplete-behavior seams, root composition, and sealed source/artifact
identities. The owner and author independently read the complete current C# trio
and reported matching personal SHA256 fingerprints.

After the storage interruption, local-cache restores rebuilt missing assets with
locked mode with the existing local package cache as its explicit source,
preserving pinned dependencies
and NuGetAudit settings; no fresh online vulnerability audit is claimed. Fresh
Core, Root, Unit, and Integration Release builds passed with zero warnings/errors.
Exact severity-info format verification covered all 174 live C# files: Core 171
and Root 3. All six managed-bin Library help invocations exited zero with nonempty
stdout and empty stderr. No new tests, dependency/config edits, semantic Library
behavior, published EndToEnd, or AOT acceptance is claimed.

The ordinary Root build produced its authorized same-worktree development
publication. An overly narrow owner instruction caused one unnecessary passing
Root rebuild (28.87 seconds) before the root corrected that instruction. This is
a workflow observation, not evidence of external publication or model superiority.
One stale empty Git index lock from the interruption was removed only after the
root and owner verified its exact identity and absence of a live holder. Source
bytes and the actual index were preserved until authorized exact-path staging.

Exact commands, logs, model graph, manifests, and artifact identities are retained
under lane-local `artifacts/task23-gray/`. Red may now use this accepted callable
freeze; its three disjoint evidence owners remain the bounded next step. T23-R1
and T23-C1 remain reserved and unconsumed. Green still waits for Task 20 completion
and the root's exact post-Task 20 baseline refreeze.

### Red assignments and bounded callable gaps

The accepted Gray commit is `acdf93f759a1092504e23ae930b07d2a10727320`, tree
`c7fb69e72ee656c5da1f11bebdda362aa4ad2d24`. The three reserved Red owners are now
assigned: Turing (`turing_red_foundations`), Hopper (`hopper_red_read`), and
Hamilton (`hamilton_red_mutation`), each assuming `red-evidence-author.agent.md`
on GPT-6 Astra/high. Each independently read the complete current C# trio and
reported matching personal fingerprints. They author disjoint test files; shared
artifact execution is held for serialized parent-owned gates.

Red exposed two bounded callable questions before execution. T23-GRAY-REC-002
concerns expressing typed missing/link recovery comparison without weakening the
existing Framework-only bundle comparison contract. T23-GRAY-RESULT-003 concerns
directly callable command completion over immutable execution evidence for
post-effect failure, cancellation, and recovery-disposition precedence. Affected
rows await the bounded immutable addendum; unaffected evidence proceeds. The root
accepted a neutral recovery-entry observation/comparison seam that retains
no-follow observations plus independently obtained ordinary content identity,
and leaf-owned completion over immutable execution receipts and independent
cleanup, cancellation, and unexpected-failure facts. The same sole Gray author
is implementing these two corrections. Existing Framework producer admission
remains unchanged; missing behavior explicitly throws. Real production callers
must consume both seams, and pure evidence cannot substitute for IO integration.

The owner mistakenly described Hamilton's boundary as Library compensation
wiring. Hamilton caught this before edits; the instruction was retracted for both
affected Red owners. Attach, Sync, and Detach never automatically roll back or
compensate. They preserve verified effects and residual recovery evidence after
failure. Neutral recovery supports explicit recovery only.

Existing Library operational contributor/view contracts already represent bounded
Status and complete registered-source Doctor coverage. Their producer-local Red
rows proceed now. Status, Doctor, Repair, and Cleanup consumer shapes, wiring,
and affected evidence remain Task 23 scope but are explicitly deferred until the
post-Task 20 baseline merge/refreeze to preserve shared ownership. Extension Remove, Repair, and
Cleanup commands are absent from this lane baseline; their required integration
rows are likewise deferred, while existing Extension Install/Update evidence
proceeds. Complete final Task 23 Red
acceptance requires that later callable addendum and actual consumer evidence;
current prepared Red is not full acceptance. Green remains held. These are
contract-completeness and prerequisite-alignment corrections; T23-R1 and T23-C1
remain reserved.

T23-GRAY-PLAN-004 records the third Red-discovered callable gap: mutation
planning inputs omitted consumer-boundary and required-ancestor observations,
although the immutable plan includes directory creation. The root accepted the
smallest explicit immutable fact input, reusing neutral observations where
sufficient. Planners remain pure and never infer parent absence from a final
leaf or inspect the filesystem. The same Gray author closes this gap alongside
REC-002 and RESULT-003 before affected planning evidence resumes.

### Deferred consumer obligations after the prerequisite baseline

This manifest preserves required Task 23 scope. Its rows are not waived, passing,
or covered by producer-only tests. They require the accepted post-Task 20
baseline, a bounded consumer callable/composition freeze, and actual caller
integration evidence before complete Red acceptance and Green release.

| Consumer         | Remaining callable/composition and evidence obligation                                                                                                                                                                                                                                                | Current evidence boundary                                                                              |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| Status           | Consume bounded Library record, source-root and registered-link facts; expose the accepted Library result and finding vocabulary with no complete source inventory.                                                                                                                                   | Direct Library producer tests now; consumer integration deferred.                                      |
| Doctor           | Consume complete inventory for every registered source root in workspace-entry coverage, preserve incomplete coverage and typed Library findings/provenance/proposals.                                                                                                                                | Direct Library producer tests now; consumer integration deferred.                                      |
| Repair           | Preserve the accepted selected `library.recovery-safe-exact` exception: verified current-v1 attribution and no-follow proof for bounded prior-missing record, ordinary-byte, and relative-link recovery. No new syntax, generic rollback, automatic compensation, source-target effects, or fixpoint. | Neutral comparison/application evidence now; consumer integration waits for the prerequisite baseline. |
| Cleanup          | Preserve source and consumer effects while classifying and deleting only positively recognized eligible residuals.                                                                                                                                                                                    | Neutral bundle/deletion evidence now; consumer command absent from lane baseline.                      |
| Extension Remove | Preserve independent Library ownership and apply the accepted collision/no-follow boundaries.                                                                                                                                                                                                         | Install/Update boundaries now; Remove command absent from lane baseline.                               |

The root clarified that its earlier “real Library recovery path” instruction
meant the accepted neutral guarded recovery capability, not automatic Library
recovery. The owner caught an overstrict new guard before acceptance: matching
the recovery lease to the original producer command and operation ID would
exclude a later explicit held lease. The same Gray author corrected it to held
lease, verified workspace agreement, and exact entry membership; the accepted
producer admission policy remains unchanged.

Root then temporarily instructed removal of the deferred Repair exception after
consulting Task 19's earlier contract snapshot. The owner checked Task 23's clean
immutable Repair Behavior blob `0a6487b580639597b39fca9b7ffce7ee9b7bba80` and
Interface blob `5b0b66f2106952f2c1569ef3baa9abe43d55e2ff`, which explicitly retain
the selected typed Library residual exception. Root reconciled history and
confirmed that accepted `c3f01acb`/tree `5ccffb71` introduced that exception. Its
narrower intervention was a false positive and was superseded; the exact bounded
deferred obligation above was restored. Original accepted contract blobs were
not changed. This correction required no user decision or T23-R1/T23-C1 budget.
The sole Gray author and both affected Red owners received the corrected scope.
All consumer implementation still waits for the prerequisite baseline.

### Accepted bounded Gray addendum

The owner accepted REC-002, RESULT-003, and PLAN-004 over immutable tree
`79b978a2bafdf83b201e506157483f8a5dadb68b`, based on `acdf93f759a1092504e23ae930b07d2a10727320`.
All 36 exact production paths (20 modified, 16 new; Core 35, Root 1) were staged
only after corrected evidence passed, and every immutable blob matches source
manifest `4e04628aebead3aec8c51bd2eb4e8c46c2efc72fde4cb1c5ec0d18c28a3697b7`.
The owner independently reconstructed final unchanged-byte format coverage,
including the later one-file recovery-guard verification, and checked current
Core/Root artifact identities. Final Core/Root Release builds passed with zero
warnings/errors (25.67/21.52 seconds). Earlier passing source receipts are
preserved as superseded. No warning suppression or source-byte verification
probe was added.

The reviewed graph preserves independent execution evidence and direct real
leaf completion callers, pure planning over explicit ancestor observations, and
a neutral observation/comparison path consumed by the existing guarded recovery
capability. Missing behavior still throws. Red owners may now adapt to this
immutable shape; no Red execution or final Red acceptance is claimed yet.
The later feature commit will combine the accepted correction with independently
verified current Red evidence after a complete file freeze. Green and deferred
consumer scope remain held. Full owner evidence is under
`artifacts/task23-gray-addendum/owner-acceptance.json`.

### Prepared Red evidence and bounded corrections

The accepted Gray addendum was exercised by three continuing Red owners:
Turing (foundations), Hopper (List/Inspect), and Hamilton (mutation and public
journeys), each assuming `red-evidence-author.agent.md` on GPT-6 Astra/high.
Lovelace remained the sole `gray-contract-implementer.agent.md` owner. Every
C# author independently loaded the complete current directive trio and reported
unchanged SHA256 fingerprints. All source owners returned a full-file freeze;
Noether serialized compilation, publication, formatting, and execution artifacts.

T23-RED-EVIDENCE-001 records first-pass evidence defects and their correction,
without consuming the reserved whole-task T23-R1/T23-C1 budget. Initial compile
failures included raw interpolated JSON delimiters, platform-flow analysis,
a null assertion, and reflection-dependent fixture serialization. Corrections
used proper syntax, compiler-proven platform branches, and AOT-compatible literal
fixture encoding; no warning suppression was introduced. Subsequent execution
exposed native-parser/help expectations outside the Library binding owner,
three fixture replacement errors, malformed Loader declarations, two record
race rows falsely passing on blanket contract rejection, and an under-lease row
that stopped at initial validation. The same owners corrected those defects.
Human and JSON renderer rows were separated so each missing renderer is reached.
The discovery increases were authored evidence changes (21 independent JSON
rows and four under-lease rows), not production improvements.
Five public journey files remained byte-identical during this correction; the
shared fixture and diagnostics changed. Exactly fifteen public journeys remain.

The first corrected source manifest was
`648420555b7f4935e8c7324add3512c2ecba49e636ad9688cebd6175f7c8801c`
for 89 test files: 34 Unit, 49 Integration, and 6 EndToEnd. Final info-format
coverage combines the prior exact-file verifications with the latest 18-file
verification (3 Unit, 14 Integration, 1 EndToEnd), all unchanged-byte passes.
Fresh test-project Release builds passed with zero warnings/errors in
10.75, 14.42, and 4.18 seconds respectively, using the already verified unchanged
Core/Root assemblies. Their copied assembly identities match the accepted
production artifacts. Managed same-worktree development publication was refreshed
through the standard no-build/no-restore target; cache-only locked E2E restore
preserved dependencies and NuGetAudit. This is not a fresh online audit or a
native/AOT release gate.

| Current evidence                                 | Discovered/executed | Passed | Failed | Skipped |
| ------------------------------------------------ | ------------------- | ------ | ------ | ------- |
| Library Unit                                     | 454                 | 73     | 381    | 0       |
| Library Integration                              | 258                 | 37     | 221    | 0       |
| Exact public journeys                            | 15                  | 0      | 15     | 0       |
| Existing Framework recovery Unit controls        | 20                  | 20     | 0      | 0       |
| Existing Framework recovery Integration controls | 10                  | 10     | 0      | 0       |

All selected discovery locations belong to the frozen test manifest. Unit has
374 named missing implementation failures and seven enum-oracle rows stopped
at missing result projection. Integration has 203 named missing implementation
failures and 13 existing behavior gaps: ordinary final-link rejection (2),
record-create admission/race classification (3), cancellation result formation
(3), Route Update (2), Index (1), and Extension ownership (2). The remaining
six Integration rows are deferred Status/Doctor consumer obligations: five fail
and one absent-record control passes. None of those six counts as accepted
consumer integration. Prepared current Integration therefore comprises 252 rows
(36 pass, 216 fail), with six explicitly deferred rows retained in source.

Every public process failure identifies the respective Library request-binding
stub, three per command. Direct same-publication `--help` succeeded with empty
stderr; this is not a shared startup or composition crash. Binding failure,
including omitted Inspect ID, leaves later scenario assertions unreached. A
failing row establishes its recorded reached stage or behavior mismatch only;
it does not establish coverage of later assertions. Earlier failing reports,
corrected compiler receipts, and superseded manifests remain available rather
than being rewritten as first-pass success.

Current receipts, per-row cause attribution, unchanged-byte format coverage,
source and execution artifact identities are under
`artifacts/task23-red/correction1/`. The original first-pass epoch is retained
under `artifacts/task23-red/`. This is prepared Red evidence against the accepted
Gray shape, not complete final M3 acceptance. The deferred consumer manifest
above remains binding, including the accepted typed Repair exception. M3 and
Green remain held for the exact post-Task20 baseline and consumer addendum.
These observations record actual defects, correction scope, outcomes, and
elapsed times; they make no model-superiority or token-savings claim.

T23-RED-EVIDENCE-002 records the separate causal-review discovery that the four
passing Move/Remove rows could be blocked by missing lifecycle ownership before
link safety. The same author added ownership setup and exact existing safety
finding assertions. The first focused renewal then exposed an invalid reused
fixture: complete Framework coverage had no managed targets. All four rows
correctly failed on `ownership-unavailable`, rather than falsely passing on exit 5. That intermediate receipt remains under `artifacts/task23-red/correction2/`
and is rejected as Red behavior evidence. The author then read the real reader's
complete-coverage invariants and a passing Route/lifecycle fixture, supplied one
unrelated ordinary managed target with consistent independently encoded record
and ownership facts, and explicitly qualified both trusted sections and the
exact unrelated claim before each Route safety assertion. The shared fixture
also feeds fifteen Library Application rows, so the final affected selection is
nineteen rows across five classes. This is repeated fixture rework under the same
finding, not a production improvement or a new review budget.

The qualified nineteen-row renewal proved that the real reader trusts the
corrected lifecycle setup. Fifteen Application rows reached their named missing
Apply stages. Four Route rows then exposed an overconstrained oracle: the exact
linked target was blocked as `reference-unsafe`, while the tests required only
`source-unsafe`. The owner checked accepted Move reference coverage (Behavior
217–228), Remove reference coverage (Behavior 211–221), and the ordinary-target
catalogue classification. The contract permits this safety boundary without
prescribing which of those stages must discover it. The same author therefore
allowed those two existing command safety classifications while retaining the
exact target, blocked status, cause, qualified ownership, raw link, and complete
unchanged snapshot assertions. Qualified/intermediate receipts are preserved;
no production failure is inferred from these four oracle mismatches.

Final 002 verification passed all four Route rows with the qualified lifecycle
and exact linked-target safety/no-effect assertions. The accepted current test
manifest is `650fff330e805136d1112d225757ed4a56edb9d16bc1a15af76b6afae98af514`.
The final 89-file format coverage overlays the qualified shared-fixture check
and the last two Route checks over unchanged earlier receipts. The final
Integration build passed with zero warnings/errors. Current Integration counts
are explicitly composed from 235 unchanged correction1 rows, fifteen renewed
Application rows, four final Route rows, and four whitespace-renewed generated
region rows; all 258 original selected identities
are accounted for exactly once. Unit 454 and public 15 source/callers remained
unchanged, so their prior receipts remain applicable. Existing recovery controls
also retain unchanged test and production source; they were not rerun merely to
refresh receipt wording. The current counts in the table above remain unchanged,
but the four Route passes now prove the intended safety boundary.

Noether accepted this bounded prepared evidence set for a coherent feature
commit together with the already accepted 36-path Gray addendum. The complete
freeze contains 36 production files, 89 new test files, and this task record
(126 paths). Full-file hashes, final per-row causal attribution and receipt
composition, and execution identities are retained under
`artifacts/task23-red/accepted/`. This does not close M3: the six current deferred
Status/Doctor rows and the remaining consumer callable/integration obligations
must be refrozen after Task20 before final Red acceptance and Green release.
T23-R1/T23-C1 remain reserved. All four descendants returned ownership and are
inactive; actual models/reasoning remained GPT-6 Astra/high throughout.

The final cached Git whitespace gate caught two indentation-only empty lines
inside Attach/Sync raw-string fixtures after scoped `dotnet format` had passed.
Noether removed only those indentation bytes; delimiter stripping already made
the runtime lines empty. The exact two-file format/build and four affected
GeneratedRegion rows were refreshed before final blob replacement. This is a
mechanical final-gate correction. Future packets check the complete authored
delta, including new files, for Git whitespace before expensive artifact gates,
and retain the cached check at staging.
