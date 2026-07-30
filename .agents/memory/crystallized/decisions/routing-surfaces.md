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
