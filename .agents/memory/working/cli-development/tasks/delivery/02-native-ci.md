---
open-forge:
  description: Complete current linux-x64 native CI and reproducible artifact collection
  tags: [Memory, Working, CLI, Task, Distribution, NativeAOT, CI, Contextual]
---

# Complete Current Native CI

## Task State

- State: Active for preparation only. CI and artifact mutation remains Planned
  after complete command acceptance and package source.
- Permanent mapping: Task 13 “Native linux-x64 CI and Reproducible Artifacts” in
  the [project control ledger](../../project-control.md).
- Parent: [CLI Delivery](_delivery.md).
- Selected profile: The opt-in
  [Supervised Luna Preparation Trial](../../../../../workflows/supervised-luna-preparation-trial.md).
- Exact preparation base: local `develop` commit
  `c8051478127ab9204ca31b1d86ef358dd982207f`, tree
  `ccc762115238c4317b3b35963be18edd38be2609`.
- Lane: `codex/native-ci-preparation` in
  `open-forge-worktree/native-ci-preparation`.
- Current phase and milestone: phase 1 of 3, milestone 0 of 6.
- Current-state suffix: M1 current CI, package, artifact, and checksum gap
  inventory active.
- Responsible role: One experimental Luna/max Task Mastermind supervising at
  most three bounded read-only evidence workers, followed by one fresh Sol/xhigh
  whole-task review.

The project control ledger defines permanent identity, queue state, worktree
mapping, and integration state. This Task record defines the accepted horizon,
preparation result, evidence, trial measurements, and later revalidation.

## Expected Outcome

CI proves the current `linux-x64` native build and smoke, packed install and
invocation, and checksums. It records exact SDK/dependency and artifact facts and
collects reproducible bounded artifacts without publishing them.

## Matrix

- `linux-x64` on one native Linux runner.
- Restore from repository-root `NuGet.Config` with package audit.
- Native root build and direct smoke for `linux-x64`.
- Packed install/invocation journey for the accepted launcher and platform
  package.
- SHA-256 checksums for the exact bounded native and packed artifacts.

D1 consumes the separately recorded proportional whole-candidate managed and
Native AOT material gate. Full managed Unit, Integration, and EndToEnd suites and
native Integration/EndToEnd publishes or executions are Architecture/A1
evidence, not D1-owned requirements. A shared CI workflow may schedule them, but
that scheduling does not widen D1 scope.

Additional RIDs and support-floor jobs are outside current D1. They require a
later explicit maintainer decision with updated package, runner, evidence,
documentation, and maintenance boundaries.

## Preparation Trial Boundary

The active preparation lane may inspect the exact committed CI workflow,
repository-root build and package commands, accepted Task 7 package graph,
artifact paths, checksum requirements, and local evidence already present. It
may update only this Task record and directly required generated navigation.

The lane must record:

- the current workflow matrix and its difference from accepted D1;
- the exact future native build, direct smoke, packed install and invocation,
  checksum, and bounded artifact steps;
- inputs or paths that remain unstable until command acceptance;
- protected package, product, support-floor, and publication meaning;
- the exact facts and commands that must be revalidated before CI mutation.

Protected surfaces include `.github/workflows/`, CLI production and tests,
package-manager source and manifests, root project and dependency files, public
documentation, release configuration, and generated runtime projections. This
preparation does not edit CI, execute remote jobs, publish, install globally,
or claim D1 acceptance.

## Execution Horizon

1. Phase 1, preparation.
   - M1 inventories the current CI, package, artifact, checksum, and accepted
     authority boundary on the exact base.
   - M2 freezes a future execution capsule, stale-data risks, revalidation
     triggers, and the experimental workflow evidence. Task 13 then returns to
     the queue until its implementation prerequisite is satisfied.
2. Phase 2, implementation after all retained commands, Task 10 findings, and
   every accepted Task 10 remediation are complete.
   - M3 applies the bounded native Linux CI and artifact collection change.
   - M4 passes focused local/static validation and the available CI-equivalent
     journey without remote publication.
3. Phase 3, acceptance.
   - M5 passes the exact native runner, packed install/invocation, checksum, and
     bounded artifact evidence on the final candidate.
   - M6 records the final evidence, residual limits, integration identity, and
     trial comparison.

The phase and milestone counts do not regress. Preparation evidence is not
implementation or acceptance evidence, and every prepared fact is revalidated
before M3.

## Reproducibility And Artifacts

Record SDK, `linux-x64`, commit, informational version, dependency graph, source
archive identity, binary/package hashes, package inventory, and smoke/packed
results. Upload only bounded release-candidate artifacts. Run publishes
sequentially when output is shared.

## Stop Conditions

Stop on emulated evidence presented as native, warning-bearing publish, skipped
required behavior, runner-specific uncommitted patch, root-path C# assumption,
automatic publication, or any broader RID/support-floor claim without explicit
acceptance.

## Trial Measurements

Record elapsed preparation time, child count, handoffs, missing-context or model
fallbacks, accepted and rejected Sol/xhigh review findings, grouped correction
count, facts invalidated before M3, later revalidation cost, gate failures,
integration conflict, and any defect found after acceptance. The initial
read-only candidate comparison required a fallback evidence child because its
requested Luna runtime was unavailable; that child result is useful input but
does not count as successful Luna-only execution evidence.
