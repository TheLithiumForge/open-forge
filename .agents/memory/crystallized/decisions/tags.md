---
open-forge:
  description: Accepted tag semantics - LoadNow follows visible parent chains, KeepInMind restores the complete continuity catalogue, other tags classify
  tags: [Memory, Decision, CurrentTruth, Tags, Routing]
---

# Tags

Extracted 2026-07-06; load-policy semantics reworked 2026-07-09 from the dogfood evidence.

- Defined loader tags with behavior or truth-status semantics are #LoadNow, #KeepInMind, #Contextual, and #CurrentTruth.
- #LoadNow: read the entry when it appears in loaded `Entries`, in listed order. It creates visibility, not authority, scope, or precedence.
- #KeepInMind: read or recheck the complete routed #KeepInMind catalogue at task start or resume, after context restoration or compaction, at meaningful phase transitions or handoffs, and before closeout. Keep every result active as binding follow-up context within the authority of its owning content. It replaces closeout-only loading and is deliberately not limited to the current parent chain.
- #OpenForge, #LoadWithParentEntrypoint, and #LoadForPostWorkReview are retired: #OpenForge fused core identity with load policy (identity is carried by #Core, #Memory, and framework paths); #LoadWithParentEntrypoint duplicated the same visibility rule; #LoadForPostWorkReview asked for a load at the moment agents demonstrably forget.
- No load-policy tag creates authority, scope, precedence, or mechanical enforcement; compliance is agent self-enforcement, so tag definitions use agent-imperative wording ("read X") rather than tool-implying wording ("X is loaded").
- #Core, #Memory, and #Extension are layer/routing tags.
- Route type tags such as #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace are routing/search signals unless a loaded entrypoint defines more.
- Tags stay bare in markdown so tools can parse and graph them.
- Use normal words when defining the local concept itself; use tags when pointing to routed ownership, classification, promotion, load policy, truth status, search, or references.
- Extensions use load-policy tags only when they deliberately add baseline-loaded or continuity-critical material.
