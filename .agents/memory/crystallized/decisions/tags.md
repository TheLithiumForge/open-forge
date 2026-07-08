---
open-forge:
  description: Accepted tag semantics - load-policy tags create visibility only, layer and route type tags classify, tags stay bare and parseable
  tags: [Memory, Decision, CurrentTruth, Tags, Routing]
---

# Tags

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- Defined loader tags with behavior or truth-status semantics are #OpenForge, #LoadWithParentEntrypoint, #LoadForPostWorkReview, #Contextual, and #CurrentTruth.
- #OpenForge loads visible Open Forge core routes by default. It creates visibility, not authority, scope, or precedence.
- #Core, #Memory, and #Extension are layer/routing tags.
- Route type tags such as #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace are routing/search signals unless a loaded entrypoint defines more.
- Tags stay bare in markdown so tools can parse and graph them.
- Use normal words when defining the local concept itself; use tags when pointing to routed ownership, classification, promotion, load policy, truth status, search, or references.
