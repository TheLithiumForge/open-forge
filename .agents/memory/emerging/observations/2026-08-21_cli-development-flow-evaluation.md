---
open-forge:
  description: Evidence-based comparison of the current replacement-CLI development flow with prior CLI flows
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, Evaluation, Workflow, Architecture, Task, Delegation]
---

# CLI Development Flow Evaluation

This Emerging Observation compares the current replacement-CLI flow with the
earlier implementation and reset flows. It is contextual evidence, not an
accepted workflow rule.

## Evidence Baselines

The current baseline is the architecture, Task, foundation, and route-list
sequence beginning at `50f27ad`: architecture and Plan (`50f27ad`), Task hierarchy
(`adb885b`), scoped workspace (`ad2d49e`), command-free foundation (`2662f50`),
foundation evidence (`7a601cc`), foundation acceptance (`e7716ce`), route-list
contracts (`f3529ee`), selection (`9a62995`), and filesystem inventory
(`fa03662`). The current [CLI Architecture](../../crystallized/documents/cli/architecture.md),
[Plan](../../working/cli-development/plan.md), [Task index](../../working/cli-development/tasks/_tasks.md),
[Route Discovery](../../working/cli-development/tasks/route-discovery/_route-discovery.md),
[route-list acceptance Task](../../working/cli-development/tasks/route-discovery/done/route-list-acceptance.md),
and [Checkpoint](../../working/checkpoints/cli-development.md) carry the active
structure and evidence. The current topology and presentation acceptance work is
also recorded there as verified. The maintainer authorized route-list closeout
after the local evidence; the coherent closeout commit and any squash integration
are not claimed until Git executes them.

The prior baseline is the implementation and reset range represented by WIP
commit `4b873de` and greenfield reset commit `40ba03e`, together with the archived
[CLI implementation reset](../../archived/cli-release/implementation-reset-2026-08-21.md),
[CLI development workflow](../../archived/cli-v2/implementation-history/cli-development-workflow.md),
and [historical development edge cases](../../archived/cli-v2/implementation-history/development-edge-cases.md).
The [review-rationale observation](2026-08-18_cli-review-rationale-and-dogfooding-anomalies.md)
and [architectural-context observation](2026-08-21_architectural-context-delegation-gap.md)
preserve the comparison evidence and its limits.

## Supported Comparison

The current flow is materially better in these dimensions:

- **Architecture closure before delegation:** The current sequence closes the
  Architecture, Plan, Task boundaries, and command-free foundation before route
  behavior consumes them. The earlier implementation delegated a local behavior
  packet before the structural foundation was closed, expanded an obsolete layout,
  and missed the intermediate physical escape later described in the architectural
  context observation.
- **Task slicing:** Current Route Discovery separates contracts, selection,
  filesystem, topology, presentation, and acceptance in
  [Route Discovery](../../working/cli-development/tasks/route-discovery/_route-discovery.md).
  The earlier WIP increment changed 24 files with 1,224 insertions and 254
  deletions, including a 446-line physical-containment class. The current slices
  make those boundaries inspectable before integration.
- **Test-tier sequencing:** The current Architecture and route-list acceptance
  distinguish Unit, Integration, EndToEnd, managed process, and published Native
  AOT evidence. The [route-list acceptance record](../../working/cli-development/tasks/route-discovery/done/route-list-acceptance.md)
  names the 259 Unit, 84 Integration, and 7 EndToEnd cases and the published
  `win-x64` executions. The reset record explicitly rejects test counts as a
  substitute for architecture, contract, or Native AOT proof.
- **Evidence traceability:** Current Tasks map contracts to evidence, state exact
  gates, and preserve results in the Plan and Checkpoint. The prior review record
  documents how finding-only summaries and incomplete review returns lost the
  reasoning needed for later comparison.
- **Edge-case handling:** Current route-list evidence names aliases, cycles,
  external-then-reentry, cancellation retention, raw-token interaction, output
  ordering, and no-write checks. The new [replacement-CLI edge-case ledger](../../working/cli-development/edge-cases.md)
  keeps unresolved breadth visible with stable IDs and closure conditions. The
  earlier flow discovered the physical escape only after passing local gates.
- **Honest acceptance status:** The current [Plan](../../working/cli-development/plan.md),
  [Checkpoint](../../working/checkpoints/cli-development.md), and [route-list
  acceptance Task](../../working/cli-development/tasks/route-discovery/done/route-list-acceptance.md)
  distinguish green local evidence from Git integration. The maintainer
  authorized route-list closeout after the local evidence, and the two
  legacy-router errors are recorded in [CLI-EDGE-001 — Legacy routing-tool
  duplicate-entrypoint reports](../../working/cli-development/edge-cases.md#cli-edge-001--legacy-routing-tool-duplicate-entrypoint-reports)
  rather than treated as route-list blockers. No squash integration is claimed
  before Git executes it. This does not claim route-list acceptance from test
  counts alone.

The current flow is worse in two operational dimensions:

- **Upfront planning and state-maintenance cost:** Architecture, Plan, Checkpoint,
  a 57-file Task set, completed-Task routing, and generated Entries require more
  preparation and clerical maintenance than the earlier implementation path.
  That cost is visible in the current [Task index](../../working/cli-development/tasks/_tasks.md)
  and [Checkpoint](../../working/checkpoints/cli-development.md); it has not been
  converted into a speed or token measure.
- **Branch and closeout ergonomics:** The current route-list presentation work has
  local evidence, and the maintainer authorized closeout after that evidence. The
  coherent closeout commit and any squash integration still await Git execution.
  The legacy-router errors are recorded in CLI-EDGE-001 rather than treated as
  route-list blockers. Those boundaries make branch and closeout work less
  convenient than a single implementation path, even though they make the
  acceptance boundary more visible. The archived [CLI development workflow](../../archived/cli-v2/implementation-history/cli-development-workflow.md)
  also shows that branch and no-push rules existed before, so this is a qualitative
  ergonomics comparison, not a claim that the earlier flow had no closeout cost.

## Interpretation And Change Conditions

The strongest counterargument is that the current gains come from the greenfield
reset, heavier Mastermind ownership, and a closed foundation rather than from the
flow in isolation. The strongest documented alternative is to give each
implementation role the complete review and ask it to discover the architecture
independently. That may expose useful alternatives, but the prior
[architectural-context observation](2026-08-21_architectural-context-delegation-gap.md)
records the costs of repeated discovery, distributed architecture authority, and
reconciliation at integration. The present tradeoff favors deliberate closure and
traceability over local speed or convenience.

This evaluation should be narrowed or rejected if route inspect repeats the prior
integration defects despite the current boundaries, if the planning records add
maintenance without reducing rework, or if later slices provide actual evidence
about delegation efficiency or productivity.

## Delegation Qualification

The current route-list work did use one bounded implementer for the presentation
slice. Most route-list children are Mastermind-owned in the current Task records,
including contracts, selection, filesystem, topology, and acceptance. The result
therefore demonstrates stronger architecture closure, slicing, and Mastermind
integration, but it does not prove delegation efficiency or empirically superior
delegation.

### Find Gray Evidence

Find Child 2 adds evidence against treating a closed architecture packet as proof
that one large bounded assignment is operationally efficient. Three Gray contract
implementation attempts reached their execution limit before completing the full
neutral Markdown, Find model, wiring, and validation packet. The first retained
partial artifacts, the second completed most source but not the final gate, and a
fresh attempt completed analysis without edits. The Mastermind inspected and
completed the integrated result. Bounded correctness review still found material
model-invariant gaps and required focused continuations before passing.

The result does not show that delegation itself is unsuitable. The packet was
closed and scope remained intact, but its volume combined several independently
inspectable contract families. The strongest future option is to keep one Gray
phase and commit while assigning smaller neutral-document, result-model, and
wiring packets. That reduces per-assignment breadth but adds handoff and
integration cost. Another similarly closed slice must reproduce the limit before
this observation supports a reusable workflow or agent-package change.

## Guidance To Retain For Route Inspect

The [route-inspect Task](../../working/cli-development/tasks/route-discovery/route-inspect.md)
should retain these boundaries:

- Split Tasks before code begins.
- Close promotion decisions before shared facts move.
- Keep private behavior local until identical meaning is proved.
- Use one bounded implementation packet for a closed slice.
- Keep Mastermind integration responsible for architecture and promotion.
- Require real-OS, public-process, and Native AOT evidence.
- Route each finding to the earliest invalid boundary.

## Changes For The Next Slice

For route inspect, change the flow as follows:

1. Create the edge-case ledger early, before implementation work expands.
2. Run compact acceptance checks for wire-field order and names, non-complete
   output ordering, cancellation after a result exists, and raw-token versus typed
   parse interaction before final Native AOT evidence.
3. Separate Task archival churn from behavior commits where practical.
4. Establish merge/worktree boundaries and commit authorization earlier.
5. Avoid broad, repetitive review loops when one bounded review can answer the
   named question.

## Do Not Claim

This comparison does not support claims that the current flow is:

- faster;
- cheaper;
- lower-token;
- empirically superior at delegation;
- six-RID ready; or
- accepted because test counts passed.

The supported conclusion is limited to the structural and evidence dimensions
listed above, the identified planning and closeout costs, and the need to test the
next slice before promoting a reusable workflow rule.

## Astra Restart Comparison

The maintainer explicitly requested model-performance observations for the
2026-09-07 restart. This bounded comparison therefore records the actual model
and reasoning allocation: GPT-6 Astra/high for substantive task ownership,
implementation, and review; GPT-5.6 Luna/max for bounded exploration, literal
mechanical work, and exact verification. This is a current experiment, not a
change to the installed Framework or proof of general model superiority.

The [restart handoff](../../working/handoffs/2026-09-07_cli-astra-restart.md)
and [inventory](../../working/handoffs/2026-09-07_cli-astra-restart-inventory.md)
preserve the inherited baseline. Repair had an uncompiled operation draft;
Library recovery had 19 passing Unit cases and one failure, followed by an
unverified null suppression. These were known defects at transfer, so fixing
them does not count as an independent discovery by the new model.

Early evidence from the restart is bounded:

- Astra's Repair owner reproduced a warning-free Core Release build from the
  preserved draft. Full operation and interaction acceptance remain pending.
- Astra's Library owner replaced the suppression with explicit delete and
  non-delete branches. Nonincremental Core and Root Release builds passed with
  zero warnings or errors. Focused recovery evidence remains pending at this
  observation boundary.
- Astra's Library owner found that the Task's second Inspect public journey
  used an unknown supplied ID while the accepted Interface required an omitted
  ID. The Overseer checked the Interface and returned the Task to that exact
  public scenario, retaining unknown-ID evidence at a lower tier. This is an
  evidence-authority correction before Red, not a product change.
- Astra's Repair owners identified missing interaction and application-integrity
  evidence in the six accepted Integration cases. Supplemental evidence must
  demonstrate the failures before implementation, preserve the original frozen
  cases, and retain exactly three public Repair journeys. Its result is pending.

The comparison must distinguish inherited defects, independent findings,
accepted corrections, false positives, and final gate outcomes. Record useful
follow-up evidence here after task acceptance. No controlled speed, cost, token,
or same-task model comparison is available yet.

### Continuity Cost At Restart

The Plan, project ledger, checkpoint, and Overseer memory contain repeated
historical receipts alongside current state. Large combined reads repeatedly
truncated tool output during restart; one Task owner independently reported
the same occurrence. The recovery was to read the required complete sources in
bounded chunks and select current governing sections of the broader records.
This preserves authority while reducing repeated historical output. It does
not waive complete C# or handoff reading, or change Loader rules.

The existing workflow already calls for compact Working Memory. A later
coherent maintenance pass should keep current state in those sources and move
useful completed receipts to a linked historical record, preserving evidence
and immutable handoffs. This observation does not authorize deleting history.

### Correction Evidence During Resumption

The Library owner replaced the inherited recovery null suppression with
compiler-proven branches and obtained a fresh 20/20 recovery Unit result.
Subsequent Integration compilation exposed missed static fixture callers;
severity-info formatting then exposed nine instance members needing static
modifiers in that corrected test scope. These are unfinished inherited
migration consequences. The owner subsequently froze correction `02df72d3`
after four warning-free builds, severity-info formatting, and focused recovery
Unit 20/20 and Integration 10/10 with zero failures or skips. Do not count each
sequential gate as a separate review or as evidence of lower cost.

Repair’s supplemental interaction evidence produced nine intended Unit and
six intended Integration failures before the corresponding implementation.
The original Red files and three public journeys per command stayed fixed.
This establishes independent failure evidence for a newly identified coverage
gap; it does not yet establish the quality of the completed implementation.

### New Draft Defects And Owner Detection

Repair’s Astra-authored supplemental test needed a teardown-only correction
after its target-drift assertions passed. Four later analyzer-only corrections
required another supplemental freeze update. These are author defects caught
before acceptance, not clean first-pass evidence. Format supplemental tests
before freezing their Red hashes. A wrong Doctor selection also produced zero
tests; the minimum-count gate rejected it before an acceptance claim.

The Repair author separately found missing fresh diagnosis after pre-effect
refusal/failure. The added one-case oracle failed on the earlier implementation
with the intended observation-status mismatch; all unrelated inputs survived
the bounded compiler-input swap unchanged. Corrected acceptance remains pending.

Library finding `T23-GRAY-ARCH-001` identified six newly authored plan/application
dependencies on presentation types. The owner caught this before Gray acceptance
and required domain-owned facts plus one semantic result graph consumed by both
renderers. Record both the author defect and owner detection; neither establishes
model superiority or measured cost savings.

Root also repeated the already documented handoff Index metadata-incomplete
condition during unrelated navigation work. It made no changes; targeting only
the relevant Task parents then passed. Keep known failure conditions in compact
resumption context to avoid unnecessary probes.

### Storage And Repeated Build Contexts

The user interrupted work when the Windows host volume was nearly full. Local
inventory found 27.35 GiB in 111 regenerable build-output directories and about
1.1 GiB of retained recovery archives attributed to temporary test workspaces.
Only the build outputs were removed under explicit authorization. Exact dirty
work, task evidence, recovery data, and the global installation were preserved.
The user deferred test-environment cleanup as an idea and resumed command work.

This shows a storage cost from accumulated worktree build outputs; it is not
evidence that a particular model caused the disk pressure. Rebuild only active
required evidence after cleanup, and never reuse removed binaries or restore
assets as proof of current execution.
