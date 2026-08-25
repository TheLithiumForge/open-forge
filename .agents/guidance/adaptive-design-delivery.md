---
open-forge:
  description: Deliver greenfield and brownfield work through explicit authority, profile-based rigor, compact context, persistent ownership, and proportionate review
  tags: [Core, Guidance, Design, Implementation, Adaptive, Greenfield, Brownfield, Context, Planning, Authority, Review, Efficiency]
---

# Adaptive Design And Delivery

## Scenario

Use this Guidance when an open-ended design or implementation request must become a safe, resumable change. It applies to greenfield and brownfield systems where current behavior, contracts, projections, or history may constrain the result.

## Preferred Approach

Keep one primary owner responsible for intent, architecture, planning, integration, and final judgment. Use the smallest complete execution profile that protects the work:

| Profile             | Use when                                                                                                              | Default shape                                                                                        |
| ------------------- | --------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| Direct              | Small, reversible, context-heavy work with local effects                                                              | Primary owner works directly and runs focused evidence.                                              |
| Standard            | New behavior inside established architecture                                                                          | One coherent implementation owner, focused evidence, conditional single review.                      |
| Assured             | New public or shared contracts, safety, persistence, concurrency, migration, destructive behavior, or a new archetype | Freeze material boundaries, use stronger evidence, and add one independent review.                   |
| Derivative or batch | Work follows a proven archetype                                                                                       | Plan the delta, parallelize non-overlapping slices, and review or gate at a coherent batch boundary. |

Change profile when evidence changes. Do not preserve foundation-level ceremony after the pattern is proven.

## Build A Compact Authority Map

For each source that could change the work, record the question it answers, its status, its authority for that question, and whether the task may update it. Distinguish:

- accepted current truth;
- accepted rationale;
- candidate analysis or ideas;
- active task, plan, or checkpoint state;
- generated or projected surfaces; and
- historical evidence.

A familiar path is not automatically authoritative. Resolve conflicts that could change behavior, scope, safety, acceptance, or reversibility before mutation.

## Classify The Starting Point And Risk

- **Greenfield:** define the first useful vertical slice and external boundaries without inventing a current implementation.
- **Brownfield:** inspect the source of current behavior, direct consumers, contracts, tests, projections, and migration boundaries. Existing code is evidence, not automatic authority.
- **Unclear or rescue:** treat as brownfield until authority and integration boundaries are known.

Increase rigor for public or shared contracts, persisted data, security, concurrency, filesystem safety, external integration, generated or packaged surfaces, release boundaries, and hard-to-undo operations. File count alone does not determine risk.

## Create One Execution Capsule

For nontrivial work, preserve only the context needed to execute and resume:

- outcome, profile, authority, accepted decisions, and non-goals;
- architecture invariants and placement map;
- behavior or acceptance matrix;
- expected paths, protected paths, and direct integration neighborhood;
- dependencies, evidence ladder, review budget, and stop conditions;
- current owner, completed boundary, decisive evidence, and next action.

Link full sources instead of copying them. Refresh the capsule only when meaning or state changes.

## Preserve Ownership And Delegate Proportionately

- Keep architecture and integration in one accepted top-down context.
- Use a delegated architect only when a separate deep context can return a compact packet and reduce repeated loading.
- Keep one implementation owner through tests, production, local refactoring, and correction whenever frozen-surface independence does not require separation.
- Use separate phase owners only when write isolation, a frozen contract, independent evidence, or fresh review materially protects correctness.
- Parallelize only non-overlapping mutation lanes or independent read-only work with an explicit integration point.
- Treat expected paths as forecasts. Protected paths are hard. Report directly required neighboring paths rather than stopping for a non-semantic allowlist gap.

## Verify And Review By Risk

Use an evidence ladder:

1. direct focused evidence during implementation;
2. boundary or integration evidence for changed interactions;
3. public, packaged, or end-to-end evidence at a coherent acceptance boundary;
4. full repository gates at task, archetype, or batch boundaries rather than after every small edit.

The primary owner always inspects actual artifacts and evidence. Use no external review for routine direct work, normally zero or one for standard work, and normally one for assured work. A second reviewer needs a named distinct risk. Give findings stable IDs, group corrections, and recheck only changed findings.

## Reconcile Durable Sources And Close

Update each affected current source, Decision, map, public document, generated projection, or active task record according to its role. Do not preserve a large synchronized source set when links and generated projections are enough.

At closeout, report the delivered result, changed paths, evidence, residual risk, unresolved decisions, and next authorized action. Create a sealed handoff only for a real transfer. Keep local-only requests local and require exact authorization for external or destructive effects.

## Tradeoffs

Compact context improves speed and resumption but can omit a hidden dependency. Use explicit authority, consumers, and stop conditions to protect it. Persistent ownership reduces repeated reconstruction but can carry implementation bias, so use fresh review only where independent scrutiny is worth its cost.
