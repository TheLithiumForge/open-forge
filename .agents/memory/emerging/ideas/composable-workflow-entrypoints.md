---
open-forge:
  description: Evaluate whether the locally dogfooded recipe-bearing Development subtree should become a shipped specialization example or Extension
  tags: [Memory, Idea, Contextual, Candidate, Workflow, Routing, Orchestration, Agent]
---

# Composable Workflow Entrypoint Promotion

## Status

The recipe-bearing Development subtree is applied and proven as this
workspace's local Workflow. Promotion into the shipped Development Toolkit
remains an open product and distribution question, so this note stays an
Emerging Idea rather than moving to historical Memory.

## Current Accepted Boundary

The current Workflow contract already permits a complete recipe-bearing
`entrypoint` to expose descendant Workflows through ordinary generated
`Entries`. The original local trial used that shape under `.agents/workflows/`.
Its current successor is in the selector Skill's references:

```text
.agents/skills/use-workflow/references/open-forge/development/
├── _development.md        orchestration recipe and generated Entries
├── phase-0-preflight.md   read-only Task blueprint and decision frontier
├── phase-1-contract.md    Gray callable contract and skeleton authority
├── phase-2-red.md         executable expectation authority
├── phase-3-green.md       minimal implementation authority
├── phase-4-blue.md        protected refactoring authority
├── phase-5-purple.md      protected test-refactoring authority
└── phase-6-review.md      independent Whole-Task Review
```

The parent [Development Workflow](../../../skills/use-workflow/references/open-forge/development/_development.md)
is authoritative for orchestration. Its seven descendants define independently
delegable Preflight, Gray, Red, Green, Blue, Purple, and Whole-Task Review
phases. This
is current local behavior, not a candidate primitive, hidden dependency graph,
or new loading rule.

The shipped [Development catalogue](../../../skills/use-workflow/references/development/_development.md)
offers generic methods through the `use-workflow` Skill. The repository-local
phase method remains a separate specialization. Neither creates a Workflow
root route or a new loading rule.

## Promotion Opportunity

The unresolved question is whether evidence from the local trial justifies a
shipped specialization example or optional Extension. Ordinary routing should
continue to provide the experience:

```text
1. Read the use-workflow Skill and select the Development catalogue.
2. Compare the generic method with an explicitly selected specialization.
3. Read the selected method and its required phase sources.
```

Exact `context` closure remains governed by the accepted CLI and routing
contracts.

## Specialization Examples

A descendant Workflow may make execution policy explicit when that policy
materially changes the recipe. Examples include:

- Only the orchestrator stages and commits; phase agents return unstaged and
  uncommitted changes, and nothing is pushed.
- All implementation work occurs on an isolated child branch.
- Preflight, Gray, Red, Green, Blue, Purple, and Whole-Task Review agents run
  sequentially.
- A named agent runtime or provider performs a particular phase.
- A phase has a narrower mutation allowlist, evidence requirement, or handoff
  contract than the generic recipe.

These statements remain Workflow steps or recipe-specific context. They do not
become binding Directives merely because they appear beneath Workflows. A
policy that must bind outside the selected recipe belongs in a Directive and is
linked explicitly.

## Experience Boundary

- The parent recipe must remain complete and understandable without selecting a
  child.
- Child `description` values must explain the material difference before load.
- Selecting a specialization must be explicit. Discovery does not activate it.
- Provider-neutral behavior belongs in the generic recipe. Provider-specific
  details stay in an opt-in descendant or Extension.
- Examples may showcase flexible orchestration without implying that every
  workspace needs agents, branches, commits, phases, or a particular runtime.

## Distribution Boundary

Development Toolkit is a dependency bundle for Development, Planning, Project
Documents, and Flows and Scenarios. Method files are supplied by their packages
through the selector Skill. Promotion of the local nested specialization still
requires evidence and a deliberate package decision. It does not require new
Framework or CLI semantics.

## Evidence Before Promotion

The accepted CLI Foundation satisfies the first item. The remaining evidence
still governs any shipped promotion.

1. Dogfood the local phase-based Development subtree through at least one
   complete CLI slice.
2. Compare the generic and specialized selection experience with unfamiliar
   users or agents.
3. Prove that ordinary `find` and `context` routing exposes the parent and
   descendants without loading an unwanted specialization.
4. Verify that nested variants do not duplicate Directives, Skills, task state,
   or provider configuration.
5. Promote only the smallest recurring structure supported by that evidence.
