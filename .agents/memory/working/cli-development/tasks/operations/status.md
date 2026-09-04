---
open-forge:
  description: Implement complete workspace, lifecycle, managed-source, generated, and recovery-bundle status facts
  tags: [Memory, Working, CLI, Task, Status, Observation, Contextual]
---

# Task 15: Status

## Task State

- State: Active in isolated worktree
  `/home/tedy/dev/open-forge-worktree/status`, branch `codex/status`, from exact
  activation base `e321fd45067cf7e2105e6ed62334636afb5fa136`, tree
  `5e84659b5310b8cc1672764b8ae3ce253122cbb4`.
- Permanent mapping: Task 15 “Status” in the
  [project control ledger](../../project-control.md).
- Current progress: phase 3 of 5, milestone 3 of 8. The five streamlined
  phases are Preflight, explicit Gray/Red, one coherent implementation and
  focused-verification pass, one fresh whole-task review with at most one
  grouped improvement pass, and acceptance. Two bounded Gray/Red corrections
  are accepted. The second preserves the mechanical failure stage required for
  existing lifecycle consumers to retain distinct status policies. Coherent
  production and focused verification have resumed from that boundary.
- Incremental completeness: the first accepted horizon covers every observable
  producer present in its exact frozen baseline and records that complete typed
  contributor inventory. It does not call absent future commands healthy,
  supported, or silently omitted. Every later producer must extend the explicit
  contributor inventory and pass affected Status evidence before that producer
  is accepted. Final release requires a complete revalidated inventory for all
  retained producers.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/status/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/status/behavior.md).

## Read-Only Preparation Receipt

Read-only Sol/xhigh preparation completed against clean `develop` commit
`328599a006ef206fd82004e778296c2cac2bc10c`, tree
`56a24a7a50550702eae13bcbfaed9e9ead2a19f3`, without edits, builds, tests,
artifacts, activation, or consumption of Task 14's mutable worktree. It verified
that no Status command or operational contributor catalogue exists yet and
inventoried the current workspace, context/source, embedded Framework,
lifecycle/Extension, generated-navigation, and recovery producers and evidence.

The smallest neutral forecast is `Framework/OperationalContributors/**` for one
immutable application-scoped catalogue and its narrow typed view contracts,
with each concrete contributor remaining beside its producer and Status-only
joining, ordering, availability, findings, and rendering remaining under
`Commands/Status/**`. This forecast does not freeze signatures or authorize
source changes.

After Task 14 integrates, Preflight must refresh the producer inventory and
resolve three Gray authority gaps before Red: the exact command-local JSON
result graph, the generated-state representation, and confirmation that
"current target" means lifecycle-managed current/changed/missing facts rather
than recovery-derived target inspection. Task 15 remains queued with milestone
0/8 and no active phase until that revalidation and activation occur.

## Task 14 Green Revalidation

The just-completed read-only Task 15 revalidation used immutable Task 14 Green
commit `9a6ae2fa509d7bf1268f6012650e41655d3c27fe`, tree
`c39cfcbec1ea532ee27060020c681caa9b306245`, from its clean
`codex/extension-install` worktree. The current `develop` tree is not that
candidate. No mutation, build, test, artifact, activation, Gray, production,
or implementation work was performed; Status remains queued at milestone
0/8 with no active phase.

Task 12's accepted architecture remains one immutable application-scoped
`OperationalContributorCatalogue` explicitly built by `CliCompositionRoot`.
Status and Doctor receive the catalogue through producer-owned narrow typed
views backed by fresh per-invocation observations. This is not DI, a service
locator, reflection, a runtime registry, a generic operational engine, ambient
registration, or a broad context bag. Catalogue members are concrete typed
properties, never an enumerable runtime registry. The six domains remain:

1. workspace and entry;
2. recovery and residuals;
3. routes, metadata, overwrites, and generated navigation;
4. local references, catalogued for Doctor even when Status does not render them;
5. Framework lifecycle; and
6. Extension lifecycle.

Task 14 Green adds persisted Extension lifecycle facts for package
ID/version/source/dependencies/paths and path owners/baseline fingerprint/
fingerprint kind, Framework generated-region lifecycle baselines, and neutral
exact-name recovery-catalogue inputs. Status must not consume the Extension
Install request, selection, topology, plan, intended bytes, effects, result,
application progress or receipts, command-local recovery preparation or
cleanup, or final-verification wrappers as current truth.

Gray must still close:

- complete Framework per-target observation;
- complete Extension per-target observation with source availability and
  shared-owner deduplication;
- generated-navigation observation that distinguishes current, changed,
  missing, unavailable, blocked, and not applicable;
- reusable startup and continuity closure rather than Context full-graph reuse;
- initial embedded-payload measurement;
- the exact command-local JSON graph, order, nullability, finding schema, and
  source-generated context;
- the exact six contributor/view call surfaces and same-invocation snapshot
  rules; and
- root registration and help order.

Current, changed, and missing are fresh lifecycle-managed target comparisons.
Recovery exposes candidate path, kind, and integrity only; it cannot infer
activity or health.

Expected production locality remains `Commands/Status/**`, neutral
`Framework/OperationalContributors/**`, narrow producer-adjacent views, and
the exact standalone/root composition surfaces. No other command's private
`Shared/**` may be imported.

Evidence must test Open Forge-owned behavior: focused Unit typed mapping,
policy, rendering, and JSON; real-producer Integration journeys with
byte-for-byte no-write checks; small published Native AOT EndToEnd public
journeys; and the full managed and Native AOT gates because composition, shared
contributors, and serialization surfaces change. False-green guards are
explicit:

- generic status mapping can pass while no Status command is callable;
- testing current `develop` misses the Task 14 producer tree;
- Context full-graph reuse, aggregate Framework currentness,
  projection/planning states, embedded-payload identity, or the narrower
  Extension Inspect reader can yield plausible but incomplete facts;
- recovery evidence must prove no fallback, live-target, lock, or activity
  inspection, preparation, or payload materialization; and
- envelope-only, partial or zero, stale `--no-build`, skipped-test,
  warning-bearing, or wrong-scope negative evidence is false green.

Activation remains blocked only on Task 14 acceptance/integration plus Gray
closure. Any change to lifecycle publication, generated topology, source
identity, ownership, recovery, verification, or composition invalidates this
revalidation.

## Active Implementation Receipt

Corrected raw-snapshot Gray `b859d0aafd34bbe0ba86ef466e0f9eb441e0a634`,
tree `8f417c58306dbd1f2b80084993bb683b93bec207`, and corrected Red
`d768ca5292eec73493ad62fe843063b317b1f7bb`, tree
`7a9b785b976182959868b83ee2c82eb547f70f4c`, are accepted. Resume checkpoint
`d120d49eca2b02ecda3ccaab5a3f3f573dc5abc0`, tree
`28f23430e312276fecc7c61efaa98e2c0ad6562b`, restored the retained production
work under the same Brilliant Implementer.

The first coherent production seam is
`44104f03f1d596b47f395eeb7798b974fb88877c`, tree
`71a19f8563a5c4c5e3b99fd8dd223a0849c79617`. It implements Status observation
aggregation and one immutable lifecycle-document snapshot shared by both
lifecycle contributors. Core Release builds with zero warnings and errors.
The direct Status operation Integration seam passes, while the other nine
selected Status Integration cases remain expected Green work at absent command
composition or rendering. Sixty-two selected lifecycle-store and Extension
compatibility tests pass. Production continues from this checkpoint; no task
completion or broader evidence claim is made.

Immutable seam review found `T15-S1`: the common snapshot had collapsed
physical-resolution failure into the contained-file-access classification,
changing Extension List/Inspect from blocked to incomplete for unresolved
physical identity. Corrected Gray
`e6a89c24e3eaad1fc2f785ded785ac5ec53e9928`, tree
`9f98c0e4ebd6e7fda4daf47cdba67d3f82e677f6`, freezes only three raw mechanical
failure stages and the exact prior-consumer mapping. Corrected Red
`c5726e5cda571fe09a22a742121e5f1a3ec3859d`, tree
`f719db23d57a79f15d731da133bc569cdd597751`, adds one in-memory compatibility
fact plus snapshot invariants. Its isolated 2/2 Unit run passed the invariant
fact and failed only the two intended physical-resolution and
physical-reconfirmation Extension mappings, with zero skips or warnings.
Rendering stash `fa35ffbc1fc44f952d1e469d4ca9f78dd1b17232` is reapplied losslessly;
both recovery stashes remain retained while the same Brilliant Implementer
continues Green. Phase and milestone remain unchanged.

## Expected Outcome

`status` reports complete typed current facts for workspace, Framework and
Extension lifecycle, managed content, generated navigation, and external
recovery bundles without diagnosis, recommendation, repair, cleanup, or
mutation. It does not inspect or report repository state.

## Architecture

- Keep definitions, binding, request, operation, result, rows, findings,
  renderers, and help at `Commands/Status/`.
- Consume the same readers and fact models used by real producers. Do not copy
  lifecycle, generated, recovery, or ownership parsing.
- Create local `Shared/{Aggregation,Rendering}/` support for status-specific
  joining, ordering, availability, and projections.
- Preserve domain provenance so Doctor can later consume typed facts without
  parsing status output.

## Requirements

Implement accepted workspace/no-workspace behavior, lifecycle sections,
installed identities, drift/missing/changed states, generated state, recovery
bundle and current-target states, availability, deterministic order,
compact/expanded/JSON, diagnostics, help, status precedence, and next actions.
Distinguish zero, absent, unavailable, unmanaged, not applicable, incomplete,
and unsafe. Recognized bundles use the strict external schema and static
prior/intended fingerprints; Status never extracts, restores, or rebinds them.

## Evidence

Unit fixed-fact aggregation and renderer tests; Integration fixtures produced by
actual install/update/extension/index/mutation paths; malformed and unknown
bundle state; read failures; unchanged snapshots; complete process scenarios;
AOT; and regressions for every producer.

## Stop Conditions

Stop before diagnosing causes, assigning health grades, recommending changes,
planning effects, or maintaining a duplicate workspace database.
