---
open-forge:
  description: Accepted tag semantics for loading, layers, truth status, Evergreen synchronization, and ordinary classification
  tags: [Memory, Decision, CurrentTruth, Tags, Routing]
---

# Tags

Extracted 2026-07-06; load-policy semantics reworked 2026-07-09 from the dogfood evidence and aligned with typed authority on 2026-07-26.

- The [loader](../../../loader.md) is authoritative for the exact behavior of #LoadNow, #KeepInMind, #Contextual, #CurrentTruth, and #Evergreen.
- #LoadNow: read the entry when it appears in an already-loaded parent's `Entries`, in listed order. It creates visibility, not authority, scope, or precedence. Direct directive files carry it so generic parent traversal loads every direct file after the directive route has established scope.
- #KeepInMind: read or recheck the complete routed #KeepInMind catalogue at task start or resume, after actual context restoration, before handoff, and before closeout, plus a transition where its follow-ups may have changed. Keep every result active as follow-up context within the authority and scope established by its route and content. It remains deliberately independent of the current parent chain.
- #Contextual and #CurrentTruth distinguish supporting context from accepted current state.
- #Evergreen remains independent of #CurrentTruth: synchronization creates neither authority nor load policy.
- #OpenForge, #LoadWithParentEntrypoint, and #LoadForPostWorkReview are retired: #OpenForge fused core identity with load policy (identity is carried by #Core, #Memory, and framework paths); #LoadWithParentEntrypoint duplicated the same visibility rule; #LoadForPostWorkReview asked for a load at the moment agents demonstrably forget.
- No load-policy tag creates authority, scope, precedence, or mechanical enforcement; compliance is agent self-enforcement, so tag definitions use agent-imperative wording ("read X") rather than tool-implying wording ("X is loaded").
- #Core, #Memory, and #Extension are layer/routing tags.
- Route type tags such as #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace are routing/search signals unless a loaded entrypoint defines more.
- Tags stay bare in markdown so tools can parse and graph them.
- Use normal words when defining the local concept itself; use tags when pointing to routed authority, classification, promotion, load policy, truth status, search, or references.
- Extensions use load-policy tags only when they deliberately add baseline-loaded or continuity-critical material.
