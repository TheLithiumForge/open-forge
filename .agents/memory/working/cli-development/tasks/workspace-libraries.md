---
open-forge:
  description: Execute the accepted Workspace Libraries contracts through bounded preparation, implementation, evidence, and integration
  tags: [Memory, Working, CLI, Task, Workspace, Library, Contextual, Active]
---

# Task 23: Workspace Libraries

## Sol Implementation Transfer

The user selected Sol for the next Task 23 implementation session, then a return
to the Astra Overseer. The [Sol transfer](../../handoffs/2026-09-07_library-sol-implementation-handoff.md)
supersedes earlier Astra-only allocation and pending-resumption wording for that
slice: GPT-5.6 Sol/xhigh owns substantive work; Luna/max handles bounded routine
execution. The receiving session may begin Green after live-state verification,
then complete Task 23 review and acceptance evidence and return a clean feature
candidate. This session remains halted at phase 2/5, milestone 3/8. No worker
is running; later tasks, integration and publication are not part of the transfer.

## Task State

Task 23 “Workspace Libraries” (phase 2/5): milestone 3/8 — M2 Gray and M3
qualified Red are accepted, including all post-Cleanup consumer obligations.
The task is stopped at the user-directed boundary before P3 Green. It remains
incomplete and `ACTIVE` in the project sequence; no completion is claimed. Task20 Cleanup was
accepted and integrated at `148d378da376d196de183d9564261658164d9d23`, tree
`ab7e488192b435fdefa0b8d30bf1dc853a6b2327`. Root released the consumer merge,
Gray, affected Red, and later Green. The user's subsequent stop boundary
supersedes that Green release: finish and freeze the consumer contracts, complete
and freeze Red, prepare a detailed Green implementation handoff/preflight, then
halt. No Green implementer is assigned and no Green behavior is authorized in
this continuation. Task 23 remains incomplete. The detailed
[Green implementation preflight](#green-implementation-preflight--preparation-before-the-red-stop)
preserves the later implementation sequence and protected boundaries.

- Permanent mapping: Task 23 “Workspace Libraries” in the
  [project control ledger](../project-control.md).
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Accepted group contracts at the recorded contract tip:
  `.agents/memory/crystallized/documents/cli/contracts/library/_library.md`.
- Accepted realization at the recorded contract tip:
  `.agents/memory/crystallized/documents/cli/technical-designs/workspace-libraries.md`.
- Shared boundaries: [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md), [CLI Architecture](../../../crystallized/documents/cli/architecture.md), and [CLI Implementation Directive](../../../../directives/open-forge/cli/implementation.md).
- Contract-freeze preparation baseline: `eb5829ef`, supplied for this
  disjoint contract-freeze pack. The accepted post-Task 20 code baseline and
  completed merge are recorded below.

No Green production owner is assigned. Task Mastermind Noether (GPT-6 Astra/high)
accepted the prepared boundary. The Gray author, all three Red authors, and the
exact execution worker have returned their source/artifact leases. No descendant
is authorized to continue implementation automatically.

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

The accepted stopped boundary is P2/M3: M1, M2 and qualified M3 are complete.
P3/M4–M6 Green, P4/M7 review and P5/M8 full acceptance remain unreleased or
uncompleted. No later milestone or whole-task completion is claimed.

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

Task18 Extension Remove, Task19 Repair, and Task20 Cleanup are accepted
prerequisites. The exact integrated code baseline is `148d378da376d196de183d9564261658164d9d23`
with tree `ab7e488192b435fdefa0b8d30bf1dc853a6b2327`. The existing prepared
Task23 lane at `1aa461dc2ede55553d172ccb02d3878522335e6a` merged that
baseline in `c9fcd98a3037d0374857cefcd4841d1b1796b868`, followed by the
accepted prose closeout in `e1c80f419b04d4f0bdf295134da71843e37b6661`.
Consumer callable and affected Red refreeze remain the current boundary. The
production branch remains `codex/workspace-libraries-contracts`. A continuous
Green owner remains reserved; assignment now requires a later explicit restart
after the requested stop following Red and its Green preflight.

Task24 Extensions Evolution and Task25 projections remain outside this task's
implementation scope. Their separate unresolved product choices are not
implemented here. The original contract-freeze baseline `eb5829ef` remains
historical acceptance evidence.

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

### Post-Cleanup activation and baseline reconciliation

Root released Task23 after accepting Task20 at `148d378d` / tree `ab7e4881`.
The merge preserves the normalized incoming Task23 record, including removal
of the machine-specific cache command and historical-tip contract links. Other
superseded central/history records take the accepted incoming versions. The
Doctor contract resolution retains all twelve accepted Library kinds: 108
baseline kinds plus twelve Library kinds gives 120, with six domains and the
Library subcatalogue under workspace and entry. The incoming removal of the
unsupported Extension unmanaged-like kind remains intact.

The sole continuing Gray author, Lovelace (GPT-6 Astra/high), reconciles shared
C# callables. Cleanup's opaque deletion session remains stateful; stateless
recovery Reader/Catalogue/Store/DeletionGuard callers are adapted directly. The
final Task20 C1 `RecoveryDeletionStorageBoundary` and every operation/session
storage and held-lease guard remain required. Root composition retains both
Library and the integrated Repair, Cleanup, and Extension Remove bindings.
The later consumer addendum follows the accepted preparation order: typed
Library contributor and residual observations, Status/Doctor graphs, selected
typed Repair recovery, Extension Remove ownership, and Cleanup vocabulary and
bundle-only deletion. No compatibility facade or automatic compensation is
introduced. Existing Repair-produced attribution and the Framework-only
comparator remain separate from selected Library residual admission.

T23-GATE-LINE-001 records a missed preparation gate: the independent
`max_line_length = 200` scan over the complete Task23 delta found forty
pre-existing overlong lines in four touched test files at `1aa461dc`. Thirty-nine
are in existing Integration caller files and one is in the Library Unit planning
fixture. Their exact predecessor file hashes and forty line texts are pinned in
`artifacts/task23-post-cleanup/line-length-predecessor.json`, SHA256
`4bc345ac8204a4f9f3251e9f996050552ce94e208b225730e2c977505fe470d5`.
Root authorized newline/indentation-only wrapping, preserving every token,
string, action, and assertion. Lovelace handles the existing caller files;
Hamilton handles only the Library fixture line. Literal patches remain distinct
from required static-call adaptation. Inherited lines are not attributed to the
Astra authors. This omission and correction do not consume or reset T23-R1/C1.

Verification uses repository-relative `dotnet format --include` paths and exact
whole-task file/hash coverage, checks actual analyzer execution, parses complete
JSON diagnostics including lowercase xUnit identifiers, and independently scans
all changed/new C# files for the 200-character maximum. Git whitespace checks
include new authored files before expensive gates and the exact staged delta.
Source writes and artifact execution remain serialized by Noether; disjoint
literal Red wrapping does not release semantic Red changes. Full managed/public
and supported Native AOT evidence remains the M8 gate. Root's later Markdown-only
closeout `6ce0da71` will be adopted at the next clean boundary without replacing
this task record or invalidating executable evidence.

The first post-Cleanup merge verification compiled all six Release solution
projects with zero warnings and errors. The ordinary Root build also produced
the authorized same-worktree development publication. The complete 385-path C#
delta relative to the accepted prerequisite passed the independent 200-character
and Git whitespace checks. Informational formatter analysis ran against those
repository-relative paths and found eight unique diagnostics in three touched
Extension Remove files: seven collection-expression simplifications and one
concrete private return type. All flagged existing lines also occur in the
accepted prerequisite source. The same Gray author corrects that bounded
inherited formatting surface before renewed affected verification; no warning
suppression or behavior change is authorized by this correction.

The consumer callable outline preserves one explicit Library contributor and
neutral typed residual observations, one concrete Status/Doctor result graph,
a finite Library sibling through Repair selection and planning, independent
Extension Remove Library ownership and no-follow facts, and exhaustive Cleanup
vocabulary with bundle-only deletion. Repair residuals never receive fabricated
Markdown occurrences. A selected ordinary recovery effect requires a truthful
neutral callable alongside the existing relative-link capability. Consumer Gray
will be compilable and explicitly incomplete: real observation callers reach
named missing stages until Green. Existing regression assertions remain intact;
passing pure tests cannot discharge those real caller obligations. Baseline
controls precede consumer wiring. T23-R1 and T23-C1 remain reserved.

Post-correction merge gates passed informational formatting for the three
changed files and rebuilt all six Release projects with zero warnings/errors.
The complete formatter coverage is a pinned overlay: 382 unchanged paths retain
the first actual analyzer receipt and the three corrected paths use the renewed
empty diagnostic report. Existing Integration controls passed 143/143; Unit
controls passed 186/187. The remaining Unit failure is the preserved exhaustive
Cleanup vocabulary oracle encountering the newly accepted `Library` producer,
an explicitly deferred consumer mapping obligation, not a static-call or
recovery-session regression. All nine Library and existing consumer help checks
exited zero. Current prepared Library Unit 454 (73 passing, 381 failing) and
Integration 258 (37 passing, 221 failing) reproduce the accepted predecessor's
complete multisets of class, display name, status, and exact failure message.
Duplicate truncated theory names retain their multiplicity. The fifteen public
Library journey assertions remain unchanged; these help checks do not replace
those journeys or advance final Red acceptance.

The owner accepted the compiled prerequisite merge with that named Cleanup
consumer gap still open. Exact source, formatting, execution, row reconciliation,
and wrapping receipts remain under `artifacts/task23-post-cleanup/`. The next
source boundary is the serialized consumer Gray addendum; current phase and
completed milestone count remain P2/5 and M2/8.

### Consumer Gray pre-freeze corrections

`T23-GRAY-ENUM-005` records the owner's bounded conformance correction to four
new finite dispatch models: `RepairAtomicEffect` and
`RepairLibraryRecoveryReceipt` in
`Commands/Repair/Models/Application/RepairLibraryExecution.cs`, plus
`DoctorExactProposal` in `Commands/Doctor/Models/Result/DoctorCandidateModels.cs`
and `DoctorLibrarySubject` in
`Commands/Doctor/Models/Result/DoctorSubjectModels.cs`. These paths are relative
to `src/cli/core/OpenForge.Cli.Core/` in the Library lane. The same author
replaced their abstract dispatch variants with explicit enum keys and guarded
concrete payloads, retaining heterogeneous facts and rejecting unknown values.
The authority is the finite-key and enum guidance in
`.agents/directives/csharp/design.md` and the exhaustive-enum Pattern at
`.agents/patterns/software/exhaustive-csharp-enum-switch.md`. This correction is
specific to those finite dispatch models; it does not prohibit every data
inheritance relationship or C# type pattern.

`T23-GRAY-FIXTURE-006` records a separate pure Unit fixture qualification. The
new Library argument in `StatusAggregationObservationSeed` initially described
unavailable observation, while the unchanged ordering oracle requires exactly
six existing non-Library findings. Correct Library behavior would add another
finding and invalidate that fixture's old meaning. Before freeze, the same
author supplied explicit missing-record facts with a missing snapshot at the
fixture workspace's exact record path, complete observation coverage, and zero
Library sources/mappings. All existing assertions remain unchanged. This is a
declared Unit input, not a production empty-success fallback or a claim about
an unobserved filesystem. Real composition still reaches the named missing
Library observer/classifier. Both corrections precede immutable consumer Gray
acceptance and consume neither T23-R1 nor T23-C1.

T23-GRAY-IDENTITY-007 records the owner-detected receipt construction gap:
entry membership alone did not bind a selected original residual preparation
against a different preparation containing equal entry facts. The same Gray
author strengthened only `RepairLibraryExecution.cs` to compare the selected
verified bundle path, workspace identity, operation, command, attribution and
ordered entries, plus ordinary Before/After contexts and relative-link
Current/After logical paths. It does not resolve link targets or change the
constructor parameter surface. This protects the accepted distinction between
original Library residuals and a new forward Repair preparation; focused Red
must prove mismatched preparation/context refusal independently.

The first consumer gate covered 469 complete C# paths using relative format
includes. It built successfully with zero warnings/errors, but format returned
40 diagnostic entries across 12 files (including repeated reports for the same
sites). The same author corrected the reported findings and required static
Status/Doctor builder/factory callsites without changing existing test oracles.
The revised consumer delta contains 97 C# paths: 87 production and 10 existing
test callers, including 28 new production files. Its canonical sorted path/hash
manifest is `40ccae3ffb42fbd20253e85a5e9f8d725ffbebed160d45ad61348ab6736933e3`.
The second owner freeze covers 476 whole-task paths, with 22 affected renewal
paths; Git whitespace and the independent 200-character scan pass. These
prefreeze corrections consume neither T23-R1 nor T23-C1. Compilation alone does
not establish completed consumer behavior or final Red acceptance.

### Post-Cleanup consumer Gray acceptance

The owner accepted the corrected consumer callable boundary after complete source
freeze, relative analyzer coverage, clean compilation and healthy executable
composition controls. The final 97-path consumer C# manifest is
`f1464b7608f584a35fde90968d58ca3624e2f4cc9f8dce1a90e2c61a70f44e66`;
`artifacts/task23-post-cleanup/consumer-gray/source-freeze-3.json` has SHA256
`5769d3a4bbc4da09cac42dc3c906ac673aac30131125a52f3bf2c6ec12c4b134`.
It pins 476 whole-task C# paths against the accepted operational code baseline.

Format coverage is an explicit source-verified overlay: 454 unchanged paths
from gates-1, 21 unchanged paths from gates-2, and the one corrected Doctor Unit
fixture from gates-3. The third format gate passed with an empty complete JSON
report (36.181 seconds). The second solution build passed with zero warnings and
errors (46.952 seconds); the final one-file test-only correction rebuilt its
Unit project and dependencies cleanly (11.076 seconds). That correction removed
an unused pure workspace local exposed by the static adaptation. It changed no
assertion or production source. All eleven help checks passed against the actual
`open-forge-dev` alias: Library group and five leaves, Status, Doctor, Repair,
Cleanup and Extension Remove. Public scenario behavior remains unimplemented.

The C# source hashes stayed frozen through each gate. Configuration, dependency,
project topology and JavaScript are unchanged by this consumer addendum. The
97-path delta includes 87 production paths and ten existing test callers, with
28 formerly untracked new production files included in exact staging/accounting.
The owner inspected the command→producer→typed domain result→presentation flow,
neutral comparison versus producer policy, selected original recovery authority,
independent Remove ownership, and preserved Cleanup C1 scope. New behavior
stages remain named throws. This is consumer Gray acceptance only: final Red
requires the original three owners to close affected evidence and qualify actual
failures before M3 can complete. Green remains held by the explicit stop order.

### Final Red prefreeze evidence corrections

T23-RED-EVIDENCE-003 records Hopper's own prefreeze correction to a proposed
capability oracle. An unrelated unregistered working link does not require the
read-only producer to discover a Supported capability fact. Doctor forbids
capability probing, and the accepted view retains a nullable independently
proven fact. The corrected evidence preserves independent lifecycle assertions,
forbids inferring Unsupported solely from missing lifecycle, permits null when
no capability observation exists, and requires nonempty evidence for a supplied
fact. Explicit capability states have separate Unit evidence. This was caught
before executable qualification; it is not a production defect or new product
choice, and consumes neither T23-R1 nor T23-C1.

T23-RED-EVIDENCE-004 records the owner's pre-execution fixture qualification
finding. Hopper's first Integration archive setup synthesized a verified
candidate without existing real final readback, leaving the positive archive
precondition unproven before the missing attribution stage. The same author
changed three of its eight files: independently encoded healthy archives now
use the reader-required canonical location for a unique test workspace and
must pass actual final readback. Payload corruption/removal occurs afterward,
retaining the original verified snapshot. Malformed record content and wrong
record targets remain recovery-valid but attribution-invalid. Only exact newly
owned test artifacts are tracked and disposed; existing cleanup policy and
preserved recovery evidence are unchanged. All 42 forecast scenario oracles
remain; no execution is claimed by this correction. Both source freezes are
preserved in the final Red artifacts, and R1/C1 remain unconsumed.

T23-RED-AUTHORITY-005 records a clarified antecedent in Repair's recovery
prose. Hamilton asked whether “delete the bundle” on successful verification
included a consumed original Library residual. The owner and root checked the
unchanged accepted c3f01acb Repair contracts against the current source and
confirmed that this means Repair's newly prepared forward bundle. The original
Library ZIP remains byte-identical, including unselected entries; only explicit
Cleanup may delete it. Concise clarification was added to both Repair passages,
with predecessor blobs and hashes retained in the final Red artifacts. Existing
`OriginalResidual` and `ForwardCleanup` facts and the original-byte-preservation
oracle remain correct. No production shape, behavior, product choice or budget
changed; mixed-operation and selected-subset evidence remains required.

The final Red authors also identified required additive updates to inherited
exhaustive consumer catalogues. Doctor's original 108 literal finding rows remain
and gain exactly twelve accepted Library rows, with truthful 120 totals. New
Doctor subject/proposal, Repair dependency/verification and Cleanup
producer/operation members extend the existing literal sets while preserving
full named-enum coverage, undefined guards and every previous spelling. Exact
predecessors are pinned in
`artifacts/task23-final-red/finite-oracle-predecessors.json`. These test changes
have changed lineage; they are not reported as unchanged prepared controls or
as production improvements. Command consumer recovery fixtures shared by Repair,
Cleanup and Doctor live together under the Integration test
`Commands/Shared/LibraryRecovery/` scope, without shared TestSupport mutation.

T23-RED-GATE-006 records the first combined final Red gate. The 507-file
freeze passed owner whitespace and line-length checks, but informational format
reported 81 entries at 49 unique sites, and compilation found three nullable
uses in new consumer tests. No discovery or test execution followed that failed
build, so none of these failures is classified as intended Red. The original
freeze and logs remain preserved. The same authors corrected only twelve test
files: reported whitespace, static declarations and collection syntax, plus
three explicit non-null assertions before use. No null suppression, relaxed
project settings, production change or removed assertion was used. The renewed
507-source manifest is
`687b5766d7da003917db645d3cad8c2607eb6cea4756fb56e80ec5e701f8a9bd`;
the twelve-file relative informational format renewal passed with an empty
complete diagnostic report (57.994 seconds). The Release solution build passed
with zero warnings/errors (23.13 seconds). All 507 source identities remained
unchanged. Full metadata discovery returned 2582 Unit, 1453 Integration and
208 EndToEnd rows; these are discovered rows, not executions or passing totals.
The focused selection contains 583 Library Unit, 376 Library Integration,
fifteen Library public and three Doctor public rows, plus 234 Unit and 164
Integration controls. All 1375 selected identities are distinct and have direct
source pins. Actual execution and causal qualification remain required before
final Red acceptance.

T23-RED-EVIDENCE-007 records seven setup failures from that first execution,
separately from intended missing behavior. Six new Extension Remove rows had
already proved real installation and trusted lifecycle ownership but lacked the
ordinary parent for their independent source file. One Repair planning row
constructed an inaccessible observation without its required failure fact.
The same author added the two local parent-creation statements and an independent
AccessDenied failure fact. No shared fixture policy, production behavior,
scenario, or safety assertion changed. The exact four-file correction packet
retains predecessor byte copies and hashes under
`artifacts/task23-final-red/correction-1/`.

T23-RED-EVIDENCE-008 records two inherited oracle omissions exposed by affected
controls. Status preserves all 37 prior literal tuples and their order, adding
the ten Library tuples from its accepted Interface catalogue. Repair preserves
all fifteen prior result fields and the envelope, adding `libraryExecution`
first according to the immutable accepted Gray model. The Repair Interface owns
the typed output obligation; that exact field spelling and placement come from
the accepted Gray shape, not a separate Interface property-order table. Root
independently checked both authorities before the same-author four-file
correction. No cases were added, no product choice changed, and R1/C1 remain
reserved. The renewed whole-task freeze contains 508 live C# paths and explicitly
accounts for the canonical deleted `RecoveryBundleEntry.cs`; its source SHA256
is `6a24b82b99ab193c46de5f906c1aa0d1e545bd28ea976be3af79d5254a8734d7`.

The four-file renewal passed informational format and Release compilation with
zero diagnostics/warnings/errors. Its 79 Unit rows reached 18 passing controls
and 61 named missing stages: both additive oracles now pass, and the unavailable
observation reaches its intended planner. The six Remove rows remain
unqualified in this renewal: three outer disposals reject a file link in an
ordinary-only cleanup tree, and three reject the newly arranged unowned source
subtree. Those disposal failures can mask the preceding assertion. They remain
part of T23-RED-EVIDENCE-007's fixture qualification trail; the same author must
inspect the actual complete snapshot/disposal ownership rules before another
bounded correction. No shared cleanup policy or production change is authorized.

### Final Red acceptance and stopped boundary

The one-file fixture lifetime correction is qualified. A narrow private owner
tracks only its successfully created source file, four ordinary directories and
exact projection link, then performs reverse nonrecursive teardown before the
parent fixture disposes. Every existing argument, snapshot and assertion remains.
All six Remove rows now reach the hosted command after validated installation,
lifecycle and source/link setup. They require blocked status (exit 5), but receive
failed status (exit 1) with `extension-remove.operation-failed`. Their method
assertion traces survive disposal. The record-reader prerequisite is inferred
from frozen call order; the generic handled finding alone does not establish
the first exception. Later finding, no-effect, source and raw-link assertions
remain unexecuted. Earlier failed fixtures and receipts remain preserved.

A separate artifact-identity omission was found in the initial execution packet:
its fourteen pinned paths included apphosts and Core/Root, but omitted test DLLs.
Those old Unit/Integration DLL identities cannot be recovered after recompilation;
the first receipt remains limited. Root authorized one final focused execution
to close that gap, without a full managed/AOT expansion. The decisive receipt is
`artifacts/task23-final-red/execution-4/`, freshly executed against the final
508-source freeze. All 39 selected runtime files were present and unchanged
before/after: every Open Forge-owned assembly and symbol at the four
runtime locations, including TestSupport and Core copies, four selected
executables, and relevant deps/runtimeconfig/version files. All 35 expected
owned assembly/metadata members matched the observed runtime inventory.

| Final focused selection       | Selected/executed | Passed | Failed | Skipped |
| ----------------------------- | ----------------: | -----: | -----: | ------: |
| Library Unit                  |               583 |    125 |    458 |       0 |
| Library Integration           |               376 |     62 |    314 |       0 |
| Library public journeys       |                15 |      0 |     15 |       0 |
| Affected Unit controls        |               234 |    221 |     13 |       0 |
| Affected Integration controls |               164 |    108 |     56 |       0 |
| Doctor public journeys        |                 3 |      2 |      1 |       0 |
| Total                         |              1375 |    518 |    857 |       0 |

All eleven help commands passed against the actual development alias. Full
metadata discovery is 2582 Unit, 1453 Integration and 208 EndToEnd rows; those
full-suite totals are not execution or passing claims. Exact class, method and
display-name multisets reconcile every selected/executed row. Discovery UIDs
and CTRF row IDs belong to different identifier namespaces and are not equated.
This is one fresh final focused receipt, not a synthetic execution overlay.

The 857 failures comprise 756 directly named missing implementation stages,
seven projection prerequisites before undefined-enum oracles, thirteen retained
existing behavior gaps, 66 handled consumer expectation failures, and fifteen
public Binder prerequisites. Every row has an observed message/trace, direct
source hash and qualification limit in
`artifacts/task23-final-red/final-cause-attribution.json`. For hosted
Doctor/Repair/Remove failures, exact missing-stage identities inferred from the
frozen caller order are distinguished from observed exception traces. Generic
failed status does not exclude alternative earlier causes. Later safety,
selection, inverse-effect, original-ZIP-preservation and postcondition assertions
remain unexecuted where an earlier stage stops the row.

Passing evidence is bounded too. New neutral admission controls prove rejection,
not inverse application. Attribution cancellation proves ingress cancellation
and unchanged captured state, not successful attribution or in-flight behavior.
The retained foreign-operation create refusal is masked by blanket create
rejection; a dangling ordinary-guard pass alone cannot establish no-follow
safety. Eight Repair receipt controls validate real opaque identities; ten
Cleanup cases exercise actual typed bundle deletion, dry-run, malformed-final
preservation and real lease refusal. They do not establish completed Library or
Repair behavior.

Prepared lineage is preserved: 86 of 89 source files remain byte-and-mode
identical to `1aa461dc`; the two Status/Doctor consumer files and the previously
authorized planning-data line wrap have explicit changed lineage. All 454 prior
Unit outcomes and 253 of 258 prior Integration outcomes reproduce exactly on
status/message multisets. The five changed outcomes are confined to the
previously deferred Status/Doctor rows. The formerly deferred six Status/Doctor rows now have frozen callables and
qualified Red, alongside the other consumer additions; their Green behavior is
still open.
The original fifteen Library public journeys and three Doctor journeys are
unchanged.

The final C# freeze is `artifacts/task23-final-red/source-freeze-4.json`, SHA256
`9c2bf9639a9edb1781bbe8c0fa9549726f25e45a7137830163364316965b564f`.
It covers 508 live paths against accepted code baseline `148d378d`, accounts for
the canonical deleted `RecoveryBundleEntry.cs`, and includes the 35-file final
Red delta with all 27 formerly untracked test files. Production remains exactly
at accepted Gray `cdcc988f`; Repair clarification `05674893` changes prose only.
Whole-file 200-character and Git whitespace gates pass. Informational format
coverage is the verified 494 + 10 + 3 + 1 path overlay across four preserved
receipts, with actual analyzer runs, relative includes and complete diagnostic
JSON. Each covered source hash matches its receipt; final applicable reports are
empty. Renewed Release builds passed with zero warnings/errors. No dependency,
configuration, JavaScript, production behavior or public journey was changed by
final Red. R1/C1 remain reserved and unconsumed.

Exact immutable commit/tree, artifact SHA256 index and stopped-process evidence
are sealed in the final acceptance artifacts and the identity section below.
This accepts the Gray/Red preparation boundary only. M4–M6, M7 and M8 remain
unfinished, and Green requires a new explicit release.

### Green implementation preflight — preparation before the Red stop

This section prepares later M4–M6 work; it does not release it. The final
immutable source, evidence and artifact identities are sealed with the accepted
Red boundary. Paths in this section are repository-relative code paths that resolve
in the frozen Library feature worktree. Abbreviated `Framework/...` and
`Commands/...` paths are relative to `src/cli/core/OpenForge.Cli.Core/`.
A Library leaf `Shared/...` path resolves beneath
`src/cli/core/OpenForge.Cli.Core/Commands/Library/<Leaf>/`, where `<Leaf>` is
`List`, `Inspect`, `Attach`, `Sync`, or `Detach` as named in that paragraph.
The Library-wide `Shared/...` path resolves beneath
`src/cli/core/OpenForge.Cli.Core/Framework/Libraries/` when describing foundation
record, source, inventory or observation capabilities. Main receives this prose record only;
the unfinished Library source is not integrated into `develop`.

#### Authority and ownership

Read the current Loader and selected CLI, C#, architecture, implementation and
evidence-tier authorities before resuming. Every C# author and reviewer must
independently read the complete current C# directive trio and record its actual
SHA256 fingerprints. The Library group and five leaf Interface/Behavior
contracts under `.agents/memory/crystallized/documents/cli/contracts/library/`,
and `.agents/memory/crystallized/documents/cli/technical-designs/workspace-libraries.md`,
define the accepted capability. The accepted Task23 Repair exception introduced
at `c3f01acb` remains authoritative alongside the later operational baseline.
Do not restore Task19's earlier narrower Repair snapshot over this accepted
Library exception. The explicit exception is selected typed residual recovery,
with no new syntax, generic rollback, automatic compensation or fixpoint.

Retain one coherent Green owner for production and focused verification after
a later explicit release. Serialize shared foundations, composition, shared
fixtures and build/publication artifacts in this worktree. Leaf planning,
completion and presentation policy remains leaf-owned; place only identical
shared meaning at the nearest actual shared scope. Preserve one semantic
result/fact graph for human and JSON presentation, with envelopes, projections
and converters owned by presentation. Do not add a generic reducer, compatibility
facade, runtime registry, reflection dispatch, dependency injection or JavaScript
implementation. The reserved whole-task review and grouped correction budgets
remain T23-R1 and T23-C1; preparation corrections do not reset or consume them.

#### M4: neutral observation, recovery and Library foundations

Implement `Framework/Filesystem/PhysicalPaths/NoFollowLeafObserver.cs` first,
using the accepted physical containment and leaf facts. Observe links themselves,
retaining raw target and target form; never resolve or read their targets.
Ordinary paths, missing leaves, inaccessible or special objects, aliasing and
parent-boundary uncertainty remain distinct. Apply the accepted guards through
real ordinary mutation and Route/Index callers as well as Library callers.

Implement `Framework/Recovery/Observation/RecoveryEntryObservationReader.cs`
and `RecoveryEntrySetObserver.cs` separately from the pure
`Framework/Recovery/Comparison/RecoveryEntryComparer.cs`. Ordinary content
identity must be independently read when applicable; a no-follow observation
alone has no bytes or hash. Preserve exact verified candidate and ordered entry
identity, context agreement and prior/intended/third-state distinctions. Keep
the existing Framework-only comparison and producer admission policy separate.

Complete `RecoveryBundleReader.ReadSelectedFinalAsync` using fresh exact final
readback to obtain the opaque original preparation. Implement
`OrdinaryFileRecoveryApplier.ApplyAsync` and
`RelativeFileLinkRecoveryApplier.ApplyAsync` under a held same-workspace lease,
exact original candidate and entry membership, fresh intended-state proof,
verified prior payload when needed, and post-effect verification. A later lease
does not need the original producer command or operation ID. Ordinary Create
restores prior missing by exact deletion; Replace and Delete restore verified
prior bytes, including Delete's intended missing state. Relative-link Create
is inverted by exact deletion and Delete by recreation of the exact raw relative
target, including a dangling target. Preserve all unrelated source and consumer
state. Recovery application does not belong in Cleanup.

Implement the strict record codec/reader in `Framework/Libraries/Shared/Record/`,
source-boundary reader in `Shared/Source/`, complete inventory in
`Shared/Inventory/`, and registered mapping observations in `Shared/Observation/`.
Use the accepted strict schema-v1 `.agents/open-forge.libraries.json` record;
distinguish missing, malformed, unavailable and complete records. Inventory
only the declared registered source roots. Preserve the first-release `.agents/**`
restriction, reserved control exclusions, ordinal identities and physically
disjoint source/destination boundaries. Never infer adoption from matching
unregistered links.

#### M5: List and Inspect

Complete the existing leaf `Shared/Binding/` binders and invalid-result formation,
then `LibraryListOperation` and `LibraryInspectOperation`, followed by their
leaf-owned projection and human presentation. List uses the accepted bounded
record/root/registered-link observations; Inspect inventories only its selected
registered source. Do not introduce complete source enumeration for List.
The omitted required Inspect ID is the second public journey; an unknown supplied
ID remains lower-tier evidence. All five commands retain exactly three public
journeys each. Output-only converter `Read` methods intentionally throw and are
not missing Green behavior.

#### M6: mutations and operational consumers

Implement `LibraryConsumerBoundaryReader` and the Attach/Sync/Detach pure
planners over explicit immutable boundary and ancestor observations. Do not
perform hidden filesystem reads in a planner or infer parent absence from the
final leaf. Preserve source-relative raw links, destination collision checks,
independent lifecycle ownership and complete intended inventories.

Each leaf Operation must collect real preflight facts, apply its plan under the
accepted lease and recovery protocol, collect independent execution receipts,
and call its existing `Shared/Completion/` stage. Complete monotonic verified
and residual facts survive cancellation, cleanup failure and unexpected failure.
Do not feed a derived `Outcome.Application` back as proof of itself. Revalidate
all required facts before effects, retain verified preparation before mutation,
and publish the consumer Library record last. Sync and Detach preserve source
bytes and unknown occupants; no automatic recovery or compensation is introduced.

Complete `LibraryOperationalContributor` with separate bounded Status and
complete Doctor views, independently observed lifecycle ownership and proven
capability facts. Status uses `StatusResultBuilder` → `StatusLibraryAggregator`,
with command-owned counts, ten finding kinds and its own rendering. Doctor uses
`DoctorDiagnosisReader` → `LibraryResidualAttributionReader` →
`DoctorResultBuilder` → `LibraryDoctorInspector`, retaining every registered
root in declared coverage. Missing inventory contributes
`library.inventory-incomplete`; it never shrinks coverage to a safe prefix.
The twelve Library finding kinds remain within Doctor's six existing domains.
A safely missing record means zero Libraries with complete coverage.

The attribution reader must independently connect current-v1 record membership,
verified residual candidate and exact observed entry. A verified prior record
can establish accepted membership when the current record is missing; it must
be decoded from and bound to the exact verified prior payload. Do not fabricate
a Markdown occurrence or treat a path-only match as Library recovery authority.

Repair consumes typed Library proposals as a sibling of reference proposals:
`RepairOperation`, `RepairLibraryCatalogueReader`,
`RepairLibraryRecoveryWizard`, and `RepairLibraryRecoveryPlanner.Build` own
selection and planning. `RepairPlanRevalidator.ValidateAsync` reads fresh facts
and calls `RepairLibraryRecoveryPlanner.Revalidate`.
`RepairLibraryRecoveryApplication` owns mixed preflight, lease acquisition,
all-effect revalidation, one global atomic effect order, forward-reference
preparation, original Library entry application and retained execution facts.
Original Library residual preparations and the new forward Repair preparation
remain distinct throughout the receipt graph. Success cleanup deletes only the
newly prepared forward Repair bundle. Preserve the original Library ZIP bytes
and all unselected entries; deletion of that original belongs only to explicit
Cleanup.

`RepairPostVerifier.ReadLibraryPostDiagnosisAsync` must obtain fresh diagnosis;
`RepairLibraryRecoveryApplication.Complete` and `RepairLibraryResultFormation`
form the existing result from immutable receipts and independent cleanup,
cancellation, unexpected-failure and post-diagnosis facts. Prove mixed selection,
partial outcomes and all-effects-before-first-write revalidation through the
real Operation; pure seam tests alone cannot establish these caller obligations.

Extension Remove must consume `ExtensionRemoveLibraryBoundaryReader` and
`ExtensionRemoveLibraryBoundaryPolicy` before target resolution or content reads,
including shared-owner paths. Plans retain independent record/no-follow facts,
and `ExtensionRemovePlanComparer` compares fresh observations. Existing lifecycle
ownership is a separate input, not a substitute for Library ownership.

Cleanup retains its accepted bundle-validation/deletion scope and the stateful
opaque deletion session. Preserve Task20's `RecoveryDeletionStorageBoundary`,
lease/storage checks, frozen membership and semantic entry comparisons. Its
Library change is exhaustive producer/operation vocabulary and correctly
validated attributed bundles; never apply an entry or delete a Library record,
projection or source through Cleanup.

#### Verification and restart boundaries

The exact command recipe is retained at
`artifacts/task23-post-cleanup/green-preflight/verification-recipe.md`. It separates
current Red renewals from deferred Green/full managed/public/Native AOT gates,
and includes static, protected-path, callable and prohibited-pattern inspection
commands. Its final SHA256 and exact discovery/execution index will be sealed
with the final Red receipt.
The immutable Gray missing-stage inventory is
`artifacts/task23-post-cleanup/green-preflight/missing-stage-inventory.json`:
79 named missing stages across 50 Core files, pinned to `af402efb`. This source
inventory excludes intentional output-only converter reads and does not replace
real caller, safety, effect or result evidence.

Preserve original prepared receipts, failed renewals, corrected overlays and
source lineage. A test stopped by a named missing Gray stage qualifies only
that early failure; its later safety or effect assertions remain unexecuted.
Healthy public help is a startup control, not a replacement for a public journey.
Lifecycle and residual fixtures must first prove their real-reader preconditions;
a refusal caused by missing ownership cannot establish link safety.

Before every executable renewal, freeze exact source paths and hashes, run Git
whitespace checks over the final authored delta including new files, and scan
every whole changed C# file for the 200-character ceiling. Use repository-relative
`dotnet format --include` paths, actual analyzer-run receipts and complete
diagnostic JSON, including lowercase xUnit IDs. A passing absolute-include probe
or empty partial report is not format coverage. Preserve unchanged-row receipt
overlays when exact source pins justify them.

Restore locked dependencies only if required assets are missing. Use the
explicit existing local NuGet cache source without dependency/config changes or
NuGetAudit suppression; this proves cached restoration, not a fresh online
vulnerability audit. Ordinary same-worktree Root builds produce the accepted
managed development publication. Pin the executable actually selected by public
tests, its version marker and corresponding assemblies; a differently named
apphost help check is not proof of the E2E-selected alias.

M7 whole-task review and M8 full managed/public plus supported linux-x64 Native
AOT gates remain deferred until Green. They are not satisfied by compilation,
help checks, qualified Red failures or historical Task20 receipts. Task24
destination allowlist/package naming and Task25 projection choices remain open
and out of scope. Restart requires explicit release, verification of the sealed
clean lane and preserved artifact/source identities, personal authority loading,
and assignment of the reserved coherent Green owner. Do not resume automatically
from this preflight.

#### Sealed Red identities and stopped restart point

Final Red source/test commit is
`fa29630db0f825b47023b8db02db96c8c0284584`, tree
`b9177404b3fce7ff86c79bf47bf5253f26504885`: exactly 35 test paths and this
Task record, including all 27 formerly untracked test paths. Production remains
identical to accepted consumer Gray `cdcc988fed4b6889b5268fa20d6114abbff11b8e`,
tree `12552d3b63060d1c5c48853ef2b9f12d6c839636`. Repair clarification
`05674893e3e23fcf0df3ac8902a553668f2fe54f`, tree
`ecb1ba98094925489e24b7d4168ed1e4271e7bba`, remains authoritative. The commit
containing this identity section is a prose-only descendant of the final Red
commit; its exact tip/tree is pinned in the root's sealed transfer receipt.

The following repository-relative artifact paths resolve in the frozen Library
worktree. They retain complete receipts without duplicating row traces here:

| Artifact                                                               | SHA256                                                             |
| ---------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/task23-final-red/owner-acceptance.json`                     | `b919d81ccb15654c67463a1affa85458cd662fba0f021fa167ebd11379dd6750` |
| `artifacts/task23-final-red/artifact-index.json`                       | `5aaea02d6c92b8541393d00eba8020e402a9d4b2617d778f8275e82e6a126bd8` |
| `artifacts/task23-final-red/source-freeze-4.json`                      | `9c2bf9639a9edb1781bbe8c0fa9549726f25e45a7137830163364316965b564f` |
| `artifacts/task23-post-cleanup/green-preflight/verification-recipe.md` | `43cb56fda754de85e68f564893c99e15d931e558dcfd582c4376254454fedc81` |

The 277-entry index pins final execution 4, all preserved failed/corrected
receipts, row qualification, original-source/row lineage, source freezes,
format/build evidence, configuration/toolchain provenance, accepted consumer
Gray and the Green preflight. It excludes itself and the subsequently written
prose-only handoff commit receipt to avoid circular hashes. Execution 4 is the
single decisive final 1,375-row receipt; format coverage is the explicitly
verified 494 + 10 + 3 + 1 source overlay. The whole-task freeze retains all 508
live C# paths and the canonical `RecoveryBundleEntry.cs` deletion.

All five descendants completed and returned their source/artifact leases:
Lovelace, Turing, Hopper and Hamilton used Astra/high; Curie's exact mechanical
execution used Luna/max. The owner confirmed no active build, compiler, test,
format or Open Forge CLI process at the source stop. Final clean-tip and process
confirmation is retained in `artifacts/task23-final-red/final-handoff-commit-receipt.json`
and the root transfer. Task23 is stopped at Phase 2/5, Milestone 3/8. Green is
unimplemented and unreleased; no descendant may restart automatically. Resume
only after explicit release and verification of these identities, using the
M4/M5/M6 sequence and deferred M7/M8 gates above. R1/C1 remain reserved.
