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
`Entries`. This workspace now dogfoods that accepted shape locally:

```text
.agents/workflows/development/
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

The generic shipped Development Workflow remains one direct complete recipe
for people and agents that do not want this phase-separated method.

## Promotion Opportunity

The unresolved question is whether evidence from the local trial justifies a
shipped specialization example or optional Extension. Ordinary routing should
continue to provide the experience:

```text
open-forge find workflows/development
  -> shows the generic Workflow and its visible variants

open-forge context workflows/development
  -> returns the generic recipe and the Entries needed to discover variants

open-forge context workflows/development/phase-2-red
  -> returns the independently delegable Red phase with its routed context
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

The Development Toolkit still deliberately ships six direct Workflow files
with no per-Workflow folders or generated `Entries`. Promotion requires real
evidence and a deliberate source change. It does not require new Framework or
CLI semantics.

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
