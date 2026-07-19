---
open-forge:
  description: The entry description is the selection surface; the routed body is the execution recipe
  tags: [Memory, Decision, CurrentTruth, Routing, Formatting]
---

# Routing Surfaces

Accepted 2026-07-08 during the workflow redesign analysis.

- The `entry` description is the selection surface; the routed body is the execution recipe.
- Selection wording such as "use this when..." belongs in frontmatter descriptions and generated `entries`, not in routed bodies that are only read after selection already happened.
- Descriptions are decision-grade: trigger plus outcome, enough to select or skip the route without opening it.
- A routed body opens with one goal statement so a mis-selected agent can confirm fit or back out. The goal statement states the outcome; it does not restate the selection description.
- Directives are the binding exception to body-level back-out: selecting an active directive route settles scope, and every direct directive file loaded from that scope must be obeyed. Its Axioms state the rule; they do not ask the agent to decide applicability again.
- #KeepInMind is the loading exception to ordinary parent-chain visibility: continuity checkpoints recover the complete routed catalogue, while each result keeps the authority of its owning content.
- A workflow's primary phase tag is cheap wayfinding, not its execution contract or a mandatory sequence. The routed Goal remains the decision surface for whether that workflow covers the requested transition.
