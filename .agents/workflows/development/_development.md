---
open-forge:
  description: Deliver custom behavior through a Mastermind-owned strict cycle with read-only Preflight, complete evidence, bounded cleanup, a public gate, and one correction allowance
  tags: [Workflow, Development, Orchestration, Implementation, Testing, Refactoring, Review]
---

# Development

## Goal

Deliver one authorized custom-behavior Task through a strict lifecycle that begins with read-only Preflight analysis owned and adopted by the Mastermind. One complete delivery cycle is Gray callable contract, complete affected Red evidence, the smallest correct Green implementation, one material Blue production-improvement pass, one material Purple test-improvement pass, a public scenario and full gate, independent review when useful, and Mastermind final review.

## Applicability

Use this lifecycle for custom product behavior when explicit contract, evidence, implementation, cleanup, and review boundaries materially protect correctness. Routine dependency, configuration, script, documentation, repository, test-infrastructure, formatting, linting, and generated-maintenance work may use proportionate direct or adaptive execution instead. Do not impose this cycle on routine work that does not need it.

## Ownership And Boundaries

- The Mastermind owns planning, phase transitions, integration, integrated inspection, correction-cost decisions, final review, and commits. The maintainer retains architecture and acceptance decisions unless that exact authority is explicitly delegated. The Mastermind may perform the work directly or use a helper for bounded evidence. Helpers are optional evidence sources, not mandatory ceremony or parallel owners.
- Read-only Preflight selects the active toolchain, focused commands, full-gate commands, affected paths, evidence depth, and material decision frontier. The generic lifecycle does not prescribe a language, runner, provider, model, command name, or filter syntax.
- The generic [Testing Directive](../../directives/open-forge/testing/evidence-integrity.md) binds test selection and isolation. Technology-specific rules apply only after Preflight selects the active implementation scope.
- The Gray, Red, and Green subcycle is the behavior-discovery boundary. Complete affected Red evidence and the smallest correct Green implementation must expose the affected behavior defects before Blue or Purple begins. A behavior defect first noticed in Blue, Purple, or final review invalidates the applicable contract, Red, or Green work; it is evidence that the core subcycle was incomplete.
- Blue makes one material production-structure improvement pass. Purple makes one material test-structure and evidence improvement pass. Neither phase is an alternate behavior-discovery phase. Missing, incorrect, or incomplete behavior returns to the earliest applicable contract, Red, or Green work.
- The Mastermind groups findings across phase outputs, integrated inspection, the public scenario, the full gate, and any independent review. It explicitly decides whether an exceptional correction cycle is worth its cost.

## Steps

1. Run [Phase 0 - Preflight](phase-0-preflight.md) as read-only analysis. The Mastermind adopts its bounded blueprint before mutation. Preflight may occur before Task creation, branch mutation, or plan approval. It does not write Task state, Git state, contracts, or executable evidence.
2. Apply [Task Lifecycle](task-lifecycle.md). Use the maintainer-accepted architecture, adopt routine plan details within existing authority, stop while a material decision remains unresolved, and establish the exact baseline and focused feature branch before implementation mutation.
3. Run [Phase 1 - Gray Contract](phase-1-contract.md) and freeze the accepted callable contract.
4. Run [Phase 2 - Red](phase-2-red.md) and complete the affected executable behavior evidence against that contract.
5. Run [Phase 3 - Green](phase-3-green.md) and implement the smallest correct behavior that satisfies the frozen Red evidence.
6. Run [Phase 4 - Blue](phase-4-blue.md) once for a material production improvement pass, then [Phase 5 - Purple](phase-5-purple.md) once for a material test-improvement pass. A pass may report no justified change, but neither may become cosmetic cleanup or new behavior discovery.
7. Run one real public-surface scenario and the full gate selected by Preflight. The Mastermind inspects the resulting paths, diff, and evidence as an integrated result.
8. Use [Phase 6 - Whole-Task Review](phase-6-review.md) for an independent read-only review when a fresh perspective is useful. The review is optional. The Mastermind always performs the final review and acceptance decision.
9. Allow at most two complete cycles: the initial cycle and one exceptional correction cycle. If a material or blocking finding remains, the Mastermind groups the findings and explicitly decides whether the second cycle is worth its cost. If it is, rerun from the earliest invalidated phase through all applicable downstream phases, then rerun the public scenario, full gate, any useful independent review, and Mastermind final review. An environment-only retry with no tracked change is not a cycle. There is no third cycle. Unresolved blocking findings return to the maintainer.
10. Apply [Task Acceptance](task-acceptance.md) only after Mastermind final review and the selected full gate pass.

## Completion

- Read-only Preflight produced an adopted blueprint with the active toolchain, focused evidence, public scenario, full gate, correction boundary, and material decisions explicit.
- The work used the maintainer-accepted architecture, the plan was adopted within applicable authority, and no unresolved material decision permitted mutation.
- Each completed cycle preserved the order Gray, Red, Green, Blue, Purple, public scenario, full gate, optional independent review when useful, and Mastermind final review. Blue and Purple each ran as one bounded material-improvement pass and did not discover alternate behavior.
- The contract, complete affected Red evidence, and smallest correct Green implementation agree, and the affected behavior defects were exposed in that subcycle rather than deferred to cleanup phases.
- No more than one correction cycle was used. Any correction began at the earliest invalidated phase, covered all applicable downstream work, and repeated review and the gate. Environment-only retries did not consume a cycle.
- The Mastermind inspected the integrated result, made the correction-cost decision, committed accepted work, and recorded acceptance, evidence, residual risk, and any unresolved finding.
- The branch and integration rules in [Task Lifecycle](task-lifecycle.md) and [Task Acceptance](task-acceptance.md) were followed.

## Entries

<!-- open-forge:generated-index:start -->
- [Produce a read-only development Task blueprint, active toolchain and commands, evidence plan, and material decision frontier before mutation](phase-0-preflight.md) - #Workflow #Development #Phase #Preflight #Planning #Evidence #Review #Toolchain
- [Define and freeze the Gray callable contract for one authorized custom-behavior Task](phase-1-contract.md) - #Workflow #Development #Phase #Contract #Interface
- [Create complete affected executable behavior evidence against the frozen Gray callable contract](phase-2-red.md) - #Workflow #Development #Phase #Red #Testing #Evidence
- [Implement the smallest correct custom behavior that satisfies the frozen Red evidence](phase-3-green.md) - #Workflow #Development #Phase #Green #Implementation #Testing
- [Make one material production-structure improvement after Green without changing accepted behavior or evidence](phase-4-blue.md) - #Workflow #Development #Phase #Blue #Refactoring #Readability
- [Make one material test-improvement pass after Green and Blue without changing behavior or expectation meaning](phase-5-purple.md) - #Workflow #Development #Phase #Purple #Testing #Refactoring #Evidence
- [Provide an optional independent read-only review of a delivery cycle and route material findings to the earliest invalidated phase](phase-6-review.md) - #Workflow #Development #Phase #Review #Quality #Evidence
- [Finalize an authorized development Task after the full gate and Mastermind final review, then integrate it under the repository branch and release rules](task-acceptance.md) - #LoadNow #Workflow #Development #Orchestration #Acceptance #Continuation
- [Adopt Preflight, plan and isolate one development Task from exact develop, preserve coherent commits, and keep authoritative progress current](task-lifecycle.md) - #LoadNow #Workflow #Development #Orchestration #Planning #Git #Branch #Commit #Progress
<!-- open-forge:generated-index:end -->
