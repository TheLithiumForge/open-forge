---
open-forge:
  description: Historical observation that early load-policy compliance was strongest when obligations loaded unconditionally and early
  tags: [Memory, Archived, Observation, Contextual, Historical, Tags, Loading]
---

# Observation: Tag Load Compliance

Status: archived 2026-07-18.  
Original route: `.agents/memory/emerging/observations/2026-07-09_tag-load-compliance.md`.  
Archived because: its recurring signal was accepted into current loading, tag, and continuity contracts.  
Current owner or replacement: `.agents/memory/crystallized/decisions/loading-reliability.md`, `.agents/memory/crystallized/decisions/tags.md`, the loader, and the continuity-checkpoint pattern.

Date: 2026-07-09. Source: dogfood v2-v4 reports plus maintainer confirmation on 2026-07-08. Scope: agent behavior in this framework's dogfood sandboxes; small session counts, so treat as strong signal rather than proof.

- Agents loaded root routes reliably when load-policy tags were present on loader entries.
- Post-work loading (#LoadForPostWorkReview) was often forgotten at the end of sessions, in one case even after confirmed direct exposure to the exact instruction.
- The remaining load-policy behavior was rarely exercised; it matters more as workspaces grow.
- No tag was ever mechanically enforced by tooling; every observed success was agent self-enforcement made cheap enough to happen.
- Candidate consequence: collapse load timing into "load now" semantics and turn end-of-work behavior into a standing obligation that is loaded early; see the unstaged tag rework proposal and `.agents/memory/emerging/ideas/observations-rework.md`.
