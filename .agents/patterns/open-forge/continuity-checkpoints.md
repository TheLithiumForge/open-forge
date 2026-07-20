---
open-forge:
  description: Preserve goals, decisions, unresolved ideas, and next actions across long sessions, phase changes, handoffs, and context restoration
  tags: [Pattern, Memory, KeepInMind, Continuity, ContextRestoration, LongRunning]
---

# Continuity Checkpoints

## Shape

When work is likely to cross a context-restoration or handoff boundary, maintain one bounded active session or handoff record with:

- current Goal and development phase
- accepted decisions and their owner routes
- unresolved ideas, risks, and questions without premature promotion
- completed evidence and known failures
- exact next action and relevant paths

Refresh it at meaningful continuity boundaries:

1. task start or resume
2. a major decision or transition that changes standing follow-ups
3. before a handoff or anticipated context boundary when possible
4. immediately after actual context restoration is detected
5. closeout, transfer, or explicit pause

At every refresh, recover and recheck the complete effective #KeepInMind set, then treat every result as binding follow-up context within its owner's authority. `open-forge load --bodies` may batch the same traversal when available, placing each user-owned `.overwrite.md` after its base; ordinary generated-route traversal remains complete.

## Review Checks

- The checkpoint is small enough to reread, but contains enough to resume without private context.
- Candidate ideas remain visibly candidate; accepted choices point to current truth.
- A context collapse loses narration, not decisions, unresolved possibilities, or the next executable step.
