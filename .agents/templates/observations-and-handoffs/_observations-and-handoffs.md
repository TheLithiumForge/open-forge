---
open-forge:
  description: "Preserve useful execution evidence or prepare a fixed snapshot for an actual transfer"
  tags: [Extension, Template, Memory, Observation, Handoff]
---

# Observation And Handoff Templates

## What observation or transfer is worth preserving?

An `Observation` records a concrete occurrence that may prevent future cost or support reusable learning.

A `Handoff` records an actual transfer or planned resumption only when a fixed snapshot is needed while live state may change. A routine pause does not require a `Handoff`. The active `Working` record holds immediate progress and recovery.

These records do not require a task coordination method.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Seal a transfer boundary with the recipient's next action and an honest verification state](handoff.md) - #Extension #Template #Memory #Handoff
- [Preserve one useful occurrence without turning an interpretation into a rule](observation.md) - #Extension #Template #Memory #Observation
