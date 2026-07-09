---
name: workflow-primitives
description: Shared workflow primitives for context loading, memory routing, completion, and handoff. Use when a workflow needs reusable agent behavior that is not specific to one workflow domain.
---

# Workflow Primitives

Use this skill to keep workflow packs from duplicating basic agent behavior.

## Process

- Load only the reference needed by the active workflow step.
- Keep shared behavior generic enough to be reused by multiple workflows.
- If a shared primitive becomes workflow-specific, move or copy it into that workflow's narrower skill package.

## References

- `references/loaded-context-check.md` - Read before a workflow makes a decision, edits files, creates accepted memory, or hands off work.
- `references/memory-routing.md` - Read when workflow output may be worth saving beyond the current response and needs a #Memory or #Core owner.
- `references/completion-handoff.md` - Read before ending meaningful workflow work, especially when work may continue in another context.
