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
