---
open-forge:
  description: Shared skills used by multiple workflow extensions
  tags: [OpenForge, Extension, Core, Skill, WorkflowPrimitive, Index]
---

# Workflow Primitives

Workflow primitives are shared skills that keep workflow packs from duplicating basic agent behavior.

## Axioms

- Use `Entries` when an active workflow needs reusable context loading, completion, memory routing, or handoff behavior.
- Load only the skill files relevant to the active workflow step.
- Keep shared skills generic enough to be reused by multiple workflows.
- If a shared skill becomes workflow-specific, move or copy it into that workflow's narrower skill route.
- Generated `entries` are navigation and reserved load policy only.

## Entries

<!-- open-forge:generated-index:start -->
- `completion-handoff.md` - Finish a workflow with status, verification, next action, and resume context - #OpenForge #Extension #Core #Skill #WorkflowPrimitive #Handoff #AgentCommunication
- `loaded-context-check.md` - Check that the workflow has loaded the context needed to act safely - #OpenForge #Extension #Core #Skill #WorkflowPrimitive #Context
- `memory-routing.md` - Route useful workflow output to the right memory or Core owner - #OpenForge #Extension #Core #Skill #WorkflowPrimitive #Memory #OrganicGrowth
<!-- open-forge:generated-index:end -->
