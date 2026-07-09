---
open-forge:
  description: Accepted tag semantics - two load-policy tags (LoadNow, KeepInMind), layer and route type tags classify, tags stay bare and parseable
  tags: [Memory, Decision, CurrentTruth, Tags, Routing]
---

# Tags

Extracted 2026-07-06; load-policy semantics reworked 2026-07-09 from the dogfood evidence.

- Defined loader tags with behavior or truth-status semantics are #LoadNow, #KeepInMind, #Contextual, and #CurrentTruth.
- #LoadNow: read the entry when it appears in loaded `Entries`, in listed order. It creates visibility, not authority, scope, or precedence.
- #KeepInMind: read the entry like #LoadNow, keep its instructions active while working, and recheck it before ending meaningful work. It replaces end-of-work loading, which agents demonstrably forgot.
- #OpenForge, #LoadWithParentEntrypoint, and #LoadForPostWorkReview are retired: #OpenForge fused core identity with load policy (identity is carried by #Core, #Memory, and framework paths); #LoadWithParentEntrypoint duplicated the same visibility rule; #LoadForPostWorkReview asked for a load at the moment agents demonstrably forget.
- No load-policy tag is mechanically enforced; compliance is agent self-enforcement, so tag definitions use agent-imperative wording ("read X") rather than tool-implying wording ("X is loaded").
- #Core, #Memory, and #Extension are layer/routing tags.
- Route type tags such as #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace are routing/search signals unless a loaded entrypoint defines more.
- Tags stay bare in markdown so tools can parse and graph them.
- Use normal words when defining the local concept itself; use tags when pointing to routed ownership, classification, promotion, load policy, truth status, search, or references.
- Extensions use load-policy tags only when they deliberately add baseline-loaded material.
