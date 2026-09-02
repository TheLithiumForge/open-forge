---
open-forge:
  description: Deliver development work through profile-based rigor, one persistent owner per coherent slice, bounded parallelism, and conditional review
  tags: [Workflow, Development, Adaptive, Implementation, Testing, Review, Delegation, Efficiency]
---

# Adaptive Development

## Goal

Deliver authorized development work safely and efficiently while preserving one primary owner for intent, architecture, integration, and acceptance, and one persistent implementation owner for each coherent slice.

## Steps

1. **Orient and classify.** Establish the outcome, authority, current architecture, affected consumers, external and destructive boundaries, and success evidence. Select Direct, Standard, Assured, Derivative, or Batch execution based on consequence and novelty, not file count. Set explicit non-negative review, council, and correction budgets.
2. **Create the execution capsule.** Record the profile, maximum budgets, and consumed-budget IDs in one Markdown Execution Capsule, either in the durable task record or the current working context. Record accepted decisions, invariants, placement map, behavior or acceptance matrix, expected paths, protected paths, direct integration neighborhood, dependencies, evidence ladder, and stop conditions. Keep it compact and link full sources.
3. **Resolve architecture before delegation.** Keep cross-cutting decisions in the primary context. Use a separate architecture pass only when it can be bounded and returned compactly. Do not spend an implementation run discovering unresolved system structure.
4. **Execute with persistent ownership.** Work directly when context dominates. Otherwise give one implementation owner a closed slice and keep tests, production, configuration, local refactoring, and repair together by default. Use separate contract, Red, Green, production-structure, or test-structure boundaries only when the separation materially protects correctness.
5. **Parallelize only independent lanes.** Give each lane non-overlapping mutation ownership, shared read-only inputs, an integration point, and its own evidence. Do not parallelize several owners over the same responsibility.
6. **Verify progressively.** Run focused evidence throughout, direct integration evidence after the slice, and public or full gates at coherent task, archetype, or batch boundaries. Preserve canonical execution receipts and classify failures before changing artifacts.
7. **Review proportionately.** The primary owner inspects the actual result. Consume one recorded review-budget unit for each independent reviewer. Use at most one independent reviewer for ordinary meaningful work and one by default for assured work. Add a second only after exact authorization expands the recorded budget for a named distinct risk. Do not run separate correctness, improvement, and writing reviews automatically. When the repository-local coordinated-review trial is explicitly selected, allocate one unit to each triggered topic, allocate none to its read-only coordinator, and keep at most one wave over one immutable task snapshot. Topic passes remain advisory and do not replace a separately budgeted fresh holistic review required by the profile.
8. **Correct once and narrowly.** The original writer revalidates findings against current relevant content, records dispositions, groups accepted corrections, and owns the repair. Recheck changed findings and affected context rather than rerunning the complete review. Reopen architecture, contract, or evidence only when that boundary is invalidated.
9. **Accept and close.** Account for every change, run the selected gate, update only affected durable sources, preserve unrelated state, and report the result, evidence, residual risk, and next authorized action.

### Profile Defaults

| Profile             | Architecture                                         | Implementation                                        | External review                      | Full gate                    |
| ------------------- | ---------------------------------------------------- | ----------------------------------------------------- | ------------------------------------ | ---------------------------- |
| Direct              | Existing context                                     | Primary owner                                         | None                                 | Only if required by risk     |
| Standard            | Compact plan                                         | One persistent owner                                  | Zero or one                          | Task boundary                |
| Assured             | Explicit architecture and frozen critical boundaries | One persistent owner or deliberately separated phases | One, second only for a distinct risk | Required acceptance boundary |
| Derivative or Batch | Delta from a golden archetype                        | Parallel non-overlapping owners                       | Batched or trigger-based             | Batch boundary               |

## Completion

- The selected profile matched the actual risk and changed when evidence required it.
- Architecture and semantic decisions remained with one accepted owner.
- Each coherent slice had one implementation owner unless deliberate write isolation justified separation.
- Delegated packets were closed, compact, and explicit about expected and protected paths.
- Review, councils, full gates, and durable prose work occurred only when their expected value justified their cost.
- Any coordinated topic wave was opt-in, budgeted by topic, bound to immutable Git objects, and kept separate from required holistic review.
- The requested behavior, integration, evidence, durable sources, and safety boundaries agree.
