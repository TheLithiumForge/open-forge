---
open-forge:
  description: Explore whether reusable perspective lenses should become a distinct Framework primitive or remain guidance and Task context
  tags: [Memory, Idea, Contextual, Candidate, Perspective, Architecture, Task, Review, Agent]
---

# Perspective Lenses

## Opportunity

Open Forge may benefit from a reusable way to name the questions and planning
horizon an agent should apply independently of its operational role. A top-down
architect, task master, safety advisor, and architecture reviewer can use
different perspectives even when the same agent performs the work.

## Current Understanding

Roles describe who or what performs an action. Directives define required
behavior. Guidance helps with recurring choices. A perspective instead describes
what the actor must keep in view and which questions it must ask.

The replacement CLI has accepted immediate use of explicit top-down architect and
task-master perspectives. The current implementation places those requirements in
a Directive and their reusable question sets in Guidance. This solves the local
need without adding a new Core primitive.

Tasks and Plans are undergoing a related local trial through experimental
Templates and the Task Work Modes Idea. They may become reusable Memory roles,
Templates, an Extension, or remain workspace-local records. They do not require a
backlog to be useful.

## Possibilities

1. Keep perspectives as named sections in existing Guidance and delegation
   packets.
2. Add perspective Templates that can be copied into local agent or review
   instructions.
3. Add a routed Perspective primitive only if distinct selection, loading,
   inheritance, and composition behavior is demonstrated.
4. Package task, plan, and perspective support together or separately as optional
   Extensions after local trials establish their independent value.

## Evidence

- The CLI route-list reset showed that an implementation role and a high-reasoning
  primary agent did not by themselves preserve the top-down architecture horizon.
- Existing advisor and reviewer roles already accept a named lens, but no reusable
  source currently defines the top-down architect and task-master question sets.
- Experimental Task and Plan Templates now separate what must be accomplished
  from how coordinated execution reaches it.

## Open Questions

- Does a perspective need loading or inheritance behavior that Guidance and Task
  links cannot provide?
- Should perspectives compose, and how would conflicts be reported?
- Are perspectives selected by work type, agent role, Task, or maintainer choice?
- Can task and plan records remain ordinary Memory without a backlog or new
  primitive?
- Which local trials demonstrate lower rework rather than more ceremony?

## Promotion Signals

Consider a new primitive only after several independent workstreams reuse the
same perspective shapes and need behavior that existing Guidance, Directives,
Templates, and Task links cannot express cleanly. Preserve a smaller mechanism if
it remains sufficient.

## Related Records And Sources

- [Architectural Perspectives](../../../guidance/architectural-perspectives.md)
- [Program Architecture And Delegation](../../../directives/program-architecture.md)
- [Architectural Context Delegation Gap](../observations/2026-08-21_architectural-context-delegation-gap.md)
- [Task Work Modes](task-work-modes.md)
