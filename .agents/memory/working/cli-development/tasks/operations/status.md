---
open-forge:
  description: Implement complete workspace, lifecycle, managed-source, generated, and recovery-bundle status facts
  tags: [Memory, Working, CLI, Task, Status, Observation, Contextual]
---

# Task 15: Status

## Task State

- State: Queued after Task 14 “Extension Install”.
- Permanent mapping: Task 15 “Status” in the
  [project control ledger](../../project-control.md).
- Planned progress horizon: five streamlined phases—Preflight, explicit
  Gray/Red, one coherent implementation and focused-verification pass, one
  fresh whole-task review with at most one grouped improvement pass, and
  acceptance. While queued, no phase is active and milestone 0 of 8 remains
  pending activation.
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
