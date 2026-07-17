---
open-forge:
  description: Turn an accepted direction into an executable, verifiable plan; use when scope, ordering, dependencies, and completion evidence must be made explicit before implementation
  tags: [Extension, Workflow, Planning]
---

# Planning

Planning turns an accepted direction into an executable sequence with one source of task truth.

## Goal

- outcome: an ordered plan with scope, dependencies, verification, decision points, and a clear completion boundary
- acceptance: another capable agent can execute the plan without guessing material intent or maintaining duplicate task state
- stop: the direction is not accepted, a material choice lacks authority, or required system context cannot be inspected

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `.agents/skills/planning/SKILL.md` - plan construction, option handling, and task-authority discipline

## Constraints

- Use exactly one authoritative task source, declared by the user or workspace, for task status, ownership, acceptance, and completion.
- If none is declared, default to the current user task; never nominate a workspace artifact merely because it exists.
- Never create or mirror a task in an external tracker without authority.
- Do not convert unresolved product or architecture choices into hidden implementation assumptions.
- Keep accepted rationale, decisions, and durable project truth with their established owners; the task source may link to them but must not replace them.

## Steps

1. Name the one user- or workspace-declared authoritative task source, or explicitly default to the current user task.
2. Confirm the accepted outcome, non-goals, constraints, acceptance evidence, and decision owner.
3. Inspect the affected system, current patterns, interfaces, and available verification.
4. Identify dependencies, risks, unknowns, and decisions that must precede implementation.
5. Divide the work into the smallest coherent, ordered steps that each produce observable progress.
6. For every step, name its expected result and proportionate verification.
7. Mark parallelizable work, sequencing constraints, rollback or containment needs, and user decision points.
8. Review the plan against the requested scope, update task state only in the authoritative task source, and route accepted rationale or durable project truth to its established owner with links between them when useful.

## Loop

Iterative until executable: repeat steps 3 through 8 when inspection exposes a missing dependency, risky assumption, or better slice. Stop when the plan is actionable or a material choice requires user input.

## Outputs

- declared authoritative task source
- accepted outcome, scope, and non-goals
- ordered implementation steps with expected results and verification
- dependencies, risks, decision points, and parallel work
- explicit completion boundary

## Completion

- [ ] exactly one authoritative task source is named or the current user task is the explicit default
- [ ] every step has an observable result and verification path
- [ ] material unknowns and decisions are explicit
- [ ] no unauthorized external task or mirror was created
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
