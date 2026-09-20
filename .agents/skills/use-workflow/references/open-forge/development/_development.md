---
open-forge:
  description: Deliver high-consequence custom behavior through explicit architecture, frozen critical boundaries, progressive evidence, conditional improvement passes, and one correction budget
  tags: [Workflow, Development, Assurance, Orchestration, Implementation, Testing, Refactoring, Review]
---

# Assured Development

## Goal

Deliver one authorized high-consequence behavior change with explicit architecture, independently inspectable contract and evidence boundaries, protected mutation surfaces, integrated public proof, and bounded correction.

## Applicability

Use this workflow when new public or shared contracts, filesystem or security safety, concurrency, persistence, migration, destructive behavior, compatibility, or a new architecture archetype makes ordinary adaptive development insufficient. Do not use it for routine dependency, configuration, formatting, documentation, generated maintenance, or derivative work.

## Ownership And Boundaries

- One primary owner retains intent, architecture, transitions, integration, evidence, correction routing, and acceptance.
- One implementation owner should continue across contract, evidence, production, and local refactoring when surface permissions can change safely. Use separate phase contexts only when independent evidence, frozen write boundaries, or fresh scrutiny materially protects the result.
- When an execution capsule selects the streamlined assured lane, keep Preflight,
  Gray, and Red as explicit boundaries, then give one Brilliant Implementer
  continuous ownership of Green, local refactoring, focused verification, and
  the grouped improvement pass. The Task Mastermind owns one fresh whole-task
  review that explicitly includes the production-structure and test/evidence
  questions otherwise assessed in Blue and Purple. Do not invoke separate Blue
  or Purple owners unless Preflight names a material risk that requires an
  independently protected boundary.
- Record the baseline and protected surfaces at each selected boundary. A local commit may provide a useful snapshot when authorized, but a commit per phase is not mandatory ceremony.
- Expected paths are forecasts. Protected paths are hard boundaries. Directly required neighboring paths inside accepted meaning may be added and reported.
- A finding returns to the earliest invalidated architecture, contract, evidence, or implementation boundary. Improvement phases must not become alternate behavior-discovery phases.

## Steps

1. Run [Phase 0 - Preflight](phase-0-preflight.md) as read-only analysis and adopt its architecture readiness, behavior matrix, evidence ladder, path boundaries, profile justification, and review budget.
2. Establish the task baseline and compact execution capsule through [Task Lifecycle](task-lifecycle.md). Stop while a material product, architecture, safety, authority, or compatibility decision remains unresolved.
3. Run [Phase 1 - Gray Contract](phase-1-contract.md) only when the task introduces or changes a callable contract or shared foundation that benefits from an independently frozen surface. Otherwise record the accepted existing contract and continue.
4. Run [Phase 2 - Red](phase-2-red.md). Freeze the accepted behavior matrix and complete affected evidence before production mutation. Missing behavior has an observed intended failure; preserved behavior may already pass. Safety, destructive, compatibility, regression, boundary, failure, and externally visible behavior require explicit Red evidence.
5. Run [Phase 3 - Green](phase-3-green.md) and make the frozen evidence pass with the smallest correct production implementation. Keep tests protected. Allow necessary local refactoring while evidence remains green.
6. Run [Phase 4 - Blue](phase-4-blue.md) only when a named material production-structure trigger requires a separate protected pass. Run [Phase 5 - Purple](phase-5-purple.md) only when a named material test-structure or evidence trigger requires one. In the streamlined assured lane, skip these separate phases and carry both assessments into Step 8. A pass or assessment may conclude that no change is justified.
7. Run one real public or externally visible scenario when applicable and the selected integrated or full gate. Inspect the actual artifacts and classify anomalies instead of explaining them away.
8. Use [Phase 6 - Whole-Task Review](phase-6-review.md) for one fresh read-only review. In the streamlined assured lane, the Task Mastermind owns this review and explicitly inspects behavior, production architecture and structure, and test/evidence quality. Add a second independent lens only for a named distinct risk. The primary owner always performs final integrated review.
9. Permit one grouped correction cycle. Give findings stable IDs, start at the earliest invalidated boundary, return accepted implementation and improvement findings to the continuous Brilliant Implementer in the streamlined lane, repeat only affected downstream work, and recheck the changed findings. There is no automatic third cycle.
10. Apply [Task Acceptance](task-acceptance.md) only when required evidence passes and no blocking finding remains.

## Completion

- The assured profile was justified by concrete consequence or novelty.
- Architecture, authority, invariants, behavior, and protected surfaces were explicit before mutation.
- Selected contract and Red boundaries were frozen truthfully without requiring artificial phase ownership or commits.
- Green satisfied the accepted evidence without weakening expectations.
- Blue and Purple were either assessed inside the streamlined whole-task review
  or ran separately only for named material triggers.
- Public and integrated evidence passed, review stayed within budget, and correction remained bounded.
- Acceptance preserved local-only and external-effect boundaries and recorded residual risk.

## Entries

- [Produce a read-only assurance blueprint with architecture readiness, behavior classes, evidence, path boundaries, and review budget before mutation](phase-0-preflight.md) - #Workflow #Development #Phase #Preflight #Planning #Evidence #Review #Toolchain
- [Define and freeze a callable contract only when an independent Gray boundary materially protects later work](phase-1-contract.md) - #Workflow #Development #Phase #Contract #Interface
- [Freeze accepted behavior classes and representative failing evidence before production mutation](phase-2-red.md) - #Workflow #Development #Phase #Red #Testing #Evidence
- [Implement the smallest correct behavior against frozen evidence while protecting expectations](phase-3-green.md) - #Workflow #Development #Phase #Green #Implementation #Testing
- [Apply one conditional material production-structure improvement without changing behavior or evidence](phase-4-blue.md) - #Workflow #Development #Phase #Blue #Refactoring #Readability
- [Apply one conditional material test-structure or evidence improvement without changing accepted meaning](phase-5-purple.md) - #Workflow #Development #Phase #Purple #Testing #Refactoring #Evidence
- [Provide one bounded independent review and route material findings by stable ID to the earliest invalidated boundary](phase-6-review.md) - #Workflow #Development #Phase #Review #Quality #Evidence
- [Finalize an authorized development task after evidence and final review without implying remote integration or release authority](task-acceptance.md) - #LoadNow #Workflow #Development #Orchestration #Acceptance #Continuation
- [Prepare and run one task from an exact local baseline with a compact execution capsule, protected surfaces, and coherent checkpoints](task-lifecycle.md) - #LoadNow #Workflow #Development #Orchestration #Planning #Git #Branch #Commit #Progress
