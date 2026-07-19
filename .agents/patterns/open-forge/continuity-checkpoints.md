---
open-forge:
  description: Preserve goals, decisions, unresolved ideas, and next actions across long sessions, phase changes, handoffs, and context restoration
  tags: [Pattern, Memory, KeepInMind, Continuity, ContextRestoration, LongRunning]
---

# Continuity Checkpoints

## Shape

Maintain one bounded active session or handoff record with:

- current Goal and development phase
- accepted decisions and their owner routes
- unresolved ideas, risks, and questions without premature promotion
- completed evidence and known failures
- exact next action and relevant paths

Refresh it at meaningful continuity boundaries:

1. task start or resume
2. workflow phase transition or major decision
3. before a handoff or anticipated context boundary when possible
4. immediately after context restoration or compaction is detected
5. closeout, transfer, or explicit pause

At every refresh, recover and recheck the complete routed #KeepInMind set, then treat every result as binding follow-up context. Use the single CLI lookup when it is available; traverse generated routes as the plain-file fallback.

## Review Checks

- The checkpoint is small enough to reread, but contains enough to resume without private context.
- Candidate ideas remain visibly candidate; accepted choices point to current truth.
- A context collapse loses narration, not decisions, unresolved possibilities, or the next executable step.
